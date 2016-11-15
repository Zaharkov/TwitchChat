using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Texts<TAccess>
    {
        [JsonProperty("text", Required = Required.Always)]
        public string Text { get; set; }

        [JsonProperty("access", Required = Required.Always)]
        [JsonConverter(typeof(FlagsEnumConverter))]
        public TAccess Access { get; set; }
    }
}
