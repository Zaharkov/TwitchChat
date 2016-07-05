using TwitchChat.Controls;

namespace TwitchChat.Code.Commands
{
    public static class MyTimeCommand
    {
        public static string GetMyTime(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var seconds = (int)userModel.Timer.Elapsed.TotalSeconds;

            return $"слежу за тобой уже {seconds} секунд";
        }
    }
}
