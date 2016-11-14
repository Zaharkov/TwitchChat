using System.Collections.Generic;
namespace TwitchChat.Code.DelayDecorator
{
    public class UserDecorator : BaseDecorator
    {
        private static readonly Dictionary<string, Dictionary<string, IDelayDecorator>> UserInstances = new Dictionary<string, Dictionary<string, IDelayDecorator>>();

        private UserDecorator(string command, bool useMultiplier) : base(command, useMultiplier)
        {
        }

        public static IDelayDecorator Get(string username, string command, bool useMultiplier = false)
        {
            if (UserInstances.ContainsKey(command))
            {
                if (UserInstances[command].ContainsKey(username))
                    return UserInstances[command][username];

                var userDecorator = new UserDecorator(command, useMultiplier);

                UserInstances[command].Add(username, userDecorator);

                return userDecorator;
            }

            var decorator = new UserDecorator(command, useMultiplier);
            var user = new Dictionary<string, IDelayDecorator> { { username, decorator } };

            UserInstances.Add(command, user);

            return decorator;
        }
    }
}
