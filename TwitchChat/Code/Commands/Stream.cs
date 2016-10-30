using System;
using TwitchApi;
using TwitchChat.Controls;
using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class StreamCommand
    {
        private static readonly Stream Texts = ConfigHolder.Configs.Stream;

        public static SendMessage GetUpTime(ChatMemberViewModel userModel)
        {
            var stream = TwitchApiClient.GetStreamInfo(userModel.Channel.ChannelName);

            if (stream.Stream == null)
                return SendMessage.GetMessage(Texts.End);

            var time = DateTime.UtcNow.Subtract(stream.Stream.CreatedAt);

            var message = string.Format(Texts.Active,
                time.Hours > 0 ? string.Format(Texts.Hours, time.Hours, GetHoursName(time.Hours)) : "",
                time.Minutes > 0 ? string.Format(Texts.Minutes, time.Minutes, GetMinutesName(time.Minutes)) : "",
                string.Format(Texts.Seconds, time.Seconds, GetSecondsName(time.Seconds))
            );
            return SendMessage.GetMessage(message);
        }

        public static SendMessage GetDelay(ChatMemberViewModel userModel)
        {
            var stream = TwitchApiClient.GetStreamInfo(userModel.Channel.ChannelName);

            if (stream.Stream == null)
                return SendMessage.GetMessage(Texts.End);

            var delay = stream.Stream.Delay;

            return SendMessage.GetMessage(string.Format(Texts.Delay, string.Format(Texts.Seconds, delay, GetSecondsName(delay))));
        }

        private static string GetHoursName(long hours)
        {
            if (hours % 10 == 1)
                return "час";

            if (hours % 10 != 2 && hours % 10 != 3 && hours % 10 != 4)
                return "часов";

            if (hours % 100 > 11 && hours % 100 < 15)
                return "часов";

            return "часа";
        }

        private static string GetMinutesName(long minutes)
        {
            if (minutes % 10 == 1)
                return "минуту";

            if (minutes % 10 != 2 && minutes % 10 != 3 && minutes % 10 != 4)
                return "минут";

            if (minutes % 100 > 11 && minutes % 100 < 15)
                return "минут";

            return "минуты";
        }

        private static string GetSecondsName(long seconds)
        {
            if (seconds % 10 == 1)
                return "секунду";

            if (seconds % 10 != 2 && seconds % 10 != 3 && seconds % 10 != 4)
                return "секунд";

            if (seconds % 100 > 11 && seconds % 100 < 15)
                return "секунд";

            return "секунды";
        }
    }
}
