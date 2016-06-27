using System;
using System.Configuration;

namespace RestClientHelper
{
    public class Configuration
    {
        public static string GetSetting(string name)
        {
            var value = ConfigurationManager.AppSettings.Get(name);

            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(name), @"Not exists");

            return value;
        }
    }
}
