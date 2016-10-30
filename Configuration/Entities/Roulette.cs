using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Roulette
    {
        [JsonProperty("params", Required = Required.Always)]
        public RouletteParams Params { get; set; }

        [JsonProperty("texts", Required = Required.Always)]
        public RouletteTexts Texts { get; set; }
    }

    public class RouletteParams
    {
        [JsonProperty("timeout", Required = Required.Always)]
        public int Timeout { get; set; }

        [JsonProperty("top", Required = Required.Always)]
        public int Top { get; set; }
    }

    public class RouletteTexts
    {
        [JsonProperty("admin", Required = Required.Always)]
        public string Admin { get; set; }

        [JsonProperty("moder", Required = Required.Always)]
        public string Moder { get; set; }

        [JsonProperty("misfire", Required = Required.Always)]
        public string Misfire { get; set; }

        [JsonProperty("death", Required = Required.Always)]
        public string Death { get; set; }

        [JsonProperty("luck", Required = Required.Always)]
        public string Luck { get; set; }

        [JsonProperty("stats", Required = Required.Always)]
        public string Stats { get; set; }

        [JsonProperty("topStart", Required = Required.Always)]
        public string TopStart { get; set; }

        [JsonProperty("topUser", Required = Required.Always)]
        public string TopUser { get; set; }
    }
}
