using System;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyBolt
    {
        public static SendMessage Bolt(MessageEventArgs userModel)
        {
            if (userModel.UserType.HasFlag(UserType.Broadcaster))
                return SendMessage.GetMessage("божечки ты мой! Самый огромный болт, который я видел - 42см...в диаметре Kappa");

            var rnd = new Random();
            return SendMessage.GetMessage($"твой писюн - {rnd.Next(1, 35)} см. Kappa");
        }
    }
}