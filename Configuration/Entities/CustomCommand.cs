using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Configuration.Entities
{
    public class CustomCommand<TCommandType, TAccess, TCooldownType>
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TCommandType Type { get; set; }

        [JsonProperty("access", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TAccess Access { get; set; }

        [JsonProperty("cooldownType", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TCooldownType CooldownType { get; set; }

        [JsonProperty("cooldownTime", Required = Required.Always)]
        public int CooldownTime { get; set; }
        
        [JsonProperty("text", Required = Required.Always)]
        public string Text { get; set; }

        [JsonProperty("params")]
        public List<CommandParams> CommandParams { get; set; }
    }
}
