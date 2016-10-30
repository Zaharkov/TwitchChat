using System;
using System.Collections.Generic;
using System.Diagnostics;
using Configuration;
using TwitchChat.Code.Commands;

namespace TwitchChat.Code.DelayDecorator
{
    public class BaseDecorator : IDelayDecorator
    {
        private readonly bool _useMultiplier;
        private bool _firstTime = true;
        private readonly Command _command;
        private readonly Stopwatch _timer = new Stopwatch();
        private static readonly int Multi = ConfigHolder.Configs.Global.HybridDelayMulti;
        protected static readonly Dictionary<Command, int> Configs = DelayConfig<Command>.GetDelayConfig(ConfigHolder.Configs.Global.Cooldowns.Commands);

        protected BaseDecorator(Command command, bool useMultilpier = false)
        {
            _command = command;
            _useMultiplier = useMultilpier;
        }

        public bool CanExecute(out int needWait)
        {
            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];

            if (_useMultiplier)
                delay = delay * Multi;

            var seconds = (int) _timer.Elapsed.TotalSeconds;

            if (_firstTime)
            {
                needWait = seconds;
                return true;
            }

            if (seconds < delay)
            {
                needWait = delay - seconds;
                return false;
            }

            needWait = seconds;
            return true;
        }

        public SendMessage Execute(Func<SendMessage> func, bool needExec = true)
        {
            var delay = Configs.ContainsKey(_command) ? Configs[_command] : Configs[Command.Global];

            if (_useMultiplier)
                delay = delay * Multi;

            var seconds = (int)_timer.Elapsed.TotalSeconds;

            if (!_timer.IsRunning)
                _timer.Start();

            if (_firstTime)
            {
                _firstTime = false;
                return needExec ? func() : SendMessage.None;
            }

            if (seconds >= delay)
            {
                _timer.Restart();
                return needExec ? func() : SendMessage.None;
            }

            return SendMessage.None;
        }

        public void RestartTimer()
        {
            if (_timer.IsRunning)
                _timer.Restart();
        }
    }
}
