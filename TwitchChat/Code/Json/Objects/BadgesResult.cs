namespace TwitchChat.Code.Json.Objects
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Result from https://api.twitch.tv/kraken/chat/{channel}/badges
    /// </summary>
    [DataContract]
    public class BadgesResult
    {
        [DataContract]
        public class BadgeFormat
        {
            [DataMember(Name = "aplha")]
            public string Aplha;
            [DataMember(Name = "image")]
            public string Image;
            [DataMember(Name = "svg")]
            public string Svg;
        }

        [DataMember(Name = "admin")]
        public BadgeFormat Admin;
        [DataMember(Name = "broadcaster")]
        public BadgeFormat Broadcaster;
        [DataMember(Name = "global_mod")]
        public BadgeFormat GlobalMod;
        [DataMember(Name = "mod")]
        public BadgeFormat Mod;
        [DataMember(Name = "staff")]
        public BadgeFormat Staff;
        [DataMember(Name = "subscriber")]
        public BadgeFormat Subscriber;
        [DataMember(Name = "turbo")]
        public BadgeFormat Turbo;
    }
}
