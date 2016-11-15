using System;
using System.Collections.Generic;
using System.Linq;
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
        [JsonConverter(typeof(FlagsEnumConverter))]
        public TAccess Access { get; set; }

        [JsonProperty("cooldownType", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TCooldownType CooldownType { get; set; }

        [JsonProperty("cooldownTime", Required = Required.Always)]
        public int CooldownTime { get; set; }
        
        [JsonProperty("texts", Required = Required.Always)]
        public List<Texts<TAccess>> Texts { get; set; }

        [JsonProperty("params")]
        public List<CommandParams> CommandParams { get; set; }
    }

    public class FlagsEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum && objectType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var values = serializer.Deserialize<string[]>(reader);

            return values
                .Select(x => Enum.Parse(objectType, x, true))
                .Aggregate(0, (current, value) => current | (int)value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var enumArr = Enum.GetValues(value.GetType())
                .Cast<int>()
                .Where(x => (x & (int)value) == x)
                .Select(x => Enum.ToObject(value.GetType(), x).ToString())
                .ToArray();

            serializer.Serialize(writer, enumArr);
        }
    }
}
