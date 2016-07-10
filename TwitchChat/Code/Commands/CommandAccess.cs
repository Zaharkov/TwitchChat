using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchChat.Code.Commands
{
    public static class CommandAccess
    {
        private static readonly Dictionary<Command, List<UserType>> Accesses = new Dictionary<Command, List<UserType>>
        {
            { Command.Global, new List<UserType>() },
            { Command.Mmr, new List<UserType>() },
            { Command.MmrUpdate, new List<UserType> { UserType.Broadcaster } },
            { Command.Song, new List<UserType>() },
            { Command.Music, new List<UserType>() },
            { Command.MyTime, new List<UserType>() },
            { Command.Help, new List<UserType>() },
            { Command.Шейкер, new List<UserType>() }
        };

        private static readonly List<Command> UserAttachedCommands = new List<Command>
        {
            Command.Help,
            Command.MyTime,
            Command.Шейкер
        };

        public static Dictionary<List<Command>, List<UserType>> GetGroupedAccess()
        {
            var groupedAccess = new Dictionary<List<Command>, List<UserType>>();
            var copy = Accesses.ToDictionary(k => k.Key, v => v.Value.ToList());

            foreach (var access in copy)
            {
                if (access.Key == Command.Global)
                    continue;

                var added = false;
                foreach (var grouped in groupedAccess)
                {
                    if (ScrambledEquals(access.Value, grouped.Value))
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

                    if (!grouped.Value.Any())
                        emptyUserList = grouped.Key;
                }

                if (!inList)
                {
                    if (emptyUserList != null)
                        emptyUserList.Add(command);
                    else
                    {
                        groupedAccess.Add(new List<Command> { command }, new List<UserType>());
                    }
                }
            }

            return groupedAccess;
        }

        public static bool IsHaveAccess(MessageEventArgs e, Command command)
        {
            if (e.User == "artemzaharkov")
                return true;

            if (Accesses.ContainsKey(command))
                return !Accesses[command].Any() || Accesses[command].Contains(e.UserType);

            return true;
        }

        public static bool IsUserAttachedCommand(Command command)
        {
            return UserAttachedCommands.Contains(command);
        }

        private static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
