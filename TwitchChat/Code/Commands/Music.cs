using System.Diagnostics;
using System.Linq;
using RestClientHelper;
using VkApi;

namespace TwitchChat.Code.Commands
{
    public static class Music
    {
        private static bool _firstTime = true;
        private static readonly Stopwatch Timer = new Stopwatch();
        private static readonly string UserName = Configuration.GetSetting("VkAudioName");
        private static readonly int Delay = int.Parse(Configuration.GetSetting("VkAudioDelay"));

        public static string GetMusic(MessageEventArgs e)
        {
            if (!e.Mod && !e.Subscriber && !e.Broadcaster)
                return null;

            if (_firstTime)
            {
                _firstTime = false;
                Timer.Start();
                return GetMusic();
            }

            if (Timer.Elapsed.TotalSeconds > Delay)
            {
                Timer.Restart();
                return GetMusic();
            }

            return null;
        }

        private static string GetMusic()
        {
            var list = VkApiClient.GetBroadcastList();

            var user = list.FirstOrDefault(t => t.Name.Equals(UserName) || $"{t.FirstName} {t.SecondName}".Equals(UserName));

            return user == null
                ? $"Сейчас у {UserName} ничего не играет"
                : $"Сейчас у {UserName} играет - '{user.StatusAudio.Artist + " - " + user.StatusAudio.Title}'";
        }
    }
}
