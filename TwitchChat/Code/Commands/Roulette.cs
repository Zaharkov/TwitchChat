using System;
using System.Text;
using CommonHelper;
using Domain.Repositories;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class RouletteCommand
    {
        private static readonly int RouletteTimeout = int.Parse(Configuration.GetSetting("RouletteTimeout"));
        private static readonly int RouletteTop = int.Parse(Configuration.GetSetting("RouletteTop"));

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

            RouletteInfoRepository.Instance.AddTry(rouletteId);

            if (firstNumber == 1)
            {
                var secondNumber = rnd.Next(1, 20);

                if (secondNumber == 1)
                {
                    RouletteInfoRepository.Instance.ResetCurrentTry(rouletteId);
                    RouletteInfoRepository.Instance.AddPercent(rouletteId, 0.05);
                    return SendMessage.GetMessage("осечка! невероятное везение...крутанем барабан ещё раз!");
                }

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

        public static SendMessage RouletteGetTop()
        {
            var top = RouletteInfoRepository.Instance.GetTop(RouletteTop);

            var builder = new StringBuilder();
            builder.Append("топ везунчиков:");

            for (var i = 1; i <= top.Count; i++)
            {
                var info = top[i-1];
                var chatter = ChatterInfoRepository.Instance.GetByRouletteId(info.Id);

                if (chatter != null)
                    builder.Append($" {i}. @{chatter.Name} - {info.MaxPercent.ToString("0.####%")}({info.MaxStreak})");
            }

            return SendMessage.GetMessage(builder.ToString());
        }
    }
}
