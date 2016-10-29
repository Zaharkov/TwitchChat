using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class MonthText
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
