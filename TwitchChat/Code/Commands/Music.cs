using System.Linq;
using CommonHelper;
using TwitchChat.Texts;
using TwitchChat.Texts.Entities;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class MusicCommand
    {
        private static readonly string UserName = Configuration.GetSetting("VkAudioName");
        private static readonly Music Texts = TextsHolder.Texts.Music;

        public static SendMessage GetMusic()
        {
            var list = VkApiClient.GetBroadcastList();

            var user = list.FirstOrDefault(t => t.Name.Equals(UserName) || $"{t.FirstName} {t.SecondName}".Equals(UserName));

            var message = user == null
                ? string.Format(Texts.NoMusic, UserName)
                : string.Format(Texts.Played, UserName, user.StatusAudio.Artist, user.StatusAudio.Title);

            return SendMessage.GetMessage(message);
        }
    }
}
