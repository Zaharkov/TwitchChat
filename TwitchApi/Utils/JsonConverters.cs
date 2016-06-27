using System;
using Newtonsoft.Json;
using TwitchApi.Entities;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace TwitchApi.Utils
{
    public class TwitchServerConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var server = value as Server;

            if(server == null)
                throw new ArgumentException(nameof(value));

            writer.WriteStartObject();
            serializer.Serialize(writer, $"{server.Host}:{server.Port}");
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString().Split(':');

            return new Server
            {
                Host = value[0],
                Port = int.Parse(value[1])
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Server).IsAssignableFrom(objectType);
        }
    }
}
