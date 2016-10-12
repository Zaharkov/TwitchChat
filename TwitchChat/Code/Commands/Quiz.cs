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
                return SendMessage.GetMessage($"вопрос викторины : {QuizHolder.Question.Value.Key}");

            return SendMessage.GetMessage("сейчас викторина выключена");
        }

        public static SendMessage Score(ChatMemberViewModel userModel)
        {
            var score = ChatterInfoRepository.Instance.GetQuizScore(userModel.Name, userModel.Channel.ChannelName);

            return SendMessage.GetMessage($"ты ответил(а) на {score.Key} {GetName(score.Key)}. Позиция в рейтинге - {score.Value}");
        }

        public static SendMessage Answer(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            if (!QuizHolder.IsQuizActive || !QuizHolder.Question.HasValue)
                return SendMessage.None;

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

                return SendMessage.GetMessage($"красавчег! Правильный ответ! Перезапуск викторины через {Delay} секунд");
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
