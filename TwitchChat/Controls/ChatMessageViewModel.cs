﻿using TwitchChat.Code;
using TwitchChat.Code.Json.Objects;

namespace TwitchChat.Controls
{
    using System.Collections.ObjectModel;

    //  Our viewmodel to display messages with badges
    public class ChatMessageViewModel : MessageViewModel
    {
        public ObservableCollection<string> Badges { get; set; }

        public ChatMessageViewModel(MessageEventArgs message, BadgesResult badges) : base(message)
        {
            Badges = new ObservableCollection<string>();

            if (message.UserType == UserType.Admin)
                Badges.Add(badges.Admin.Image);
            if (message.UserType == UserType.GlobalMod)
                Badges.Add(badges.GlobalMod.Image);
            if (message.UserType == UserType.Staff)
                Badges.Add(badges.Staff.Image);
            if (message.Subscriber)
                Badges.Add(badges.Subscriber.Image);
            if (message.Mod || message.UserType == UserType.Mod)
                Badges.Add(badges.Mod.Image);
            if (message.Turbo)
                Badges.Add(badges.Turbo.Image);
            if (message.Channel == message.User)
                Badges.Add(badges.Broadcaster.Image);
        }
    }
}
