﻿using System.Diagnostics;

namespace TwitchChat.Controls
{
    //  This can be expanded to store age and other badge information
    public class ChatMemberViewModel
    {
        public string Name { get; set; }

        public Stopwatch Timer { get; } = Stopwatch.StartNew();
    }
}
