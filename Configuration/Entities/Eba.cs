using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Eba
    {
        [JsonProperty("do", Required = Required.Always)]
        public string Do { get; set; }
    }
}
