using System;
using Domain.Repositories;
using Twitchiedll.IRC.Interfaces;

namespace TwitchChat.Code
{
    public class Logger : ILogger
    {
        public void LogException(string message, Exception e)
        {
            LogRepository.Instance.LogException(message, e);
        }
    }
}
