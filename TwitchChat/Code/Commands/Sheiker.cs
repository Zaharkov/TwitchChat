using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class SheikerCommand
    {
        private static readonly Sheiker Texts = ConfigHolder.Configs.Sheiker;

        public static SendMessage GetSheiker()
        {
            return SendMessage.GetMessage(Texts.Hei);
        }
    }
}
