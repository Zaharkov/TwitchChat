using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Music
    {
        [JsonProperty("noMusic")]
        public string NoMusic { get; set; }

        [JsonProperty("played")]
        public string Played { get; set; }
    }
}
