using System.Collections.Generic;
using TwitchChat.Code.Commands;

namespace TwitchChat.Code.DelayDecorator
{
    public class GlobalDecorator : BaseDecorator
    {
        private static readonly Dictionary<CommandHandler, IDelayDecorator> Instances = new Dictionary<CommandHandler, IDelayDecorator>();

        private GlobalDecorator(CommandHandler command) : base(command)
        {
        }

        public static IDelayDecorator Get(CommandHandler command)
        {
            if (Instances.ContainsKey(command))
                return Instances[command];

            var decorator = new GlobalDecorator(command);

            Instances.Add(command, decorator);

            return decorator;
        }
    }
}
