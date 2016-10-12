using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using CommonHelper;
using Domain.Repositories;
using TwitchApi;
using TwitchApi.Entities;
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
                            var newUsers = TwitchApiClient.GetUsersList(channelModel.ChannelName);
                            var listForUpdate = new List<ChatterInfo>();

                            foreach (ChatterType chatterType in Enum.GetValues(typeof(ChatterType)))
                            {
                                var group = channelModel.GetGroup(chatterType);
                                var forDelete = new List<ChatMemberViewModel>();
                                foreach (var user in group.Get())
                                {
                                    if (!newUsers.Chatters[chatterType].Any(t => t.Equals(user.Name)))
                                    {
                                        forDelete.Add(user);
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
                                        group.Remove(memberViewModel);

                                    foreach (var user in newUsers.Chatters[chatterType])
                                    {
                                        if (!group.Any(t => t.Name.Equals(user)))
                                            group.Add(new ChatMemberViewModel(user, channelModel));
                                    }
                                });
                            }

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
