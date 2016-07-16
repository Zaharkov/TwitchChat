using System.Diagnostics;
using SteamKit2;

namespace DotaClient
{
    public class DebugListener : IDebugListener
    {
        public void WriteLine(string category, string msg)
        {
            Debug.WriteLine($"DebugListener - {category}: {msg}");
        }

        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }
    }
}
