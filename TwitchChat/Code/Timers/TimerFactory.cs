using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public static List<CancellationTokenSource> GetTimers(ChannelViewModel channelModel)
        {
            return CancellationTokenSources.ContainsKey(channelModel) 
                ? CancellationTokenSources[channelModel] 
                : new List<CancellationTokenSource>();
        }

        public static void AddToken(ChannelViewModel channelModel, CancellationTokenSource token)
        {
            if (CancellationTokenSources.ContainsKey(channelModel))
            {
                if(!CancellationTokenSources[channelModel].Contains(token))
                    CancellationTokenSources[channelModel].Add(token);
            }
            else
            {
                CancellationTokenSources.Add(channelModel, new List<CancellationTokenSource> {token});
            }
        }

        public static void RemoveToken(ChannelViewModel channelModel, CancellationTokenSource token)
        {
            if (CancellationTokenSources.ContainsKey(channelModel) && CancellationTokenSources[channelModel].Contains(token))
                CancellationTokenSources[channelModel].Remove(token);
        }

        public static void InitTimers(ChannelViewModel channelModel)
        {
            var tokenSources = new List<CancellationTokenSource>();
            CancellationTokenSources.Add(channelModel, tokenSources);
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

                var cancelToken = new CancellationTokenSource();
                tokenSources.Add(cancelToken);
                PeriodicTaskFactory.Start(action, delay * 1000, cancelToken: cancelToken.Token);
            }
        }
    }
}
