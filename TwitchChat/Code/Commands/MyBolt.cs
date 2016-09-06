using System;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyBolt
    {
        public static string Bolt(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var rnd = new Random();
            return $"твой писюн - {rnd.Next(1, 35)} см. Kappa";
        }
    }
}