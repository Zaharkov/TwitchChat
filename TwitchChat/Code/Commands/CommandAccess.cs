using System.Collections.Generic;
using System.Linq;

namespace TwitchChat.Code.Commands
{
    public static class CommandAccess
    {
        public static readonly Dictionary<Command, List<UserType>> Accesses = new Dictionary<Command, List<UserType>>
        {
            { Command.Global, new List<UserType>() },
            { Command.Mmr, new List<UserType>() },
            { Command.MmrUpdate, new List<UserType> { UserType.Broadcaster } },
            { Command.Song, new List<UserType>() },
            { Command.Music, new List<UserType>() },
            { Command.MyTime, new List<UserType> { UserType.Broadcaster} },
            { Command.Help, new List<UserType>() }
        };

        private static readonly List<Command> UserAttachedCommands = new List<Command>
        {
            Command.Help,
            Command.MyTime
        };

        public static bool IsHaveAccess(MessageEventArgs e, Command command)
        {
            if (Accesses.ContainsKey(command))
                return !Accesses[command].Any() || Accesses[command].Contains(e.UserType);

            return true;
        }

        public static bool IsUserAttachedCommand(Command command)
        {
            return UserAttachedCommands.Contains(command);
        }
    }
}
