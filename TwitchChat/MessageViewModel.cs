using Twitchiedll.IRC.Events;

namespace TwitchChat
{
    //  Our viewmodel to show messages
    public class MessageViewModel
    {
        public string User { get; set; }
        public string Message { get; set; }
        public string ColorUser { get; set; }
        public string ColorMessage { get; set; }

        public MessageViewModel(MessageEventArgs message)
        {
            User = string.IsNullOrWhiteSpace(message.DisplayName) ? message.Username : message.DisplayName;
            Message = message.Message;
            ColorUser = string.IsNullOrWhiteSpace(message.ColorHex) ? "#000000" : message.ColorHex;
            ColorMessage = message.IsAction && !string.IsNullOrWhiteSpace(message.ColorHex) ? message.ColorHex : "#000000";
        }

        public MessageViewModel(string user, string message, string color, bool isAction)
        {
            User = user;
            Message = message;
            ColorUser = string.IsNullOrWhiteSpace(color) ?"#000000" : color;
            ColorMessage = isAction && !string.IsNullOrWhiteSpace(color) ? color : "#000000";
        }
    }
}
