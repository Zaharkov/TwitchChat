using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    public class StreamInfo
    {
        [JsonProperty("stream")]
        public Stream Stream;
    }
}
