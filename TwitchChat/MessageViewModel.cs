using Twitchiedll.IRC.Events;

namespace TwitchChat
{
    //  Our viewmodel to show messages
    public class MessageViewModel
    {
        public string User { get; set; }
        public string Message { get; set; }
        public string Color { get; set; }

        public MessageViewModel(MessageEventArgs message)
        {
            User = string.IsNullOrWhiteSpace(message.DisplayName) ? message.Username : message.DisplayName;
            Message = message.Message;
            Color = string.IsNullOrWhiteSpace(message.ColorHex) ? "#000000" : message.ColorHex;
        }

        public MessageViewModel(string user, string message, string color)
        {
            User = user;
            Message = message;
            Color = string.IsNullOrWhiteSpace(color) ?"#000000" : color;
        }
    }
}
