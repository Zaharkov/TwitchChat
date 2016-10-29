using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Global
    {
        [JsonProperty("cooldown", Required = Required.Always)]
        public string Cooldown { get; set; }

        [JsonProperty("globalCD", Required = Required.Always)]
        public string GlobalCd { get; set; }

        [JsonProperty("userCD", Required = Required.Always)]
        public string UserCd { get; set; }

        [JsonProperty("seconds", Required = Required.Always)]
        public string Seconds { get; set; }
    }
}
