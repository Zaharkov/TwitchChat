using System.IO;
using Newtonsoft.Json;
using TwitchChat.Texts.Entities;

namespace TwitchChat.Texts
{
    public static class TextsHolder
    {
        public static Text Texts;

        static TextsHolder()
        {
            var str = File.ReadAllText("Texts\\texts.json");

            Texts = JsonConvert.DeserializeObject<Text>(str);
        }
    }
}
