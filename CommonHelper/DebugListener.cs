using System.Diagnostics;
using System.IO;

namespace CommonHelper
{
    public class DebugListener
    {
        private readonly bool _toFile;

        public DebugListener(bool toFile)
        {
            _toFile = toFile;
        }

        public void WriteLine(string category, string msg)
        {
            Debug.WriteLine($"DebugListener - {category}: {msg}");

            if(_toFile)
                File.AppendAllText("Logs.txt", $"DebugListener - {category}: {msg}");
        }

        public void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);

            if (_toFile)
                File.AppendAllText("Logs.txt", string.Format(format, args));
        }
    }
}
