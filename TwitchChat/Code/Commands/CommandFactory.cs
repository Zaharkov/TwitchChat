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

            Func<MessageEventArgs, string> commandFunc;
            switch (command)
            {
                case Command.Music:
                    commandFunc = MusicCommand.GetMusic;
                    break;
                case Command.Mmr:
                    commandFunc = MmrCommand.GetMmr;
                    break;
                case Command.MmrUpdate:
                    commandFunc = MmrCommand.MmrUpdate;
                    break;
                case Command.Global:
                    return null;
                default:
                    return null;
            }

            return DelayDecorator.Get(command).Execute(commandFunc, e);
        }
    }
}
