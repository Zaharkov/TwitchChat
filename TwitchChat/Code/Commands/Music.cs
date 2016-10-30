using System.Linq;
using Configuration;
using Configuration.Entities;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class MusicCommand
    {
        private static readonly Music Music = ConfigHolder.Configs.Music;

        public static SendMessage GetMusic()
        {
            var list = VkApiClient.GetBroadcastList();

            var user = list.FirstOrDefault(t => t.Name.Equals(Music.Params.VkAudioName) || $"{t.FirstName} {t.SecondName}".Equals(Music.Params.VkAudioName));

            var message = user == null
                ? string.Format(Music.Texts.NoMusic, Music.Params.VkAudioName)
                : string.Format(Music.Texts.Played, Music.Params.VkAudioName, user.StatusAudio.Artist, user.StatusAudio.Title);

            return SendMessage.GetMessage(message);
        }
    }
}
