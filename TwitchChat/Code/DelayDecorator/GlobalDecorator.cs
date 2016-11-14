using System.Collections.Generic;

namespace TwitchChat.Code.DelayDecorator
{
    public class GlobalDecorator : BaseDecorator
    {
        private static readonly Dictionary<string, IDelayDecorator> Instances = new Dictionary<string, IDelayDecorator>();

        private GlobalDecorator(string command) : base(command)
        {
        }

        public static IDelayDecorator Get(string command)
        {
            if (Instances.ContainsKey(command))
                return Instances[command];

            var decorator = new GlobalDecorator(command);

            Instances.Add(command, decorator);

            return decorator;
        }
    }
}
