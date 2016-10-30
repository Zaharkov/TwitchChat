using Newtonsoft.Json;

namespace Configuration.Entities
{
    public class Music
    {
        [JsonProperty("texts", Required = Required.Always)]
        public MusicTexts Texts { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public MusicParams Params { get; set; }
    }

    public class MusicParams
    {
        [JsonProperty("vkClientId", Required = Required.Always)]
        public string VkClientId { get; set; }

        [JsonProperty("vkClientSecret", Required = Required.Always)]
        public string VkClientSecret { get; set; }

        [JsonProperty("vkAudioName", Required = Required.Always)]
        public string VkAudioName { get; set; }
    }

    public class MusicTexts
    {
        [JsonProperty("noMusic", Required = Required.Always)]
        public string NoMusic { get; set; }

        [JsonProperty("played", Required = Required.Always)]
        public string Played { get; set; }
    }
}
