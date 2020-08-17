using System.Resources;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    public static class AutoSploitServer
    {
        internal delegate void PacketFlow(dynamic tgt,Host host);
        internal static event PacketFlow PacketCatch;
        
        internal static object ExploitUsingExps(string address,string exploit,Host host)
        {
            dynamic tgt = (Target) GameController.HostList.Find(h => h.Addr == address);
            PacketCatch?.Invoke(tgt,host);
            return !exploit.ToLowerInvariant().Contains(((string)tgt.Exp).ToLowerInvariant()) ? false : tgt.Pw;
        }

        internal static void AddExploit(ResourceManager rm)
        {
            for (var i = 1; i < GameController.HostList.Count; i++)
            {
                ((dynamic)GameController.HostList[i]).Exp=UsefulTools.RetrieveHostInfo(rm.GetString("Host" + i), "exploit");
            }
        }
    }
}