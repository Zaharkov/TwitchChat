using System.Collections.Generic;
using TwitchChat.Code.Commands;

namespace TwitchChat.Code.DelayDecorator
{
    public class GlobalDecorator : BaseDecorator
    {
        private static readonly Dictionary<Command, IDelayDecorator> Instances = new Dictionary<Command, IDelayDecorator>();

        private GlobalDecorator(Command command) : base(command)
        {
        }

        public static IDelayDecorator Get(Command command)
        {
            if (Instances.ContainsKey(command))
                return Instances[command];

            var decorator = new GlobalDecorator(command);

            Instances.Add(command, decorator);

            return decorator;
        }
    }
}
