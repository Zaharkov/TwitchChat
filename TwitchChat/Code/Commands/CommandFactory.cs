using System;
using System.Linq;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Controls;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public class CommandFactory
    {
        public static SendMessage ExecuteCommand(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var forParse = e.Message.TrimStart('!').Split(' ').First();
            Command command;
            var parse = Enum.TryParse(forParse, true, out command);

            if (!parse)
            {
                return SendMessage.None;
                //return $"Команда !{forParse} не поддерживается. Список команд можно посмотреть через команду !{Command.Помощь}";
            }

            if (!CommandAccess.IsHaveAccess(e, command))
            {
                return SendMessage.None;
                //return $"У Вас нет доступа к команде !{command}. Список доступных команд можно посмотреть через команду !{Command.Помощь}";
            }

            Func<SendMessage> commandFunc;
            switch (command)
            {
                case Command.Музыка:
                case Command.Песня:
                    commandFunc = MusicCommand.GetMusic;
                    break;
                case Command.Ммр:
                    commandFunc = MmrCommand.GetMmr;
                    break;
                case Command.МмрОбновить:
                    commandFunc = MmrCommand.MmrUpdate;
                    break;
                case Command.Помощь:
                    commandFunc = HelpCommand.GetHelp;
                    break;
                case Command.МоеВремя:
                    commandFunc = () => MyTimeCommand.GetMyTime(e, userModel);
                    break;
                case Command.Шейкер:
                    commandFunc = SheikerCommand.GetSheiker;
                    break;
                case Command.ДобавитьСтим:
                    commandFunc = () => SteamCommand.AddSteam(e);
                    break;
                case Command.УбратьСтим:
                    commandFunc = () => SteamCommand.RemoveSteam(e);
                    break;
                case Command.Global:
                    return SendMessage.None;
                case Command.Мойписюн:
                    commandFunc = () => MyBolt.Bolt(e);
                    break;
                case Command.ВикторинаСтарт:
                    commandFunc = () => QiuzCommand.Start(userModel);
                    break;
                case Command.ВикторинаСтоп:
                    commandFunc = () => QiuzCommand.Stop(userModel);
                    break;
                case Command.МояВикторина:
                    commandFunc = () => QiuzCommand.Score(userModel);
                    break;
                case Command.О:
                    commandFunc = () => QiuzCommand.Answer(e, userModel);
                    break;
                case Command.ВикторинаВопрос:
                    commandFunc = QiuzCommand.Question;
                    break;
                case Command.Эба:
                    commandFunc = () => Eba.EbaComeOn(userModel);
                    break;
                case Command.Аптайм:
                    commandFunc = () => StreamCommand.GetUpTime(userModel);
                    break;
                case Command.Задержка:
                    commandFunc = () => StreamCommand.GetDelay(userModel);
                    break;
                case Command.Рулетка:
                    commandFunc = () => RouletteCommand.RouletteTry(e);
                    break;
                case Command.МояРулетка:
                    commandFunc = () => RouletteCommand.RouletteInfo(e);
                    break;
                case Command.ТопРулетки:
                    commandFunc = RouletteCommand.RouletteGetTop;
                    break;
                case Command.Дуэль:
                    commandFunc = () => DuelCommand.Duel(e, userModel);
                    break;
                case Command.Принять:
                    commandFunc = () => DuelCommand.DuelStart(e, userModel);
                    break;
                default:
                    return SendMessage.None;
            }

            var delayType = CommandAccess.GetCommandDelayType(command);

            IDelayDecorator delayDecorator;
            switch (delayType)
            {
                case DelayType.User:
                    delayDecorator = UserDecorator.Get(e.Username, command);
                    break;
                case DelayType.Global:
                    delayDecorator = GlobalDecorator.Get(command);
                    break;
                case DelayType.Hybrid:
                    delayDecorator = HybridDecorator.Get(e.Username, command);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(delayType), delayType, null);
            }

            if (e.UserType.HasFlag(UserType.Broadcaster))
                return commandFunc();

            int needWait;
            if (!delayDecorator.CanExecute(out needWait))
            {
                var message = $"Команда !{command} на {(delayType != DelayType.Global ? "пользовательском" : "глобальном")} кулдауне. Вы сможете её повторить через {needWait} {MyTimeCommand.GetSecondsName(needWait)}";
                return SendMessage.GetWhisper(message);
            }

            return delayDecorator.Execute(() => commandFunc());
        }
    }
}
