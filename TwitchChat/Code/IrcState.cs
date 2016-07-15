namespace Twitchiedll.IRC
{
    public enum IrcState
    {
        Closed,
        Connecting,
        Connected,
        Disconnected,
        Registering,
        Registered,
        Closing,
        Reconnecting,
        Error
    }
}
