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
}
