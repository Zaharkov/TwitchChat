using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("second_name")]
        public string SecondName { get; set; }

        [JsonProperty("status_audio")]
        public StatusAudio StatusAudio { get; set; }
    }
}
