using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Eba
    {
        [JsonProperty("do", Required = Required.Always)]
        public string Do { get; set; }
    }
}
