using System.Threading;
using System.Threading.Tasks;
using CommonHelper;
using Domain.Repositories;
using TwitchChat.Code.Quiz;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class QiuzCommand
    {
        private static readonly int Delay = int.Parse(Configuration.GetSetting("QuizRestartDelay"));

        public static string Start(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            
            QuizHolder.StartNewQuiz(userModel.Channel);
            return null;
        }

        public static string Stop(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            QuizHolder.StopQuiz(userModel.Channel);
            return null;
        }

        public static string Question(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (QuizHolder.IsQuizActive && QuizHolder.Question.HasValue)
                return $"вопрос викторины : {QuizHolder.Question.Value.Key}";

            return "сейчас викторина выключена";
        }

        public static string Score(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var score = ChatterInfoRepository.Instance.GetQuizScore(userModel.Name, e.Channel);

            return $"ты ответил(а) на {score.Key} {GetName(score.Key)}. Позиция в рейтинге - {score.Value}";
        }

        public static string Answer(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (!QuizHolder.IsQuizActive || !QuizHolder.Question.HasValue)
                return null;

            var answerUser = e.Message.ToUpper().Replace($"!{Command.О}".ToUpper(), "").Trim();
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

                return $"красавчег! Правильный ответ! Перезапуск викторины через {Delay} секунд";
            }

            return null;
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
