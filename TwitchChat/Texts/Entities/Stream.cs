using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Stream
    {
        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("active")]
        public string Active { get; set; }

        [JsonProperty("hours")]
        public string Hours { get; set; }

        [JsonProperty("minutes")]
        public string Minutes { get; set; }

        [JsonProperty("seconds")]
        public string Seconds { get; set; }

        [JsonProperty("delay")]
        public string Delay { get; set; }
    }
}
