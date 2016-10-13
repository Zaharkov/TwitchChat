using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class ChatterInfoRepository : BaseRepository<ChatterInfo>
    {
        private readonly RouletteInfoRepository _rouletteInfoRepository = RouletteInfoRepository.Instance;

        private ChatterInfoRepository()
        {
        }

        private static ChatterInfoRepository _instance;

        public static ChatterInfoRepository Instance => _instance ?? (_instance = new ChatterInfoRepository());

        public void UpdateChatterInfo(string chatName, List<ChatterInfo> chattersInfo)
        {
            if (!chattersInfo.Any())
                return;

            var context = new ChatterInfoRepository();
            var namesList = chattersInfo.Select(t => t.Name);
            var exists = context.Table.Where(t => namesList.Contains(t.Name) && t.ChatName.Equals(chatName));

            foreach (var chatterInfo in exists)
            {
                var chatter = chattersInfo.FirstOrDefault(t => 
                    t.Name.Equals(chatterInfo.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    t.ChatName.Equals(chatterInfo.ChatName, StringComparison.InvariantCultureIgnoreCase)
                );

                if (chatter != null)
                {
                    chatterInfo.Seconds += chatter.Seconds;
                    chattersInfo.Remove(chatter);
                }
            }

            context.UpdateRange(exists);
            context.AddRange(chattersInfo);
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

        public bool IsChatterExist(string name)
        {
            return Table.Any(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public long? GetChatterSteamId(string name)
        {
            var chatter = Table.FirstOrDefault(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return chatter?.SteamId;
        }

        public bool IsSteamIdAttachedToChatter(long steamId, ref string name)
        {
            var chatter = Table.FirstOrDefault(t => t.SteamId.HasValue && t.SteamId.Value == steamId);

            if (chatter != null)
                name = chatter.Name;

            return chatter != null;
        }

        public void AddChatterSteamId(string name, long steamId)
        {
            var chatters = Table.Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = steamId;

            UpdateRange(chatters);
        }

        public void DeleteChatterSteamId(string name)
        {
            var chatters = Table.Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = null;

            UpdateRange(chatters);
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
                    _rouletteInfoRepository.Delete(chatter.RouletteId.Value);

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
                    _rouletteInfoRepository.Delete(chatterInfo.RouletteId.Value);
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
            var chatterInfo = Table.FirstOrDefault(t =>
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );
            var count = chatterInfo?.QuizScore ?? 0;

            var group = Table.GroupBy(t => t.QuizScore).OrderByDescending(t => t.Key);
            var score = 0;

            foreach (var entity in group)
            {
                score++;

                if(entity.Key == count)
                    break;
            }

            return new KeyValuePair<int, int>(count, score);
        }

        public long GetRouletteId(string name, string chatName)
        {
            var chatterInfo = GetOrCreate(name, chatName);

            if (!chatterInfo.RouletteId.HasValue)
            {
                var info = _rouletteInfoRepository.Create();
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
