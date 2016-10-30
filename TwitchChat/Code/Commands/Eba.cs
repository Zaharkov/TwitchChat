using System;
using TwitchChat.Controls;
using Configuration;

namespace TwitchChat.Code.Commands
{
    public static class Eba
    {
        private static readonly Configuration.Entities.Eba Texts = ConfigHolder.Configs.Eba;
        private static readonly Random Rnd = new Random();

        public static SendMessage EbaComeOn(ChatMemberViewModel userModel)
        {
            var chatters = userModel.Channel.GetAllChatters();
            var chatter = chatters[Rnd.Next(chatters.Count - 1)];
            var message = string.Format(Texts.Do, chatter.Name);

            return SendMessage.GetMessage(message);
        }
    }
}