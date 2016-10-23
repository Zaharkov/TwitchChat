using TwitchApi;
using TwitchApi.Utils;
using TwitchChat.Code;
using TwitchChat.Dialog;
using TwitchChat.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DotaClient;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat
{
    public class MainWindowViewModel : ViewModelBase
    {
        //  Main IRC client
        private readonly TwitchIrcClient _irc;

        //  New Channel name input
        private string _newChannelName;
        public string NewChannelName
        {
            get
            {
                return _newChannelName;
            }
            set
            {
                _newChannelName = value;
                NotifyPropertyChanged();
            }
        }

        // New Whisper name input
        private string _newWhisperUserName;
        public string NewWhisperUserName
        {
            get { return _newWhisperUserName; }
            set
            {
                _newWhisperUserName = value;
                NotifyPropertyChanged();
            }
        }

        private bool IsLogged => _irc?.State == IrcState.Registered;

        public ICommand LoginCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public ICommand JoinCommand { get; private set; }
        public ICommand WhisperCommand { get; private set; }

        private ChannelViewModel _currentChannel;
        public ChannelViewModel CurrentChannel
        {
            get { return _currentChannel; }
            set
            {
                _currentChannel = value;
                NotifyPropertyChanged();
            }
        }

        //  Stores all joined channels
        public ObservableCollection<ChannelViewModel> Channels { get; set; }
        //  Stores all active whisper sessions
        public ObservableCollection<WhisperWindowViewModel> Whispers { get; set; }

        public MainWindowViewModel()
        {
            _irc = new TwitchIrcClient();

            _irc.OnWhisper += OnWhisper;
            _irc.OnDisconnect += OnDisconnect;
            _irc.OnError += OnError;

            //  Setup delegate commands
            LoginCommand = new RelayCommand(Login, () => !IsLogged);
            LogoutCommand = new RelayCommand(Logout, () => IsLogged);

            JoinCommand = new RelayCommand(Join, () => IsLogged);
            WhisperCommand = new RelayCommand(Whisper, () => IsLogged);

            //  Setup observable collections
            Channels = new ObservableCollection<ChannelViewModel>();
            Whispers = new ObservableCollection<WhisperWindowViewModel>();
        }

        private void OnError(Exception e)
        {
            throw e;
        }

        private void OnDisconnect()
        {
            Logout();
            MessageBox.Show("Server was disconnected");
        }

        //  Join a new channel
        private void Join()
        {
            if (Channels.All(x => !x.ChannelName.Equals(NewChannelName, StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    var result = TwitchApiClient.GetUserByName(NewChannelName.ToLower());

                    var vm = new ChannelViewModel(_irc, result.Name);
                    vm.Parted += OnParted;
                    Channels.Add(vm);
                    CurrentChannel = vm;
                }
                catch (ErrorResponseDataException ex)
                {
                    if ((int)ex.Status == 422)
                        MessageBox.Show("Channel not found");
                }  
            }
            NewChannelName = string.Empty;
        }

        //  Channel was parted
        private void OnParted(ChannelViewModel model)
        {
            model.Parted -= OnParted;

            Application.Current.Dispatcher.Invoke(() => {
                Channels.Remove(model);
            });
        }

        //  Open a whisper once a whisper is received
        private void OnWhisper(MessageEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Whispers.Any(x => x.UserName.Equals(e.Username)))
                    Whispers.Add(new WhisperWindowViewModel(_irc, e.Username, e));
            });
        }

        //  Start a new private message
        private void Whisper()
        {
            if (!Whispers.Any(x => x.UserName.Equals(NewWhisperUserName)))
            {
                try
                {
                    var result = TwitchApiClient.GetUserByName(NewWhisperUserName.ToLower());
                    if (result.Name.Equals(_irc.User))
                        MessageBox.Show("Unable to message self");
                    else
                        Whispers.Add(new WhisperWindowViewModel(_irc, NewWhisperUserName));
                }
                catch (ErrorResponseDataException ex)
                {
                    if(ex.Status == HttpStatusCode.NotFound)
                        MessageBox.Show("User not found");
                }
            }
            NewWhisperUserName = string.Empty;
        }

        //  Login to twitch
        private void Login()
        {
            LoginWindow.Login(LoginType.Twitch);
            LoginWindow.Login(LoginType.Vk);
            Task.Run(() => DotaClientApi.Init());

            try
            {
                var user = TwitchApiClient.GetUserByToken();

                if(_irc.State == IrcState.Closed)
                    _irc.Reconnect();

                _irc.Login(user.Name, "oauth:" + TwitchApiClient.GetToken());
            }
            catch (ErrorResponseDataException ex)
            {
                if (ex.Status == HttpStatusCode.NotFound)
                    MessageBox.Show("Cannot login - twitch user not found");
            }
        }

        //  Logout from twitch
        public void Logout()
        {
            var channels = Channels.ToList();
            var whispers = Whispers.ToList();

            foreach (var channel in channels)
                channel.PartCommand.Execute(new string[] {});

            foreach (var whisper in whispers)
                whisper.Remove(whisper, EventArgs.Empty);

            DotaClientApi.Disconnect();

            if (_irc.State == IrcState.Registered)
                _irc.Quit();
        }
    }
}