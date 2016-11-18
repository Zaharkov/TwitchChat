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
        private CancellationTokenSource _tokenSource;
        private static readonly Dictionary<ChannelViewModel, CancellationTokenSource> TokenSources = new Dictionary<ChannelViewModel, CancellationTokenSource>();
        private static readonly Configuration.Entities.Vote Vote = ConfigHolder.Configs.Vote;

        public bool IsVoteActive { get; private set; }
        public Dictionary<string, int> Votes { get; private set; }
        public List<string> Voted { get; private set; }
        public string Question { get; private set; }

        public VoteHolder(ChannelViewModel channelView)
        {
            _channel = channelView;
        }

        public string GetVoteText()
        {
            if (!IsVoteActive)
                return null;

            return string.Format(Vote.Question, Question, string.Join(",", Votes), Command.Голос);
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

            _tokenSource = new CancellationTokenSource();

            Action sleep = () =>
            {
                Thread.Sleep(seconds * 1000);
            };

            var wait = TimerFactory.RunOnce(_channel, sleep);
            wait.ContinueWith(task => StopVote(), _tokenSource.Token);

            var cancelToken = TimerFactory.Start(_channel, action, Vote.Delay * 1000);
            TokenSources.Add(_channel, cancelToken);
        }

        public void StopVote()
        {
            if (IsVoteActive)
            {
                var votes = string.Join(";", Votes);
                var text = string.Format(Vote.Result, Question, votes);
                _channel.SendMessage(null, SendMessage.GetMessage(text));
            }

            if (TokenSources.ContainsKey(_channel))
            {
                TimerFactory.Stop(_channel, TokenSources[_channel]);
                TokenSources.Remove(_channel);
            }

            IsVoteActive = false;
            Question = null;
            Votes = null;
            Voted = null;

            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }
    }
}
