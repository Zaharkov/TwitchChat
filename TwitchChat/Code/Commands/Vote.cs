using System.Linq;
using TwitchChat.Controls;
using Configuration;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class VoteCommand
    {
        private static readonly Configuration.Entities.Vote Vote = ConfigHolder.Configs.Vote;

        public static SendMessage Start(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var config = e.Message.ToLower().Replace($"{TwitchConstName.Command}{Command.ОпросСтарт}".ToLower(), "").Trim();

            var split = config.Split(';');
            int seconds;

            if (!int.TryParse(split[0], out seconds) || split.Length < 3 || seconds < 5)
                return SendMessage.GetWhisper(string.Format(Vote.IncorrectSyntax, Command.ОпросСтарт));

            var question = split[1];
            var votes = split.Select(t => t.Trim()).ToList();
            votes.RemoveAt(0);
            votes.RemoveAt(0);

            userModel.Channel.VoteHolder.StartNewVote(question, votes, seconds);
            return SendMessage.None;
        }

        public static SendMessage Stop(ChatMemberViewModel userModel)
        {
            userModel.Channel.VoteHolder.StopVote();
            return SendMessage.None;
        }

        public static SendMessage Question(ChatMemberViewModel userModel)
        {
            var message = userModel.Channel.VoteHolder.GetVoteText();
            return message == null ? SendMessage.GetMessage(Vote.Off) : SendMessage.GetMessage(message);
        }

        public static SendMessage UserVote(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var vote = userModel.Channel.VoteHolder;

            if (!vote.IsVoteActive)
                return SendMessage.None;

            if (vote.Voted.Contains(e.Username))
                return SendMessage.None;

            var userVote = e.Message.ToLower().Replace($"{TwitchConstName.Command}{Command.Голос}".ToLower(), "").Trim();
            var keys = vote.Votes.Keys.ToList();
            int voteNumber;

            if (vote.Votes.ContainsKey(userVote))
            {
                vote.Votes[userVote]++;
                vote.Voted.Add(e.Username);
            }
            else if(int.TryParse(userVote, out voteNumber) && voteNumber > 0 && voteNumber <= keys.Count)
            {
                var voteString = keys[voteNumber-1];
                vote.Votes[voteString]++;
                vote.Voted.Add(e.Username);
            }

            return SendMessage.None;
        }

        public static SendMessage LastResults(ChatMemberViewModel userModel)
        {
            var vote = userModel.Channel.VoteHolder;

            return SendMessage.GetMessage(string.IsNullOrEmpty(vote.LastResults) ? Vote.NoLast : vote.LastResults);
        }
    }
}
