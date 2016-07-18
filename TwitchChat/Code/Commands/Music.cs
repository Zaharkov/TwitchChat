using System.Linq;
using CommonHelper;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class MusicCommand
    {
        private static readonly string UserName = Configuration.GetSetting("VkAudioName");

        public static string GetMusic(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var list = VkApiClient.GetBroadcastList();

            var user = list.FirstOrDefault(t => t.Name.Equals(UserName) || $"{t.FirstName} {t.SecondName}".Equals(UserName));

            return user == null
                ? $"Сейчас у {UserName} ничего не играет"
                : $"Сейчас у {UserName} играет - '{user.StatusAudio.Artist + " - " + user.StatusAudio.Title}'";
        }
    }
}
