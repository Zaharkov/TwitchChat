using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Subscribe
    {
        [JsonProperty("list")]
        public List<MonthText> List { get; set; }
    }
}
