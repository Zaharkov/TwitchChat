﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CommonHelper;
using Domain.Repositories;
using TwitchApi.Entities;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Code.Timers;
using TwitchChat.Controls;
using TwitchChat.Texts;
using TwitchChat.Texts.Entities;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class DuelCommand
    {
        private static readonly Duel Texts = TextsHolder.Texts.Duel;
        private static readonly int RouletteTimeout = int.Parse(Configuration.GetSetting("RouletteTimeout"));
        private static readonly int DuelWait = int.Parse(Configuration.GetSetting("DuelWait"));
        private static readonly int DuelDelay = int.Parse(Configuration.GetSetting("DuelDelay"));
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> RemoveDuelNameTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static SendMessage Duel(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var sourceRouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var sourceduelName = RouletteInfoRepository.Instance.Get(sourceRouletteId).DuelName;

            if (!string.IsNullOrEmpty(sourceduelName))
                return SendMessage.GetWhisper(string.Format(Texts.AlreadyInDuel, sourceduelName));

            var split = e.Message.Split(' ');

            if(split.Length < 2)
                return SendMessage.GetWhisper(string.Format(Texts.IncorrectSyntax, Command.Дуэль));

            var nameForDuel = split[1];

            if (nameForDuel.StartsWith(TwitchConstName.UserStartName.ToString()))
                nameForDuel = nameForDuel.TrimStart(TwitchConstName.UserStartName);

            var allChatters = userModel.Channel.GetAllChatters();

            if (!allChatters.Any(t => t.Name.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase)))
                return SendMessage.GetWhisper(string.Format(Texts.NoInChat, nameForDuel));

            if(e.Username.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetMessage(string.Format(Texts.Roulette, Command.Рулетка));

            var targetRouletteId = ChatterInfoRepository.Instance.GetRouletteId(nameForDuel, e.Channel);
            var targetduelName = RouletteInfoRepository.Instance.Get(targetRouletteId).DuelName;

            if (!string.IsNullOrEmpty(targetduelName))
            {
                if(e.Username.Equals(targetduelName, StringComparison.InvariantCultureIgnoreCase))
                    return SendMessage.GetWhisper(string.Format(Texts.NeedConfirm, nameForDuel, Command.Принять));

                return SendMessage.GetWhisper(string.Format(Texts.TargetAlreadyInDuel, nameForDuel, targetduelName));
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
                var message = SendMessage.GetMessage(string.Format(Texts.NoConfirm, nameForDuel));
                userModel.Channel.SendMessage(e, message);

            };

            var wait = TimerFactory.RunOnce(userModel.Channel, sleep);
            wait.ContinueWith(task => continueWith(), tokenSource.Token);

            return SendMessage.GetMessage(string.Format(Texts.Start, nameForDuel, DuelWait, Command.Принять, e.Username, Command.Рулетка));
        }

        public static SendMessage DuelStart(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var sourceRouletteId = ChatterInfoRepository.Instance.GetRouletteId(e.Username, e.Channel);
            var sourceduelName = RouletteInfoRepository.Instance.Get(sourceRouletteId).DuelName;

            if (!string.IsNullOrEmpty(sourceduelName))
                return SendMessage.GetWhisper(string.Format(Texts.AlreadyInDuel, sourceduelName));

            var split = e.Message.Split(' ');

            if (split.Length < 2)
                return SendMessage.GetWhisper(string.Format(Texts.IncorrectSyntaxStart, Command.Принять));

            var nameForDuel = split[1];

            if (nameForDuel.StartsWith(TwitchConstName.UserStartName.ToString()))
                nameForDuel = nameForDuel.TrimStart(TwitchConstName.UserStartName);

            var allChatters = userModel.Channel.GetAllChatters();

            if (!allChatters.Any(t => t.Name.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase)))
                return SendMessage.GetWhisper(string.Format(Texts.NoInChat, nameForDuel));

            if (e.Username.Equals(nameForDuel, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetMessage(string.Format(Texts.Roulette, Command.Рулетка));

            var targetRouletteId = ChatterInfoRepository.Instance.GetRouletteId(nameForDuel, e.Channel);
            var targetduelName = RouletteInfoRepository.Instance.Get(targetRouletteId).DuelName;

            if (!string.IsNullOrEmpty(targetduelName) && !e.Username.Equals(targetduelName, StringComparison.InvariantCultureIgnoreCase))
                return SendMessage.GetWhisper(string.Format(Texts.TargetAlreadyInDuel, nameForDuel, targetduelName));

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
                        var mes = SendMessage.GetMessage(string.Format(Texts.AdminVsModer, e.Channel, user));
                        userModel.Channel.SendMessage(null, mes);
                        return;
                    }

                    var timeout = SendMessage.GetTimeout(user, RouletteTimeout);
                    var message = SendMessage.GetMessage(string.Format(Texts.AdminVsUser, e.Channel, user));
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
                        var mes = SendMessage.GetMessage(string.Format(Texts.ModerVsModer, e.Username, nameForDuel));
                        userModel.Channel.SendMessage(null, mes);
                        return;
                    }

                    var timeout = SendMessage.GetTimeout(user, RouletteTimeout);
                    var message = SendMessage.GetMessage(string.Format(Texts.ModerVsUser, moder, user));
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

                        var win = SendMessage.GetMessage(string.Format(Texts.Win, e.Username, nameForDuel));
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

                        var win = SendMessage.GetMessage(string.Format(Texts.Win, nameForDuel, e.Username));
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

            return SendMessage.GetMessage(string.Format(Texts.Confirm, nameForDuel, e.Username));
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
                    return SendMessage.GetMessage(string.Format(Texts.Misfire, source, target));
                }

                context.AddDeath(rouletteId);
                return SendMessage.GetTimeout(target, RouletteTimeout).AddMessage(SendMessage.GetMessage(string.Format(Texts.Death, source, target, currentTry)));
            }

            context.AddPercent(rouletteId, percent);

            return SendMessage.GetMessage(string.Format(Texts.Luck, currentTry, target, source));
        }
    }
}