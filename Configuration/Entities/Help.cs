using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Help
    {
        [JsonProperty("startMessage", Required = Required.Always)]
        public string StartMessage { get; set; }

        [JsonProperty("defaultMessage", Required = Required.Always)]
        public string DefaultMessage { get; set; }

        [JsonProperty("customMessage", Required = Required.Always)]
        public string CustomMessage { get; set; }
    }
}
