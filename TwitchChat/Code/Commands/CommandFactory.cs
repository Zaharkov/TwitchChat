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
                    return Music.GetMusic(e);
                default:
                    return null;
            }
        }
    }
}
