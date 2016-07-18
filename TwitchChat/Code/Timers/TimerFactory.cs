using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using CommonHelper;
using Database;
using Database.Entities;
using TwitchApi;
using TwitchApi.Entities;
using TwitchChat.Code.Helpers;
using TwitchChat.Controls;

namespace TwitchChat.Code.Timers
{
    public static class TimerFactory
    {
        private static readonly Dictionary<Timer, int> Config = DelayConfig<Timer>.GetDelayConfig("TimersConfig");

        public static List<CancellationTokenSource> InitTimers(ChannelViewModel channelModel)
        {
            var cancelTokens = new List<CancellationTokenSource>();
            foreach (Timer timer in Enum.GetValues(typeof(Timer)))
            {
                var delay = Config.ContainsKey(timer) ? Config[timer] : Config[Timer.Global];

                Action action;

                switch (timer)
                {
                    case Timer.Global:
                        continue;
                    case Timer.Help:
                    {
                        action = () =>
                        {
                            var text = Help.GetHelpTimerText();
                            channelModel.Client.Message(channelModel.ChannelName, text);
                        };
                        break;
                    }
                    case Timer.UpdateChatters:
                    {
                        action = () =>
                        {
                            var newUsers = TwitchApiClient.GetUsersList(channelModel.ChannelName);
                            var listForUpdate = new List<ChatterData>();

                            foreach (ChatterType chatterType in Enum.GetValues(typeof(ChatterType)))
                            {
                                var group = channelModel.GetGroup(chatterType).Members;
                                var forDelete = new List<ChatMemberViewModel>();
                                foreach (var user in group)
                                {
                                    if (!newUsers.Chatters[chatterType].Any(t => t.Equals(user.Name)))
                                    {
                                        forDelete.Add(user);
                                        listForUpdate.Add(new ChatterData(user.Name, channelModel.ChannelName, chatterType.ToString(), user.GetTimeAndRestart()));
                                    }
                                }

                                foreach (var memberViewModel in forDelete)
                                    Application.Current.Dispatcher.Invoke(() => group.Remove(memberViewModel));

                                foreach (var user in newUsers.Chatters[chatterType])
                                {
                                    if (!group.Any(t => t.Name.Equals(user)))
                                        Application.Current.Dispatcher.Invoke(() => group.Add(new ChatMemberViewModel { Name = user }));
                                }
                            }

                            SqLiteClient.UpdateChatterInfo(listForUpdate);
                        };
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var cancelToken = new CancellationTokenSource();
                cancelTokens.Add(cancelToken);
                PeriodicTaskFactory.Start(action, delay * 1000, cancelToken: cancelToken.Token);
            }

            return cancelTokens;
        }
    }
}
