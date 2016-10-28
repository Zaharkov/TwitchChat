using TwitchApi;
using TwitchApi.Entities;
using TwitchChat.Code;
using TwitchChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Domain.Repositories;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Controls
{
    public class ChannelViewModel : ViewModelBase
    {
        private readonly TwitchIrcClient _irc;

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
        public delegate void PartedEventHandler(ChannelViewModel model);
        public event PartedEventHandler Parted;

        public ChannelViewModel(TwitchIrcClient irc, string channelName)
        {
            _irc = irc;
            _irc.OnRawMessage += OnRawMessage;
            _irc.OnMessage += OnMessage;
            //_irc.OnRoomState;
            _irc.OnMode += OnMode;
            _irc.OnNames += OnNames;
            _irc.OnJoin += OnJoin;
            _irc.OnPart += OnPart;
            //_irc.OnNotice;
            _irc.OnSubscribe += OnSubscribe;
            //_irc.OnClearChat;

            _channelName = channelName;

            _irc.Join(_channelName);

            SendCommand = new DelegateCommand(Send);
            PartCommand = new DelegateCommand(Part);

            _badges = TwitchApiClient.GetBadges(channelName);

            TimerFactory.InitTimers(this);
            RouletteInfoRepository.Instance.ResetAllDuelNames();
        }

        private void OnRawMessage(string buffer)
        {
            Debug.WriteLine(buffer);
        }

        private void OnNames(NamesEventArgs e)
        {
            if (e.Names.ContainsKey(ChannelName))
            {
                //  Add names to general viewer group
                var allChatters = GetAllChatters();
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var name in e.Names[ChannelName])
                    {
                        if (!allChatters.Any(x => name.Equals(x.Name)))
                            group.Add(new ChatMemberViewModel(name, this, group));
                    }
                });
            }
        }

        private void OnJoin(JoinEventArgs e)
        {
            if (e.Channel.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase))
            {
                //  Add names to general viewer group
                var allChatters = GetAllChatters();
                var group = GetGroup();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!allChatters.Any(x => e.Username.Equals(x.Name)))
                        group.Add(new ChatMemberViewModel(e.Username, this, group));
                });
            }
        }

        private ChatMemberViewModel FindOrJoinUser(string name, UserType type)
        {
            var mapType = Map(type);
            var group = GetGroup(mapType);
            var allChatters = GetAllChatters();
            var user = allChatters.FirstOrDefault(x => name.Equals(x.Name));

            if (user == null)
            {
                user = new ChatMemberViewModel(name, this, group);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!allChatters.Any(x => name.Equals(x.Name)))
                        group.Add(user);
                });
            }
            else if(user.Group.Name != mapType.ToString())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    user.Group.Remove(user);
                    group.Add(user);
                });
            }

            return user;
        }

        private ChatterType Map(UserType type)
        {
            if (type.HasFlag(UserType.Admin))
                return ChatterType.Admins;

            if (type.HasFlag(UserType.Globalmoderator))
                return ChatterType.GlobalMods;

            if (type.HasFlag(UserType.Staff))
                return ChatterType.Staff;

            if (type.HasFlag(UserType.Moderator) || type.HasFlag(UserType.Broadcaster))
                return ChatterType.Moderators;

            return ChatterType.Viewers;
        }

        private void OnPart(PartEventArgs e)
        {
            if (e.Channel.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase))
            {
                var group = GetAllChatters();

                var user = group.FirstOrDefault(x => e.Username.Equals(x.Name));
                if (user != null)
                {
                    //  Remove once user is gound and removed
                    Application.Current.Dispatcher.Invoke(() => { group.Remove(user); });

                    ChatterInfoRepository.Instance.AddSecods(user.Name, ChannelName, user.GetTimeAndRestart());
                }
            }
        }

        private void OnMessage(MessageEventArgs e)
        {
            if (e.Channel.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessageViewModel(e, _badges));
                    if (Messages.Count > App.Maxmessages)
                        Messages.RemoveAt(0);
                });

                if (IsChatCommand(e))
                {
                    var result = CommandFactory.ExecuteCommand(e, FindOrJoinUser(e.Username, e.UserType));
                    SendMessage(e, result);
                }
            }
        }

        private void OnMode(ModeEventArgs e)
        {
            if (e.Channel.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (e.AddingMode)
                {
                    var allChatters = GetAllChatters();
                    var modGroup = GetGroup(ChatterType.Moderators);
                    var user = allChatters.FirstOrDefault(x => e.Username.Equals(x.Name));

                    if (user == null)
                    {
                        user = new ChatMemberViewModel(e.Username, this, modGroup);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!allChatters.Any(x => e.Username.Equals(x.Name)))
                                modGroup.Add(user);
                        });
                    }
                    else if (user.Group.Name != ChatterType.Moderators.ToString())
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            user.Group.Remove(user);
                            modGroup.Add(user);
                        });
                    }
                }
            }
        }

        public void OnSubscribe(SubscriberEventArgs e)
        {
            if (e.Channel.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase))
            {
                string message;
                if (e.Months == 1)
                    message = $"Пополнение в стаде медведей! Приветствуем {e.Username}!!! napoSub napoSub napoSub";
                else if (e.Months < 12)
                    message = $"От реподписки у {e.Username} растет пиписька/сиськи!!! napoSub napoSub napoSub";
                else
                    message = $"{e.Username} с нами уже больше года!!! napoLove napoMurr napoMeow napoSub napoSub napoSub";

                _irc.Message(ChannelName, message);
                var userInfo = _irc.UserStateInfo[_channelName];

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessageViewModel(userInfo.UserType, _irc.User, message, userInfo.ColorHex, false, _badges));
                    if (Messages.Count > App.Maxmessages)
                        Messages.RemoveAt(0);
                });
            }
        }

        private static bool IsChatCommand(MessageEventArgs e)
        {
            return !e.Message.StartsWith("! ") && e.Message.StartsWith("!");
        }

        public void SendMessage(MessageEventArgs e, SendMessage message)
        {
            while (message != null)
            {
                if (!string.IsNullOrEmpty(message.Message))
                {
                    var userInfo = _irc.UserStateInfo[_channelName];
                    switch (message.Type)
                    {
                        case SendType.None:
                            break;
                        case SendType.Message:

                            var botMessage = $"БОТ: {(e != null ? "@" + e.Username : "")} {message.Message}";

                            if (e != null && e.IsAction)
                                _irc.Action(ChannelName, botMessage);
                            else
                                _irc.Message(ChannelName, botMessage);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Messages.Add(new ChatMessageViewModel(userInfo.UserType, _irc.User, botMessage,
                                    userInfo.ColorHex, e?.IsAction ?? false, _badges));
                                if (Messages.Count > App.Maxmessages)
                                    Messages.RemoveAt(0);
                            });
                            break;
                        case SendType.Whisper:
                            if(e != null)
                                _irc.Whisper(e.Username, message.Message);
                            break;
                        case SendType.Timeout:
                            _irc.Timeout(ChannelName, message.Message, message.Timeout);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                    }
                }

                message = message.NextMessage;
            }
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

            _irc.Message(_channelName, Message);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new ChatMessageViewModel(userInfo.UserType, _irc.User, Message, userInfo.ColorHex, isAction, _badges));
                if (Messages.Count > App.Maxmessages)
                    Messages.RemoveAt(0);

                Message = string.Empty;
            });
        }

        private void Part()
        {
            foreach (var cancellationToken in TimerFactory.GetTimers(this))
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }

            UpdateChattersInfo();

            if (_irc.State == IrcState.Registered)
                _irc.Part(_channelName);

            _irc.OnRawMessage -= OnRawMessage;
            _irc.OnMessage -= OnMessage;
            //_irc.OnRoomState;
            _irc.OnMode -= OnMode;
            _irc.OnNames -= OnNames;
            _irc.OnJoin -= OnJoin;
            _irc.OnPart -= OnPart;
            //_irc.OnNotice;
            _irc.OnSubscribe -= OnSubscribe;
            //_irc.OnClearChat;

            //  Raise the event.  MainWindowViewModel uses this to remove from list of channels
            Parted?.Invoke(this);
        }

        #region Helpers

        //  Helper to get and create groups
        public List<ChatMemberViewModel> GetAllChatters()
        {
            var allChaters = new List<ChatMemberViewModel>();
            foreach (var chatGroupViewModel in ChatGroups)
                allChaters.AddRange(chatGroupViewModel.Get());

            return allChaters;
        }

        public ChatGroupViewModel GetGroup(ChatterType type = ChatterType.Viewers)
        {
            //  Attempt to get group
            var group = ChatGroups.FirstOrDefault(x => x.Name == type.ToString());
            if (group == null)
            {
                //  Create group if it doesnt exist
                group = new ChatGroupViewModel { Name = type.ToString() };

                Application.Current.Dispatcher.Invoke(() => { ChatGroups.Add(group); });
            }
            return group;
        }

        public void UpdateChattersInfo()
        {
            var listForUpdate = new List<Domain.Models.ChatterInfo>();
            foreach (var user in GetAllChatters())
            {
                var exists = listForUpdate.FirstOrDefault(t => t.Name.Equals(user.Name, StringComparison.InvariantCultureIgnoreCase) && t.ChatName.Equals(ChannelName, StringComparison.InvariantCultureIgnoreCase));

                if (exists != null)
                {
                    exists.Seconds += user.GetTimeAndRestart();
                    continue;
                }

                listForUpdate.Add(new Domain.Models.ChatterInfo
                {
                    Name = user.Name, ChatName = ChannelName, Seconds = user.GetTimeAndRestart()
                });
            }

            ChatterInfoRepository.Instance.UpdateChatterInfo(ChannelName, listForUpdate);
        }

        #endregion
    }
}
