using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class RequestParam
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
