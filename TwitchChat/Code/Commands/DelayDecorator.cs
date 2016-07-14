using System;
using System.Collections.Generic;
using System.Diagnostics;
using TwitchChat.Code.Helpers;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public class DelayDecorator
    {
        private bool _firstTime = true;
        private Command _command;
        private readonly Stopwatch _timer = new Stopwatch();
        private static readonly Dictionary<Command, int> Configs = DelayConfig<Command>.GetDelayConfig("CommandsConfig");
        private static readonly Dictionary<Command, DelayDecorator> Instances = new Dictionary<Command, DelayDecorator>();
        private static readonly Dictionary<Command, Dictionary<string, DelayDecorator>> UserInstances = new Dictionary<Command, Dictionary<string, DelayDecorator>>();

        private DelayDecorator()
        {
        }

        public static DelayDecorator Get(Command command)
        {
            if (Instances.ContainsKey(command))
                return Instances[command];

            var decorator = new DelayDecorator {_command = command};

            Instances.Add(command, decorator);

            return decorator;
        }

        public static DelayDecorator Get(string username, Command command)
        {
            if (UserInstances.ContainsKey(command))
            {
                if (UserInstances[command].ContainsKey(username))
                    return UserInstances[command][username];

                var userDecorator = new DelayDecorator { _command = command };

                UserInstances[command].Add(username, userDecorator);

                return userDecorator;
            }

            var decorator = new DelayDecorator { _command = command };
            var user = new Dictionary<string, DelayDecorator> {{username, decorator}};

            UserInstances.Add(command, user);

            return decorator;
        }

        public string Execute(Func<MessageEventArgs, ChatMemberViewModel, string> func, MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _timer.Start();
                return func(e, userModel);
            }

            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];

            if (_timer.Elapsed.TotalSeconds > delay)
            {
                _timer.Restart();
                return func(e, userModel);
            }

            return null;
        }
    }
}
