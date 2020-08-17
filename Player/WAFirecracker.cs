using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Player
{
    [Serializable]
    public class WaFirecracker
    {
        internal bool IsAcquiredMasterKey;
        internal bool IsFirstTime;
        internal double DefenceStrength;
        internal CounterMeasure Counter;

        private WaFirecracker()
        {
            DefenceStrength = 1.04;
            IsFirstTime = true;
            IsAcquiredMasterKey = false;
        }

        private static class StatusVisualizer
        {
            private static int _currentCursorTopPosition; //当前光标位置
            private static readonly ManualResetEvent HackCodeLock = new ManualResetEvent(false); //Ratio绘制完再解锁
            private static readonly ManualResetEvent RatioLock = new ManualResetEvent(true); //HackCode绘制完再解锁

            internal static void HackCodeVisualize()
            {
                HackCodeLock.WaitOne();
                RatioLock.Reset();
                Console.SetCursorPosition(0, _currentCursorTopPosition);
                var hackCodeSplit = WAF_TextResource.HackCode_AJPy.Replace("\r\n", "\n").Replace("\t", "  ")
                    .Split('\n');
                Console.Write(hackCodeSplit[_currentCursorTopPosition % hackCodeSplit.Length]);
                _currentCursorTopPosition++;
                if (_currentCursorTopPosition > Console.WindowHeight - 3)
                {
                    Console.WindowTop++;
                    Console.CursorTop += 1;
                    Console.WriteLine(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                    Console.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                }

                RatioLock.Set();
            }

            internal static void RatioVisualize(double ratio)
            {
                RatioLock.WaitOne();
                HackCodeLock.Reset();
                var ratioInCycle = ratio;
                Console.SetCursorPosition(Console.WindowWidth / 2 - 39,
                    Console.WindowHeight + Console.WindowTop - 2);
                Console.Write(String.Empty.PadRight(Console.WindowWidth - 1, ' '));
                Console.SetCursorPosition(Console.WindowWidth / 2 - 39,
                    Console.WindowHeight + Console.WindowTop - 1);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($@"Defend:{ratioInCycle:0.00}%[");
                Console.ResetColor();
                Console.Write(string.Empty.PadRight((int) ratioInCycle / 2, '#'));
                Console.Write(string.Empty.PadRight(50 - (int) ratioInCycle / 2, ' '));
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($@"]{100 - ratioInCycle:0.00}%:Attack");
                Console.ResetColor();
                HackCodeLock.Set();
            }

            internal static int SlideVisualize() //你猜我在neta什么？
            {
                var tempTPos = Console.CursorTop;
                var tempLPos = Console.CursorLeft;

                var isFinish = false;

                Console.SetCursorPosition(0, Console.WindowTop);
                var divided = WAF_TextResource.AcStyle_Nodata.Replace("\r\n", "\n").Split('\n');
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var line in divided)
                {
                    Console.CursorLeft = Console.WindowWidth - 1 - line.Length;
                    Console.WriteLine(line);
                }

                Console.SetCursorPosition(0, Console.WindowTop + divided.Length - 1);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(WAF_TextResource.Pixy_Its_Time.PadLeft(
                    (Console.WindowWidth - 1) / 2 - WAF_TextResource.Pixy_Its_Time.Length / 2));
                Console.ResetColor();
                var enterLPos =
                    new Random().Next(Console.WindowWidth - 1 -
                                      WAF_TextResource.Enter_Frame.Split('\r')[0].Length);
                Console.SetCursorPosition(enterLPos, Console.WindowTop + Console.WindowHeight - 5);

                static int GetAuditionResult(int auditionResult)
                {
                    return auditionResult < 16 ? 0 //normal
                        : auditionResult < 28 ? 1 //good
                        : auditionResult < 32 ? 2 //perfect
                        : 3; //bad
                }

                void StopSlide()
                {
                    SendKeys.Flush();
                    while (Console.ReadKey(true).Key != ConsoleKey.Spacebar)
                    {
                    }
                    isFinish = true;
                }
                
                SendKeys.SendWait("Enter");


                //添加教程 说明使用空格操作
                Console.SetCursorPosition(0,
                    Console.WindowHeight + Console.WindowTop - 2);
                Console.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                Console.SetCursorPosition(Console.WindowWidth / 2 - 21,
                    Console.WindowHeight + Console.WindowTop - 2);
                Console.Write('[' + WAF_TextResource.Press_Space + ']');

                // 先按一次，熟悉操作
                Console.SetCursorPosition(0,
                    Console.WindowHeight + Console.WindowTop - 1);
                Console.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                Console.SetCursorPosition(Console.WindowWidth / 2 + 20,
                    Console.WindowHeight + Console.WindowTop - 1);
                Console.Write(']');
                Console.SetCursorPosition(Console.WindowWidth / 2 - 21,
                    Console.WindowHeight + Console.WindowTop - 1);
                Console.Write('[');
                Console.Write(WAF_TextResource.Press_Space_To_Continue);
                while (Console.ReadKey(true).Key != ConsoleKey.Spacebar)
                {
                }
                SendKeys.Flush();
                
                Console.SetCursorPosition(Console.WindowWidth / 2 - 20,
                    Console.WindowHeight + Console.WindowTop - 1);
                Console.Write(string.Empty.PadLeft(40));
                Console.SetCursorPosition(Console.WindowWidth / 2 - 20,
                    Console.WindowHeight + Console.WindowTop - 1);
                foreach (var ch in WAF_TextResource.Audition_Start)
                {
                    Console.Write(ch);
                    Thread.Sleep(50);
                }

                foreach (var unused in WAF_TextResource.Audition_Start)
                {
                    Console.Write(@" ");
                    Thread.Sleep(50);
                }

                var stopSlideTask = new Task(StopSlide);
                stopSlideTask.Start();
                
                while (true)
                {
                    for (var i = 0; i < 40; i++)
                    {
                        static ConsoleColor SelectColor(int i)
                        {
                           return i switch
                            {
                                0 => ConsoleColor.DarkCyan,
                                1 => ConsoleColor.Yellow,
                                2 => ConsoleColor.White,
                                3 => ConsoleColor.DarkMagenta,
                                _ => Console.ForegroundColor
                            };
                        }

                        Console.ForegroundColor = SelectColor(GetAuditionResult(i));
                        Console.Write('#');
                        Console.ResetColor();
                        Thread.Sleep(50);
                        if (!isFinish) continue;
                        
                        Console.SetCursorPosition(0,
                            Console.WindowHeight + Console.WindowTop - 2);
                        Console.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                        Console.SetCursorPosition(Console.WindowWidth / 2 - 21,
                            Console.WindowHeight + Console.WindowTop - 2);

                        var result = GetAuditionResult(i);

                        static string GetResultText(int i)
                        {
                            return i switch
                            {
                                0 => WAF_TextResource.Normal_Result,
                                1 => WAF_TextResource.Good_Result,
                                2 => WAF_TextResource.Perfect_Result,
                                3 => WAF_TextResource.Bad_Result,
                                _ => string.Empty
                            };
                        }

                        Console.Write('[');

                        foreach (var ch in GetResultText(result))
                        {
                            Console.Write($@"{ch}]");
                            Thread.Sleep(50);
                        }

                        Thread.Sleep(1000);

                        Console.CursorTop = tempTPos;
                        Console.CursorLeft = tempLPos;
                        return result;
                    }
                    for(var i = 0;i<40;i++)
                        Console.Write(@" ");
                }
            }
        }

        private static class SituationHandler
        {
            internal static double RatioUpdate(double defenceStatus, double incursionStatus)
            {
                return defenceStatus * 100 / (defenceStatus + incursionStatus);
            }

            internal static double OneClickDefence(ref ConsoleKeyInfo prevInput, double defenceStrength,
                double defenceStatus)
            {
                SendKeys.Flush();
                var currentInput = Console.ReadKey(true);
                if (prevInput == currentInput) return defenceStatus;
                StatusVisualizer.HackCodeVisualize();
                prevInput = currentInput;
                defenceStatus *= defenceStrength;
                return defenceStatus;
            }

            internal static bool AuditionCombat(ref double defenceStatus, ref double incursionStatus)
            {
                var isGetMasterKey = false;
                switch (StatusVisualizer.SlideVisualize())
                {
                    case 0:
                        break; //normal
                    case 1:
                        while (RatioUpdate(defenceStatus, incursionStatus) < 92)
                        {
                            defenceStatus += 5;
                            StatusVisualizer.HackCodeVisualize();
                            StatusVisualizer.RatioVisualize(RatioUpdate(defenceStatus, incursionStatus));
                            Thread.Sleep(50);
                        }
                        break; //good
                    case 2:
                        isGetMasterKey = true;
                        goto case 1; //perfect
                    case 3:
                        defenceStatus /= 2;
                        break; //bad
                }

                return isGetMasterKey;
            }
        }

        [Serializable]
        internal class CounterMeasure // 防御程序，每次进入就new一个
        {
            internal readonly CancellationTokenSource Source = new CancellationTokenSource();
            private readonly ManualResetEvent _cmLock = new ManualResetEvent(true);
            internal bool IsGetMasterKey { get; private set; }

            private double DefenceStatus
            {
                get => _defenceStatus;
                set
                {
                    _defenceStatus = value;
                    Ratio = SituationHandler.RatioUpdate(DefenceStatus, IncursionStatus);
                }
            }

            private double IncursionStatus
            {
                get => _incursionStatus;
                set
                {
                    _incursionStatus = value;
                    Ratio = SituationHandler.RatioUpdate(DefenceStatus, IncursionStatus);
                }
            }

            private double Ratio { get; set; } = 0.5;

            private double _incursionStatus = 100; //入侵的代码量，从WAFServer中获得
            private double _defenceStatus = 100; //防御的代码量，调用OneClickDefence来提升

            internal bool GetResult()
            {
                bool isPlayedQte = false;
                while (true)
                {
                    StatusVisualizer.RatioVisualize(Ratio);
                    if (Ratio < 10)
                    {
                        _cmLock.Set();
                        _cmLock.Dispose();
                        return false;
                    }

                    //输了
                    if (Ratio > 80 && !isPlayedQte) //进入QTE
                    {
                        _cmLock.Reset();
                        IsGetMasterKey = SituationHandler.AuditionCombat(ref _defenceStatus, ref _incursionStatus);
                        isPlayedQte = true;
                        _cmLock.Set();
                    }

                    if (Ratio > 90)
                    {
                        _cmLock.Set();
                        _cmLock.Dispose();
                        return true;
                    }

                    Thread.Sleep(50);
                }
            }


            internal CounterMeasure(ref double defenceStrength, bool isFirstTime)
            {
                Console.SetWindowSize(120, 30);
                // 4. 判断是否进入教程
                if (isFirstTime)
                {
                    Thread.Sleep(2000);
                    IntroTutorial();
                }
                
                // 5. 闪2次Status，开始
                for (var i = 0; i < 2; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2 - 39,
                                        Console.WindowHeight + Console.WindowTop - 1);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(@"Defend:50%[");
                    Console.ResetColor();
                    Console.Write(string.Empty.PadRight(25, '#'));
                    Console.Write(string.Empty.PadRight(25, ' '));
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(@"]50%:Attack");
                    Console.ResetColor();
                    
                    Thread.Sleep(400);
                    
                    Console.SetCursorPosition(Console.WindowWidth / 2 - 39,
                        Console.WindowHeight + Console.WindowTop - 1);
                    Console.Write(string.Empty.PadLeft(Console.WindowWidth,' '));
                    
                    Thread.Sleep(400);
                }
                
                
                var currentDefenceStrength = defenceStrength;
                var defenceTask = new Task(() =>
                {
                    var prevInput = new ConsoleKeyInfo();
                    while (!Source.Token.IsCancellationRequested)
                    {
                        _cmLock.WaitOne();
                        DefenceStatus =
                            SituationHandler.OneClickDefence(ref prevInput, currentDefenceStrength, DefenceStatus);
                    }
                }, Source.Token);
                _cmLock.Set();
                defenceTask.Start();
            }

            internal void AttackAnalyzer(double incursionStrength) // 处理攻击的数据
            {
                _cmLock.WaitOne();
                if (!Source.Token.IsCancellationRequested)
                    IncursionStatus += incursionStrength;
            }
        }

        private static void IntroTutorial()
        {
            // 1. 先走几个进度条
            Console.ResetColor();
            WriteAnimation.PrintBarOnly(WAF_TextResource.Wafc_Main_Sys,5);
            Thread.Sleep(500);
            Console.WriteLine();
            WriteAnimation.PrintBarOnly(WAF_TextResource.Wafc_Package_Sender_Sys,5);
            Thread.Sleep(500);
            Console.WriteLine();
            WriteAnimation.PrintBarOnly(WAF_TextResource.Wafc_Flow_Analyser_Sys,5);
            Thread.Sleep(500);
            Console.WriteLine();
            WriteAnimation.PrintBarOnly(WAF_TextResource.Wafc_Pseudo_Gui_Sys,5);
            Thread.Sleep(500);
            Console.WriteLine();
            
            // 2.用帮助文档形式来介绍系统吧！
            Console.Clear();
            static void ShinyDialog(string title, string text) //text不能超过3行，74字符长度，最后一行不超过60
            {
                var textSplit = text.Replace("\r\n","\n").Split('\n');
                var startingPointL = Console.WindowWidth / 2 - 40;
                var startingPointT = Console.WindowHeight - 10;

                Console.ResetColor();
                
                Console.SetCursorPosition(startingPointL,startingPointT);
                Console.WriteLine(string.Empty.PadRight(30,'#'));
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine('#'+string.Empty.PadRight(28)+'#');
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine(@"#  "+title.PadRight(26-UsefulTools.GetLength(title)+title.Length)+string.Empty.PadLeft(51,'#'));
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine('#'+string.Empty.PadRight(78)+'#');
                
                Console.CursorLeft = startingPointL;
                var startTPos = Console.CursorTop;
                var startLPos = Console.CursorLeft+3;
                Console.WriteLine('#'+string.Empty.PadRight(78)+'#');
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine('#'+string.Empty.PadRight(78)+'#');
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine(@"#  "+WAF_TextResource.Press_Enter.PadLeft(74-UsefulTools.GetLength(WAF_TextResource.Press_Enter)+WAF_TextResource.Press_Enter.Length)+@"  #");
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine('#'+string.Empty.PadRight(78)+'#');
                
                Console.CursorLeft = startingPointL;
                Console.WriteLine(string.Empty.PadRight(80,'#'));
                foreach (var str in textSplit)
                {
                    Console.SetCursorPosition(startLPos,startTPos);
                    foreach (var ch in str.Trim())
                    {
                        Console.Write(ch);
                        Thread.Sleep(20);
                    }
                    startTPos++;
                    while(Console.ReadKey(true).Key!=ConsoleKey.Enter){}
                    SendKeys.Flush();
                }
            }

            static void ShinyOption(string text, string key, int lPos, int tPos, ConsoleColor color) //text不能超过2行，14字符长度，key为1行，10字符长度
            {
                Console.ForegroundColor = color;
                var textSplit = text.Replace("\r\n","\n").Split('\n');
                
                Console.SetCursorPosition(lPos,tPos);
                Console.WriteLine(string.Empty.PadRight(20,'#'));
                
                Console.CursorLeft=lPos;
                Console.WriteLine('#'+string.Empty.PadRight(18)+'#');
                
                Console.CursorLeft=lPos;
                var startTPos = Console.CursorTop;
                var startLPos = Console.CursorLeft+3;
                Console.WriteLine('#'+string.Empty.PadRight(18)+'#');
                
                Console.CursorLeft=lPos;
                Console.WriteLine('#'+string.Empty.PadRight(18)+'#');
                
                Console.CursorLeft=lPos;
                Console.WriteLine('#'+string.Empty.PadRight(18)+'#');
                
                Console.CursorLeft=lPos;
                Console.Write('#');
                Console.ResetColor();
                Console.Write((string.Empty.PadLeft(9-UsefulTools.GetLength(key)/2)+key).PadRight(18-UsefulTools.GetLength(key)+key.Length));
                Console.ForegroundColor = color;
                Console.WriteLine('#');
                
                Console.CursorLeft=lPos;
                Console.WriteLine('#'+string.Empty.PadRight(18)+'#');
                
                Console.CursorLeft=lPos;
                Console.WriteLine(string.Empty.PadRight(20,'#'));

                foreach (var str in textSplit)
                {
                    Console.SetCursorPosition(startLPos,startTPos);
                    Console.Write(str.Trim());
                    startTPos++;
                }
            }
            Console.Clear();
            while (true)
            {
                Console.Clear();
                ShinyDialog(WAF_TextResource.Wafc_HelpTitle,WAF_TextResource.Wafc_HelpText);
                ShinyOption(WAF_TextResource.Wafc_About,ConsoleKey.A.ToString(),10,8,ConsoleColor.Magenta);
                ShinyOption(WAF_TextResource.Wafc_Manual,ConsoleKey.W.ToString(),50,5,ConsoleColor.DarkGreen);
                ShinyOption(WAF_TextResource.Wafc_Exit,ConsoleKey.D.ToString(),90,8,ConsoleColor.DarkYellow);

                ConsoleKey option1;
                do
                {
                    option1 = Console.ReadKey(true).Key;
                } while (option1 != ConsoleKey.A && option1 != ConsoleKey.W && option1 != ConsoleKey.D);
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (option1)
                { 
                    case ConsoleKey.A: 
                    {
                        Console.Clear();
                        ShinyDialog(WAF_TextResource.Wafc_HelpTitle,WAF_TextResource.Wafc_About_Text);
                        break;
                    }
                    case ConsoleKey.W:
                    {
                        Console.Clear();
                        ShinyDialog(WAF_TextResource.Wafc_HelpTitle,WAF_TextResource.Wafc_Manual_Text);
                        break;
                    }
                    case ConsoleKey.D:
                    {
                        Console.Clear();
                        ShinyDialog(WAF_TextResource.Wafc_HelpTitle,WAF_TextResource.Wafc_Exit_Text);
                        ShinyOption(WAF_TextResource.Wafc_Exit_Yes,ConsoleKey.Y.ToString(),20,8,ConsoleColor.Magenta);
                        ShinyOption(WAF_TextResource.Wafc_Exit_No,ConsoleKey.N.ToString(),80,8,ConsoleColor.DarkGreen);
                        ConsoleKey option2;
                        do
                        {
                            option2 = Console.ReadKey(true).Key;
                        } while (option2 != ConsoleKey.Y && option2 != ConsoleKey.N);
                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (option2)
                        {
                            case ConsoleKey.Y:
                            {
                                return;
                            }
                            case ConsoleKey.N:
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        private static readonly Shell.Command InstallWafcCommand = new Shell.Command(
            "inswafc",
            (argList, host) =>
            {
                const string commandName = "inswafc";
                if(argList.Count!=0)
                    throw new CustomException.UnknownArgumentException(commandName);
                try
                {
                    WaFirecracker test = ((dynamic) host).wafc;
                    if (test.IsAcquiredMasterKey)
                        Console.WriteLine(WAF_TextResource.Wafc_ExpAcquired, commandName);
                    else
                        Console.WriteLine(WAF_TextResource.Wafc_DefenceLevel, commandName,
                            (test.DefenceStrength - 1) / 0.04);
                }
                catch
                {
                    ((dynamic) host).wafc=new WaFirecracker();
                    Console.WriteLine(WAF_TextResource.Wafc_Installed, commandName);
                }

            },
            WAF_TextResource.Inswafc_Help);
        
        internal static readonly Host.FileSystem.Exec WafcExec = new Host.FileSystem.Exec("WAFirecracker",
            new List<Shell.Command>(new[]
            {
                InstallWafcCommand
            }), true);
    }
}