using System;
using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    public class Stream
    {
        [JsonProperty("_id")]
        public long Id;

        [JsonProperty("game")]
        public string Game;

        [JsonProperty("viewers")]
        public int Viewers;

        [JsonProperty("delay")]
        public int Delay;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("channel")]
        public Channel Channel;
    }
}
