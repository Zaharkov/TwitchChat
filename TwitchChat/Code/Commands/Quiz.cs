using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using TwitchChat.Controls;
using Configuration;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class QiuzCommand
    {
        private static readonly Configuration.Entities.Quiz Quiz = ConfigHolder.Configs.Quiz;

        public static SendMessage Start(ChatMemberViewModel userModel)
        {
            userModel.Channel.QuizHolder.StartNewQuiz();
            return SendMessage.None;
        }

        public static SendMessage Stop(ChatMemberViewModel userModel)
        {
            userModel.Channel.QuizHolder.StopQuiz();
            return SendMessage.None;
        }

        public static SendMessage Question(ChatMemberViewModel userModel)
        {
            var quiz = userModel.Channel.QuizHolder;

            if (quiz.IsQuizActive && quiz.Question.HasValue)
            {
                var message = string.Format(Quiz.Texts.Question, quiz.Question.Value.Key, quiz.GetAnswer(quiz.Question.Value.Value), Command.О);
                return SendMessage.GetMessage(message);
            }
            return SendMessage.GetMessage(Quiz.Texts.Off);
        }

        public static SendMessage Score(ChatMemberViewModel userModel)
        {
            var score = ChatterInfoRepository.Instance.GetQuizScore(userModel.Name, userModel.Channel.ChannelName);
            var message = string.Format(Quiz.Texts.Score, string.Format(Quiz.Texts.Answers, score.Key, GetName(score.Key)), score.Value);
            return SendMessage.GetMessage(message);
        }

        public static SendMessage Answer(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var quiz = userModel.Channel.QuizHolder;

            if (!quiz.IsQuizActive || !quiz.Question.HasValue)
                return SendMessage.None;

            var answerUser = e.Message.ToUpper().Replace($"{TwitchConstName.Command}{Command.О}".ToUpper(), "").Trim();
            var answerQuiz = quiz.Question.Value.Value.ToUpper();

            if (answerQuiz == answerUser)
            {
                quiz.StopQuiz();
                ChatterInfoRepository.Instance.AddQuizScore(userModel.Name, e.Channel);

                Task.Run(() =>
                {
                    Thread.Sleep(Quiz.Params.RestartDelay * 1000);
                    quiz.StartNewQuiz();
                });

                return SendMessage.GetMessage(string.Format(Quiz.Texts.RightAnswer, Quiz.Params.RestartDelay));
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
