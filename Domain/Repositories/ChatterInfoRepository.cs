using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Models;

namespace Domain.Repositories
{
    public class ChatterInfoRepository : BaseRepository<ChatterInfo>
    {
        private ChatterInfoRepository()
        {
        }

        public static ChatterInfoRepository Instance => new ChatterInfoRepository();

        public void UpdateChatterInfo(string chatName, List<ChatterInfo> chattersInfo, int seconds)
        {
            if (!chattersInfo.Any())
                return;

            var namesList = chattersInfo.Select(t => t.Name);
            var exists = Table.Where(t => namesList.Contains(t.Name) && t.ChatName.Equals(chatName)).ToList();

            var updateBuilder = new StringBuilder();
            updateBuilder.AppendLine( "UPDATE [ChatterInfoes]");
            updateBuilder.AppendLine($"SET [Seconds] = [Seconds] + {seconds}");
            updateBuilder.AppendLine($"WHERE [ChatName] = N'{chatName}' AND [Name] IN (");

            if (exists.Any())
            {
                var lastUpdate = exists.Last();

                foreach (var chatterInfo in exists)
                {
                    var chatter = chattersInfo.Single(t =>
                        t.Name.Equals(chatterInfo.Name, StringComparison.InvariantCultureIgnoreCase) &&
                        t.ChatName.Equals(chatterInfo.ChatName, StringComparison.InvariantCultureIgnoreCase)
                        );

                    chattersInfo.Remove(chatter);
                    updateBuilder.Append($"N'{chatterInfo.Name}'{(lastUpdate == chatterInfo ? ")" : ", ")}");
                }

                Sql(updateBuilder.ToString());
            }

            if (chattersInfo.Any())
                AddRange(chattersInfo);
        }

        public void AddSecods(string name, string chatName, long seconds)
        {
            var chatterInfo = GetOrCreate(name, chatName);
            chatterInfo.Seconds += seconds;
            AddOrUpdate(chatterInfo);
        }

        public long GetChatterTime(string name, string chatName)
        {
            var chatter = GetOrCreate(name, chatName);
            return chatter.Seconds;
        }

        public bool IsChatterExist(string name, string chatName)
        {
            return Table.Any(t => 
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && 
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        public long? GetChatterSteamId(string name, string chatName)
        {
            var chatter = GetOrCreate(name, chatName);
            return chatter.SteamId;
        }

        public bool IsSteamIdAttachedToChatter(long steamId, string chatName, ref string name)
        {
            var chatter = Table.FirstOrDefault(t => 
                t.SteamId.HasValue && t.SteamId.Value == steamId && 
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (chatter != null)
                name = chatter.Name;

            return chatter != null;
        }

        public void AddChatterSteamId(string name, string chatName, long steamId)
        {
            var chatterInfo = GetOrCreate(name, chatName);
            chatterInfo.SteamId = steamId;
            AddOrUpdate(chatterInfo);
        }

        public void DeleteChatterSteamId(string name, string chatName)
        {
            var chatterInfo = GetOrCreate(name, chatName);
            chatterInfo.SteamId = null;
            AddOrUpdate(chatterInfo);
        }

        public List<ChatterInfo> GetChattersInfo()
        {
            return Table.ToList();
        }

        public ChatterInfo GetChatterInfo(string name, string chatName)
        {
            return GetOrCreate(name, chatName);
        }

        public List<ChatterInfo> GetChattersInfo(Func<ChatterInfo, bool> predicate)
        {
            return Table.Where(predicate).ToList();
        }

        public void DeleteChatterInfo(string name, string chatName)
        {
            var chatter = Table.FirstOrDefault(t => 
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (chatter != null)
            {
                if(chatter.RouletteId.HasValue)
                    RouletteInfoRepository.Instance.Delete(chatter.RouletteId.Value);

                Delete(chatter);
            }
        }

        public void DeleteChatterInfo(string chatName, List<ChatterInfo> chatters)
        {
            var namesList = chatters.Select(t => t.Name);
            var exists = Table.Where(t => namesList.Contains(t.Name) && t.ChatName.Equals(chatName));

            foreach (var chatterInfo in exists)
            {
                if (chatterInfo.RouletteId.HasValue)
                    RouletteInfoRepository.Instance.Delete(chatterInfo.RouletteId.Value);
            }

            DeleteRange(exists);
        }

        public void AddQuizScore(string name, string chatName)
        {
            var chatterInfo = GetOrCreate(name, chatName);
            chatterInfo.QuizScore++;
            AddOrUpdate(chatterInfo);
        }

        public KeyValuePair<int, int> GetQuizScore(string name, string chatName)
        {
            var chatterInfo = GetOrCreate(name, chatName);
            var group = Table.GroupBy(t => t.QuizScore).OrderByDescending(t => t.Key);
            var score = 0;

            foreach (var entity in group)
            {
                score++;

                if(entity.Key == chatterInfo.QuizScore)
                    break;
            }

            return new KeyValuePair<int, int>(chatterInfo.QuizScore, score);
        }

        public long GetRouletteId(string name, string chatName)
        {
            var chatterInfo = GetOrCreate(name, chatName);

            if (!chatterInfo.RouletteId.HasValue)
            {
                var info = RouletteInfoRepository.Instance.Create();
                chatterInfo.RouletteId = info.Id;
                AddOrUpdate(chatterInfo);
            }

            return chatterInfo.RouletteId.Value;
        }

        public ChatterInfo GetByRouletteId(long id)
        {
            return Table.FirstOrDefault(t => t.RouletteId.HasValue && t.RouletteId.Value.Equals(id)); 
        }

        private ChatterInfo GetOrCreate(string name, string chatName)
        {
            var chatterInfo = Table.FirstOrDefault(t =>
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (chatterInfo == null)
            {
                chatterInfo = new ChatterInfo
                {
                    Name = name,
                    ChatName = chatName
                };

                AddOrUpdate(chatterInfo);
            }

            return chatterInfo;
        }
    }
}
