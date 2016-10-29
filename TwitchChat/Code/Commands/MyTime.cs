using Domain.Repositories;
using TwitchChat.Controls;
using TwitchChat.Texts;
using TwitchChat.Texts.Entities;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyTimeCommand
    {
        private static readonly MyTime Texts = TextsHolder.Texts.MyTime;

        public static SendMessage GetMyTime(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var chatTime = userModel.GetTime();
            var dbTime = ChatterInfoRepository.Instance.GetChatterTime(userModel.Name, e.Channel);

            var totalTime = chatTime + dbTime;

            var message = string.Format(Texts.Time, string.Format(Texts.Seconds, totalTime, GetSecondsName(totalTime)));
            return SendMessage.GetMessage(message);
        }

        public static string GetSecondsName(long seconds)
        {
            if (seconds % 100 > 10 && seconds % 100 < 20)
                return "секунд";

            if (seconds % 10 == 1)
                return "секунду";

            if (seconds%10 != 2 && seconds%10 != 3 && seconds%10 != 4)
                return "секунд";

            return "секунды";
        }
    }
}
