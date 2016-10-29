using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Subscribe
    {
        [JsonProperty("list", Required = Required.Always)]
        public List<MonthText> List { get; set; }
    }
}
