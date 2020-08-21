using System;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using WarzoneConnect.Player;
using WarzoneConnect.Properties;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace WarzoneConnect.Planner
{
    public static class WafServer
    {
        internal static void FirewallInstall(ResourceManager rm)
        {
            for (var index = 1; index < GameController.HostList.Count; index++)
            {
                var wafStrength=UsefulTools.RetrieveHostInfo(rm.GetString("Host" + index), "waf");
                ((dynamic) GameController.HostList[index]).Firewall = wafStrength switch
                {
                    "tough" => new WebApplicationFirewall(4, true),
                    "weak" => new WebApplicationFirewall(2, false),
                    _ => null
                };
            }
        }

        private static void CheckFirewall(dynamic tgt, Host host)
        {
            try
            {
                ((WebApplicationFirewall)tgt.Firewall).FlowAnalyser(host.Addr);
            }
            catch
            {
                // 没有填WAF走这里
            }
        }
        
        private static void CheckFirewallInDangerZone(dynamic tgt, Host host)
        {
            try
            {
                if(((WebApplicationFirewall)tgt.Firewall).IsTough) 
                    ((WebApplicationFirewall)tgt.Firewall).DangerZone(host.Addr);
            }
            catch
            {
                // 没有填WAF走这里
            }
        }
        
        internal static void FirewallBootUp()
        {
            LinkServer.PacketCatch += CheckFirewall;
            AutoSploitServer.PacketCatch += CheckFirewallInDangerZone;
        }
        
        internal static bool IncursionSetup(string source,double incursionStrength)
        {
            try
            {
                Console.CursorVisible = false;
                // 1. 黑屏
                Console.Clear();
                Thread.Sleep(5000);
                
                // 2. Unauthorized Connection Detected 红字
                Console.ForegroundColor = ConsoleColor.Red;
                for (var i = 0; i < 50; i++)
                {
                    Console.WriteLine(WAF_TextResource.Unauthorized_Connection);
                    Thread.Sleep(50);
                }
                Console.ResetColor();
                Thread.Sleep(2000);
                Console.Clear();
                Thread.Sleep(2000);
                
                // 3. 类似电脑蓝屏，启动防御（此处可参考E3 2077）
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                for (var i = 0; i < Console.WindowHeight; i++)
                {
                    Console.SetCursorPosition(0,i);
                    Console.Write(string.Empty.PadLeft(Console.WindowWidth,' '));
                }
                
                Console.SetCursorPosition(0,0);
                Console.WriteLine(WAF_TextResource.Bsod_Text);
                
                // 动画跳转至WAFC继续编写
                
                
                var wafc = (WaFirecracker) ((dynamic) GameController.HostList.Find(h => h.Addr == source)).wafc; //如果没有wafc就输了
                if (wafc.IsAcquiredMasterKey)
                {
                    Console.ResetColor();
                    Console.WriteLine(WAF_TextResource.Wafc_Bypass);
                    Thread.Sleep(2000);
                    return true;
                }
                
                var counter=new WaFirecracker.CounterMeasure(ref wafc.DefenceStrength, wafc.IsFirstTime); //这个使用主线程

                // 开一个线程用来加攻击
                var attackAnalyzerTask = new Task(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    while (!counter.Source.Token.IsCancellationRequested)
                    {
                        counter.AttackAnalyzer(incursionStrength);
                        Thread.Sleep(100);
                    }
                }, counter.Source.Token);
                
                var getResultTask = new Task<bool>(() =>
                {
                    var result =  counter.GetResult();
                    if (counter.IsGetMasterKey)
                        wafc.IsAcquiredMasterKey = true;
                    // ReSharper disable once AccessToDisposedClosure
                    counter.Source.Cancel();
                    Task.WaitAll(attackAnalyzerTask);
                    return result;
                });

                attackAnalyzerTask.Start();
                getResultTask.Start();

                Task.WaitAll(getResultTask, attackAnalyzerTask);
                if (getResultTask.Result)
                {
                    wafc.IsFirstTime = false;
                    wafc.DefenceStrength += 0.04;
                }
                else
                {
                    BigFirework.YouDied();
                }
                Console.CursorVisible = true;
                counter.CmLock.Dispose();
                counter.Source.Dispose();
                return getResultTask.Result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Thread.Sleep(2000);
                Console.WriteLine(WAF_TextResource.Wafc_Failed);
                Thread.Sleep(2000);
                Console.ResetColor();
                BigFirework.YouDied();
                return false;
            }
        }
    }
}