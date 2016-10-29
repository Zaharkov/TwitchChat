using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Text
    {
        [JsonProperty("help")]
        public Help Help { get; set; }

        [JsonProperty("quiz")]
        public Quiz Quiz { get; set; }

        [JsonProperty("subscribe")]
        public Subscribe Subscribe { get; set; }

        [JsonProperty("stream")]
        public Stream Stream { get; set; }

        [JsonProperty("steam")]
        public Steam Steam { get; set; }

        [JsonProperty("sheiker")]
        public Sheiker Sheiker { get; set; }

        [JsonProperty("roulette")]
        public Roulette Roulette { get; set; }

        [JsonProperty("myTime")]
        public MyTime MyTime { get; set; }

        [JsonProperty("myBolt")]
        public MyBolt MyBolt { get; set; }

        [JsonProperty("music")]
        public Music Music { get; set; }

        [JsonProperty("mmr")]
        public Mmr Mmr { get; set; }

        [JsonProperty("eba")]
        public Eba Eba { get; set; }

        [JsonProperty("duel")]
        public Duel Duel { get; set; }

        [JsonProperty("global")]
        public Global Global { get; set; }
    }
}











