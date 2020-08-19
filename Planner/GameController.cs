using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WarzoneConnect.Properties;
using System.Resources;
using WarzoneConnect.Planner.PlotMaker;
using WarzoneConnect.Player;
using System.Threading.Tasks;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace WarzoneConnect.Planner
{//TODO 还有视频文件！
    internal static class GameController //本class用来记录，读取游戏进程
    {

        internal static List<Host> HostList;
        
        internal static List<SaveLoadActions> SaveLoadList = new List<SaveLoadActions>();

        internal static void Initialize()
        {
            if (File.Exists(@".\Save.resx"))
            {
                try
                {
                    using (var resxSet = new ResXResourceSet(@".\Save.resx"))
                    {
                        try
                        {
                            if (resxSet.GetObject("RootKit") != null)
                            {
                                BigFirework.YouDied();
                                return;
                            }
                        }
                        catch
                        {
                            // 忽略掉
                        }
                        HostList = (List<Host>) resxSet.GetObject("hosts");
                        SaveLoadList = (List<SaveLoadActions>) resxSet.GetObject("slList");
                        if ((SaveLoadList ?? throw new BrokenSaveException()).Any(sla => !sla.Load(resxSet))) throw new BrokenSaveException();
                    }
                    if (HostList == null) throw new BrokenSaveException();
                }
                catch
                {
                    File.Delete(@".\Save.resx");
                    Initialize();
                }
            }
            else
            {

                Task initTask = new Task(() =>
                {
                    HostList = HostStorage.InitializeHost();
                
                var rm = GlobalConfig.ResourceManager;
                
                LinkStorage.ReLink(rm);
                WafServer.FirewallInstall(rm);
                MailServer.RebuildMails();
                AutoSploitServer.AddExploit(rm);
                });
                //HostList = HostStorage.InitializeHost();

                //var rm = GlobalConfig.ResourceManager;

                //LinkStorage.ReLink(rm);
                //WafServer.FirewallInstall(rm);
                //MailServer.RebuildMails();
                //AutoSploitServer.AddExploit(rm);

                initTask.Start();

                foreach (var s in GameController_TextResource.BootUp.Replace("\r\n","\n").Split('\n'))
                {
                    if (s.Trim() == string.Empty)
                    {
                        Thread.Sleep(1000);
                    }
                
                    Console.WriteLine(s);
                    Thread.Sleep(50);
                }
                Task.WaitAny(initTask);
                Console.Clear();

                PlotObserver.InitializePlot();
                PlotObserver.StartObserve();
            }
            WafServer.FirewallBootUp();
            MediaPlayer.RegisterMediaFile();
            AutoSploit.RegisterExpFile();
            
            new Terminal(HostList?[0].Sh).Open();
        }

        internal static void Save()
        {
            if (File.Exists(@".\Save.resx"))
                File.Delete(@".\Save.resx");
            using var resx = new ResXResourceWriter(@".\Save.resx");
            resx.AddResource("hosts", HostList);
            resx.AddResource("time", DateTime.Now);
            resx.AddResource("slList", SaveLoadList);
            foreach (var sla in SaveLoadList) sla.Save(resx);
            
            Console.WriteLine(Shell_TextResource.Saved);
        }

        internal static void Load()
        {
            Console.CursorVisible = false;
            if (File.Exists(@".\Save.resx"))
                try
                {
                    using var resxSet = new ResXResourceSet(@".\Save.resx");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(GameController_TextResource.Load_Option, resxSet.GetObject("time"));
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(GameController_TextResource.Yes);
                    Console.ResetColor();
                    Console.Write('\t'+GameController_TextResource.No);
                    var isYes = true;
                    ConsoleKey key;
                    do
                    {
                        key = Console.ReadKey().Key;
                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (key)
                        {
                            case ConsoleKey.RightArrow:
                            {
                                if (isYes)
                                {
                                    isYes = false;
                                    Console.CursorLeft = 0;
                                    Console.Write(GameController_TextResource.Yes+'\t');
                                    Console.ForegroundColor = ConsoleColor.Black;
                                    Console.BackgroundColor = ConsoleColor.Gray;
                                    Console.Write(GameController_TextResource.No);
                                    Console.ResetColor();
                                }
    
                                break;
                            }
                            case ConsoleKey.LeftArrow:
                            {
                                if (!isYes)
                                {
                                    isYes = true;
                                    Console.CursorLeft = 0;
                                    Console.ForegroundColor = ConsoleColor.Black;
                                    Console.BackgroundColor = ConsoleColor.Gray;
                                    Console.Write(GameController_TextResource.Yes);
                                    Console.ResetColor();
                                    Console.Write('\t'+GameController_TextResource.No);
                                }
    
                                break;
                            }
                        }
                    } while (key != ConsoleKey.Enter);
    
                    if (isYes)
                    {
                        HostList = (List<Host>) resxSet.GetObject("hosts");
                        SaveLoadList = (List<SaveLoadActions>) resxSet.GetObject("slList");
                        if ((SaveLoadList ?? throw new BrokenSaveException()).Any(sla => !sla.Load(resxSet))) throw new BrokenSaveException();
                        Console.Clear();
                        Console.WriteLine(GameController_TextResource.Load);
                        new Terminal(HostList?[0].Sh).Open();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    File.Delete(@".\Save.resx");
                }
            else
                Console.WriteLine(GameController_TextResource.SaveNotExist);
    
            Console.CursorVisible = true;
        }

        [Serializable]
        internal readonly struct SaveLoadActions
        {
            internal Action<ResXResourceWriter> Save { get; }
            internal Func<ResXResourceSet, bool> Load { get; }
    
            internal SaveLoadActions(Action<ResXResourceWriter> s, Func<ResXResourceSet, bool> l)
            {
                Save = s;
                Load = l;
            }
        }

        private class BrokenSaveException : Exception
        {
            public override string Message { get; } = GameController_TextResource.BrokenSave;
        }
    }
    
}