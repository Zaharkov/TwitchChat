using System;
using System.Linq;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public class CommandFactory
    {
        public static string ExecuteCommand(MessageEventArgs e, ChatMemberViewModel userModel, out SendType sendType)
        {
            var forParse = e.Message.TrimStart('!').Split(' ').First();
            Command command;
            var parse = Enum.TryParse(forParse, true, out command);

            if (!parse)
            {
                sendType = SendType.Whisper;
                return $"Команда !{forParse} не поддерживается. Список команд можно посмотреть через команду !{Command.Help}";
            }

            if (!CommandAccess.IsHaveAccess(e, command))
            {
                sendType = SendType.Whisper;
                return $"У Вас нет доступа к команде !{command}. Список доступных команд можно посмотреть через команду !{Command.Help}";
            }

            sendType = SendType.Message;

            Func<MessageEventArgs, ChatMemberViewModel, string> commandFunc;
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
                    sendType = SendType.Whisper;
                    break;
                case Command.MyTime:
                    commandFunc = MyTimeCommand.GetMyTime;
                    break;
                case Command.Шейкер:
                    commandFunc = SheikerCommand.GetSheiker;
                    break;
                case Command.AddSteam:
                    commandFunc = SteamCommand.AddSteam;
                    sendType = SendType.Whisper;
                    break;
                case Command.RemoveSteam:
                    commandFunc = SteamCommand.RemoveSteam;
                    sendType = SendType.Whisper;
                    break;
                case Command.Global:
                    return null;
                default:
                    return null;
            }

            var userAttached = CommandAccess.IsUserAttachedCommand(command);

            var delayDecorator = userAttached
                ? DelayDecorator.Get(e.Username, command)
                : DelayDecorator.Get(command);

            int needWait;
            if (!delayDecorator.CanExecute(out needWait))
            {
                sendType = SendType.Whisper;
                return $"Команда !{command} на {(userAttached ? "пользовательском" : "глобальном")} кулдауне. Вы сможете её повторить через {needWait} {MyTimeCommand.GetSecondsName(needWait)}";
            }

            var result = delayDecorator.Execute(commandFunc, e, userModel);

            if (!string.IsNullOrEmpty(result) && sendType != SendType.Whisper)
                result = $"БОТ: @{e.Username} {result}";

            return result;
        }
    }
}
