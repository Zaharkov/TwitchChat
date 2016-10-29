using Newtonsoft.Json;

namespace TwitchChat.Texts.Entities
{
    public class Steam
    {
        [JsonProperty("incorrectAddSyntaxAdmin")]
        public string IncorrectAddSyntaxAdmin { get; set; }

        [JsonProperty("notIntoDataBase")]
        public string NotIntoDataBase { get; set; }

        [JsonProperty("userAlreadyAttached")]
        public string UserAlreadyAttached { get; set; }

        [JsonProperty("notNumber")]
        public string NotNumber { get; set; }

        [JsonProperty("steamAlreadyAttached")]
        public string SteamAlreadyAttached { get; set; }

        [JsonProperty("incorrectAddSyntaxUser")]
        public string IncorrectAddSyntaxUser { get; set; }

        [JsonProperty("addBugAfterDelete")]
        public string AddBugAfterDelete { get; set; }

        [JsonProperty("addBug")]
        public string AddBug { get; set; }

        [JsonProperty("notInFriends")]
        public string NotInFriends { get; set; }

        [JsonProperty("inBlackList")]
        public string InBlackList { get; set; }

        [JsonProperty("alreadyInRequest")]
        public string AlreadyInRequest { get; set; }

        [JsonProperty("alreadyInFriends")]
        public string AlreadyInFriends { get; set; }

        [JsonProperty("alreadyNeedConfirm")]
        public string AlreadyNeedConfirm { get; set; }

        [JsonProperty("needConfirm")]
        public string NeedConfirm { get; set; }

        [JsonProperty("incorrectRemoveSyntaxAdmin")]
        public string IncorrectRemoveSyntaxAdmin { get; set; }

        [JsonProperty("notAttachedSteam")]
        public string NotAttachedSteam { get; set; }

        [JsonProperty("notAttachedUser")]
        public string NotAttachedUser { get; set; }

        [JsonProperty("removeBugAfterDelete")]
        public string RemoveBugAfterDelete { get; set; }

        [JsonProperty("removeBug")]
        public string RemoveBug { get; set; }

        [JsonProperty("removed")]
        public string Removed { get; set; }

        [JsonProperty("wtf")]
        public string Wtf { get; set; }
    }
}
