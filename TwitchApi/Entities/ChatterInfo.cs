using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    /// <summary>
    /// The result from a get request to http://tmi.twitch.tv/servers?channel={channel_name}
    /// </summary>

    public class ChatterInfo
    {
        [JsonProperty("chatters")]
        public Chatters Chatters { get; set; }

        [JsonProperty("chatter_count")]
        public int ChatterCount { get; set; }
    }
}

