using System.Collections.Generic;
using Newtonsoft.Json;
using TwitchApi.Utils;

namespace TwitchApi.Entities
{
    /// <summary>
    /// The result from a get request to http://tmi.twitch.tv/servers?channel={channel_name}
    /// </summary>

    public class Servers
    {
        [JsonProperty("cluster")]
        public string Clustert { get; set; }

        [JsonProperty("servers")]
        public List<Server> ServersList { get; set; }

        [JsonProperty("websockets_servers")]
        public List<string> WebSocketsServers { get; set; }
    }
}

