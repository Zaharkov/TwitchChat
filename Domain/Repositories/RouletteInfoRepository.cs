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

        private static RouletteInfoRepository _instance;

        public static RouletteInfoRepository Instance => _instance ?? (_instance = new RouletteInfoRepository());

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

        public void AddDeath(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.DeathCount++;
            AddOrUpdate(info);
        }

        public void AddTry(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.TryCount++;
            AddOrUpdate(info);
        }

        public void AddCurrentTry(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.CurrentTry++;
            AddOrUpdate(info);
        }

        public void AddStreak(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.Streak++;

            if (info.Streak > info.MaxStreak)
                info.MaxStreak = info.Streak;

            AddOrUpdate(info);
        }

        public void AddPercent(long id, double percent)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.Percent *= percent;

            if (info.Percent < info.MaxPercent)
                info.MaxPercent = info.Percent;

            AddOrUpdate(info);
        }

        public void ResetCurrentTry(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.CurrentTry = 0;
            AddOrUpdate(info);
        }

        public void ResetStreak(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.Streak = 0;
            AddOrUpdate(info);
        }

        public void ResetPercent(long id)
        {
            var info = Table.First(t => t.Id.Equals(id));
            info.Percent = 1;
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
    }
}
