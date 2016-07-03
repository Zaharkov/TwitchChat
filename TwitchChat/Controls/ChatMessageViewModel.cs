using TwitchApi.Entities;
using TwitchChat.Code;
using System.Collections.ObjectModel;

namespace TwitchChat.Controls
{
    //  Our viewmodel to display messages with badges
    public class ChatMessageViewModel : MessageViewModel
    {
        public ObservableCollection<string> Badges { get; set; }

        public ChatMessageViewModel(MessageEventArgs message, Badges badges) : base(message)
        {
            Badges = new ObservableCollection<string>();

            if (message.UserType == UserType.Admin)
                Badges.Add(badges.Admin.Image);
            if (message.UserType == UserType.GlobalMod)
                Badges.Add(badges.GlobalMod.Image);
            if (message.UserType == UserType.Staff)
                Badges.Add(badges.Staff.Image);
            if (message.UserType == UserType.Subscriber)
                Badges.Add(badges.Subscriber.Image);
            if (message.UserType == UserType.Mod)
                Badges.Add(badges.Mod.Image);
            if (message.UserType == UserType.Turbo)
                Badges.Add(badges.Turbo.Image);
            if (message.UserType == UserType.Broadcaster)
                Badges.Add(badges.Broadcaster.Image);
        }
    }
}
