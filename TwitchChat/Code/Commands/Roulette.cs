using System;
using System.Text;
using Domain.Repositories;
using Configuration;
using Configuration.Entities;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class RouletteCommand
    {
        private static readonly Roulette Roulette = ConfigHolder.Configs.Roulette;

        public static SendMessage RouletteTry(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage(Roulette.Texts.Admin);

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage(Roulette.Texts.Moder);

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var currentTry = RouletteInfoRepository.Instance.Get(rouletteId).CurrentTry;

            var rnd = new Random();
            var firstNumber = rnd.Next(1, 7 - currentTry);
            var percent = 1 - 1 / (6.0 - currentTry);
            currentTry++;

            RouletteInfoRepository.Instance.AddTry(rouletteId);

            if (firstNumber == 1)
            {
                var secondNumber = rnd.Next(1, 21);

                if (secondNumber == 1)
                {
                    RouletteInfoRepository.Instance.ResetCurrentTry(rouletteId);
                    RouletteInfoRepository.Instance.AddPercent(rouletteId, 0.05);
                    return SendMessage.GetMessage(Roulette.Texts.Misfire);
                }

                RouletteInfoRepository.Instance.AddDeath(rouletteId);
                var message = SendMessage.GetMessage(string.Format(Roulette.Texts.Death, currentTry));
                return SendMessage.GetTimeout(e.Username, Roulette.Params.Timeout).AddMessage(message);
            }

            RouletteInfoRepository.Instance.AddPercent(rouletteId, percent);

            return SendMessage.GetMessage(string.Format(Roulette.Texts.Luck, currentTry));
        }

        public static SendMessage RouletteInfo(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage(Roulette.Texts.Admin);

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage(Roulette.Texts.Moder);

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var roulette = RouletteInfoRepository.Instance.Get(rouletteId);

            var message = string.Format(Roulette.Texts.Stats, roulette.TryCount, roulette.DeathCount, roulette.MaxStreak, roulette.MaxPercent.ToString("0.####%"), roulette.Streak, roulette.Percent.ToString("0.####%"), roulette.DuelScore);
            return SendMessage.GetMessage(message);
        }

        public static SendMessage RouletteGetTop()
        {
            var top = RouletteInfoRepository.Instance.GetTop(Roulette.Params.Top);

            var builder = new StringBuilder();
            builder.Append(Roulette.Texts.TopStart);

            for (var i = 1; i <= top.Count; i++)
            {
                var info = top[i-1];
                var chatter = ChatterInfoRepository.Instance.GetByRouletteId(info.Id);

                if (chatter != null)
                {
                    var message = string.Format(Roulette.Texts.TopUser, i, chatter.Name, info.MaxPercent.ToString("0.####%"), info.MaxStreak);
                    builder.Append(message);
                }
            }

            return SendMessage.GetMessage(builder.ToString());
        }
    }
}
