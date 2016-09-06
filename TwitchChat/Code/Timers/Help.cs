using System.Linq;
using System.Text;
using TwitchChat.Code.Commands;
using Twitchiedll.IRC;

namespace TwitchChat.Code.Timers
{
    public static class Help
    {
        public static string GetHelpTimerText()
        {
            var groupedAccess = CommandAccess.GetGroupedAccess();

            var builder = new StringBuilder();
            builder.Append("Доступны следующие комадны для бота: ");

            foreach (var grouped in groupedAccess)
            {
                var commands = string.Join(",", grouped.Key.Select(t => $"!{t}"));

                var ending = grouped.Key.Count > 1 ? "ы" : "а";

                builder.Append(grouped.Value == UserType.Default
                    ? $"{commands} - доступн{ending} всем; "
                    : $"{commands} - доступн{ending} для {string.Join(",", grouped.Value)}; ");
            }

            return builder.ToString();
        }
    }
}
