using System;
using System.Collections.Generic;
using TwitchChat.Code;
using TwitchChat.ViewModel;
using Twitchiedll.IRC.Events;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Twitchiedll.IRC.Enums;

namespace TwitchChat.Dialog
{
    public class WhisperWindowViewModel : ViewModelBase
    {
        private readonly TwitchIrcClient _irc;

        //  User viewmodel is for
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyPropertyChanged();
            }
        }

        //  Message to be sent to user
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

        public delegate void RemoveEventHandler(WhisperWindowViewModel model);
        public event RemoveEventHandler OnRemove; 

        //  Collectrion of all messages
        public ObservableCollection<MessageViewModel> Messages { get; set; } = new ObservableCollection<MessageViewModel>();

        //  Command to send message
        public ICommand SendCommand { get; private set; }

        public WhisperWindowViewModel(TwitchIrcClient irc, string userName, MessageEventArgs e = null)
        {
            _irc = irc;
            _irc.OnWhisper += WhisperReceived;
            _userName = userName;

            SendCommand = new DelegateCommand(Send);

            if (e != null)
                WhisperReceived(e);
        }

        public void Remove(object sender, EventArgs args)
        {
            _irc.OnWhisper -= WhisperReceived;
            OnRemove?.Invoke(this);
        }

        private void WhisperReceived(MessageEventArgs e)
        {
            if (e.Username == UserName)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageViewModel(e));
                    if (Messages.Count > App.Maxmessages)
                        Messages.RemoveAt(0);
                });
            }
        }

        //  Send a whisper to chosen user
        private void Send()
        {
            var userInfo = _irc.UserStateInfo.FirstOrDefault();
            var color = userInfo.Equals(default(KeyValuePair<string, UserStateEventArgs>)) ? null : userInfo.Value.ColorHex;

            var isAction = false;
            if (Message.StartsWith(TwitchConstName.Action))
            {
                isAction = true;
                Message = Message.Remove(0, 3).TrimStart(' ');
            }

            Messages.Add(new MessageViewModel(_irc.User, Message, color, isAction));
            if (Messages.Count > App.Maxmessages)
                Messages.RemoveAt(0);

            _irc.Whisper(_userName, Message);
            Message = string.Empty;
        }
    }
}