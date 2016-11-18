using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Config
    {
        [JsonProperty("help", Required = Required.Always)]
        public Help Help { get; set; }

        [JsonProperty("quiz", Required = Required.Always)]
        public Quiz Quiz { get; set; }

        [JsonProperty("subscribe", Required = Required.Always)]
        public Subscribe Subscribe { get; set; }

        [JsonProperty("stream", Required = Required.Always)]
        public Stream Stream { get; set; }

        [JsonProperty("steam", Required = Required.Always)]
        public Steam Steam { get; set; }

        [JsonProperty("roulette", Required = Required.Always)]
        public Roulette Roulette { get; set; }

        [JsonProperty("myTime", Required = Required.Always)]
        public MyTime MyTime { get; set; }

        [JsonProperty("music", Required = Required.Always)]
        public Music Music { get; set; }

        [JsonProperty("duel", Required = Required.Always)]
        public Duel Duel { get; set; }

        [JsonProperty("global", Required = Required.Always)]
        public Global Global { get; set; }
    }
}