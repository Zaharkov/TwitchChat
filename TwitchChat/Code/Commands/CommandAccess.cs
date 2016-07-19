using System;
using System.Collections.Generic;
using System.Linq;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class CommandAccess
    {
        private static readonly Dictionary<Command, UserType> Accesses = new Dictionary<Command, UserType>
        {
            { Command.Global, UserType.Default },
            { Command.Mmr, UserType.Default },
            { Command.MmrUpdate, UserType.Broadcaster },
            { Command.Song, UserType.Default },
            { Command.Music, UserType.Default },
            { Command.MyTime, UserType.Default },
            { Command.Help, UserType.Default },
            { Command.Шейкер, UserType.Default },
            { Command.AddSteam, UserType.Broadcaster },
            { Command.RemoveSteam, UserType.Broadcaster }
        };

        private static readonly Dictionary<Command, DelayType> CommandDelayType = new Dictionary<Command, DelayType>
        {
            { Command.Help, DelayType.User},
            { Command.AddSteam, DelayType.User},
            { Command.RemoveSteam, DelayType.User},
            { Command.MyTime, DelayType.Hybrid},
            { Command.Шейкер, DelayType.Hybrid}
        };

        public static Dictionary<List<Command>, UserType> GetGroupedAccess()
        {
            var groupedAccess = new Dictionary<List<Command>, UserType>();
            var copy = Accesses.ToDictionary(k => k.Key, v => v.Value);

            foreach (var access in copy)
            {
                if (access.Key == Command.Global)
                    continue;

                var added = false;
                foreach (var grouped in groupedAccess)
                {
                    if (access.Value == grouped.Value)
                    {
                        grouped.Key.Add(access.Key);
                        added = true;
                    }
                }

                if (!added)
                    groupedAccess.Add(new List<Command> { access.Key }, access.Value);
            }

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                if (command == Command.Global)
                    continue;

                var inList = false;
                List<Command> emptyUserList = null;
                foreach (var grouped in groupedAccess)
                {
                    if (grouped.Key.Any(t => t == command))
                        inList = true;

                    if (grouped.Value == UserType.Default)
                        emptyUserList = grouped.Key;
                }

                if (!inList)
                {
                    if (emptyUserList != null)
                        emptyUserList.Add(command);
                    else
                    {
                        groupedAccess.Add(new List<Command> { command }, UserType.Default);
                    }
                }
            }

            return groupedAccess;
        }

        public static bool IsHaveAccess(MessageEventArgs e, Command command)
        {
            if (Accesses.ContainsKey(command))
                return Accesses[command] == UserType.Default || (Accesses[command] & e.UserType) != 0;

            return true;
        }

        public static DelayType GetCommandDelayType(Command command)
        {
            if (CommandDelayType.ContainsKey(command))
                return CommandDelayType[command];

            return DelayType.Global;
        }
    }
}
