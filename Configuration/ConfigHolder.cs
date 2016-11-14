using System.Collections.Generic;
using System.IO;
using Configuration.Entities;
using Newtonsoft.Json;

namespace Configuration
{
    public static class ConfigHolder
    {
        public static Config Configs;

        static ConfigHolder()
        {
            var str = File.ReadAllText("configs.json");

            Configs = JsonConvert.DeserializeObject<Config>(str);
        }
    }

    public static class CustomCommands<T1, T2, T3>
    {
        public static List<CustomCommand<T1, T2, T3>> Commands;

        static CustomCommands()
        {
            var str = File.ReadAllText("customCommands.json");

            Commands = JsonConvert.DeserializeObject<List<CustomCommand<T1, T2, T3>>>(str);
        }
    }
}
