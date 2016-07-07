using System;
using System.Collections.Generic;
using System.Linq;
using RestClientHelper;

namespace TwitchChat.Code.Helpers
{
    public class DelayConfig<T> where T: struct
    {
        private readonly string _configName;
        private readonly string _delaysConfig;
        private readonly Dictionary<T, int> _configs = new Dictionary<T, int>();

        private DelayConfig(string configName)
        {
            if(!typeof(T).IsEnum)
                throw new ArgumentException($"{nameof(T)} must be Enum");

            Check.ForNullReference(configName, nameof(configName));

            _configName = configName;
            _delaysConfig = Configuration.GetSetting(configName);

            BuildDelays();
        }

        public static Dictionary<T, int> GetDelayConfig(string configName)
        {
            var config = new DelayConfig<T>(configName);
            return config._configs;
        }

        public void BuildDelays()
        {
            var delays = _delaysConfig.Split(';');

            foreach (var delay in delays)
            {
                var pair = delay.Split('=');

                if (pair.Length < 2)
                    throw new ArgumentException($"{_configName} have invalid structure. Must be 'command1=delay1;command2=delay2...'");

                T command;
                if (Enum.TryParse(pair[0], out command))
                {
                    int commandDelay;
                    if (int.TryParse(pair[1], out commandDelay))
                    {
                        if (commandDelay < 0)
                            throw new ArgumentException("Delay must cannot be less then zero");

                        _configs.Add(command, commandDelay);
                    }
                    else
                        throw new ArgumentException("Delay must be a integer");
                }
                else
                    throw new ArgumentException($"There is no such command: {pair[0]}");
            }

            if (!_configs.Select(t => t.Key.ToString()).Any(k => "Global".Equals(k)))
                throw new ArgumentException("'Global' default delay must be set");
        }

    }
}
