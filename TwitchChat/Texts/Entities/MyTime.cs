using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class MyTime
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("seconds")]
        public string Seconds { get; set; }
    }
}
