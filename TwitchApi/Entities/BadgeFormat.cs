using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    public class BadgeFormat
    {
        [JsonProperty("alpha")]
        public string Aplha { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("svg")]
        public string Svg { get; set; }
    }
}
