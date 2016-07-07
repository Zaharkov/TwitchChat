using System;
using System.Collections.Generic;
using System.Linq;
using TwitchApi;
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

                            foreach (var moderators in channelModel.GetGroup("Moderators").Members)
                            {

                            }

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
