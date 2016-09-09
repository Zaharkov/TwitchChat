using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TwitchChat.Code.Quiz
{
    public static class QuizCollection
    {
        private static readonly Random Rnd = new Random();
        private static readonly Dictionary<string, string> Questions = new Dictionary<string, string>();

        static QuizCollection()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "TwitchChat.Code.Quiz.questions.txt";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                    throw new NullReferenceException();

                using (var reader = new StreamReader(stream))
                {
                    var questions = reader.ReadToEnd().Split('\n');

                    foreach (var question in questions)
                    {
                        var q = question.Replace("\r", "").Split('*');

                        if (q.Length < 2)
                            continue;

                        if (Questions.ContainsKey(q[0]))
                            continue;

                        Questions.Add(q[0], q[1]);
                    }
                }
            } 
        }

        public static KeyValuePair<string, string> GetNew()
        {
            var value = Questions.ElementAt(Rnd.Next(0, Questions.Count));
            Questions.Remove(value.Key);

            return value;
        }
    }
}
