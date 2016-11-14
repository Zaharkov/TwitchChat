using System;
using System.Collections.Generic;
using System.Linq;

namespace Configuration
{
    public class DelayConfig
    {
        private readonly Dictionary<string, int> _delaysConfig;
        private readonly Dictionary<string, int> _configs = new Dictionary<string, int>();

        private DelayConfig(Dictionary<string, int> cooldowns)
        {
            _delaysConfig = cooldowns;

            BuildDelays();
        }

        public static Dictionary<string, int> GetDelayConfig(Dictionary<string, int> cooldowns)
        {
            var config = new DelayConfig(cooldowns);
            return config._configs;
        }

        public void BuildDelays()
        {
            foreach (var delay in _delaysConfig)
                _configs.Add(delay.Key, delay.Value);

            if (!_configs.Select(t => t.Key.ToString()).Any(k => "Global".Equals(k)))
                throw new ArgumentException("'Global' default delay must be set");
        }

    }
}
