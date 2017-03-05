using System;
using System.IO;
using Domain.Models;
using Newtonsoft.Json;

namespace Domain.Repositories
{
    public class CustomSqlRepository : BaseRepository<Log>
    {
        private CustomSqlRepository()
        {
        }

        public static CustomSqlRepository Instance => new CustomSqlRepository();

        public string Change(string sql)
        {
            try
            {
                var result = SqlChange(sql);
                var json = JsonConvert.SerializeObject(result);
                return json;
            }
            catch (Exception e)
            {
                LogException("Custom Sql execute error", e);
                return e.ToString();
            }
        }

        public string Get(string sql)
        {
            try
            {
                var result = SqlGet(sql);
                var json = JsonConvert.SerializeObject(result);
                return json;
            }
            catch (Exception e)
            {
                LogException("Custom Sql execute error", e);
                return e.ToString();
            } 
        }

        public void LogException(string message, Exception e)
        {
            var log = new Log
            {
                Exception = e.GetBaseException().ToString(),
                Message = message,
                Time = DateTime.UtcNow
            };

            AddOrUpdate(log);

            File.AppendAllText("ErrorLog.txt", $"Mes: {message}, Time: {DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss")}, Ex: {e}");
        }
    }
}
