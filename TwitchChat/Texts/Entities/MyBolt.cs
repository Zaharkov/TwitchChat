using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class MyBolt
    {
        [JsonProperty("admin")]
        public string Admin { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("min")]
        public int Min { get; set; }
        
        [JsonProperty("max")]
        public int Max { get; set; }
    }
}
