using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Sheiker
    {
        [JsonProperty("hei", Required = Required.Always)]
        public string Hei { get; set; }
    }
}
