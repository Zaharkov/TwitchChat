namespace TwitchChat.Code.Json.Objects
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The result from a get request to http://tmi.twitch.tv/servers?channel={channel_name}
    /// </summary>
    [DataContract]
    public class TwitchServerResult
    {
        [DataMember(Name = "cluster")]
        public string Cluster;
        [DataMember(Name = "servers")]
        public string[] Servers;
        [DataMember(Name = "websockets_servers")]
        public string[] WebSocketsServers;
    }
}
