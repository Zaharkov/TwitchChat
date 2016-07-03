using System;
using System.Linq;
using System.Text;

namespace TwitchChat.Code.Commands
{
    public static class HelpCommand
    {
        public static string GetHelp(MessageEventArgs e)
        {
            var accessList = CommandAccess.Accesses;
            var builder = new StringBuilder();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                if(command == Command.Global)
                    continue;

                if (!accessList.ContainsKey(command))
                    builder.Append($"!{command} - доступна всем, ");
                else
                {
                    builder.Append(!accessList[command].Any()
                        ? $"!{command} - доступна всем, "
                        : $"!{command} - доступна для {string.Join(",", accessList[command])}, ");
                }
            }

            return $"/w {e.User} {builder}";
        }
    }
}
