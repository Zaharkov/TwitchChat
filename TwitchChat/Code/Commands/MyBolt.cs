using System;
using TwitchChat.Controls;
namespace TwitchChat.Code.Commands
{
    public static class MyBolt
    {
        public static SendMessage Bolt(ChatMemberViewModel userModel)
        {
            if (userModel.Name.Equals(userModel.Channel.ChannelName, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetMessage("божечки ты мой! Самый огромный болт, который я видел - 42см...в диаметре Kappa");

            var rnd = new Random();
            return SendMessage.GetMessage($"твой писюн - {rnd.Next(1, 35)} см. Kappa");
        }
    }
}