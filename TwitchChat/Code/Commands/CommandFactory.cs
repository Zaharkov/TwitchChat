using System;

namespace TwitchChat.Code.Commands
{
    public class CommandFactory
    {
        public static string ExecuteCommand(MessageEventArgs e)
        {
            Command command;
            var parse = Enum.TryParse(e.Message.TrimStart('!'), true, out command);

            if (!parse)
                return null;

            if (!CommandAccess.IsHaveAccess(e, command))
                return null;

            Func<MessageEventArgs, string> commandFunc;
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
                case Command.Global:
                    return null;
                default:
                    return null;
            }

            var result = CommandAccess.IsUserAttachedCommand(command)
                ? DelayDecorator.Get(e.User, command).Execute(commandFunc, e)
                : DelayDecorator.Get(command).Execute(commandFunc, e);

            if (!string.IsNullOrEmpty(result) && command != Command.Help)
                result = $"БОТ: @{e.User} {result}";

            return result;
        }
    }
}
