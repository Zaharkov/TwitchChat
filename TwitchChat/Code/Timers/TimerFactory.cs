using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonHelper;
using Domain.Repositories;
using TwitchApi;
using TwitchChat.Controls;
using ChatterInfo = Domain.Models.ChatterInfo;

namespace TwitchChat.Code.Timers
{
    public static class TimerFactory
    {
        private static readonly Dictionary<Timer, int> Config = DelayConfig<Timer>.GetDelayConfig("TimersConfig");
        private static readonly Dictionary<ChannelViewModel, List<CancellationTokenSource>> CancellationTokenSources = new Dictionary<ChannelViewModel, List<CancellationTokenSource>>();
        private static readonly object LockObject = new object();

        private static void AddToken(ChannelViewModel channelModel, CancellationTokenSource tokenSource)
        {
            lock (LockObject)
            {
                if (CancellationTokenSources.ContainsKey(channelModel) &&
                    !CancellationTokenSources[channelModel].Contains(tokenSource))
                    CancellationTokenSources[channelModel].Add(tokenSource);
                else
                    CancellationTokenSources.Add(channelModel, new List<CancellationTokenSource> {tokenSource});
            }
        }

        public static List<CancellationTokenSource> GetTimers(ChannelViewModel channelModel)
        {
            lock (LockObject)
            {
                return CancellationTokenSources.ContainsKey(channelModel)
                    ? CancellationTokenSources[channelModel]
                    : new List<CancellationTokenSource>();
            }
        }

        public static CancellationTokenSource Start(ChannelViewModel channelModel, Action action, int intervalInMilliseconds)
        {
            var tokenSource = new CancellationTokenSource();
            AddToken(channelModel, tokenSource);
            PeriodicTaskFactory.Start(action, intervalInMilliseconds, cancelToken: tokenSource.Token);
            return tokenSource;
        }

        public static Task RunOnce(ChannelViewModel channelModel, Action action)
        {
            var tokenSource = new CancellationTokenSource();
            AddToken(channelModel, tokenSource);
            return PeriodicTaskFactory.Start(action, cancelToken: tokenSource.Token, maxIterations: 1);
        }

        public static void Stop(ChannelViewModel channelModel, CancellationTokenSource tokenSource)
        {
            lock (LockObject)
            {
                if (CancellationTokenSources.ContainsKey(channelModel) &&
                    CancellationTokenSources[channelModel].Contains(tokenSource))
                {
                    if (!tokenSource.IsCancellationRequested)
                    {
                        tokenSource.Cancel();
                        tokenSource.Dispose();
                    }

                    CancellationTokenSources[channelModel].Remove(tokenSource);
                }
            }
        }

        public static void InitTimers(ChannelViewModel channelModel)
        {
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
                            var chatterInfo = TwitchApiClient.GetUsersList(channelModel.ChannelName);
                            var newUsers = chatterInfo.Chatters.GetAll();
                            var listForUpdate = new List<ChatterInfo>();

                            var allChatters = channelModel.GetAllChatters();
                            var forDelete = new List<ChatMemberViewModel>();
                            foreach (var user in allChatters)
                            {
                                if (!newUsers.Any(t => t.Key.Equals(user.Name)))
                                {
                                    forDelete.Add(user);

                                    var exists = listForUpdate.FirstOrDefault(t => 
                                        t.Name.Equals(user.Name, StringComparison.InvariantCultureIgnoreCase) && 
                                        t.ChatName.Equals(channelModel.ChannelName, StringComparison.InvariantCultureIgnoreCase)
                                    );

                                    if (exists != null)
                                    {
                                        exists.Seconds += user.GetTimeAndRestart();
                                        continue;
                                    }

                                    listForUpdate.Add(new ChatterInfo
                                    {
                                        Name = user.Name,
                                        ChatName = channelModel.ChannelName,
                                        Seconds = user.GetTimeAndRestart()
                                    });
                                }
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var memberViewModel in forDelete)
                                    allChatters.Remove(memberViewModel);

                                foreach (var user in newUsers)
                                {
                                    var group = channelModel.GetGroup(user.Value);

                                    if (!allChatters.Any(t => t.Name.Equals(user.Key)))
                                        group.Add(new ChatMemberViewModel(user.Key, channelModel, group));
                                }
                            });

                            ChatterInfoRepository.Instance.UpdateChatterInfo(channelModel.ChannelName, listForUpdate);
                        };
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Start(channelModel, action, delay*1000);
            }
        }
    }
}
