using System.Linq;
using System.Text;
using Twitchiedll.IRC;

namespace TwitchChat.Code.Commands
{
    public static class HelpCommand
    {
        public static SendMessage GetHelp()
        {
            var groupedAccess = CommandAccess.GetGroupedAccess();

            var builder = new StringBuilder();

            foreach (var grouped in groupedAccess)
            {
                var commands = string.Join(",", grouped.Key.Select(t => $"!{t}"));

                var ending = grouped.Key.Count > 1 ? "ы" : "а";

                builder.Append(grouped.Value == UserType.Default
                    ? $"{commands} - доступн{ending} всем; "
                    : $"{commands} - доступн{ending} для {string.Join(",", grouped.Value)}; ");
            }

            return SendMessage.GetWhisper(builder.ToString());
        }
    }
}
