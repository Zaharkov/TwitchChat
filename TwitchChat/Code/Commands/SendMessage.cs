namespace TwitchChat.Code.Commands
{
    public class SendMessage
    {
        public SendType Type { get; }

        public string Message { get; }

        public int Timeout { get; }

        public bool NeedPrefix { get; private set; }

        public SendMessage NextMessage { get; private set; }

        private SendMessage(SendType type, string message, int timeout)
        {
            Type = type;
            Message = message;
            Timeout = timeout;
            NeedPrefix = true;
        }

        public SendMessage AddMessage(SendMessage message)
        {
            NextMessage = message;
            return this;
        }

        public SendMessage Prefix(bool need)
        {
            NeedPrefix = need;
            return this;
        }

        public static SendMessage None = new SendMessage(SendType.None, null, 0);

        public static SendMessage GetMessage(string message)
        {
            return new SendMessage(SendType.Message, message, 0);
        }

        public static SendMessage GetWhisper(string message)
        {
            return new SendMessage(SendType.Whisper, message, 0);
        }

        public static SendMessage GetTimeout(string user, int timeout)
        {
            return new SendMessage(SendType.Timeout, user, timeout);
        }
    }
}
