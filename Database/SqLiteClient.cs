using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Database.Entities;

namespace Database
{
    public static class SqLiteClient
    {
        private const string DataBaseName = "TwitchChat.sqlite";
        private static readonly DateTime UnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static SqLiteClient()
        {
            if (!File.Exists(DataBaseName))
                SQLiteConnection.CreateFile(DataBaseName);

            if(!IsTableExists("AccessTokens"))
                Execute("CREATE TABLE AccessTokens (type nvarchar(20), value nvarchar(255), expire bigint)");
        }

        private static bool IsTableExists(string name)
        {
            var result = Execute($"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}'");

            return result.Count > 0;
        }

        private static Dictionary<int, Dictionary<string, string>> Execute(string sql)
        {
            using (var sqlite = new SQLiteConnection($"Data Source={DataBaseName}"))
            {
                sqlite.Open();
                var command = new SQLiteCommand(sql, sqlite);

                var result = new Dictionary<int, Dictionary<string, string>>();
                var sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    var row = new Dictionary<string, string>();

                    for (var i = 0; i < sqlReader.FieldCount; i++)
                        row[sqlReader.GetName(i)] = sqlReader.GetValue(i).ToString();

                    result[result.Count] = row;
                }

                return result;
            }
        }

        public static void AddToken(AccessTokenType type, string value, int? expire = null)
        {
            var diff = expire.HasValue ? (long) (DateTime.UtcNow - UnixTime).TotalSeconds + expire.Value : 0;

            Execute($"DELETE FROM AccessTokens WHERE type = '{type}'");
            Execute($"INSERT INTO AccessTokens VALUES ('{type}', '{value}', {(expire.HasValue ? $"'{diff}'" : "NULL")})");
        }

        public static string GetNotExpiredToken(AccessTokenType type)
        {
            //get time in stamp + 12 hours 
            var diff = (long)(DateTime.UtcNow - UnixTime).TotalSeconds + 3600 * 12;
            
            var result = Execute($"SELECT value FROM AccessTokens WHERE type = '{type}' AND (expire IS NULL OR expire > {diff})");

            return result.Count > 0 ? result[0]["value"] : null;
        }
    }
}
