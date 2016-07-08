using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void Execute(ChannelViewModel channelModel)
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
                            var newUsers = TwitchApiClient.GetUsersList(channelModel.ChannelName);
                            var listForUpdate = new List<ChatterData>();

                            foreach (ChatterType chatterType in Enum.GetValues(typeof(ChatterType)))
                            {
                                var group = channelModel.GetGroup(chatterType).Members;
                                foreach (var user in group)
                                {
                                    if (!newUsers.Chatters[chatterType].Any(t => t.Equals(user.Name)))
                                    {
                                        group.Remove(user);
                                        listForUpdate.Add(new ChatterData(user.Name, channelModel.ChannelName, chatterType.ToString(), (long)user.Timer.Elapsed.TotalSeconds));
                                    }
                                }

                                foreach (var user in newUsers.Chatters[chatterType])
                                {
                                    if (!group.Any(t => t.Name.Equals(user)))
                                        group.Add(new ChatMemberViewModel { Name = user });
                                }
                            }

                            SqLiteClient.UpdateChatterInfo(listForUpdate);
                        };
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                PeriodicTaskFactory.Start(action, delay * 1000);
            }
        }
    }
}
