using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Global
    {
        [JsonProperty("cooldown")]
        public string Cooldown { get; set; }

        [JsonProperty("globalCD")]
        public string GlobalCd { get; set; }

        [JsonProperty("userCD")]
        public string UserCd { get; set; }

        [JsonProperty("seconds")]
        public string Seconds { get; set; }
    }
}
