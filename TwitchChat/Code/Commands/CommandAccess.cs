using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Configuration.Entities;
using TwitchChat.Controls;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class CommandAccess
    {
        private static readonly List<CustomCommand<CommandType, UserType, DelayType>> CustomCommands = CustomCommands<CommandType, UserType, DelayType>.Commands; 
        private static readonly List<string> DisabledCommands = ConfigHolder.Configs.Global.DisabledCommands;

        private static readonly Dictionary<string, UserType> Accesses = new Dictionary<string, UserType>
        {
            { Command.Global.ToString(), UserType.Default },
            { Command.Ммр.ToString(), UserType.Default },
            { Command.МмрОбновить.ToString(), UserType.Broadcaster },
            { Command.Песня.ToString(), UserType.Default },
            { Command.Музыка.ToString(), UserType.Default },
            { Command.МоеВремя.ToString(), UserType.Default },
            { Command.Помощь.ToString(), UserType.Default },
            { Command.Шейкер.ToString(), UserType.Default },
            { Command.ДобавитьСтим.ToString(), UserType.Broadcaster },
            { Command.УбратьСтим.ToString(), UserType.Broadcaster },
            { Command.Мойписюн.ToString(), UserType.Default },
            { Command.О.ToString(), UserType.Default },
            { Command.ВикторинаВопрос.ToString(), UserType.Default },
            { Command.МояВикторина.ToString(), UserType.Default },
            { Command.ВикторинаСтарт.ToString(), UserType.Broadcaster | UserType.Moderator },
            { Command.ВикторинаСтоп.ToString(), UserType.Broadcaster | UserType.Moderator },
            { Command.Эба.ToString(), UserType.Default },
            { Command.Аптайм.ToString(), UserType.Default },
            { Command.Задержка.ToString(), UserType.Default },
            { Command.Игра.ToString(), UserType.Default },
            { Command.Рулетка.ToString(), UserType.Default },
            { Command.МояРулетка.ToString(), UserType.Default },
            { Command.ТопРулетки.ToString(), UserType.Default },
            { Command.Дуэль.ToString(), UserType.Default },
            { Command.Принять.ToString(), UserType.Default }
        };

        private static readonly Dictionary<string, DelayType> CommandDelayType = new Dictionary<string, DelayType>
        {
            { Command.Помощь.ToString(), DelayType.User},
            { Command.ДобавитьСтим.ToString(), DelayType.User},
            { Command.УбратьСтим.ToString(), DelayType.User},
            { Command.МоеВремя.ToString(), DelayType.Hybrid},
            { Command.Шейкер.ToString(), DelayType.Hybrid},
            { Command.Эба.ToString(), DelayType.Hybrid},
            { Command.О.ToString(), DelayType.Hybrid},
            { Command.МояВикторина.ToString(), DelayType.User},
            { Command.Мойписюн.ToString(), DelayType.Hybrid},
            { Command.Рулетка.ToString(), DelayType.Hybrid},
            { Command.МояРулетка.ToString(), DelayType.Hybrid},
            { Command.ТопРулетки.ToString(), DelayType.Global },
            { Command.Дуэль.ToString(), DelayType.Hybrid },
            { Command.Принять.ToString(), DelayType.Hybrid }
        };

        static CommandAccess()
        {
            foreach (var customCommand in CustomCommands)
            {
                Accesses.Add(customCommand.Name, customCommand.Access);
                CommandDelayType.Add(customCommand.Name, customCommand.CooldownType);
            }
        }

        public static Func<SendMessage> GetHandler(string command, MessageEventArgs e, ChatMemberViewModel userModel)
        {
            Command parseCommand;
            if (!Enum.TryParse(command, true, out parseCommand))
            {
                var customCommand = CustomCommands.FirstOrDefault(t => t.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase));

                if (customCommand == null)
                    return () => SendMessage.None;

                var message = CustomCommandHandler.CreateCommand(customCommand, e, userModel.Channel);
                return () => message;
            }

            switch (parseCommand)
            {
                case Command.Музыка:
                case Command.Песня:
                    return MusicCommand.GetMusic;
                case Command.Ммр:
                    return MmrCommand.GetMmr;
                case Command.МмрОбновить:
                    return MmrCommand.MmrUpdate;
                case Command.Помощь:
                    return HelpCommand.GetHelp;
                case Command.МоеВремя:
                    return () => MyTimeCommand.GetMyTime(e, userModel);
                case Command.Шейкер:
                    return SheikerCommand.GetSheiker;
                case Command.ДобавитьСтим:
                    return () => SteamCommand.AddSteam(e);
                case Command.УбратьСтим:
                    return () => SteamCommand.RemoveSteam(e);
                case Command.Global:
                    return () => SendMessage.None;
                case Command.Мойписюн:
                    return () => MyBolt.Bolt(e);
                case Command.ВикторинаСтарт:
                    return () => QiuzCommand.Start(userModel);
                case Command.ВикторинаСтоп:
                    return () => QiuzCommand.Stop(userModel);
                case Command.МояВикторина:
                    return () => QiuzCommand.Score(userModel);
                case Command.О:
                    return () => QiuzCommand.Answer(e, userModel);
                case Command.ВикторинаВопрос:
                    return QiuzCommand.Question;
                case Command.Эба:
                    return () => Eba.EbaComeOn(userModel);
                case Command.Аптайм:
                    return () => StreamCommand.GetUpTime(userModel);
                case Command.Задержка:
                    return () => StreamCommand.GetDelay(userModel);
                case Command.Игра:
                    return () => StreamCommand.GetGame(userModel);
                case Command.Рулетка:
                    return () => RouletteCommand.RouletteTry(e);
                case Command.МояРулетка:
                    return () => RouletteCommand.RouletteInfo(e);
                case Command.ТопРулетки:
                    return RouletteCommand.RouletteGetTop;
                case Command.Дуэль:
                    return () => DuelCommand.Duel(e, userModel);
                case Command.Принять:
                    return () => DuelCommand.DuelStart(e, userModel);
                default:
                    return () => SendMessage.None;
            }
        }

        public static Dictionary<List<string>, UserType> GetGroupedAccess()
        {
            var groupedAccess = new Dictionary<List<string>, UserType>();
            var copy = Accesses
                .Where(t => !DisabledCommands.Contains(t.Key))
                .ToDictionary(k => k.Key, v => v.Value);

            foreach (var access in copy)
            {
                if (access.Key == Command.Global.ToString())
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
                    groupedAccess.Add(new List<string> { access.Key }, access.Value);
            }

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                if (command == Command.Global)
                    continue;

                var inList = false;
                List<string> emptyUserList = null;
                foreach (var grouped in groupedAccess)
                {
                    if (grouped.Key.Any(t => t == command.ToString()))
                        inList = true;

                    if (grouped.Value == UserType.Default)
                        emptyUserList = grouped.Key;
                }

                if (!inList && !DisabledCommands.Contains(command.ToString()))
                {
                    if (emptyUserList != null)
                        emptyUserList.Add(command.ToString());
                    else
                    {
                        groupedAccess.Add(new List<string> { command.ToString() }, UserType.Default);
                    }
                }
            }

            return groupedAccess;
        }

        public static bool IsExistCommand(string command)
        {
            Command parseCommand;
            if (Enum.TryParse(command, true, out parseCommand))
                return true;

            if (CustomCommands.Any(t => t.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }

        public static bool IsHaveAccess(MessageEventArgs e, string command)
        {
            if (DisabledCommands.Contains(command))
                return false;

            if (Accesses.ContainsKey(command))
                return Accesses[command] == UserType.Default || (Accesses[command] & e.UserType) != 0;

            return true;
        }

        public static DelayType GetCommandDelayType(string command)
        {
            if (CommandDelayType.ContainsKey(command))
                return CommandDelayType[command];

            return DelayType.Global;
        }
    }
}
