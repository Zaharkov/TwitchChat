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
    public class CommandHandler
    {
        private static readonly List<CustomCommand<CommandType, UserType, DelayType>> CustomCommands = CustomCommands<CommandType, UserType, DelayType>.Commands;
        private static readonly List<string> DisabledCommands = ConfigHolder.Configs.Global.DisabledCommands;
        protected static readonly Dictionary<string, int> DelayConfigs = DelayConfig.GetDelayConfig(ConfigHolder.Configs.Global.Cooldowns.Commands);
        private static readonly List<CommandHandler> CommandList;

        static CommandHandler()
        {
            CommandList = new List<CommandHandler>
            {
                new CommandHandler(Command.Global.ToString(), UserType.Default, DelayType.Global, (e, m) => SendMessage.None),
                new CommandHandler(Command.Ммр.ToString(), UserType.Default, DelayType.Global, (e, m) => MmrCommand.GetMmr()),
                new CommandHandler(Command.МмрОбновить.ToString(), UserType.Broadcaster, DelayType.Global, (e, m) => MmrCommand.MmrUpdate()),
                new CommandHandler(Command.Песня.ToString(), UserType.Default, DelayType.Global, (e, m) => MusicCommand.GetMusic()),
                new CommandHandler(Command.Музыка.ToString(), UserType.Default, DelayType.Global, (e, m) => MusicCommand.GetMusic()),
                new CommandHandler(Command.МоеВремя.ToString(), UserType.Default, DelayType.Hybrid, MyTimeCommand.GetMyTime),
                new CommandHandler(Command.Помощь.ToString(), UserType.Default, DelayType.User, (e, m) => HelpCommand.GetHelp()),
                new CommandHandler(Command.ДобавитьСтим.ToString(), UserType.Broadcaster | UserType.Subscriber, DelayType.User, (e, m) => SteamCommand.AddSteam(e)),
                new CommandHandler(Command.УбратьСтим.ToString(), UserType.Broadcaster| UserType.Subscriber, DelayType.User, (e, m) => SteamCommand.RemoveSteam(e)),
                new CommandHandler(Command.О.ToString(), UserType.Default, DelayType.Hybrid, QiuzCommand.Answer),
                new CommandHandler(Command.ВикторинаВопрос.ToString(), UserType.Default, DelayType.Global, (e, m) => QiuzCommand.Question(m)),
                new CommandHandler(Command.МояВикторина.ToString(), UserType.Default, DelayType.User, (e, m) => QiuzCommand.Score(m)),
                new CommandHandler(Command.ВикторинаСтарт.ToString(), UserType.Broadcaster | UserType.Moderator, DelayType.Global, (e, m) => QiuzCommand.Start(m)),
                new CommandHandler(Command.ВикторинаСтоп.ToString(), UserType.Broadcaster | UserType.Moderator, DelayType.Global, (e, m) => QiuzCommand.Stop(m)),
                new CommandHandler(Command.ОпросВопрос.ToString(), UserType.Default, DelayType.Global, (e, m) => VoteCommand.Question(m)),
                new CommandHandler(Command.Голос.ToString(), UserType.Default, DelayType.User, VoteCommand.UserVote),
                new CommandHandler(Command.ОпросСтарт.ToString(), UserType.Broadcaster | UserType.Moderator, DelayType.Global, VoteCommand.Start),
                new CommandHandler(Command.ОпросСтоп.ToString(), UserType.Broadcaster | UserType.Moderator, DelayType.Global, (e, m) => VoteCommand.Stop(m)),
                new CommandHandler(Command.Опрос.ToString(), UserType.Broadcaster | UserType.Moderator, DelayType.Global, (e, m) => VoteCommand.LastResults(m)),
                new CommandHandler(Command.Аптайм.ToString(), UserType.Default, DelayType.Global, (e, m) => StreamCommand.GetUpTime(m)),
                new CommandHandler(Command.Задержка.ToString(), UserType.Default, DelayType.Global, (e, m) => StreamCommand.GetDelay(m)),
                new CommandHandler(Command.Игра.ToString(), UserType.Default, DelayType.Global, (e, m) => StreamCommand.GetGame(m)),
                new CommandHandler(Command.Рулетка.ToString(), UserType.Default, DelayType.Hybrid, (e, m) => RouletteCommand.RouletteTry(e)),
                new CommandHandler(Command.МояРулетка.ToString(), UserType.Default, DelayType.Hybrid, (e, m) => RouletteCommand.RouletteInfo(e)),
                new CommandHandler(Command.ТопРулетки.ToString(), UserType.Default, DelayType.Global, (e, m) => RouletteCommand.RouletteGetTop(e)),
                new CommandHandler(Command.Дуэль.ToString(), UserType.Default, DelayType.Hybrid, DuelCommand.Duel),
                new CommandHandler(Command.Принять.ToString(), UserType.Default, DelayType.Hybrid, DuelCommand.DuelStart),
                new CommandHandler(Command.Осу.ToString(), UserType.Default, DelayType.Global, (e,m) => OsuCommand.GetMusic())
            };

            foreach (var customCommand in CustomCommands.Where(t => t.Type == CommandType.Write))
                CommandList.Add(new CommandHandler(customCommand));

            var names = CommandList.Select(t => t.Name).ToList();

            if(names.Distinct().Count() != names.Count)
                throw new ArgumentException("All commands names must be unique. Check names please!");

            foreach (var command in CommandList)
            {
                if (DisabledCommands.Contains(command.Name, StringComparer.InvariantCultureIgnoreCase))
                    command.IsDisabled = true;

                var lower = DelayConfigs.ToDictionary(k => k.Key.ToLower(), v => v);

                command.Cooldown = command.Cooldown == 0 ? lower.ContainsKey(command.Name) 
                        ? lower[command.Name].Value 
                        : lower[Command.Global.ToString().ToLower()].Value 
                    : command.Cooldown;
            }
        }

        public static bool TryGet(string command, out CommandHandler handler)
        {
            if (IsExistCommand(command))
            {
                handler = CommandList.Single(t => t.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase));
                return true;
            }

            handler = null;
            return false;
        }

        public static CommandHandler Get(Command command)
        {
            var textCommand = command.ToString().ToLower();
            return CommandList.First(t => t.Name.Equals(textCommand, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsExistCommand(string command)
        {
            return CommandList.Any(t => t.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Dictionary<List<string>, UserType> GetGroupedAccess()
        {
            var groupedAccess = new Dictionary<List<string>, UserType>();
            var copy = CommandList.Where(c => !c.IsDisabled)
                .ToDictionary(k => k.Name, v => v.Access);

            foreach (var access in copy)
            {
                if (access.Key.Equals(Command.Global.ToString(), StringComparison.InvariantCultureIgnoreCase))
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

            return groupedAccess;
        }

        public string Name { get; }
        public UserType Access { get; }
        public bool IsDisabled { get; private set; }
        public DelayType DelayType { get; }
        public int Cooldown { get; private set; }
        public Func<MessageEventArgs, ChatMemberViewModel, SendMessage> Handler { get; }

        private CommandHandler(string name, UserType access, DelayType delayType, Func<MessageEventArgs, ChatMemberViewModel, SendMessage> handler)
        {
            Name = name.ToLower();
            Access = access;
            DelayType = delayType;
            Handler = handler;
        }

        private CommandHandler(CustomCommand<CommandType, UserType, DelayType> command)
        {
            Name = command.Name.ToLower();
            Access = command.Access;
            DelayType = command.CooldownType;
            Handler = (e, userModel) => CustomCommandHandler.CreateCommand(command, e, userModel.Channel);
            Cooldown = command.CooldownTime;
        }

        public bool IsHaveAccess(MessageEventArgs e)
        {
            if (IsDisabled)
                return false;

            return Access.HasFlag(UserType.Default) || (Access & e.UserType) != 0;
        }
    }
}
