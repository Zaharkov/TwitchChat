﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class ChatterInfoRepository : BaseRepository<ChatterInfo>
    {
        private ChatterInfoRepository()
        {
        }

        private static ChatterInfoRepository _instance;

        public static ChatterInfoRepository Instance => _instance ?? (_instance = new ChatterInfoRepository());

        public void UpdateChatterInfo(string chatName, List<ChatterInfo> chattersInfo, bool newContext = false)
        {
            if (!chattersInfo.Any())
                return;

            var context = newContext ? new ChatterInfoRepository() : Instance;
            var namesList = chattersInfo.Select(t => t.Name);
            var exists = context.Table.Where(t => namesList.Contains(t.Name) && t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)).ToList();

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

            context.AddOrUpdateRange(exists);
            context.AddOrUpdateRange(chattersInfo);
        }

        public long GetChatterTime(string name, string chatName)
        {
            var chatter = Table.FirstOrDefault(t => 
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );
            return chatter?.Seconds ?? 0;
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
            var chatters = Table.Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = steamId;

            AddOrUpdateRange(chatters);
        }

        public void DeleteChatterSteamId(string name)
        {
            var chatters = Table.Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var chatterInfo in chatters)
                chatterInfo.SteamId = null;

            AddOrUpdateRange(chatters);
        }

        public List<ChatterInfo> GetChattersInfo()
        {
            return Table.ToList();
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
                Delete(chatter);
        }

        public void AddQuizScore(string name, string chatName)
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
                    ChatName = chatName,
                    Type = ""
                };

                AddOrUpdate(chatterInfo);
            }

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
    }
}
