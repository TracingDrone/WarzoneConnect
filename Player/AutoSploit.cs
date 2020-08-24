using System;
using System.Collections.Generic;
using System.Linq;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

// ReSharper disable StringLiteralTypo

// ReSharper disable CommentTypo

namespace WarzoneConnect.Player
{
    public static class AutoSploit
    {
        internal static readonly Host.FileSystem.Exec AsfExec = new Host.FileSystem.Exec("ASFramework",
            new List<Shell.Command>(new[]
            {
                new Shell.Command("asfconsole", AsfConsole, AutoSploit_TextResource.AsfConsole_Help)
            }), true);

        internal static void RegisterExpFile()
        {
            static void IdentifyExp(Host.FileSystem.File file)
            {
                if (!(file is ExpFile)) return;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Yellow;
            }

            ShellCommandDict.FileIdentifier += IdentifyExp;
        }

        private static void AsfConsole(IReadOnlyCollection<string> argList, Host host)
        {
            try
            {
                if (argList.Count != 0)
                    throw new CustomException.UnknownArgumentException();
                //1.exp
                //2.输入ip地址
                //3.攻击
                var dirName = "AsfExp";
                Console.WriteLine(AutoSploit_TextResource.Banner);
                var f = host.GetRoot().FileList.Find(d => d.Name == dirName);
                if (!(f is Host.FileSystem.Dir))
                {
                    try
                    {
                        host.GetRoot().Add(new Host.FileSystem.Dir(dirName));
                    }
                    catch
                    {
                        throw new CreateDirFailedException("asfconsole");
                    }

                    f = host.GetRoot().FileList.Find(d => d.Name == dirName);
                }

                if (f is Host.FileSystem.Dir expDir)
                {
                    var exps = expDir.FileList.Where(file => file is ExpFile).Cast<ExpFile>().ToList();
                    var index = 0;

                    void ChangeFocus(int idx)
                    {
                        foreach (var exp in exps)
                        {
                            if (idx < exps.Count && exp == exps[idx])
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }

                            Console.WriteLine(
                                $@"{exp.Name}	{ExpFile.GetSystem(exp.System)}	{ExpFile.GetRank(exp.Rank)}");
                            Console.ResetColor();
                        }

                        if (idx == exps.Count)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }

                        Console.WriteLine(AutoSploit_TextResource.Exit);
                        Console.ResetColor();
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (exps.Count > 0)
                    {
                        Console.WriteLine(AutoSploit_TextResource.Sheet);
                        var currentTPos = Console.CursorTop;
                        Console.CursorVisible = false;
                        ChangeFocus(index);
                        var cki = Console.ReadKey();
                        while (cki.Key != ConsoleKey.Enter)
                        {
                            Console.CursorTop = currentTPos;
                            Console.CursorLeft = 0;
                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                            switch (cki.Key)
                            {
                                case ConsoleKey.UpArrow:
                                {
                                    if (index > 0)
                                    {
                                        index--;
                                        ChangeFocus(index);
                                    }

                                    break;
                                }
                                case ConsoleKey.DownArrow:
                                {
                                    if (index < exps.Count)
                                    {
                                        index++;
                                        ChangeFocus(index);
                                    }

                                    break;
                                }
                            }

                            cki = Console.ReadKey();
                        }

                        if (index == exps.Count)
                            return;
                        Console.WriteLine(AutoSploit_TextResource.DescriptionOutput,
                            exps[index].Name,
                            ExpFile.GetSystem(exps[index].System),
                            exps[index].Description,
                            exps[index].Date.Year, exps[index].Date.Month, exps[index].Date.Day,
                            ExpFile.GetRank(exps[index].Rank));
                        Console.WriteLine(AutoSploit_TextResource.InputIP);
                        Console.CursorVisible = true;
                        var ip = Console.ReadLine();
                        if (ip == "exit")
                            return;
                        if (!LinkServer.FindHostByAddress(ip, host))
                        {
                            Console.WriteLine(AutoSploit_TextResource.ConnectionFailed);
                            return;
                        }

                        var pw = AutoSploitServer.ExploitUsingExps(ip, exps[index].Exp, host);
                        if (pw is bool) //失败
                        {
                            Console.WriteLine(AutoSploit_TextResource.TargetImmune);
                            return;
                        }

                        Console.WriteLine(AutoSploit_TextResource.SaveMap, (string) pw);
                        Link.OpenRemoteShell(LinkServer.FindHostByAddress(ip, (string) pw, host));
                    }
                    else
                    {
                        Console.WriteLine(AutoSploit_TextResource.NoExpsAvailable);
                    }

                    Console.CursorVisible = true;
                }
                else
                {
                    Console.WriteLine(AutoSploit_TextResource.CannotAccessDirectory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [Serializable]
        internal class ExpFile : Host.FileSystem.File
        {
            internal ExpFile(string name, string description, DateTime date, int rank, string exp, int system) :
                base(name)
            {
                Description = description;
                Date = date;
                Rank = rank;
                Exp = exp;
                System = system;
            }

            internal string Description { get; }
            internal DateTime Date { get; }
            internal int Rank { get; }
            internal string Exp { get; }
            internal int System { get; } //0为Door，1为UBurst

            internal static string GetRank(int rank)
            {
                return rank switch
                {
                    0 => "Perfect",
                    1 => "Great",
                    2 => "Good",
                    _ => "Miss"
                };
            }

            internal static string GetSystem(int system)
            {
                return system switch
                {
                    0 => "Door",
                    1 => "UBurst",
                    _ => "Unknown"
                };
            }

            internal override string Output()
            {
                return $"Name:\t{Name}\n" +
                       $"System:\t{GetSystem(System)}\n" +
                       $"Desc:\t{Description}\n" +
                       $"Date:\t{Date.Year}/{Date.Month}/{Date.Day}\n" +
                       $"Rank:\t{GetRank(Rank)}\n\n" +
                       $"Exp:\n{Exp}";
            }

            public override object Clone()
            {
                return new ExpFile(Name, Description, Date, Rank, Exp, System);
            }
        }

        private class CreateDirFailedException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public CreateDirFailedException(string commandName) : base(
                $"{commandName} - {AutoSploit_TextResource.CreateDirFailed}")
            {
            }
        }
    }
}