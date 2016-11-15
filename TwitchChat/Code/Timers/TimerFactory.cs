using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonHelper;
using Configuration;
using Domain.Repositories;
using TwitchApi;
using TwitchChat.Code.Commands;
using TwitchChat.Controls;
using Twitchiedll.IRC.Enums;
using ChatterInfo = Domain.Models.ChatterInfo;

namespace TwitchChat.Code.Timers
{
    public static class TimerFactory
    {
        private static readonly Dictionary<string, int> Configs = DelayConfig.GetDelayConfig(ConfigHolder.Configs.Global.Cooldowns.Timers);
        private static readonly Dictionary<ChannelViewModel, List<CancellationTokenSource>> CancellationTokenSources = new Dictionary<ChannelViewModel, List<CancellationTokenSource>>();
        private static readonly object LockObject = new object();
        private static bool _updateWasOneTime;

        static TimerFactory()
        {
            foreach (var customCommand in CustomCommands<CommandType, UserType, DelayType>.Commands)
                Configs.Add(customCommand.Name, customCommand.CooldownTime);
        }

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
            var timerCustomCommands = CustomCommands<CommandType, UserType, DelayType>.Commands.Where(t => t.Type == CommandType.Timer);

            foreach (var timerCustomCommand in timerCustomCommands)
            {
                var message = CustomCommandHandler.CreateCommand(timerCustomCommand, null, channelModel);

                Action action = () =>
                {
                    channelModel.Client.Message(channelModel.ChannelName, message.Message);
                };

                Start(channelModel, action, timerCustomCommand.CooldownTime * 1000);
            }

            foreach (Timer timer in Enum.GetValues(typeof(Timer)))
            {
                var delay = Configs.ContainsKey(timer.ToString()) ? Configs[timer.ToString()] : Configs[Timer.Global.ToString()];

                Action action;

                switch (timer)
                {
                    case Timer.Global:
                        continue;
                    case Timer.Help:
                    {
                        action = () =>
                        {
                            var text = HelpCommand.GetHelpTimerText();
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
                            var listForUpdate = newUsers.Select(t => new ChatterInfo { ChatName = channelModel.ChannelName, Name = t.Key, Seconds = delay }).ToList();

                            var allChatters = channelModel.GetAllChatters();
                            var forDelete = allChatters.Where(user => !newUsers.Any(t => t.Key.Equals(user.Name))).ToList();

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

                            if(_updateWasOneTime)
                                ChatterInfoRepository.Instance.UpdateChatterInfo(channelModel.ChannelName, listForUpdate, delay);

                            _updateWasOneTime = true;
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
