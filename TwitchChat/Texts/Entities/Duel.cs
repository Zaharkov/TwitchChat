using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Duel
    {
        [JsonProperty("alreadyInDuel")]
        public string AlreadyInDuel { get; set; }

        [JsonProperty("incorrectSyntax")]
        public string IncorrectSyntax { get; set; }

        [JsonProperty("noInChat")]
        public string NoInChat { get; set; }

        [JsonProperty("roulette")]
        public string Roulette { get; set; }

        [JsonProperty("needConfirm")]
        public string NeedConfirm { get; set; }

        [JsonProperty("targetAlreadyInDuel")]
        public string TargetAlreadyInDuel { get; set; }

        [JsonProperty("noConfirm")]
        public string NoConfirm { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("incorrectSyntaxStart")]
        public string IncorrectSyntaxStart { get; set; }

        [JsonProperty("adminVsModer")]
        public string AdminVsModer { get; set; }

        [JsonProperty("adminVsUser")]
        public string AdminVsUser { get; set; }

        [JsonProperty("moderVsModer")]
        public string ModerVsModer { get; set; }

        [JsonProperty("moderVsUser")]
        public string ModerVsUser { get; set; }

        [JsonProperty("win")]
        public string Win { get; set; }

        [JsonProperty("confirm")]
        public string Confirm { get; set; }

        [JsonProperty("misfire")]
        public string Misfire { get; set; }

        [JsonProperty("death")]
        public string Death { get; set; }

        [JsonProperty("luck")]
        public string Luck { get; set; }
    }
}
