using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int Expire { get; set; }
    }
}
