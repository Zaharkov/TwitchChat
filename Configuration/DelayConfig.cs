using System;
using System.Collections.Generic;
using System.Linq;

namespace Configuration
{
    public class DelayConfig<T> where T: struct
    {
        private readonly Dictionary<string, int> _delaysConfig;
        private readonly Dictionary<T, int> _configs = new Dictionary<T, int>();

        private DelayConfig(Dictionary<string, int> cooldowns)
        {
            if(!typeof(T).IsEnum)
                throw new ArgumentException($"{nameof(T)} must be Enum");

            _delaysConfig = cooldowns;

            BuildDelays();
        }

        public static Dictionary<T, int> GetDelayConfig(Dictionary<string, int> cooldowns)
        {
            var config = new DelayConfig<T>(cooldowns);
            return config._configs;
        }

        public void BuildDelays()
        {
            foreach (var delay in _delaysConfig)
            {
                T command;
                if (Enum.TryParse(delay.Key, out command))
                    _configs.Add(command, delay.Value);
                else
                    throw new ArgumentException($"There is no such command: {delay.Key}");
            }

            if (!_configs.Select(t => t.Key.ToString()).Any(k => "Global".Equals(k)))
                throw new ArgumentException("'Global' default delay must be set");
        }

    }
}
