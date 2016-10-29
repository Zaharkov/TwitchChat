using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class MyTime
    {
        [JsonProperty("time", Required = Required.Always)]
        public string Time { get; set; }

        [JsonProperty("seconds", Required = Required.Always)]
        public string Seconds { get; set; }
    }
}
