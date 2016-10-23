using System;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<string, ChatterType> GetAll()
        {
            return Viewers.Select(t => new KeyValuePair<string, ChatterType>(t, ChatterType.Viewers))
                .Concat(Moderators.Select(t => new KeyValuePair<string, ChatterType>(t, ChatterType.Moderators)))
                .Concat(Staff.Select(t => new KeyValuePair<string, ChatterType>(t, ChatterType.Staff)))
                .Concat(Admins.Select(t => new KeyValuePair<string, ChatterType>(t, ChatterType.Admins)))
                .Concat(GlobalMods.Select(t => new KeyValuePair<string, ChatterType>(t, ChatterType.GlobalMods)))
                .ToDictionary(k => k.Key, v => v.Value);
        }
    }
}

