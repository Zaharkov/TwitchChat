using System;
using System.IO;

namespace Domain.Utils
{
    public static class Backup
    {
        private const string DatabaseName = "Database.sdf";
        private const string DumpDir = "Dumps";
        public static void MakeBackUp()
        {
            var dir = Environment.CurrentDirectory;
            var sourceFile = Path.Combine(dir, DatabaseName);

            if (File.Exists(sourceFile))
            {
                var destPath = Path.Combine(dir, DumpDir);

                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                var newDatabaseName = $"{DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss")}-{DatabaseName}";
                var destFile = Path.Combine(destPath, newDatabaseName);

                File.Copy(sourceFile, destFile);
            }
        }
    }
}
