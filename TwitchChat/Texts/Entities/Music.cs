using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Music
    {
        [JsonProperty("noMusic", Required = Required.Always)]
        public string NoMusic { get; set; }

        [JsonProperty("played", Required = Required.Always)]
        public string Played { get; set; }
    }
}
