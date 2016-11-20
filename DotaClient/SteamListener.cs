using CommonHelper;
using SteamKit2;

namespace DotaClient
{
    public class SteamListener : DebugListener, IDebugListener
    {
        public SteamListener(bool toFile) : base(toFile)
        {
            
        }
    }
}
