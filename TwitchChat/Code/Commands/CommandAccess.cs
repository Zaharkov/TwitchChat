﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommonHelper;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class CommandAccess
    {
        private static readonly List<Command> DisabledCommands = Configuration.GetSetting("DisabledCommands").Split(',').Select(t => (Command)Enum.Parse(typeof(Command), t, true)).ToList();

        private static readonly Dictionary<Command, UserType> Accesses = new Dictionary<Command, UserType>
        {
            { Command.Global, UserType.Default },
            { Command.Ммр, UserType.Default },
            { Command.МмрОбновить, UserType.Broadcaster },
            { Command.Песня, UserType.Default },
            { Command.Музыка, UserType.Default },
            { Command.МоеВремя, UserType.Default },
            { Command.Помощь, UserType.Default },
            { Command.Шейкер, UserType.Default },
            { Command.ДобавитьСтим, UserType.Broadcaster },
            { Command.УбратьСтим, UserType.Broadcaster },
            { Command.Мойписюн, UserType.Default },
            { Command.О, UserType.Default },
            { Command.ВикторинаВопрос, UserType.Default },
            { Command.МояВикторина, UserType.Default },
            { Command.ВикторинаСтарт, UserType.Broadcaster | UserType.Moderator },
            { Command.ВикторинаСтоп, UserType.Broadcaster | UserType.Moderator },
            { Command.Эба, UserType.Default },
            { Command.Аптайм, UserType.Default },
            { Command.Задержка, UserType.Default },
            { Command.Рулетка, UserType.Default },
            { Command.МояРулетка, UserType.Default },
            { Command.ТопРулетки, UserType.Default },
            { Command.Дуэль, UserType.Default },
            { Command.Принять, UserType.Default }
        };

        private static readonly Dictionary<Command, DelayType> CommandDelayType = new Dictionary<Command, DelayType>
        {
            { Command.Помощь, DelayType.User},
            { Command.ДобавитьСтим, DelayType.User},
            { Command.УбратьСтим, DelayType.User},
            { Command.МоеВремя, DelayType.Hybrid},
            { Command.Шейкер, DelayType.Hybrid},
            { Command.Эба, DelayType.Hybrid},
            { Command.О, DelayType.Hybrid},
            { Command.МояВикторина, DelayType.User},
            { Command.Мойписюн, DelayType.Hybrid},
            { Command.Рулетка, DelayType.Hybrid},
            { Command.МояРулетка, DelayType.Hybrid},
            { Command.ТопРулетки, DelayType.Global },
            { Command.Дуэль, DelayType.Hybrid },
            { Command.Принять, DelayType.Hybrid }
        };

        public static Dictionary<List<Command>, UserType> GetGroupedAccess()
        {
            var groupedAccess = new Dictionary<List<Command>, UserType>();
            var copy = Accesses
                .Where(t => !DisabledCommands.Contains(t.Key))
                .ToDictionary(k => k.Key, v => v.Value);

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

                if (!inList && !DisabledCommands.Contains(command))
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
            if (DisabledCommands.Contains(command))
                return false;

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
