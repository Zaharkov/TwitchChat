using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Text
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

        [JsonProperty("sheiker", Required = Required.Always)]
        public Sheiker Sheiker { get; set; }

        [JsonProperty("roulette", Required = Required.Always)]
        public Roulette Roulette { get; set; }

        [JsonProperty("myTime", Required = Required.Always)]
        public MyTime MyTime { get; set; }

        [JsonProperty("myBolt", Required = Required.Always)]
        public MyBolt MyBolt { get; set; }

        [JsonProperty("music", Required = Required.Always)]
        public Music Music { get; set; }

        [JsonProperty("mmr", Required = Required.Always)]
        public Mmr Mmr { get; set; }

        [JsonProperty("eba", Required = Required.Always)]
        public Eba Eba { get; set; }

        [JsonProperty("duel", Required = Required.Always)]
        public Duel Duel { get; set; }

        [JsonProperty("global", Required = Required.Always)]
        public Global Global { get; set; }
    }
}











