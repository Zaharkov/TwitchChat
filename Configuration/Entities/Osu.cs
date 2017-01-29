using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Osu
    {
        [JsonProperty("selecting", Required = Required.Always)]
        public string Selecting { get; set; }

        [JsonProperty("off", Required = Required.Always)]
        public string Off { get; set; }

        [JsonProperty("played", Required = Required.Always)]
        public string Played { get; set; }

        [JsonProperty("playedwithmap", Required = Required.Always)]
        public string PlayedWithMap { get; set; }

        [JsonProperty("pathtoosu", Required = Required.Always)]
        public string PathToOsu { get; set; }
        
    }
}
