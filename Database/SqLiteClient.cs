using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Database
{
    public static class SqLiteClient
    {
        private const string DataBaseName = "TwitchChat.sqlite";

        static SqLiteClient()
        {
            if (!File.Exists(DataBaseName))
            {
                SQLiteConnection.CreateFile(DataBaseName);
                Execute("CREATE TABLE AccessTokens (type nvarchar(20), value nvarchar(255), expire bigint)");
            }
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

        public static void AddToken(string type, string value, long? expire = null)
        {
            Execute($"INSERT INTO AccessTokens VALUES ('{type}', '{value}', {(expire.HasValue ? $"'{expire.Value}'" : "NULL")})");
        }

        public static string GetNotExpiredToken(string type)
        {
            //TODO
            var result = Execute($"SELECT value FROM AccessTokens WHERE type = '{type}' AND expire <");

            return result.First().Value.First().Value;
        }
    }
}
