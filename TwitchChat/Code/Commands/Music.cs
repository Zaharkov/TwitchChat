using Configuration;
using Configuration.Entities;
using HtmlAgilityPack;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class MusicCommand
    {
        private static readonly Music Music = ConfigHolder.Configs.Music;

        public static SendMessage GetMusic()
        {
            var html = VkApiClient.GetHtmlWithSong(Music.Params.VkAudioName);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var audio = doc.DocumentNode.SelectNodes("//div[@class='pp_status']");

            var message = audio != null && audio.Count != 0
                ? string.Format(Music.Texts.Played, Music.Params.VkAudioName, audio[0].InnerText)
                : string.Format(Music.Texts.NoMusic, Music.Params.VkAudioName);

            return SendMessage.GetMessage(message);
        }
    }
}
