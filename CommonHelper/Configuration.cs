using System;
using System.Configuration;

namespace CommonHelper
{
    public class Configuration
    {
        public static string GetSetting(string name)
        {
            var value = ConfigurationManager.AppSettings.Get(name);

            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(name, @"Not exists");

            return value;
        }
    }
}
