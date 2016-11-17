using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class OnSubscriberCommand
    {
        private static readonly Subscribe Texts = ConfigHolder.Configs.Subscribe;

        public static SendMessage OnSubscriber(SubscribeParams e)
        {
            SendMessage message = null;

            foreach (var monthText in Texts.List)
            {
                if (monthText.Start <= e.Months && e.Months <= monthText.End)
                {
                    var mes = SendMessage.GetMessage(string.Format(monthText.Text, e.Username));

                    message = message == null ? mes : message.AddMessage(mes);
                }
            }

            return message;
        }
    }

    public class SubscribeParams
    {
        public string Username { get; }
        public int Months { get; }

        public SubscribeParams(string username, int months)
        {
            Username = username;
            Months = months;
        }
    }
}