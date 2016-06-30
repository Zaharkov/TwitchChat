using System;
using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonProperty("second_name")]
        public string SecondName { get; set; } = string.Empty;

        [JsonProperty("status_audio")]
        public StatusAudio StatusAudio { get; set; }
    }
}
