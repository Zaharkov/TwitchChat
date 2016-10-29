using System;
using System.Text;
using CommonHelper;
using Domain.Repositories;
using TwitchChat.Texts;
using TwitchChat.Texts.Entities;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class RouletteCommand
    {
        private static readonly Roulette Texts = TextsHolder.Texts.Roulette;
        private static readonly int RouletteTimeout = int.Parse(Configuration.GetSetting("RouletteTimeout"));
        private static readonly int RouletteTop = int.Parse(Configuration.GetSetting("RouletteTop"));

        public static SendMessage RouletteTry(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage(Texts.Admin);

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage(Texts.Moder);

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var currentTry = RouletteInfoRepository.Instance.Get(rouletteId).CurrentTry;

            var rnd = new Random();
            var firstNumber = rnd.Next(1, 6 - currentTry);
            var percent = 1 - 1 / (6.0 - currentTry);
            currentTry++;

            RouletteInfoRepository.Instance.AddTry(rouletteId);

            if (firstNumber == 1)
            {
                var secondNumber = rnd.Next(1, 20);

                if (secondNumber == 1)
                {
                    RouletteInfoRepository.Instance.ResetCurrentTry(rouletteId);
                    RouletteInfoRepository.Instance.AddPercent(rouletteId, 0.05);
                    return SendMessage.GetMessage(Texts.Misfire);
                }

                RouletteInfoRepository.Instance.AddDeath(rouletteId);
                var message = SendMessage.GetMessage(string.Format(Texts.Death, currentTry));
                return SendMessage.GetTimeout(e.Username, RouletteTimeout).AddMessage(message);
            }

            RouletteInfoRepository.Instance.AddPercent(rouletteId, percent);

            return SendMessage.GetMessage(string.Format(Texts.Luck, currentTry));
        }

        public static SendMessage RouletteInfo(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage(Texts.Admin);

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage(Texts.Moder);

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var roulette = RouletteInfoRepository.Instance.Get(rouletteId);

            var message = string.Format(Texts.Stats, roulette.TryCount, roulette.DeathCount, roulette.MaxStreak, roulette.MaxPercent.ToString("0.####%"), roulette.Streak, roulette.Percent.ToString("0.####%"), roulette.DeathCount);
            return SendMessage.GetMessage(message);
        }

        public static SendMessage RouletteGetTop()
        {
            var top = RouletteInfoRepository.Instance.GetTop(RouletteTop);

            var builder = new StringBuilder();
            builder.Append(Texts.TopStart);

            for (var i = 1; i <= top.Count; i++)
            {
                var info = top[i-1];
                var chatter = ChatterInfoRepository.Instance.GetByRouletteId(info.Id);

                if (chatter != null)
                {
                    var message = string.Format(Texts.TopUser, i, chatter.Name, info.MaxPercent.ToString("0.####%"), info.MaxStreak);
                    builder.Append(message);
                }
            }

            return SendMessage.GetMessage(builder.ToString());
        }
    }
}
