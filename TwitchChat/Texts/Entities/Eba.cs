using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Eba
    {
        [JsonProperty("do")]
        public string Do { get; set; }
    }
}
