using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class RouletteInfoRepository : BaseRepository<RouletteInfo>
    {
        private RouletteInfoRepository()
        {
        }

        public static RouletteInfoRepository Instance => new RouletteInfoRepository();

        public RouletteInfo Create()
        {
            var info = new RouletteInfo
            {
                Percent = 1,
                MaxPercent = 1
            };
            AddOrUpdate(info);
            return info;
        }

        public void Delete(long id)
        {
            var info = Table.FirstOrDefault(t => t.Id.Equals(id));

            if(info != null)
                Delete(info);
        }

        public List<RouletteInfo> GetTop(int top)
        {
            return Table.OrderBy(t => t.MaxPercent).Take(top).ToList();
        }

        public void AddTry(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.TryCount++;
            info.CurrentTry++;
            AddOrUpdate(info);
        }

        public void AddDeath(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.DeathCount++;
            info.CurrentTry = 0;
            info.Streak = 0;
            info.Percent = 1;
            AddOrUpdate(info);
        }

        public void Reset(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.CurrentTry = 0;
            info.Streak = 0;
            info.Percent = 1;
            AddOrUpdate(info);
        }

        public void ResetCurrentTry(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.CurrentTry = 0;
            AddOrUpdate(info);
        }

        public void AddPercent(long id, double percent)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.Streak++;
            info.Percent *= percent;

            if (info.Percent < info.MaxPercent)
            {
                info.MaxStreak = info.Streak;
                info.MaxPercent = info.Percent;
            }

            AddOrUpdate(info);
        }

        public RouletteInfo Get(long id)
        {
            return Table.First(t => t.Id.Equals(id));
        }

        public List<RouletteInfo> GetList()
        {
            return Table.ToList();
        }

        public void SetDuelName(long id, string duelName)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.DuelName = duelName;
            AddOrUpdate(info);
        }

        public void RemoveDuelName(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.DuelName = null;
            AddOrUpdate(info);
        }

        public void AddDuelScore(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.DuelScore++;
            AddOrUpdate(info);
        }

        public void ResetAllDuelNames()
        {
            var exists = Table.Where(t => t.DuelName != null).ToList();

            foreach (var rouletteInfo in exists)
                rouletteInfo.DuelName = null;

            UpdateRange(exists);
        }
    }
}
