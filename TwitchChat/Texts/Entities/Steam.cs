using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Steam
    {
        [JsonProperty("incorrectAddSyntaxAdmin", Required = Required.Always)]
        public string IncorrectAddSyntaxAdmin { get; set; }

        [JsonProperty("notIntoDataBase", Required = Required.Always)]
        public string NotIntoDataBase { get; set; }

        [JsonProperty("userAlreadyAttached", Required = Required.Always)]
        public string UserAlreadyAttached { get; set; }

        [JsonProperty("notNumber", Required = Required.Always)]
        public string NotNumber { get; set; }

        [JsonProperty("steamAlreadyAttached", Required = Required.Always)]
        public string SteamAlreadyAttached { get; set; }

        [JsonProperty("incorrectAddSyntaxUser", Required = Required.Always)]
        public string IncorrectAddSyntaxUser { get; set; }

        [JsonProperty("addBugAfterDelete", Required = Required.Always)]
        public string AddBugAfterDelete { get; set; }

        [JsonProperty("addBug", Required = Required.Always)]
        public string AddBug { get; set; }

        [JsonProperty("notInFriends", Required = Required.Always)]
        public string NotInFriends { get; set; }

        [JsonProperty("inBlackList", Required = Required.Always)]
        public string InBlackList { get; set; }

        [JsonProperty("alreadyInRequest", Required = Required.Always)]
        public string AlreadyInRequest { get; set; }

        [JsonProperty("alreadyInFriends", Required = Required.Always)]
        public string AlreadyInFriends { get; set; }

        [JsonProperty("alreadyNeedConfirm", Required = Required.Always)]
        public string AlreadyNeedConfirm { get; set; }

        [JsonProperty("needConfirm", Required = Required.Always)]
        public string NeedConfirm { get; set; }

        [JsonProperty("incorrectRemoveSyntaxAdmin", Required = Required.Always)]
        public string IncorrectRemoveSyntaxAdmin { get; set; }

        [JsonProperty("notAttachedSteam", Required = Required.Always)]
        public string NotAttachedSteam { get; set; }

        [JsonProperty("notAttachedUser", Required = Required.Always)]
        public string NotAttachedUser { get; set; }

        [JsonProperty("removeBugAfterDelete", Required = Required.Always)]
        public string RemoveBugAfterDelete { get; set; }

        [JsonProperty("removeBug", Required = Required.Always)]
        public string RemoveBug { get; set; }

        [JsonProperty("removed", Required = Required.Always)]
        public string Removed { get; set; }

        [JsonProperty("wtf", Required = Required.Always)]
        public string Wtf { get; set; }
    }
}
