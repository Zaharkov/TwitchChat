using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Vote
    {
        [JsonProperty("incorrectSyntax", Required = Required.Always)]
        public string IncorrectSyntax { get; set; }

        [JsonProperty("question", Required = Required.Always)]
        public string Question { get; set; }

        [JsonProperty("off", Required = Required.Always)]
        public string Off { get; set; }

        [JsonProperty("result", Required = Required.Always)]
        public string Result { get; set; }

        [JsonProperty("delay", Required = Required.Always)]
        public int Delay { get; set; }

        [JsonProperty("noLast", Required = Required.Always)]
        public string NoLast { get; set; }
    }
}
