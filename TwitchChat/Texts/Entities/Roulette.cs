using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Roulette
    {
        [JsonProperty("admin")]
        public string Admin { get; set; }

        [JsonProperty("moder")]
        public string Moder { get; set; }

        [JsonProperty("misfire")]
        public string Misfire { get; set; }

        [JsonProperty("death")]
        public string Death { get; set; }

        [JsonProperty("luck")]
        public string Luck { get; set; }

        [JsonProperty("stats")]
        public string Stats { get; set; }

        [JsonProperty("topStart")]
        public string TopStart { get; set; }

        [JsonProperty("topUser")]
        public string TopUser { get; set; }
    }
}
