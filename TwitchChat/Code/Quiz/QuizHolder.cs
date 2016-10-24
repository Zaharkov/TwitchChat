using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonHelper;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;
using TwitchChat.Controls;

namespace TwitchChat.Code.Quiz
{
    public static class QuizHolder
    {
        private static readonly Dictionary<ChannelViewModel, CancellationTokenSource> TokenSources = new Dictionary<ChannelViewModel, CancellationTokenSource>();
        private static readonly int Delay = int.Parse(Configuration.GetSetting("QuizDelay"));

        public static bool IsQuizActive { get; private set; }
        public static KeyValuePair<string, string>? Question { get; private set; }

        public static string GetQuestionText()
        {
            if (!IsQuizActive || !Question.HasValue)
                return null;

            var answer = Question.Value.Value;
            var answerStars = string.Join(" ", answer.Split(' ').Select(GetWordWithStars));

            return $"Вопрос викторины: {Question.Value.Key}. Подсказка: {answerStars}. Для ответа введите: !{Command.О} ответ.";
        }

        private static string GetWordWithStars(string word)
        {
            var first = word.First().ToString();
            var last = word.Last().ToString();

            var wordWithStars = first;

            for (var i = 0; i < word.Length - 2; i++)
                wordWithStars += "*";

            wordWithStars += last;

            return wordWithStars;
        }

        public static void StartNewQuiz(ChannelViewModel channelView)
        {
            StopQuiz(channelView);

            IsQuizActive = true;
            Question = QuizCollection.GetNew();

            Action action = () =>
            {
                var text = GetQuestionText();

                if(!string.IsNullOrEmpty(text))
                    channelView.Client.Message(channelView.ChannelName, text);
            };

            var cancelToken = TimerFactory.Start(channelView, action, Delay*1000);
            TokenSources.Add(channelView, cancelToken);
        }

        public static void StopQuiz(ChannelViewModel channelView)
        {
            if (TokenSources.ContainsKey(channelView))
            {
                TimerFactory.Stop(channelView, TokenSources[channelView]);
                TokenSources.Remove(channelView);
            }

            IsQuizActive = false;
            Question = null;
        }
    }
}
