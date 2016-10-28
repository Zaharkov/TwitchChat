using System;
using TwitchChat.Code.Commands;

namespace TwitchChat.Code.DelayDecorator
{
    public interface IDelayDecorator
    {
        bool CanExecute(out int needWait);
        SendMessage Execute(Func<SendMessage> func, bool needExec = true);
        void RestartTimer();
    }
}
