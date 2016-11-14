using System;
using Configuration;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyBolt
    {
        private static readonly Configuration.Entities.MyBolt Texts = ConfigHolder.Configs.MyBolt;

        public static SendMessage Bolt(MessageEventArgs userModel)
        {
            if (userModel.UserType.HasFlag(UserType.Broadcaster))
                return SendMessage.GetMessage(Texts.Admin);

            var rnd = new Random();
            return SendMessage.GetMessage(string.Format(Texts.User, rnd.Next(Texts.Min, Texts.Max+1)));
        }
    }
}