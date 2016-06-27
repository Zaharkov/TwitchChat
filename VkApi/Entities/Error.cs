using System.Collections.Generic;
using Newtonsoft.Json;

namespace VkApi.Entities
{
    public class Error
    {
        [JsonProperty("error_code")]
        public int Code { get; set; }

        [JsonProperty("error_msg")]
        public string Message { get; set; }

        [JsonProperty("request_params")]
        public List<RequestParam> RequestParams { get; set; }
    }
}
