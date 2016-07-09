﻿using TwitchApi;
using TwitchApi.Entities;
using TwitchChat.Code;
using TwitchChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Database;
using Database.Entities;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;

namespace TwitchChat.Controls
{
    public class ChannelViewModel : ViewModelBase
    {
        private readonly TwitchIrcClient _irc;

        //  Store channel specific status
        private bool _mod;
        private bool _subscriber;

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

            _irc.MessageReceived += MessageReceived;
            _irc.UserParted += UserParted;
            _irc.UserJoined += UserJoined;
            _irc.NamesReceived += NamesReceived;
            _irc.UserStateReceived += UserStateReceived;

            _channelName = channelName;

            _irc.Join(_channelName);

            SendCommand = new DelegateCommand(Send);
            PartCommand = new DelegateCommand(Part);

            _badges = TwitchApiClient.GetBadges(channelName);

            _cancellationTokens = TimerFactory.InitTimers(this);
        }

        private void UserStateReceived(object sender, UserStateEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                _mod = e.Mod;
                _subscriber = e.Subscriber;
            }
        }

        private void NamesReceived(object sender, NamesReceivedEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                //  Add names to general viewer group
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var name in e.Names)
                    {
                        if (!group.Members.Any(x => name.Equals(x.Name)))
                            group.Members.Add(new ChatMemberViewModel { Name = name });
                    }
                });
            }
        }

        private void UserJoined(object sender, TwitchEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                //  Add names to general viewer group
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!group.Members.Any(x => e.User.Equals(x.Name)))
                        group.Members.Add(new ChatMemberViewModel { Name = e.User });
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

        private void UserParted(object sender, TwitchEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                var group = GetGroup();

                var user = group.Members.FirstOrDefault(x => e.User.Equals(x.Name));
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

        private void MessageReceived(object sender, MessageEventArgs e)
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
                    var result = CommandFactory.ExecuteCommand(e, FindOrJoinUser(e.User));

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
            //  Populate a name value collection to fake a MessageEventArg
            NameValueCollection nvc = new NameValueCollection
            {
                ["color"] = _irc.Color,
                ["display-name"] = _irc.DisplayName,
                ["mod"] = _mod ? "1" : "0",
                ["subscriber"] = _subscriber ? "1" : "0",
                ["turbo"] = _irc.Turbo ? "1" : "0",
                ["user-type"] = _irc.UserType == UserType.Moderator
                    ? "mod"
                    : _irc.UserType == UserType.GlobalMod
                        ? "global_mod"
                        : _irc.UserType == UserType.Admin
                            ? "admin"
                            : _irc.UserType == UserType.Staff
                                ? "staff"
                                : ""
            };
            Messages.Add(new ChatMessageViewModel(new MessageEventArgs(nvc, Message) { User = _irc.User, Channel = ChannelName }, _badges));
            if (Messages.Count > App.Maxmessages)
                Messages.RemoveAt(0);

            _irc.Message(_channelName, Message);
            Message = string.Empty;
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
