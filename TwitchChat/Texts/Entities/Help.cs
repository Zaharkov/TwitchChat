using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Help
    {
        [JsonProperty("startMessage")]
        public string StartMessage { get; set; }

        [JsonProperty("defaultMessage")]
        public string DefaultMessage { get; set; }

        [JsonProperty("customMessage")]
        public string CustomMessage { get; set; }
    }
}
