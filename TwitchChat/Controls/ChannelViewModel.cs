using TwitchApi;
using TwitchApi.Entities;
using TwitchChat.Code;
using TwitchChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Database;
using Database.Entities;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Controls
{
    public class ChannelViewModel : ViewModelBase
    {
        private readonly TwitchIrcClient _irc;
        private readonly List<CancellationTokenSource> _cancellationTokens;

        //  The badges that are taken from https://api.twitch.tv/kraken/chat/{0}/badges
        private readonly Badges _badges;

        //  Name of the channel
        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set
            {
                _channelName = value;
                NotifyPropertyChanged();
            }
        }

        //  New message to be sent
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }

        public TwitchIrcClient Client => _irc;

        //  Groups of chatters
        public ObservableCollection<ChatGroupViewModel> ChatGroups { get; set; } = new ObservableCollection<ChatGroupViewModel>();
        //  Messages from to the current channel
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; } = new ObservableCollection<ChatMessageViewModel>();

        //  Commands available for each channel
        public ICommand SendCommand { get; private set; }
        public ICommand PartCommand { get; private set; }

        //  Event to notify other models channel has been parted
        public event EventHandler Parted;

        public ChannelViewModel(TwitchIrcClient irc, string channelName)
        {
            _irc = irc;

            _irc.OnRawMessage += OnRawMessage;
            _irc.OnMessage += OnMessage;
            _irc.OnPing += OnPing;
            //_irc.OnRoomState;
            //_irc.OnMode; TODO move to moderator list
            _irc.OnNames += OnNames;
            _irc.OnJoin += OnJoin;
            _irc.OnPart += OnPart;
            //_irc.OnNotice;
            //_irc.OnSubscribe; TODO 
            //_irc.OnClearChat; TODO

            _channelName = channelName;

            _irc.Join(_channelName);

            SendCommand = new DelegateCommand(Send);
            PartCommand = new DelegateCommand(Part);

            _badges = TwitchApiClient.GetBadges(channelName);

            _cancellationTokens = TimerFactory.InitTimers(this);
        }

        private void OnRawMessage(string buffer)
        {
            Debug.WriteLine(buffer);
        }

        private void OnPing(string buffer)
        {
            _irc.Pong(buffer);
        }

        private void OnNames(NamesEventArgs e)
        {
            if (e.Names.ContainsKey(ChannelName))
            {
                //  Add names to general viewer group
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var name in e.Names[ChannelName])
                    {
                        if (!group.Members.Any(x => name.Equals(x.Name)))
                            group.Members.Add(new ChatMemberViewModel { Name = name });
                    }
                });
            }
        }

        private void OnJoin(JoinEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                //  Add names to general viewer group
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!group.Members.Any(x => e.Username.Equals(x.Name)))
                        group.Members.Add(new ChatMemberViewModel { Name = e.Username });
                });
            }
        }

        private ChatMemberViewModel FindOrJoinUser(string name)
        {
            var group = GetGroup();
            var user = group.Members.FirstOrDefault(x => name.Equals(x.Name));

            if (user == null)
            {
                user = new ChatMemberViewModel { Name = name };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!group.Members.Any(x => name.Equals(x.Name)))
                        group.Members.Add(user);
                });
            }

            return user;
        }

        private void OnPart(PartEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                var group = GetGroup();

                var user = group.Members.FirstOrDefault(x => e.Username.Equals(x.Name));
                if (user != null)
                {
                    //  Remove once user is gound and removed
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        group.Members.Remove(user);
                    });
                }
            }
        }

        private void OnMessage(MessageEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessageViewModel(e, _badges));
                    if (Messages.Count > App.Maxmessages)
                        Messages.RemoveAt(0);
                });

                if (IsChatCommand(e))
                {
                    var result = CommandFactory.ExecuteCommand(e, FindOrJoinUser(e.Username));
                
                    if(!string.IsNullOrEmpty(result))
                        _irc.Message(ChannelName, result);
                }
            }
        }

        private static bool IsChatCommand(MessageEventArgs e)
        {
            return e.Message.StartsWith("!");
        }

        private void Send()
        {
            if (!_irc.UserStateInfo.ContainsKey(_channelName))
                throw new Exception("Channel if undefined! UserState not sended?");

            var userInfo = _irc.UserStateInfo[_channelName];

            var isAction = false;
            if (Message.StartsWith("/me"))
            {
                isAction = true;
                Message = Message.Remove(0, 3).TrimStart(' ');
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new ChatMessageViewModel(userInfo.UserType, _irc.User, Message, userInfo.ColorHex, isAction, _badges));
                if (Messages.Count > App.Maxmessages)
                    Messages.RemoveAt(0);

                _irc.Message(_channelName, Message);
                Message = string.Empty;
            });
        }

        private void Part()
        {
            foreach (var cancellationToken in _cancellationTokens)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }

            UpdateChattersInfo();

            _irc.Part(_channelName);
            //  Raise the event.  MainWindowViewModel uses this to remove from list of channels
            Parted?.Invoke(this, EventArgs.Empty);
        }

        #region Helpers

        //  Helper to get and create groups
        public ChatGroupViewModel GetGroup(ChatterType type = ChatterType.Viewers)
        {
            //  Attempt to get group
            var group = ChatGroups.FirstOrDefault(x => x.Name == type.ToString());
            if (group == null)
            {
                //  Create group if it doesnt exist
                group = new ChatGroupViewModel { Name = type.ToString() };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatGroups.Add(group);
                });
            }
            return group;
        }

        public void UpdateChattersInfo()
        {
            var listForUpdate = new List<ChatterData>();
            foreach (ChatterType chatterType in Enum.GetValues(typeof(ChatterType)))
            {
                var group = GetGroup(chatterType).Members;
                foreach (var user in group)
                {
                    listForUpdate.Add(new ChatterData(user.Name, ChannelName, chatterType.ToString(), (long)user.Timer.Elapsed.TotalSeconds));
                }
            }

            SqLiteClient.UpdateChatterInfo(listForUpdate);
        }

        #endregion
    }
}
