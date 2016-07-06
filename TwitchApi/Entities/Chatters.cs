using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    /// <summary>
    /// The result from a get request to http://tmi.twitch.tv/servers?channel={channel_name}
    /// </summary>

    public class Chatters
    {
        [JsonProperty("moderators")]
        public List<string> Moderators { get; set; }

        [JsonProperty("staff")]
        public List<string> Staff { get; set; }

        [JsonProperty("admins")]
        public List<string> Admins { get; set; }

        [JsonProperty("global_mods")]
        public List<string> GlobalMods { get; set; }

        [JsonProperty("viewers")]
        public List<string> Viewers { get; set; }
    }
}

