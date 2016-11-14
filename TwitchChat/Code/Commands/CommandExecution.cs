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
            
            if(!CommandAccess.IsExistCommand(command))
                return SendMessage.None;

            if (!CommandAccess.IsHaveAccess(e, command))
                return SendMessage.None;

            var commandFunc = CommandAccess.GetHandler(command, e, userModel);
            var delayType = CommandAccess.GetCommandDelayType(command);

            IDelayDecorator delayDecorator;
            switch (delayType)
            {
                case DelayType.User:
                    delayDecorator = UserDecorator.Get(e.Username, command);
                    break;
                case DelayType.Global:
                    delayDecorator = GlobalDecorator.Get(command);
                    break;
                case DelayType.Hybrid:
                    delayDecorator = HybridDecorator.Get(e.Username, command);
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
