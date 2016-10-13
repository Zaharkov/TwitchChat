using System;
using System.Linq;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Controls;
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
                //return $"Команда !{forParse} не поддерживается. Список команд можно посмотреть через команду !{Command.Help}";
            }

            if (!CommandAccess.IsHaveAccess(e, command))
            {
                return SendMessage.None;
                //return $"У Вас нет доступа к команде !{command}. Список доступных команд можно посмотреть через команду !{Command.Help}";
            }

            Func<SendMessage> commandFunc;
            switch (command)
            {
                case Command.Music:
                case Command.Song:
                    commandFunc = MusicCommand.GetMusic;
                    break;
                case Command.Mmr:
                    commandFunc = MmrCommand.GetMmr;
                    break;
                case Command.MmrUpdate:
                    commandFunc = MmrCommand.MmrUpdate;
                    break;
                case Command.Help:
                    commandFunc = HelpCommand.GetHelp;
                    break;
                case Command.MyTime:
                    commandFunc = () => MyTimeCommand.GetMyTime(e, userModel);
                    break;
                case Command.Шейкер:
                    commandFunc = SheikerCommand.GetSheiker;
                    break;
                case Command.AddSteam:
                    commandFunc = () => SteamCommand.AddSteam(e);
                    break;
                case Command.RemoveSteam:
                    commandFunc = () => SteamCommand.RemoveSteam(e);
                    break;
                case Command.Global:
                    return SendMessage.None;
                case Command.Мойписюн:
                    commandFunc = () => MyBolt.Bolt(userModel);
                    break;
                case Command.QuizStart:
                    commandFunc = () => QiuzCommand.Start(userModel);
                    break;
                case Command.QuizStop:
                    commandFunc = () => QiuzCommand.Stop(userModel);
                    break;
                case Command.QuizScore:
                    commandFunc = () => QiuzCommand.Score(userModel);
                    break;
                case Command.О:
                    commandFunc = () => QiuzCommand.Answer(e, userModel);
                    break;
                case Command.QuizQuestion:
                    commandFunc = QiuzCommand.Question;
                    break;
                case Command.Эба:
                    commandFunc = () => Eba.EbaComeOn(userModel);
                    break;
                case Command.UpTime:
                    commandFunc = () => StreamCommand.GetUpTime(userModel);
                    break;
                case Command.Delay:
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
