using System;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyBolt
    {
        public static string Bolt(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (userModel.Name.Equals(userModel.Channel.ChannelName, StringComparison.InvariantCultureIgnoreCase))
                return "божечки ты мой! Самый огромный болт, который я видел - 42см...в диаметре Kappa";

            var rnd = new Random();
            return $"твой писюн - {rnd.Next(1, 35)} см. Kappa";
        }
    }
}