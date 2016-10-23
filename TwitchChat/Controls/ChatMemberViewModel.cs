using System.Diagnostics;

namespace TwitchChat.Controls
{
    //  This can be expanded to store age and other badge information
    public class ChatMemberViewModel
    {
        public string Name { get; private set; }
        public ChannelViewModel Channel { get; private set; }
        public ChatGroupViewModel Group { get; private set; }

        private Stopwatch Timer { get; } = Stopwatch.StartNew();

        public ChatMemberViewModel(string name, ChannelViewModel channel, ChatGroupViewModel group)
        {
            Name = name;
            Channel = channel;
            Group = group;
        }

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
