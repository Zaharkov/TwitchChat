using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchChat.Code.Quiz
{
    public static class QuizCollection
    {
        private static readonly Dictionary<string, string> Questions = new Dictionary<string, string>();

        static QuizCollection()
        {
            var lines = File.ReadAllLines("Code\\Quiz\\questions.txt");

            foreach (var question in lines)
            {
                var q = question.Split('*');

                if (q.Length < 2)
                    continue;

                if (Questions.ContainsKey(q[0]))
                    continue;

                Questions.Add(q[0], q[1]);
            }
        }

        public static KeyValuePair<string, string> GetNew()
        {
            var random = new Random();
            var value = Questions.ElementAt(random.Next(0, Questions.Count));
            Questions.Remove(value.Key);

            return value;
        }
    }
}
