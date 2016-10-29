using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class MonthText
    {
        [JsonProperty("start", Required = Required.Always)]
        public int Start { get; set; }

        [JsonProperty("end", Required = Required.Always)]
        public int End { get; set; }

        [JsonProperty("text", Required = Required.Always)]
        public string Text { get; set; }
    }
}
