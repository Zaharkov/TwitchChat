using System;
using System.Linq;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Controls;
using Configuration;
using Configuration.Entities;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public class CommandExecution
    {
        private static readonly GlobalTexts Texts = ConfigHolder.Configs.Global.Texts;

        public static SendMessage ExecuteCommand(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var command = e.Message.TrimStart(TwitchConstName.Command).Split(' ').First();

            CommandHandler commandHandler;

            if(!CommandHandler.TryGet(command, out commandHandler))
                return SendMessage.None;

            if (!commandHandler.IsHaveAccess(e))
                return SendMessage.None;

            Func<SendMessage> commandFunc = () => commandHandler.Handler(e, userModel);
            var delayType = commandHandler.DelayType;

            IDelayDecorator delayDecorator;
            switch (delayType)
            {
                case DelayType.User:
                    delayDecorator = UserDecorator.Get(e.Username, commandHandler);
                    break;
                case DelayType.Global:
                    delayDecorator = GlobalDecorator.Get(commandHandler);
                    break;
                case DelayType.Hybrid:
                    delayDecorator = HybridDecorator.Get(e.Username, commandHandler);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(delayType), delayType, null);
            }

            if (e.UserType.HasFlag(UserType.Broadcaster))
                return commandFunc();

            int needWait;
            if (!delayDecorator.CanExecute(out needWait))
            {
                var message = string.Format(Texts.Cooldown, command, delayType != DelayType.Global ? Texts.UserCd : Texts.GlobalCd, string.Format(Texts.Seconds, needWait, MyTimeCommand.GetSecondsName(needWait)));
                return SendMessage.GetWhisper(message);
            }

            return delayDecorator.Execute(() => commandFunc());
        }
    }
}
