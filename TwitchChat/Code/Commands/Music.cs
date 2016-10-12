using System.Linq;
using CommonHelper;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class MusicCommand
    {
        private static readonly string UserName = Configuration.GetSetting("VkAudioName");

        public static SendMessage GetMusic()
        {
            var list = VkApiClient.GetBroadcastList();

            var user = list.FirstOrDefault(t => t.Name.Equals(UserName) || $"{t.FirstName} {t.SecondName}".Equals(UserName));

            var message = user == null
                ? $"Сейчас у {UserName} ничего не играет"
                : $"Сейчас у {UserName} играет - '{user.StatusAudio.Artist + " - " + user.StatusAudio.Title}'";

            return SendMessage.GetMessage(message);
        }
    }
}
