using System.Collections.Generic;
using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Subscribe
    {
        [JsonProperty("list", Required = Required.Always)]
        public List<MonthText> List { get; set; }
    }
}
