using System.Collections.Generic;
using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Global
    {
        [JsonProperty("texts", Required = Required.Always)]
        public GlobalTexts Texts { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public GlobalParams Params { get; set; }

        [JsonProperty("cooldowns", Required = Required.Always)]
        public GlobalCooldowns Cooldowns { get; set; }

        [JsonProperty("disabledCommands", Required = Required.Always)]
        public List<string> DisabledCommands { get; set; }

        [JsonProperty("hybridDelayMulti", Required = Required.Always)]
        public int HybridDelayMulti { get; set; }
    }

    public class GlobalTexts
    {
        [JsonProperty("cooldown", Required = Required.Always)]
        public string Cooldown { get; set; }

        [JsonProperty("globalCD", Required = Required.Always)]
        public string GlobalCd { get; set; }

        [JsonProperty("userCD", Required = Required.Always)]
        public string UserCd { get; set; }

        [JsonProperty("seconds", Required = Required.Always)]
        public string Seconds { get; set; }
    }

    public class GlobalParams
    {
        [JsonProperty("clientId", Required = Required.Always)]
        public string ClientId { get; set; }

        [JsonProperty("url", Required = Required.Always)]
        public string Url { get; set; }

        [JsonProperty("twitchApiBaseUrl", Required = Required.Always)]
        public string TwitchApiBaseUrl { get; set; }

        [JsonProperty("twitchTmiBaseUrl", Required = Required.Always)]
        public string TwitchTmiBaseUrl { get; set; }

        [JsonProperty("whisperOn", Required = Required.Always)]
        public bool WhisperOn { get; set; }
    }

    public class GlobalCooldowns
    {
        [JsonProperty("commands", Required = Required.Always)]
        public Dictionary<string, int> Commands { get; set; }

        [JsonProperty("timers", Required = Required.Always)]
        public Dictionary<string, int> Timers { get; set; }
    }
}
