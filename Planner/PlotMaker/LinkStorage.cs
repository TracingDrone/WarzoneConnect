using System.Collections.Generic;
using System.Resources;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner.PlotMaker
{
    internal static class LinkStorage
    {
        internal static void ReLink(ResourceManager rm)
        {
            foreach (dynamic host in GameController.HostList)
            {
                host.Rt=new Link.Router(host.Addr,host.User+'\t'+host.Info);
                host.Sh.RemoteCommandList = new List<Link.RemoteCommand>();
            }

            LinkServer.ConnectHosts(((dynamic)GameController.HostList[0]).Rt, ((dynamic)GameController.HostList[1]).Rt); //hacktool与hacktool_storage相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[0]).Rt, ((dynamic)GameController.HostList[4]).Rt); //hacktool与dummy1相连
            
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[2]).Rt, ((dynamic)GameController.HostList[1]).Rt); //tracer_at_home与hacktool_storage相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[2]).Rt, ((dynamic)GameController.HostList[3]).Rt); //tracer_at_home与media_storage相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[2]).Rt, ((dynamic)GameController.HostList[4]).Rt); //tracer_at_home与dummy1相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[2]).Rt, ((dynamic)GameController.HostList[5]).Rt); //tracer_at_home与dummy2相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[2]).Rt, ((dynamic)GameController.HostList[6]).Rt); //tracer_at_home与dummy3相连
            
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[5]).Rt, ((dynamic)GameController.HostList[4]).Rt); //dummy2与dummy1相连
            LinkServer.ConnectHosts(((dynamic)GameController.HostList[5]).Rt, ((dynamic)GameController.HostList[6]).Rt); //dummy2与dummy3相连

            for (var index = 1; index < GameController.HostList.Count; index++)
            {
                ((dynamic)GameController.HostList[index]).Pw = UsefulTools.RetrieveHostInfo(rm.GetString("Host" + index), "password");
                ((dynamic)GameController.HostList[index].Sh).RemoteCommandList = new List<Link.RemoteCommand>();
            }
        }
    }
}