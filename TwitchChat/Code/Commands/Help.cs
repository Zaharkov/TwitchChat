﻿using System.Linq;
using System.Text;
using TwitchChat.Controls;

namespace TwitchChat.Code.Commands
{
    public static class HelpCommand
    {
        public static string GetHelp(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var groupedAccess = CommandAccess.GetGroupedAccess();

            var builder = new StringBuilder();

            foreach (var grouped in groupedAccess)
            {
                var commands = string.Join(",", grouped.Key.Select(t => $"!{t}"));

                var ending = grouped.Key.Count > 1 ? "ы" : "а";

                builder.Append(!grouped.Value.Any()
                    ? $"{commands} - доступн{ending} всем; "
                    : $"{commands} - доступн{ending} для {string.Join(",", grouped.Value)}; ");
            }

            return $"/w {e.User} {builder}";
        }
    }
}
