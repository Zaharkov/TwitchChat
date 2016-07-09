using TwitchChat.Code.Commands;

namespace TwitchChat.Code.Timers
{
    public static class Help
    {
        public static string GetHelpTimerText()
        {
            return $"БОТ: Список всех пользовательских команд можно посмотреть в справке !{Command.Help}";
        }
    }
}
