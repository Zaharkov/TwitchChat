using System.Threading;
using System.Threading.Tasks;
using CommonHelper;
using Domain.Repositories;
using TwitchChat.Code.Quiz;
using TwitchChat.Controls;
using TwitchChat.Texts;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class QiuzCommand
    {
        private static readonly int Delay = int.Parse(Configuration.GetSetting("QuizRestartDelay"));
        private static readonly Texts.Entities.Quiz Texts = TextsHolder.Texts.Quiz;

        public static SendMessage Start(ChatMemberViewModel userModel)
        {
            QuizHolder.StartNewQuiz(userModel.Channel);
            return SendMessage.None;
        }

        public static SendMessage Stop(ChatMemberViewModel userModel)
        {
            QuizHolder.StopQuiz(userModel.Channel);
            return SendMessage.None;
        }

        public static SendMessage Question()
        {
            if (QuizHolder.IsQuizActive && QuizHolder.Question.HasValue)
            {
                var message = string.Format(Texts.Question, QuizHolder.Question.Value.Key, QuizHolder.GetAnswer(QuizHolder.Question.Value.Value), Command.О);
                return SendMessage.GetMessage(message);
            }
            return SendMessage.GetMessage(Texts.Off);
        }

        public static SendMessage Score(ChatMemberViewModel userModel)
        {
            var score = ChatterInfoRepository.Instance.GetQuizScore(userModel.Name, userModel.Channel.ChannelName);
            var message = string.Format(Texts.Score, string.Format(Texts.Answers, score.Key, GetName(score.Key)), score.Value);
            return SendMessage.GetMessage(message);
        }

        public static SendMessage Answer(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (!QuizHolder.IsQuizActive || !QuizHolder.Question.HasValue)
                return SendMessage.None;

            var answerUser = e.Message.ToUpper().Replace($"{TwitchConstName.Command}{Command.О}".ToUpper(), "").Trim();
            var answerQuiz = QuizHolder.Question.Value.Value.ToUpper();

            if (answerQuiz == answerUser)
            {
                QuizHolder.StopQuiz(userModel.Channel);
                ChatterInfoRepository.Instance.AddQuizScore(userModel.Name, e.Channel);

                Task.Run(() =>
                {
                    Thread.Sleep(Delay * 1000);
                    QuizHolder.StartNewQuiz(userModel.Channel);
                });

                return SendMessage.GetMessage(string.Format(Texts.RightAnswer, Delay));
            }

            return SendMessage.None;
        }

        private static string GetName(int seconds)
        {
            if (seconds % 10 == 1)
                return "вопрос";

            if (seconds % 10 != 2 && seconds % 10 != 3 && seconds % 10 != 4)
                return "вопросов";

            if (seconds % 100 > 11 && seconds % 100 < 15)
                return "вопросов";

            return "вопроса";
        }
    }
}
