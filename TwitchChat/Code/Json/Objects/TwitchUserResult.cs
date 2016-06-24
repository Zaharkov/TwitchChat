namespace TwitchChat.Code.Json.Objects
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Result from https://api.twitch.tv/kraken/user
    /// </summary>
    [DataContract]
    public class TwitchUserResult
    {
        [DataMember(Name = "name")]
        public string Name;
    }
}
