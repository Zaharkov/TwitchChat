using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Mmr
    {
        [JsonProperty("solo")]
        public string Solo { get; set; }

        [JsonProperty("noSolo")]
        public string NoSolo { get; set; }

        [JsonProperty("party")]
        public string Party { get; set; }

        [JsonProperty("noParty")]
        public string NoParty { get; set; }
    }
}
