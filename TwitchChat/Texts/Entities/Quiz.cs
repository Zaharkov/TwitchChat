using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Quiz
    {
        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("off")]
        public string Off { get; set; }

        [JsonProperty("score")]
        public string Score { get; set; }

        [JsonProperty("answers")]
        public string Answers { get; set; }

        [JsonProperty("rightAnswer")]
        public string RightAnswer { get; set; }
    }
}
