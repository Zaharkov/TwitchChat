using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class LogRepository : BaseRepository<Log>
    {
        private LogRepository()
        {
        }

        public static LogRepository Instance => new LogRepository();

        public void LogException(string message, Exception e)
        {
            var log = new Log
            {
                Exception = e.ToString(),
                Message = message,
                Time = DateTime.UtcNow
            };

            AddOrUpdate(log);
        }

        public List<Log> GetLogs()
        {
            return Table.ToList();
        }

        public void DeleteLogs()
        {
            DeleteRange(Table);
        }
    }
}
