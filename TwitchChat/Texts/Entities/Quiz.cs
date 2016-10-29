using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Quiz
    {
        [JsonProperty("question", Required = Required.Always)]
        public string Question { get; set; }

        [JsonProperty("off", Required = Required.Always)]
        public string Off { get; set; }

        [JsonProperty("score", Required = Required.Always)]
        public string Score { get; set; }

        [JsonProperty("answers", Required = Required.Always)]
        public string Answers { get; set; }

        [JsonProperty("rightAnswer", Required = Required.Always)]
        public string RightAnswer { get; set; }
    }
}
