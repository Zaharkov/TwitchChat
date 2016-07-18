using System;
using System.IO;
using CommonHelper;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RestClientHelper
{
    internal class NewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonSerializer(JsonSerializer serializer)
        {
            Check.ForNullReference(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    try
                    {
                        return _serializer.Deserialize<T>(jsonTextReader);
                    }
                    catch (Exception e)
                    {
                        throw new JsonSerializationException(
                            $"Unable to deserialize response content '{content}' to {typeof(T)}.", e);
                    }
                }
            }
        }

        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public string ContentType { get; set; } = "application/json";

        public static NewtonsoftJsonSerializer Default => new NewtonsoftJsonSerializer(new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
    }
}
