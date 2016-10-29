using TwitchChat.Texts;
using TwitchChat.Texts.Entities;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class OnSubscriberCommand
    {
        private static readonly Subscribe Texts = TextsHolder.Texts.Subscribe;

        public static SendMessage OnSubscriber(SubscriberEventArgs e)
        {
            SendMessage message = null;

            foreach (var monthText in Texts.List)
            {
                if (monthText.Start >= e.Months && monthText.End <= e.Months)
                {
                    var mes = SendMessage.GetMessage(string.Format(monthText.Text, e.Username));

                    message = message == null ? mes : message.AddMessage(mes);
                }
            }

            return message;
        }
    }
}