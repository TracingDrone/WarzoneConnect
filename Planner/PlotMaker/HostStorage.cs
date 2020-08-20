using System;
using System.Collections.Generic;
using System.Text;
using WarzoneConnect.Player;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Planner.PlotMaker
{
    public static class HostStorage
    {
        private static readonly dynamic Protagonist = new HackTool(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host0, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host0, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host0, "info"));
        
        private static readonly dynamic HacktoolStorage = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host1, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host1, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host1, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host1, "info")));
        
        private static readonly dynamic Tracer = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host2, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host2, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host2, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host2, "info")));
        
        private static readonly dynamic MediaStorage = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host3, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host3, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host3, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host3, "info")));
        
        private static readonly dynamic Dummy1 = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host4, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host4, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host4, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host4, "info")));
        
        private static readonly dynamic Dummy2 = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host5, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host5, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host5, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host5, "info")));
        
        private static readonly dynamic Dummy3 = new Target(
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host6, "name"),
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host6, "address"), 
            UsefulTools.RetrieveHostInfo(GlobalConfig.Host6, "info"),
            UsefulTools.AnalyseSystem(UsefulTools.RetrieveHostInfo(GlobalConfig.Host6, "info")));
        
        
        public static List<Host> InitializeHost()
        {
            var hostList = new List<Host> {Protagonist,HacktoolStorage,Tracer,MediaStorage,Dummy1,Dummy2,Dummy3};
            
            // LinkServer.ReLink(rm);
            // WafServer.FirewallInstall(rm);
            // MailServer.RebuildMails();
            // AutoSploitServer.AddExploit(rm);

            //加载顺序：本体=>Link=>Mail

            static void BasicCommandInstall(Host host) //安装基础指令
            {
                host.Sh.CommandList.Add(ShellCommandDict.ListCommand);
                host.Sh.CommandList.Add(ShellCommandDict.ChangeDirCommand);
                host.Sh.CommandList.Add(ShellCommandDict.MkDirCommand);
                host.Sh.CommandList.Add(ShellCommandDict.RemoveCommand);
                host.Sh.CommandList.Add(ShellCommandDict.CopyCommand);
                host.Sh.CommandList.Add(ShellCommandDict.MoveCommand);
                host.Sh.CommandList.Add(ShellCommandDict.InstallCommand);
                host.Sh.CommandList.Add(ShellCommandDict.ConcatenateCommand);
                host.Sh.CommandList.Add(ShellCommandDict.HelpCommand);
            }

            static void SaveLoadCommandInstall(Host host) //安装存档/读档功能
            {
                host.Sh.CommandList.Add(ShellCommandDict.SaveCommand);
                host.Sh.CommandList.Add(ShellCommandDict.LoadCommand);
            }

            foreach (var host in hostList)
                BasicCommandInstall(host);

            SaveLoadCommandInstall(hostList[0]);

            //TODO Test
            MediaBuilder.BuildVideoResources("mw");//TODO 编辑此处，生成resource文件
            foreach (var mediaFile in MediaBuilder.GetVideoResources("mw"))
            {
                hostList[0].GetRoot().Add(mediaFile);
            }
            hostList[0].GetRoot().Add(new Host.FileSystem.File("third.prproj"));
            hostList[0].GetRoot().Add(MediaPlayer.PlayExec);
            hostList[0].GetRoot().Add(new AutoSploit.ExpFile("DoorByPass","影响Door全系系统",new DateTime(2020,7,15), 0,"welcome to the storage of my delights and sorrows",0));
            //Test
            
            hostList[0].InstallExec(MailClient.MailExec);
            hostList[0].GetRoot().Add(Link.LinkExec);
            hostList[0].GetRoot().Add(new Host.FileSystem.Doc("reminder",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        UsefulTools.RetrieveHostInfo(GlobalConfig.ResourceManager.GetString("Host2"), "password")))));
            
            hostList[1].GetRoot().Add(EzHack.EzHackExec);
            hostList[1].GetRoot().Add(new Host.FileSystem.Doc("modified",EzHack_Dict.Password_Dict_Modified));
            
            hostList[2].InstallExec(MailClient.MailExec);
            hostList[2].GetRoot().Add(AutoSploit.AsfExec);
            hostList[2].GetRoot().Add(new Host.FileSystem.Doc("随手记录","1.3号靶机的Door系统找到0Day了，记得回去靶机把Exp拿回来。\n" +
                                                                     "2.3号靶机密码是noMoreVVeakP@ssw0rd!\n" +
                                                                     "3.给Hacktool_Storage打补丁（已完成）\n" + 
                                                                     "4.给自己的家用电脑打补丁（已完成）\n" + 
                                                                     "5.给Media_Storage打补丁"));

            // MediaBuilder.BuildVideoResources(new List<string> {"Airstrike_3x","test"});//TODO 编辑此处，生成resource文件
            var mwVideo = new Host.FileSystem.Dir("mwVideo");
            foreach (var mediaFile in MediaBuilder.GetVideoResources("mw"))
            {
                mwVideo.Add(mediaFile);
            }
            hostList[3].GetRoot().Add(mwVideo);
            hostList[3].GetRoot().Add(new Host.FileSystem.Doc("备忘录","素材已经准备妥当了！该动手了！"));
            hostList[3].GetRoot().Add(new Host.FileSystem.File("third.prproj"));
            //TODO 编辑media_storage的视频文件

            hostList[4].GetRoot().Add(new Host.FileSystem.Doc("flag",PlotConfig.flag1));
            
            hostList[5].GetRoot().Add(new Host.FileSystem.Doc("flag",PlotConfig.flag2));
            
            hostList[6].GetRoot().Add(new Host.FileSystem.Doc("flag",PlotConfig.flag3));
            hostList[6].GetRoot().Add(new AutoSploit.ExpFile("DoorByPass","影响Door全系系统",new DateTime(2020,7,15), 0,"welcome to the storage of my delights and sorrows",0));

            return hostList;
        }
    }
}