﻿using TwitchApi.Entities;
using System.Collections.ObjectModel;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Controls
{
    //  Our viewmodel to display messages with badges
    public class ChatMessageViewModel : MessageViewModel
    {
        public ObservableCollection<string> Badges { get; set; }

        public ChatMessageViewModel(MessageEventArgs message, Badges badges) : base(message)
        {
            InitBadges(message.UserType, badges);
        }

        public ChatMessageViewModel(UserType type, string user, string message, string color, Badges badges) 
            : base(user, message, color)
        {
            InitBadges(type, badges);
        }

        private void InitBadges(UserType type, Badges badges)
        {
            Badges = new ObservableCollection<string>();

            if (type.HasFlag(UserType.Admin) && badges.Admin != null)
                Badges.Add(badges.Admin.Image);
            if (type.HasFlag(UserType.Globalmoderator) && badges.GlobalMod != null)
                Badges.Add(badges.GlobalMod.Image);
            if (type.HasFlag(UserType.Staff) && badges.Staff != null)
                Badges.Add(badges.Staff.Image);
            if (type.HasFlag(UserType.Subscriber) && badges.Subscriber != null)
                Badges.Add(badges.Subscriber.Image);
            if (type.HasFlag(UserType.Moderator) && badges.Mod != null)
                Badges.Add(badges.Mod.Image);
            if (type.HasFlag(UserType.Turbo) && badges.Turbo != null)
                Badges.Add(badges.Turbo.Image);
            if (type.HasFlag(UserType.Broadcaster) && badges.Broadcaster != null)
                Badges.Add(badges.Broadcaster.Image);
        }
    }
}
