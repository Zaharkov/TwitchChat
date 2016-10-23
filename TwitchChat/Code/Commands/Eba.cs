using System;
using TwitchChat.Controls;

namespace TwitchChat.Code.Commands
{
    public static class Eba
    {
        private static readonly Random Rnd = new Random();

        public static SendMessage EbaComeOn(ChatMemberViewModel userModel)
        {
            string name = null;
            var chatters = userModel.Channel.GetAllChatters();

            if (chatters.Count < 5)
                return SendMessage.None;

            while (string.IsNullOrEmpty(name))
            {
                var chatter = chatters[Rnd.Next(chatters.Count - 1)];
                
                if (!chatter.Name.Equals(userModel.Name, StringComparison.InvariantCultureIgnoreCase))
                    name = chatter.Name;
            }

            var message =  string.IsNullOrEmpty(name) 
                ? null 
                : $"делай ЭБА, делай ЭБА, @{name}, делай ЭБА, @{name} ДАВАААААЙ!";

            return SendMessage.GetMessage(message);
        }
    }
}