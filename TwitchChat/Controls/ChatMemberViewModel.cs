namespace TwitchChat.Controls
{
    //  This can be expanded to store age and other badge information
    public class ChatMemberViewModel
    {
        public string Name { get; private set; }
        public ChannelViewModel Channel { get; private set; }
        public ChatGroupViewModel Group { get; private set; }

        public ChatMemberViewModel(string name, ChannelViewModel channel, ChatGroupViewModel group)
        {
            Name = name;
            Channel = channel;
            Group = group;
        }
    }
}
