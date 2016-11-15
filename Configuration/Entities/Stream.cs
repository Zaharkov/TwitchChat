using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Stream
    {
        [JsonProperty("end", Required = Required.Always)]
        public string End { get; set; }

        [JsonProperty("game", Required = Required.Always)]
        public string Game { get; set; }
        
        [JsonProperty("active", Required = Required.Always)]
        public string Active { get; set; }

        [JsonProperty("hours", Required = Required.Always)]
        public string Hours { get; set; }

        [JsonProperty("minutes", Required = Required.Always)]
        public string Minutes { get; set; }

        [JsonProperty("seconds", Required = Required.Always)]
        public string Seconds { get; set; }

        [JsonProperty("delay", Required = Required.Always)]
        public string Delay { get; set; }
    }
}
