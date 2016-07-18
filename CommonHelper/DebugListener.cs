using System.Diagnostics;

namespace CommonHelper
{
    public class DebugListener
    {
        public void WriteLine(string category, string msg)
        {
            Debug.WriteLine($"DebugListener - {category}: {msg}");
        }

        public void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }
    }
}
