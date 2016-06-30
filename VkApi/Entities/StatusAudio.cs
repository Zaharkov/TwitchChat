using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class StatusAudio
    {
        [JsonProperty("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
    }
}
