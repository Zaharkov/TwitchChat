using TwitchChat.Code.Commands;

namespace TwitchChat.Code.Timers
{
    public static class Help
    {
        public static string GetHelpTimerText()
        {
            return $"список всех пользовательских команд можно посмотреть в справке !{Command.Help}";
        }
    }
}
