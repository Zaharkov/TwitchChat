using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Mmr
    {
        [JsonProperty("solo", Required = Required.Always)]
        public string Solo { get; set; }

        [JsonProperty("noSolo", Required = Required.Always)]
        public string NoSolo { get; set; }

        [JsonProperty("party", Required = Required.Always)]
        public string Party { get; set; }

        [JsonProperty("noParty", Required = Required.Always)]
        public string NoParty { get; set; }
    }
}
