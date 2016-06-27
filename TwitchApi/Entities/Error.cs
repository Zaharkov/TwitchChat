using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TwitchApi.Entities
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public HttpStatusCode Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}

