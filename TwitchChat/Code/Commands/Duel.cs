using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CommonHelper;
using Domain.Repositories;
using TwitchApi.Entities;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Code.Timers;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class DuelCommand
    {
        private static readonly int RouletteTimeout = int.Parse(Configuration.GetSetting("RouletteTimeout"));
        private static readonly int DuelWait = int.Parse(Configuration.GetSetting("DuelWait"));
        private static readonly int DuelDelay = int.Parse(Configuration.GetSetting("DuelDelay"));
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> RemoveDuelNameTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static SendMessage Duel(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var sourceRouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var sourceduelName = RouletteInfoRepository.Instance.Get(sourceRouletteId).DuelName;

            if (!string.IsNullOrEmpty(sourceduelName))
                return SendMessage.GetWhisper($"Вы уже назначили дуель c {sourceduelName}");

            var split = e.Message.Split(' ');

            if(split.Length < 2)
                return SendMessage.GetWhisper($"Некорректный синтаксис. Введите '!{Command.Дуэль} %TwitchName%. Где %TwitchName% имя пользователя twitch");

            var nameForDuel = split[1];

            if (nameForDuel.StartsWith("@"))
                nameForDuel = nameForDuel.TrimStart('@');

            var allChatters = userModel.Channel.GetAllChatters();

            if(!allChatters.Any(t => t.Name.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase)))
                return SendMessage.GetWhisper($"Юзера {nameForDuel} сейчас нет в чате");

            if(e.Username.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetMessage($"сам себя можешь пристрелить через !{Command.Рулетка}");

            var targetRouletteId = ChatterInfoRepository.Instance.GetRouletteId(nameForDuel, e.Channel);
            var targetduelName = RouletteInfoRepository.Instance.Get(targetRouletteId).DuelName;

            if (!string.IsNullOrEmpty(targetduelName))
            {
                if(e.Username.Equals(targetduelName, StringComparison.InvariantCultureIgnoreCase))
                    return SendMessage.GetWhisper($"{nameForDuel} уже назначил дуель с вами. Введите !{Command.Принять} {nameForDuel} чтобы принять её");

                return SendMessage.GetWhisper($"{nameForDuel} уже назначил дуель с {targetduelName}");
            }

            RouletteInfoRepository.Instance.SetDuelName(sourceRouletteId, nameForDuel);
            var tokenSource = new CancellationTokenSource();
            RemoveDuelNameTokens.AddOrUpdate(e.Username, tokenSource, (k, v) => tokenSource);

            Action sleep = () =>
            {
                Thread.Sleep(DuelWait * 1000);
            };

            Action continueWith = () =>
            {
                RouletteInfoRepository.Instance.RemoveDuelName(sourceRouletteId);
                var message = SendMessage.GetMessage($"похоже @{nameForDuel} струсил(а)! Или ему(ей) на тебя плевать napoUhadi");
                userModel.Channel.SendMessage(e, message);

            };

            var wait = TimerFactory.RunOnce(userModel.Channel, sleep);
            wait.ContinueWith(task => continueWith(), tokenSource.Token);

            return SendMessage.GetMessage($"вызвал @{nameForDuel} на дуель! У @{nameForDuel} есть {DuelWait} секунд чтобы принять дуель (!{Command.Принять} {e.Username}). Прогресс !{Command.Рулетка} будет сброшен для обоих");
        }

        public static SendMessage DuelStart(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var sourceRouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var sourceduelName = RouletteInfoRepository.Instance.Get(sourceRouletteId).DuelName;

            if (!string.IsNullOrEmpty(sourceduelName))
                return SendMessage.GetWhisper($"Вы уже назначили дуель c {sourceduelName}");

            var split = e.Message.Split(' ');

            if (split.Length < 2)
                return SendMessage.GetWhisper($"Некорректный синтаксис. Введите '!{Command.Принять} %TwitchName%. Где %TwitchName% имя пользователя twitch");

            var nameForDuel = split[1];

            if (nameForDuel.StartsWith("@"))
                nameForDuel = nameForDuel.TrimStart('@');

            var allChatters = userModel.Channel.GetAllChatters();

            if (!allChatters.Any(t => t.Name.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase)))
                return SendMessage.GetWhisper($"Юзера {nameForDuel} сейчас нет в чате");

            if (e.Username.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetMessage($"сам себя можешь пристрелить через !{Command.Рулетка}");

            var targetRouletteId = ChatterInfoRepository.Instance.GetRouletteId(nameForDuel, e.Channel);
            var targetduelName = RouletteInfoRepository.Instance.Get(targetRouletteId).DuelName;

            if (!string.IsNullOrEmpty(targetduelName) && !e.Username.Equals(targetduelName, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetWhisper($"{nameForDuel} уже назначил дуель с {targetduelName}");

            RouletteInfoRepository.Instance.SetDuelName(sourceRouletteId, nameForDuel);
            RouletteInfoRepository.Instance.Reset(sourceRouletteId);
            RouletteInfoRepository.Instance.Reset(targetRouletteId);

            if (RemoveDuelNameTokens.ContainsKey(nameForDuel))
                RemoveDuelNameTokens[nameForDuel].Cancel();

            Action action = () =>
            {
                WaitAndDefaultTimers();

                var moderators = userModel.Channel.GetGroup(ChatterType.Moderators);

                if (e.Username.Equals(e.Channel, StringComparison.InvariantCultureIgnoreCase) || 
                    nameForDuel.Equals(e.Channel, StringComparison.InvariantCultureIgnoreCase))
                {
                    var user = e.Username.Equals(e.Channel) ? nameForDuel : e.Username;

                    if (moderators.Any(t => t.Name.Equals(user, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var mes = SendMessage.GetMessage($"@{e.Channel}, кажется @{user} хочет остаться без меча...");
                        userModel.Channel.SendMessage(null, mes);
                        return;
                    }

                    var timeout = SendMessage.GetTimeout(user, RouletteTimeout);
                    var message = SendMessage.GetMessage($"@{e.Channel} достает свой миниган и превращает @{user} в решето...бедняжка...ни единого шанса");
                    timeout.AddMessage(message);
                    userModel.Channel.SendMessage(null, timeout);
                    return;
                }

                if (moderators.Any(t => t.Name.Equals(e.Username, StringComparison.InvariantCultureIgnoreCase)) ||
                    moderators.Any(t => t.Name.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var user = moderators.Any(t => t.Name.Equals(e.Username, StringComparison.InvariantCultureIgnoreCase)) ? nameForDuel : e.Username;
                    var moder = user == nameForDuel ? e.Username : nameForDuel;

                    if (moderators.Any(t => t.Name.Equals(user, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var mes = SendMessage.GetMessage($"@{e.Username} и @{nameForDuel} решили усторили дуель на мечах...удачи им");
                        userModel.Channel.SendMessage(null, mes);
                        return;
                    }

                    var timeout = SendMessage.GetTimeout(user, RouletteTimeout);
                    var message = SendMessage.GetMessage($"@{moder} одним взмахом меча отрубает @{user} голову...без шансов");
                    timeout.AddMessage(message);
                    userModel.Channel.SendMessage(null, timeout);
                    return;
                }

                var context = RouletteInfoRepository.Instance;

                while (true)
                {
                    var first = Roulette(context, targetRouletteId, e.Username, nameForDuel);

                    if (first.Type == SendType.Timeout)
                    {
                        context.AddDuelScore(sourceRouletteId);

                        var win = SendMessage.GetMessage($"@{e.Username} побеждает в дуели с @{nameForDuel}");
                        first.NextMessage.AddMessage(win);
                        userModel.Channel.SendMessage(null, first);
                        return;
                    }

                    userModel.Channel.SendMessage(null, first);

                    WaitAndDefaultTimers();

                    var second = Roulette(context, sourceRouletteId, nameForDuel, e.Username);

                    if (second.Type == SendType.Timeout)
                    {
                        context.AddDuelScore(targetRouletteId);

                        var win = SendMessage.GetMessage($"@{nameForDuel} побеждает в дуели с @{e.Username}");
                        second.NextMessage.AddMessage(win);
                        userModel.Channel.SendMessage(null, second);
                        return;
                    }

                    userModel.Channel.SendMessage(null, second);

                    WaitAndDefaultTimers();
                }
            };

            Action continueWith = () =>
            {
                RouletteInfoRepository.Instance.RemoveDuelName(sourceRouletteId);
                RouletteInfoRepository.Instance.RemoveDuelName(targetRouletteId);
            };

            var wait = TimerFactory.RunOnce(userModel.Channel, action);
            wait.ContinueWith(task => continueWith());

            return SendMessage.GetMessage($"принял(а) дуель от @{nameForDuel}. Теперь дороги назад нет...начнем! Первый(ая) стреляет @{nameForDuel}");
        }

        private static void WaitAndDefaultTimers()
        {
            GlobalDecorator.Get(Command.Принять).RestartTimer();
            GlobalDecorator.Get(Command.Дуэль).RestartTimer();
            Thread.Sleep(DuelDelay * 1000);
        }

        private static SendMessage Roulette(RouletteInfoRepository context, long rouletteId, string source, string target)
        {
            var currentTry = context.Get(rouletteId).CurrentTry;

            var rnd = new Random();
            var firstNumber = rnd.Next(1, 6 - currentTry);
            var percent = 1 - 1 / (6.0 - currentTry);
            currentTry++;

            context.AddTry(rouletteId);

            if (firstNumber == 1)
            {
                var secondNumber = rnd.Next(1, 20);

                if (secondNumber == 1)
                {
                    context.ResetCurrentTry(rouletteId);
                    context.AddPercent(rouletteId, 0.05);
                    return SendMessage.GetMessage($"У @{source} осечка! невероятное везение для @{target}...крутанем барабан ещё раз!");
                }

                context.AddDeath(rouletteId);
                return SendMessage.GetTimeout(target, RouletteTimeout).AddMessage(SendMessage.GetMessage($"@{source} пристрелил(а) @{target} на {currentTry} раз! Помянем napoCry"));
            }

            context.AddPercent(rouletteId, percent);

            return SendMessage.GetMessage($"На {currentTry} раз тебе повезло @{target}! Теперь очередь {source} быть мишенью...");
        }
    }
}