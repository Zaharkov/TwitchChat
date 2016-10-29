using TwitchChat.Texts;
using TwitchChat.Texts.Entities;

namespace TwitchChat.Code.Commands
{
    public static class SheikerCommand
    {
        private static readonly Sheiker Texts = TextsHolder.Texts.Sheiker;

        public static SendMessage GetSheiker()
        {
            return SendMessage.GetMessage(Texts.Hei);
        }
    }
}
