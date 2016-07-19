using System;
using System.Collections.Generic;
using System.Diagnostics;
using CommonHelper;
using TwitchChat.Code.Helpers;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public class DelayDecorator
    {
        private bool _firstTime = true;
        private Command _command;
        private readonly Stopwatch _timerUser = new Stopwatch();
        private readonly Stopwatch _timerGlobal;
        private readonly DelayType _type;
        private static readonly Dictionary<Command, int> Configs = DelayConfig<Command>.GetDelayConfig("CommandsConfig");
        private static readonly Dictionary<Command, DelayDecorator> Instances = new Dictionary<Command, DelayDecorator>();
        private static readonly Dictionary<Command, Dictionary<string, DelayDecorator>> UserInstances = new Dictionary<Command, Dictionary<string, DelayDecorator>>();

        private DelayDecorator(DelayType type, Stopwatch timerGlobal)
        {
            _timerGlobal = timerGlobal;
            _type = type;
        }

        public static DelayDecorator Get(string username, Command command, DelayType type)
        {
            switch (type)
            {
                case DelayType.User:
                case DelayType.Hybrid:
                {
                    var global = Get(username, command, DelayType.Global);
                    if (UserInstances.ContainsKey(command))
                    {
                        if (UserInstances[command].ContainsKey(username))
                            return UserInstances[command][username];

                        var userDecorator = new DelayDecorator(type, global._timerGlobal) { _command = command };

                        UserInstances[command].Add(username, userDecorator);

                        return userDecorator;
                    }

                    var decorator = new DelayDecorator(type, global._timerGlobal) { _command = command };
                    var user = new Dictionary<string, DelayDecorator> { { username, decorator } };

                    UserInstances.Add(command, user);

                    return decorator;
                }
                case DelayType.Global:
                {
                    if (Instances.ContainsKey(command))
                        return Instances[command];

                    var global = new Stopwatch();
                    var decorator = new DelayDecorator(type, global) { _command = command };

                    Instances.Add(command, decorator);

                    return decorator;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public bool CanExecute(out int needWait)
        {
            var seconds = GetSeconds();

            if (_firstTime)
            {
                needWait = seconds;
                return true;
            }

            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];
            var hybridDelayMulti = int.Parse(Configuration.GetSetting("HybridDelayMulti"));

            if (_type == DelayType.Hybrid)
                delay = delay * hybridDelayMulti;

            if (seconds < delay)
            {
                needWait = delay - seconds;
                return false;
            }

            needWait = seconds;
            return true;
        }

        public string Execute(Func<MessageEventArgs, ChatMemberViewModel, string> func, MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _timerUser.Start();
                _timerGlobal.Start();
                return func(e, userModel);
            }

            var seconds = GetSeconds();
            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];
            var hybridDelayMulti = int.Parse(Configuration.GetSetting("HybridDelayMulti"));

            if (_type == DelayType.Hybrid)
                delay = delay * hybridDelayMulti;

            if (seconds > delay)
            {
                if(_type == DelayType.Hybrid && seconds > _timerGlobal.Elapsed.TotalSeconds)
                    _timerGlobal.Restart();

                _timerUser.Restart();
                return func(e, userModel);
            }

            return null;
        }

        private int GetSeconds()
        {
            switch (_type)
            {
                case DelayType.User:
                    return (int)_timerUser.Elapsed.TotalSeconds;
                case DelayType.Global:
                    return (int)_timerGlobal.Elapsed.TotalSeconds;
                case DelayType.Hybrid:
                    return (int)Math.Min(_timerUser.Elapsed.TotalSeconds, _timerGlobal.Elapsed.TotalSeconds);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
