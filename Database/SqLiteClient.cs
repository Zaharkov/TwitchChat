using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
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

            if (!IsTableExists("ChattersInfo"))
                Execute("CREATE TABLE ChattersInfo (name nvarchar(128), chatName nvarchar(128), type nvarchar(35), seconds bigint, PRIMARY KEY(name, chatName))");
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

        public static string GetNotExpiredToken(AccessTokenType type, int? expireLag = null)
        {
            var diff = (long)(DateTime.UtcNow - UnixTime).TotalSeconds + (expireLag ?? 0);
            
            var result = Execute($"SELECT value FROM AccessTokens WHERE type = '{type}' AND (expire IS NULL OR expire > {diff})");

            return result.Count > 0 ? result[0]["value"] : null;
        }

        public static void UpdateChatterInfo(List<ChatterData> chattersInfo)
        {
            var builder = new StringBuilder();
            var last = chattersInfo.Last();
            builder.AppendLine(@"INSERT OR REPLACE INTO ChattersInfo (name, chatName, type, seconds) VALUES");

            foreach (var chatterData in chattersInfo)
                builder.AppendLine($@"('{chatterData.Name}', '{chatterData.ChatName}', '{chatterData.Type}', {chatterData.Seconds} + (SELECT COALESCE(SUM(seconds),0) as secSum FROM ChattersInfo WHERE name = '{chatterData.Name}' AND chatName = '{chatterData.ChatName}')){(last == chatterData ? "" : ",")}");

            Execute(builder.ToString());
        }

        public static long GetChatterTime(string name, string chatName)
        {
            var result = Execute($"SELECT seconds FROM ChattersInfo WHERE name = '{name}' AND chatName = '{chatName}'");

            return result.Count == 0 ? 0 : long.Parse(result[0]["seconds"]);
        }

        public static void DeletChatter(string name, string chatName)
        {
            Execute($"DELETE FROM ChattersInfo WHERE name = '{name}' AND chatName = '{chatName}'");
        }
    }
}
