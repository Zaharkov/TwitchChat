using System;
using TwitchChat.Controls;
using Configuration;

namespace TwitchChat.Code.Commands
{
    public static class Eba
    {
        private static readonly Configuration.Entities.Eba Texts = ConfigHolder.Configs.Eba;

        public static SendMessage EbaComeOn(ChatMemberViewModel userModel)
        {
            var random = new Random();
            var chatters = userModel.Channel.GetAllChatters();
            var chatter = chatters[random.Next(0, chatters.Count)];
            var message = string.Format(Texts.Do, chatter.Name);

            return SendMessage.GetMessage(message);
        }
    }
}