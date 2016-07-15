using System.Diagnostics;

namespace TwitchChat.Controls
{
    //  This can be expanded to store age and other badge information
    public class ChatMemberViewModel
    {
        public string Name { get; set; }

        private Stopwatch Timer { get; } = Stopwatch.StartNew();

        public long GetTime()
        {
            return (long) Timer.Elapsed.TotalSeconds;
        }

        public long GetTimeAndRestart()
        {
            var time = (long) Timer.Elapsed.TotalSeconds;
            Timer.Restart();
            return time;
        }
    }
}
