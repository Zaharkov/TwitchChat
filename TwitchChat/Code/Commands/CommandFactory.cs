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

            switch (command)
            {
                case Command.Music:
                    return MusicCommand.GetMusic(e);
                case Command.Mmr:
                    return MmrCommand.GetMmr(e);
                case Command.MmrUpdate:
                    return MmrCommand.MmrUpdate(e);
                default:
                    return null;
            }
        }
    }
}
