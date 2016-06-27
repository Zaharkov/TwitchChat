using Newtonsoft.Json;

namespace TwitchApi.Entities
{
    public class User
    {
        [JsonProperty("name")]
        public string Name;
    }
}
