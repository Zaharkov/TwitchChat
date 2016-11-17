using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Steam
    {
        [JsonProperty("texts", Required = Required.Always)]
        public SteamTexts Texts { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public SteamParams Params { get; set; }
    }

    public class SteamParams
    {
        [JsonProperty("disable", Required = Required.Always)]
        public bool Disable { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public uint Id { get; set; }

        [JsonProperty("user", Required = Required.Always)]
        public string User { get; set; }

        [JsonProperty("pass", Required = Required.Always)]
        public string Pass { get; set; }

        [JsonProperty("mmrDelay", Required = Required.Always)]
        public int MmrDelay { get; set; }

        [JsonProperty("imapHost", Required = Required.Always)]
        public string ImapHost { get; set; }

        [JsonProperty("imapPort", Required = Required.Always)]
        public int ImapPort { get; set; }

        [JsonProperty("imapLogin", Required = Required.Always)]
        public string ImapLogin { get; set; }

        [JsonProperty("imapPass", Required = Required.Always)]
        public string ImapPass { get; set; }

        [JsonProperty("imapUseSsl", Required = Required.Always)]
        public bool ImapUseSsl { get; set; }
    }

    public class SteamTexts
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

        [JsonProperty("solo", Required = Required.Always)]
        public string Solo { get; set; }

        [JsonProperty("noSolo", Required = Required.Always)]
        public string NoSolo { get; set; }

        [JsonProperty("party", Required = Required.Always)]
        public string Party { get; set; }

        [JsonProperty("noParty", Required = Required.Always)]
        public string NoParty { get; set; }
    }
}
