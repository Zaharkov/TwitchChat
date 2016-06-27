using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class Response<T>
    {
        [JsonProperty("response")]
        public T Data { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}
