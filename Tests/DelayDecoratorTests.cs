using System;
using System.Threading;
using NUnit.Framework;
using TwitchChat.Code.Commands;
using TwitchChat.Code.DelayDecorator;
using TwitchChat.Code.Quiz;

namespace Tests
{
    [TestFixture]
    public class DelayDecoratorTests
    {
        private static void Execute(IDelayDecorator dd, bool started, int? time = null)
        {
            bool result;
            int wait;

            if (dd.CanExecute(out wait))
            {
                dd.Execute(() => SendMessage.GetMessage("started"));
                result = true;
            }
            else
                result = false;

            Assert.AreEqual(result, started);

            if(time.HasValue)
                Assert.AreEqual(wait, time.Value);
        }

        [Test]
        public void GlobalTest()
        {
            var test1 = GlobalDecorator.Get(Command.Музыка);
            var test2 = GlobalDecorator.Get(Command.Музыка);

            Execute(test1, true);

            Thread.Sleep(300);

            Execute(test2, false, 3);

            Thread.Sleep(2800);

            Execute(test2, true);

            Thread.Sleep(1000);

            Execute(test1, false, 2);

            Thread.Sleep(1000);

            Execute(test2, false, 1);

            Thread.Sleep(1000);

            Execute(test1, true);
        }

        [Test]
        public void UserTest()
        {
            var test1 = UserDecorator.Get("test3", Command.ДобавитьСтим);
            var test2 = UserDecorator.Get("test4", Command.ДобавитьСтим);

            Execute(test1, true);

            Thread.Sleep(300);

            Execute(test1, false, 3);

            Execute(test2, true);

            Thread.Sleep(2800);

            Execute(test2, false, 1);

            Thread.Sleep(1000);

            Execute(test1, true);

            Thread.Sleep(1000);

            Execute(test2, true);

            Thread.Sleep(1000);

            Execute(test1, false, 1);
        }

        [Test]
        public void HybridTest()
        {
            var test1 = HybridDecorator.Get("test5", Command.МоеВремя);
            var test2 = HybridDecorator.Get("test6", Command.МоеВремя);
            var test3 = HybridDecorator.Get("test7", Command.МоеВремя);

            Execute(test1, true);

            Thread.Sleep(300);

            Execute(test2, false, 3);
            Execute(test3, false, 3);

            Thread.Sleep(2800);

            Execute(test1, false, 3);
            Execute(test3, true);

            Thread.Sleep(1000);

            Execute(test2, false, 2);

            Thread.Sleep(1000);

            Execute(test3, false, 4);
            Execute(test1, false, 1);

            Thread.Sleep(1000);

            Execute(test1, true);
        }

        [Test]
        public void Test3()
        {
            Console.WriteLine(QuizHolder.GetQuestionText());
            
        }
    }
}
