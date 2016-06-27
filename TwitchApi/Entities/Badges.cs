using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    /// <summary>
    /// Result from https://api.twitch.tv/kraken/chat/{channel}/badges
    /// </summary>

    public class Badges
    {
        [JsonProperty("admin")]
        public BadgeFormat Admin { get; set; }
        [JsonProperty("broadcaster")]
        public BadgeFormat Broadcaster { get; set; }
        [JsonProperty("global_mod")]
        public BadgeFormat GlobalMod { get; set; }
        [JsonProperty("mod")]
        public BadgeFormat Mod { get; set; }
        [JsonProperty("staff")]
        public BadgeFormat Staff { get; set; }
        [JsonProperty("subscriber")]
        public BadgeFormat Subscriber { get; set; }
        [JsonProperty("turbo")]
        public BadgeFormat Turbo { get; set; }
    }
}
