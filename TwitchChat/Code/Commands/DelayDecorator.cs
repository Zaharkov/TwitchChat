using System;
using System.Collections.Generic;
using System.Diagnostics;
using RestClientHelper;

namespace TwitchChat.Code.Commands
{
    public class DelayDecorator
    {
        private bool _firstTime = true;
        private Command _command;
        private readonly Stopwatch _timer = new Stopwatch();
        private static readonly string DelaysConfig = Configuration.GetSetting("DelaysConfig");
        private static readonly Dictionary<Command, int> Configs = new Dictionary<Command, int>();
        private static readonly Dictionary<Command, DelayDecorator> Instances = new Dictionary<Command, DelayDecorator>();

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

        static DelayDecorator()
        {
            BuildDelays();
        }

        public string Execute(Func<MessageEventArgs, string> func, MessageEventArgs e)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _timer.Start();
                return func(e);
            }

            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];

            if (_timer.Elapsed.TotalSeconds > delay)
            {
                _timer.Restart();
                return func(e);
            }

            return null;
        }

        private static void BuildDelays()
        {
            var delays = DelaysConfig.Split(';');

            foreach (var delay in delays)
            {
                var pair = delay.Split('=');

                if (pair.Length < 2)
                    throw new ArgumentException("DelaysConfig have invalid structure. Must be 'command1=delay1;command2=delay2...'");

                Command command;
                if (Enum.TryParse(pair[0], out command))
                {
                    int commandDelay;
                    if (int.TryParse(pair[1], out commandDelay))
                    {
                        if(commandDelay < 0)
                            throw new ArgumentException("Delay must cannot be less then zero");

                        Configs.Add(command, commandDelay);
                    }
                    else
                        throw new ArgumentException("Delay must be a integer");
                }
                else
                    throw new ArgumentException($"There is no such command: {pair[0]}");
            }

            if (!Configs.ContainsKey(Command.Global))
                throw new ArgumentException("'Global' delay must be set");
        }
    }
}
