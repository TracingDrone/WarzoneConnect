using System.Collections.Generic;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    internal static class LinkServer //用来搜索计算机，相当于交换数据的网络
    {
        internal static event PacketFlow PacketCatch;
        internal static event ConnectInspection UpperInspection;

        internal static void ConnectHosts(Link.Router r1, Link.Router r2)
        {
            if (r2.SelfAddress != GameController.HostList[0].Addr)
                r1.ConnectedAddress.Add(r2);
            if (r1.SelfAddress != GameController.HostList[0].Addr)
                r2.ConnectedAddress.Add(r1);
        }

        private static void Proxy(Link.Router r)
        {
            if (!((dynamic) GameController.HostList[0]).Rt.ConnectedAddress.Contains(r))
                ConnectHosts(((dynamic) GameController.HostList[0]).Rt, r);
        }

        internal static bool FindHostByAddress(string address, Host host) //查找是否有这台计算机
        {
            return Link.GetRouter(host).Exists(r => r.SelfAddress == address);
        }

        internal static Link.RemoteShell FindHostByAddress(string address, string password, Host host)
        {
            dynamic tgt = (Target) GameController.HostList.Find(h => h.Addr == address);
            PacketCatch?.Invoke(tgt, host);
            if (tgt.Pw != password)
                return null;
            foreach (var r in Link.GetRouter(tgt))
                Proxy(r);
            UpperInspection?.Invoke();
            return new Link.RemoteShell(host, tgt);
        }

        internal static void FileTransfer(List<string> argList, Host.FileSystem.File tfFile, Host connectorHost)
        {
            var backupDir = connectorHost.Fs.CurrentDir;
            var newName = argList.Count == 1
                ? tfFile is Host.FileSystem.Exec ? "bin/" + argList[0] : "doc/" + argList[0]
                : argList[1];
            if (newName.Contains("/"))
            {
                var newPath = newName.Substring(0, newName.LastIndexOf('/')); //目录位置
                newName = newName.Remove(0, newName.LastIndexOf('/') + 1); //新目录名
                var newArg = new List<string>
                {
                    newPath
                };
                if (newPath != string.Empty)
                    try
                    {
                        ShellCommandDict.ChangeDirCommand.Execute(newArg, connectorHost);
                    }
                    catch (CustomException.FileNotExistException)
                    {
                        throw new CustomException.FileNotExistException();
                    }
            }

            if (newName == string.Empty) //没有输入文件名（arg1以斜杠结尾）
                newName = argList[0];
            tfFile.Name = newName;
            try
            {
                connectorHost.Fs.CurrentDir.Add(tfFile);
            }
            finally
            {
                connectorHost.Fs.CurrentDir = backupDir;
            }
        }

        internal delegate void PacketFlow(dynamic tgt, Host host); //留给WAF监控数据包

        internal delegate void ConnectInspection(); //留给PlotObserver执行自动存档
    }
}