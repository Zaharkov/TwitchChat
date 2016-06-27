using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class StatusAudio
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
