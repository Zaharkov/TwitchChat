﻿using Domain.Repositories;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class MyTimeCommand
    {
        public static SendMessage GetMyTime(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var chatTime = userModel.GetTime();
            var dbTime = ChatterInfoRepository.Instance.GetChatterTime(userModel.Name, e.Channel);

            var totalTime = chatTime + dbTime;

            var message = $"слежу за тобой уже {totalTime} {GetSecondsName(totalTime)}";
            return SendMessage.GetMessage(message);
        }

        public static string GetSecondsName(long seconds)
        {
            if (seconds % 100 > 10 && seconds % 100 < 20)
                return "секунд";

            if (seconds % 10 == 1)
                return "секунду";

            if (seconds%10 != 2 && seconds%10 != 3 && seconds%10 != 4)
                return "секунд";

            return "секунды";
        }
    }
}
