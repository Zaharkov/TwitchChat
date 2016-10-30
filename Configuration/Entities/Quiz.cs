using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Quiz
    {
        [JsonProperty("texts", Required = Required.Always)]
        public QuizTexts Texts { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public QuizParams Params { get; set; }
    }

    public class QuizParams
    {
        [JsonProperty("delay", Required = Required.Always)]
        public int Delay { get; set; }

        [JsonProperty("restartDelay", Required = Required.Always)]
        public int RestartDelay { get; set; }
    }

    public class QuizTexts
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
