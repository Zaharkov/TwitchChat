using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class ChatterRepository : BaseRepository<Chatter>
    {
        private ChatterRepository()
        {
        }

        private static ChatterRepository _instance;

        public static ChatterRepository Instance => _instance ?? (_instance = new ChatterRepository());

        public Chatter AddChatter(string chatName, string name)
        {
            var chatter = new Chatter
            {
                ChatName = chatName,
                Name = name,
                Type = ""
            };

            AddOrUpdate(chatter);
            return Table.FirstOrDefault(t =>
                t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                t.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        public void DeleteChatter(Chatter chatter)
        {
            if(chatter != null)
                Delete(chatter);
        }

        public List<Chatter> GetList(Func<Chatter, bool> predicate)
        {
            return Table.Where(predicate).ToList();
        }
    }
}
