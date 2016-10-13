using System;
using System.Collections.Generic;
using TwitchChat.Code.Commands;

namespace TwitchChat.Code.DelayDecorator
{
    public class HybridDecorator : IDelayDecorator
    {
        private IDelayDecorator User { get; }
        private IDelayDecorator Global { get; }

        private static readonly Dictionary<Command, Dictionary<string, HybridDecorator>> HybridInstances = new Dictionary<Command, Dictionary<string, HybridDecorator>>();

        private HybridDecorator(IDelayDecorator user, IDelayDecorator global)
        {
            Global = global;
            User = user;
        }

        public static IDelayDecorator Get(string username, Command command)
        {
            var userDecorator = UserDecorator.Get(username, command, true);
            var globalDecorator = GlobalDecorator.Get(command);

            if (HybridInstances.ContainsKey(command))
            {
                if (HybridInstances[command].ContainsKey(username))
                    return HybridInstances[command][username];

                var hybridDecorator = new HybridDecorator(userDecorator, globalDecorator);

                HybridInstances[command].Add(username, hybridDecorator);

                return hybridDecorator;
            }

            var decorator = new HybridDecorator(userDecorator, globalDecorator);
            var hybrid = new Dictionary<string, HybridDecorator> { { username, decorator } };

            HybridInstances.Add(command, hybrid);

            return decorator;
        }

        public bool CanExecute(out int needWait)
        {
            return User.CanExecute(out needWait) && Global.CanExecute(out needWait);
        }

        public SendMessage Execute(Func<SendMessage> func, bool needExec = true)
        {
            var user = User.Execute(func);
            Global.Execute(func, false);

            return user;
        }
    }
}
