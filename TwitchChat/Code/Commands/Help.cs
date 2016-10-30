using System.Linq;
using System.Text;
using Configuration;
using Twitchiedll.IRC.Enums;

namespace TwitchChat.Code.Commands
{
    public static class HelpCommand
    {
        private static readonly Configuration.Entities.Help Texts = ConfigHolder.Configs.Help;

        public static SendMessage GetHelp()
        {
            return SendMessage.GetWhisper(GetHelpTimerText());
        }

        public static string GetHelpTimerText()
        {
            var groupedAccess = CommandAccess.GetGroupedAccess();

            var builder = new StringBuilder();
            builder.Append(Texts.StartMessage);

            foreach (var grouped in groupedAccess)
            {
                var commands = string.Join(",", grouped.Key.Select(t => $"{TwitchConstName.Command}{t}"));

                builder.Append(grouped.Value == UserType.Default
                    ? string.Format(Texts.DefaultMessage, commands)
                    : string.Format(Texts.CustomMessage, commands, string.Join(",", grouped.Value)));
            }

            return builder.ToString();
        }
    }
}
