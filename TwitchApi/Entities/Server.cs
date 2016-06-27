using Newtonsoft.Json;
using TwitchApi.Utils;

namespace TwitchApi.Entities
{
    /// <summary>
    /// The result from a get request to http://tmi.twitch.tv/servers?channel={channel_name}
    /// </summary>

    [JsonConverter(typeof(TwitchServerConverter))]
    public class Server
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }
}

