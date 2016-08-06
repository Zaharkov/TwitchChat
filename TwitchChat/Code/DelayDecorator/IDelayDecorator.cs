using System;

namespace TwitchChat.Code.DelayDecorator
{
    public interface IDelayDecorator
    {
        bool CanExecute(out int needWait);
        string Execute(Func<string> func);
    }
}
