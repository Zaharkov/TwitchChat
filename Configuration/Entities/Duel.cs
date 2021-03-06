﻿using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Duel
    {
        [JsonProperty("texts", Required = Required.Always)]
        public DuelTexts Texts { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public DuelParams Params { get; set; }
    }

    public class DuelParams
    {
        [JsonProperty("wait", Required = Required.Always)]
        public int Wait { get; set; }

        [JsonProperty("delay", Required = Required.Always)]
        public int Delay { get; set; }
    }

    public class DuelTexts
    {
        [JsonProperty("alreadyInDuel", Required = Required.Always)]
        public string AlreadyInDuel { get; set; }

        [JsonProperty("incorrectSyntax", Required = Required.Always)]
        public string IncorrectSyntax { get; set; }

        [JsonProperty("noInChat", Required = Required.Always)]
        public string NoInChat { get; set; }

        [JsonProperty("roulette", Required = Required.Always)]
        public string Roulette { get; set; }

        [JsonProperty("needConfirm", Required = Required.Always)]
        public string NeedConfirm { get; set; }

        [JsonProperty("targetAlreadyInDuel", Required = Required.Always)]
        public string TargetAlreadyInDuel { get; set; }

        [JsonProperty("noDuel", Required = Required.Always)]
        public string NoDuel { get; set; }

        [JsonProperty("noConfirm", Required = Required.Always)]
        public string NoConfirm { get; set; }

        [JsonProperty("start", Required = Required.Always)]
        public string Start { get; set; }

        [JsonProperty("incorrectSyntaxStart", Required = Required.Always)]
        public string IncorrectSyntaxStart { get; set; }

        [JsonProperty("adminVsModer", Required = Required.Always)]
        public string AdminVsModer { get; set; }

        [JsonProperty("adminVsUser", Required = Required.Always)]
        public string AdminVsUser { get; set; }

        [JsonProperty("moderVsModer", Required = Required.Always)]
        public string ModerVsModer { get; set; }

        [JsonProperty("moderVsUser", Required = Required.Always)]
        public string ModerVsUser { get; set; }

        [JsonProperty("win", Required = Required.Always)]
        public string Win { get; set; }

        [JsonProperty("confirm", Required = Required.Always)]
        public string Confirm { get; set; }

        [JsonProperty("misfire", Required = Required.Always)]
        public string Misfire { get; set; }

        [JsonProperty("death", Required = Required.Always)]
        public string Death { get; set; }

        [JsonProperty("luck", Required = Required.Always)]
        public string Luck { get; set; }
    }
}
