using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
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

            if (!IsTableExists("AccessTokens"))
            {
                using (var command = new SQLiteCommand())
                {
                    command.CommandText = "CREATE TABLE AccessTokens (type nvarchar(20), value nvarchar(255), expire bigint)";
                    Execute(command);
                }  
            }

            if (!IsTableExists("ChattersInfo"))
            {
                using (var command = new SQLiteCommand())
                {
                    command.CommandText = "CREATE TABLE ChattersInfo (name nvarchar(128), chatName nvarchar(128), type nvarchar(35), seconds bigint, PRIMARY KEY(name, chatName))";
                    Execute(command);
                }
            }

            if (!IsTableExists("Logs"))
            {
                using (var command = new SQLiteCommand())
                {
                    command.CommandText = "CREATE TABLE Logs (time nvarchar(128), message nvarchar, exception nvarchar)";
                    Execute(command);
                }
            }
        }

        private static bool IsTableExists(string name)
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @tableName";
                command.Parameters.Add(new SQLiteParameter("@tableName", name));

                var result = Execute(command);
                return result.Count > 0;
            }
        }

        private static Dictionary<int, Dictionary<string, string>> Execute(SQLiteCommand command)
        {
            using (var sqlite = new SQLiteConnection($"Data Source={DataBaseName}"))
            {
                sqlite.Open();
                command.Connection = sqlite;
                command.CommandType = CommandType.Text;

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

            using (var command = new SQLiteCommand())
            {
                command.CommandText = "DELETE FROM AccessTokens WHERE type = @tokenType";
                command.Parameters.Add(new SQLiteParameter("@tokenType", type.ToString()));
                Execute(command);
            }

            using (var command = new SQLiteCommand())
            {
                command.CommandText = "INSERT INTO AccessTokens VALUES (@tokenType, @value, @expire)";
                command.Parameters.Add(new SQLiteParameter("@tokenType", type.ToString()));
                command.Parameters.Add(new SQLiteParameter("@value", value));
                command.Parameters.Add(new SQLiteParameter("@expire", expire.HasValue ? (object)diff : DBNull.Value));
                Execute(command);
            }
        }

        public static string GetNotExpiredToken(AccessTokenType type, int? expireLag = null)
        {
            var diff = (long)(DateTime.UtcNow - UnixTime).TotalSeconds + (expireLag ?? 0);

            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT value FROM AccessTokens WHERE type = @tokenType AND (expire IS NULL OR expire > @diff)";
                command.Parameters.Add(new SQLiteParameter("@tokenType", type.ToString()));
                command.Parameters.Add(new SQLiteParameter("@diff", diff));

                var result = Execute(command);
                return result.Count > 0 ? result[0]["value"] : null;
            } 
        }

        public static Dictionary<int, Dictionary<string, string>> GetTokens()
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT * FROM AccessTokens";
                return Execute(command);
            }
        }

        public static void UpdateChatterInfo(List<ChatterData> chattersInfo)
        {
            if (!chattersInfo.Any())
                return;

            foreach (var partion in chattersInfo.Partition(200))
            {
                using (var command = new SQLiteCommand())
                {
                    var partionList = partion.ToList();
                    var builder = new StringBuilder();
                    var last = partionList.Last();
                    builder.AppendLine(@"INSERT OR REPLACE INTO ChattersInfo (name, chatName, type, seconds) VALUES");

                    var i = 1;
                    foreach (var chatterData in partionList)
                    {
                        builder.AppendLine($"(@name{i}, @chatName{i}, @type{i}, @seconds{i} + (SELECT COALESCE(SUM(seconds),0) as secSum FROM ChattersInfo WHERE name = @name{i} AND chatName = @chatName{i})){(last == chatterData ? "" : ",")}");

                        command.Parameters.Add(new SQLiteParameter($"@name{i}", chatterData.Name));
                        command.Parameters.Add(new SQLiteParameter($"@chatName{i}", chatterData.ChatName));
                        command.Parameters.Add(new SQLiteParameter($"@type{i}", chatterData.Type));
                        command.Parameters.Add(new SQLiteParameter($"@seconds{i}", chatterData.Seconds));

                        i++;
                    }
                    command.CommandText = builder.ToString();
                    Execute(command);
                }
            }
        }

        public static long GetChatterTime(string name, string chatName)
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT seconds FROM ChattersInfo WHERE name = @name AND chatName = @chatName";
                command.Parameters.Add(new SQLiteParameter("@name", name));
                command.Parameters.Add(new SQLiteParameter("@chatName", chatName));

                var result = Execute(command);
                return result.Count == 0 ? 0 : long.Parse(result[0]["seconds"]);
            }  
        }

        public static Dictionary<int, Dictionary<string, string>> GetChatters()
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT * FROM ChattersInfo";
                return Execute(command);
            }
        }

        public static void DeletChatter(string name, string chatName)
        {
            using(var command = new SQLiteCommand())
            {
                command.CommandText = "DELETE FROM ChattersInfo WHERE name = @name AND chatName = @chatName";
                command.Parameters.Add(new SQLiteParameter("@name", name));
                command.Parameters.Add(new SQLiteParameter("@chatName", chatName));

                Execute(command);
            }
        }

        public static void LogException(string message, Exception e)
        {
            var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);

            using (var command = new SQLiteCommand())
            {
                command.CommandText = "INSERT INTO Logs VALUES (@time, @message, @exception)";
                command.Parameters.Add(new SQLiteParameter("@time", time));
                command.Parameters.Add(new SQLiteParameter("@message", message));
                command.Parameters.Add(new SQLiteParameter("@exception", e.ToString()));

                Execute(command);
            }
        }

        public static Dictionary<int, Dictionary<string, string>> GetLogs()
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = "SELECT * FROM Logs";
                return Execute(command);
            }
        }
    }
}
