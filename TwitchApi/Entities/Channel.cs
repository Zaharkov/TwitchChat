using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    public class Channel
    {
        [JsonProperty("_id")]
        public long Id;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("views")]
        public long Views;

        [JsonProperty("followers")]
        public long Followers;
    }
}
