using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class ChatterInfoRepository : BaseRepository<ChatterInfo>
    {
        private readonly ChatterRepository _chatterRepository;

        private ChatterInfoRepository()
        {
            _chatterRepository = ChatterRepository.Instance;
        }

        private static ChatterInfoRepository _instance;

        public static ChatterInfoRepository Instance => _instance ?? (_instance = new ChatterInfoRepository());

        public void UpdateChatterInfo(List<ChatterInfo> chattersInfo)
        {
            if (!chattersInfo.Any())
                return;

            var context = new ChatterInfoRepository();

            var list = context._chatterRepository.GetList(t => chattersInfo.Any(k =>
                k.Chatter.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase) && 
                k.Chatter.ChatName.Equals(t.ChatName, StringComparison.InvariantCultureIgnoreCase)
            ));
            var exists = context.Table.AsEnumerable().Where(t => list.Any(k => k.Id == t.ChatterId)).ToList();

            foreach (var chatterInfo in exists)
            {
                var chatter = chattersInfo.First(t => 
                    t.Chatter.Name.Equals(chatterInfo.Chatter.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    t.Chatter.ChatName.Equals(chatterInfo.Chatter.ChatName, StringComparison.InvariantCultureIgnoreCase)
                );
                chatterInfo.Seconds += chatter.Seconds;
                chattersInfo.Remove(chatter);
            }

            context.AddOrUpdateRange(exists);
            context.AddOrUpdateRange(chattersInfo);
        }

        public long GetChatterTime(string name, string chatName)
        {
            var chatter = Table.FirstOrDefault(t => 
                t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.Chatter.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );
            return chatter?.Seconds ?? 0;
        }

        public bool IsChatterExist(string name)
        {
            return Table.Any(t => t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public long? GetChatterSteamId(string name)
        {
            var chatter = Table.FirstOrDefault(t => t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return chatter?.SteamId;
        }

        public bool IsSteamIdAttachedToChatter(long steamId, ref string name)
        {
            var chatter = Table.FirstOrDefault(t => t.SteamId.HasValue && t.SteamId.Value == steamId);

            if (chatter != null)
                name = chatter.Chatter.Name;

            return chatter != null;
        }

        public void AddChatterSteamId(string name, long steamId)
        {
            var chatters = Table.Where(t => t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = steamId;

            AddOrUpdateRange(chatters);
        }

        public void DeleteChatterSteamId(string name)
        {
            var chatters = Table.Where(t => t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = null;

            AddOrUpdateRange(chatters);
        }

        public List<ChatterInfo> GetChattersInfo()
        {
            return Table.ToList();
        }

        public void DeleteChatterInfo(string name, string chatName)
        {
            var chatter = Table.FirstOrDefault(t => 
                t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.Chatter.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (chatter != null)
            {
                Delete(chatter);
                _chatterRepository.DeleteChatter(chatter.Chatter);
            }
        }

        public void AddQuizScore(string name, string chatName)
        {
            var chatterInfo = Table.FirstOrDefault(t =>
                t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.Chatter.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (chatterInfo == null)
            {
                chatterInfo = new ChatterInfo
                {
                    Chatter = _chatterRepository.AddChatter(chatName, name)
                };

                AddOrUpdate(chatterInfo);
            }

            chatterInfo.QuizScore++;
            AddOrUpdate(chatterInfo);
        }

        public KeyValuePair<int, int> GetQuizScore(string name, string chatName)
        {
            var chatterInfo = Table.FirstOrDefault(t =>
                t.Chatter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.Chatter.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
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
    }
}
