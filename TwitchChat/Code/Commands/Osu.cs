using System.Diagnostics;
using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class OsuCommand
    {
        private static readonly Osu Osu = ConfigHolder.Configs.Osu;

        public static SendMessage GetMusic()
        {
            var ps = Process.GetProcessesByName("osu!");

            if (ps.Length <= 0)
                return SendMessage.GetMessage(Osu.Off);

            var osu = ps[0];
            var title = osu.MainWindowTitle;

            if(title.IndexOf('-') < 0)
                return SendMessage.GetMessage(Osu.Selecting);

            var song = title.Substring(title.IndexOf('-') + 2);

            return SendMessage.GetMessage(string.Format(Osu.Played, song));
        }
    }
}
