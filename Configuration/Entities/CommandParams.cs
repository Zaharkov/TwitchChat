using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class CommandParams
    {
        [JsonProperty("type", Required = Required.Always)]
        public ParamType Type { get; set; }

        [JsonProperty("min")]
        public int Min { get; set; }

        [JsonProperty("max")]
        public int Max { get; set; }

    }

    public enum ParamType
    {
        RandomNumber,
        RandomUser,
        UserName
    }
}
