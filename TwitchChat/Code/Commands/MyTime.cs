using Database;
using TwitchChat.Controls;

namespace TwitchChat.Code.Commands
{
    public static class MyTimeCommand
    {
        public static string GetMyTime(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var chatTime = (long)userModel.Timer.Elapsed.TotalSeconds;
            var dbTime = SqLiteClient.GetChatterTime(userModel.Name, e.Channel);

            var totalTime = chatTime + dbTime;

            return $"слежу за тобой уже {totalTime} {GetSecondsName(totalTime)}";
        }

        private static string GetSecondsName(long seconds)
        {
            if (seconds % 10 == 1)
                return "секунду";

            if (seconds%10 != 2 && seconds%10 != 3 && seconds%10 != 4)
                return "секунд";

            if (seconds % 100 > 11 && seconds % 100 < 15)
                return "секунд";

            return "секунды";
        }
    }
}
