using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TwitchChat.Code.Commands;
using TwitchChat.Code.Timers;
using TwitchChat.Controls;
using Configuration;

namespace TwitchChat.Code.Vote
{
    public class VoteHolder
    {
        private readonly ChannelViewModel _channel;
        private CancellationTokenSource _stopTokenSource;
        private CancellationTokenSource _timerTokenSource;
        private static readonly Configuration.Entities.Vote Vote = ConfigHolder.Configs.Vote;

        public bool IsVoteActive { get; private set; }
        public Dictionary<string, int> Votes { get; private set; }
        public List<string> Voted { get; private set; }
        public string Question { get; private set; }

        public string LastResults { get; private set; }

        public VoteHolder(ChannelViewModel channelView)
        {
            _channel = channelView;
        }

        public string GetVoteText()
        {
            if (!IsVoteActive)
                return null;

            var text = "";
            for (var i = 0; i < Votes.Keys.Count; i++)
                text += $"{Votes.Keys.ToArray()[i]}{(i == Votes.Keys.Count - 1 ? $" ({i+1})" : $" ({i+1}),")}";

            return string.Format(Vote.Question, Question, text, Command.Голос);
        }

        public void StartNewVote(string question, List<string> votes, int seconds)
        {
            StopVote();

            IsVoteActive = true;
            Question = question;
            Votes = votes.ToDictionary(k => k, v => 0);
            Voted = new List<string>();

            Action action = () =>
            {
                var text = GetVoteText();

                if(!string.IsNullOrEmpty(text))
                    _channel.SendMessage(null, SendMessage.GetMessage(text));
            };

            _stopTokenSource = new CancellationTokenSource();

            Action sleep = () =>
            {
                Thread.Sleep(seconds * 1000);
            };

            var wait = TimerFactory.RunOnce(_channel, sleep);
            wait.ContinueWith(task => StopVote(), _stopTokenSource.Token);

            var cancelToken = TimerFactory.Start(_channel, action, Vote.Delay * 1000);
            _timerTokenSource = cancelToken;
        }

        public void StopVote()
        {
            if (IsVoteActive)
            {
                var votes = string.Join(",", Votes.Select(t => $"{t.Key}:{t.Value}"));
                var text = string.Format(Vote.Result, Question, votes);
                LastResults = text;
                _channel.SendMessage(null, SendMessage.GetMessage(text));
            }

            if (_timerTokenSource != null && !_timerTokenSource.IsCancellationRequested)
            {
                TimerFactory.Stop(_channel, _timerTokenSource);
                _timerTokenSource = null;
            }

            IsVoteActive = false;
            Question = null;
            Votes = null;
            Voted = null;

            if (_stopTokenSource != null && !_stopTokenSource.IsCancellationRequested)
            {
                _stopTokenSource.Cancel();
                _stopTokenSource.Dispose();
                _stopTokenSource = null;
            }
        }
    }
}
