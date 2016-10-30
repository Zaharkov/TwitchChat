using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class MyBolt
    {
        [JsonProperty("admin", Required = Required.Always)]
        public string Admin { get; set; }

        [JsonProperty("user", Required = Required.Always)]
        public string User { get; set; }

        [JsonProperty("min", Required = Required.Always)]
        public int Min { get; set; }
        
        [JsonProperty("max", Required = Required.Always)]
        public int Max { get; set; }
    }
}
