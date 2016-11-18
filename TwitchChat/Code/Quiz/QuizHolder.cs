using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;
using TwitchChat.Controls;
using Configuration;

namespace TwitchChat.Code.Quiz
{
    public class QuizHolder
    {
        private readonly ChannelViewModel _channel;
        private static readonly Dictionary<ChannelViewModel, CancellationTokenSource> TokenSources = new Dictionary<ChannelViewModel, CancellationTokenSource>();
        private static readonly Configuration.Entities.Quiz Quiz = ConfigHolder.Configs.Quiz;

        public bool IsQuizActive { get; private set; }
        public KeyValuePair<string, string>? Question { get; private set; }

        public QuizHolder(ChannelViewModel channelView)
        {
            _channel = channelView;
        }

        public string GetQuestionText()
        {
            if (!IsQuizActive || !Question.HasValue)
                return null;

            return string.Format(Quiz.Texts.Question, Question.Value.Key, GetAnswer(Question.Value.Value), Command.О);
        }

        public string GetAnswer(string answer)
        {
            var answerStars = string.Join(" ", answer.Split(' ').Select(GetWordWithStars));
            return answerStars;
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

        public void StartNewQuiz()
        {
            StopQuiz();

            IsQuizActive = true;
            Question = QuizCollection.GetNew();

            Action action = () =>
            {
                var text = GetQuestionText();

                if(!string.IsNullOrEmpty(text))
                    _channel.SendMessage(null, SendMessage.GetMessage(text));
            };

            var cancelToken = TimerFactory.Start(_channel, action, Quiz.Params.Delay * 1000);
            TokenSources.Add(_channel, cancelToken);
        }

        public void StopQuiz()
        {
            if (TokenSources.ContainsKey(_channel))
            {
                TimerFactory.Stop(_channel, TokenSources[_channel]);
                TokenSources.Remove(_channel);
            }

            IsQuizActive = false;
            Question = null;
        }
    }
}
