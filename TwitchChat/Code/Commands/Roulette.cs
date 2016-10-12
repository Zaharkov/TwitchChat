using System;
using CommonHelper;
using Domain.Repositories;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class RouletteCommand
    {
        private static readonly int RouletteTimeout = int.Parse(Configuration.GetSetting("RouletteTimeout"));

        public static SendMessage RouletteTry(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage("бессмертного пони нельзя убить!");

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage("ты это...меч выкинь...не нужен он тебе");

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var currentTry = RouletteInfoRepository.Instance.Get(rouletteId).CurrentTry;

            var rnd = new Random();
            var firstNumber = rnd.Next(1, 6 - currentTry);
            var percent = 1 - 1 / (6.0 - currentTry);
            currentTry++;

            RouletteInfoRepository.Instance.AddCurrentTry(rouletteId);
            RouletteInfoRepository.Instance.AddTry(rouletteId);
            RouletteInfoRepository.Instance.AddStreak(rouletteId);

            if (firstNumber == 1)
            {
                RouletteInfoRepository.Instance.ResetCurrentTry(rouletteId);
                var secondNumber = rnd.Next(1, 20);

                if (secondNumber == 1)
                {
                    RouletteInfoRepository.Instance.AddPercent(rouletteId, 0.05);
                    return SendMessage.GetMessage("осечка! невероятное везение...крутанем барабан ещё раз!");
                }

                RouletteInfoRepository.Instance.ResetStreak(rouletteId);
                RouletteInfoRepository.Instance.ResetPercent(rouletteId);
                RouletteInfoRepository.Instance.AddDeath(rouletteId);

                return SendMessage.GetTimeout(e.Username, RouletteTimeout).AddMessage(SendMessage.GetMessage($"пристрелил(а) себя на {currentTry} раз! Помянем napoCry"));
            }

            RouletteInfoRepository.Instance.AddPercent(rouletteId, percent);

            return SendMessage.GetMessage($"на {currentTry} раз тебе повезло! Посмотрим, повезет ли в следующий...");
        }

        public static SendMessage RouletteInfo(MessageEventArgs e)
        {
            if (e.UserType.HasFlag(UserType.Broadcaster) || e.UserType.HasFlag(UserType.Admin) ||
                e.UserType.HasFlag(UserType.Globalmoderator) || e.UserType.HasFlag(UserType.Staff))
                return SendMessage.GetMessage("бессмертного пони нельзя убить!");

            if (e.UserType.HasFlag(UserType.Moderator))
                return SendMessage.GetMessage("ты это...меч выкинь...не нужен он тебе");

            var rouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var roulette = RouletteInfoRepository.Instance.Get(rouletteId);

            var message = $"Всего попыток: {roulette.TryCount}. Смертей: {roulette.DeathCount}. Макс стрик: {roulette.MaxStreak} ({roulette.MaxPercent.ToString("0.####%")}). Текущий стрик: {roulette.Streak} ({roulette.Percent.ToString("0.####%")})";

            return SendMessage.GetMessage(message);
        }
    }
}
