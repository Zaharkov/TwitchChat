using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expire")]
        public int Expire { get; set; }
    }
}
