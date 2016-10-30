using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Sheiker
    {
        [JsonProperty("hei", Required = Required.Always)]
        public string Hei { get; set; }
    }
}
