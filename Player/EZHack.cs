using System;
using System.Collections.Generic;
using System.Threading;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Player
{
    public static class EzHack //实现nmap,brute
    {
        private static readonly Shell.Command BruteCommand = new Shell.Command(
            "brute",
            (argList, host) =>
            {
                const string commandName = "brute";
                string[] dict;
                switch (argList.Count)
                {
                    case 1:
                        dict = EzHack_Dict.Password_Dict.Replace("\r\n", "\n").Split('\n');
                        break;
                    case 3:
                    {
                        if (argList[1] == "-f")
                        {
                            var backupDir = host.Fs.CurrentDir;
                            if (argList[2].Contains("/"))
                            {
                                var oldPath = argList[2].Substring(0, argList[0].LastIndexOf('/')); //目录位置
                                argList[2] = argList[2].Remove(0, argList[2].LastIndexOf('/') + 1); //新目录名
                                var oldArg = new List<string> {oldPath};
                                if (oldPath != string.Empty)
                                    try
                                    {
                                        ShellCommandDict.ChangeDirCommand.Execute(oldArg, host);
                                    }
                                    catch (CustomException.FileNotExistException)
                                    {
                                        throw new CustomException.FileNotExistException(commandName);
                                    }
                            }

                            if (argList[2] == string.Empty) //没有输入文件名（arg0以斜杠结尾）
                                throw new CustomException.UnknownArgumentException(commandName);

                            if (host.Fs.CurrentDir.Transfer(argList[2]) == null) //没有找到文件
                                throw new CustomException.FileNotExistException(commandName);
                            dict = host.Fs.CurrentDir.Transfer(argList[2]).Output().Split('\n');
                            host.Fs.CurrentDir = backupDir;
                        }
                        else
                        {
                            throw new CustomException.UnknownArgumentException(commandName);
                        }

                        break;
                    }
                    default:
                        throw new CustomException.UnknownArgumentException(commandName);
                }

                if (!LinkServer.FindHostByAddress(argList[0], host))
                {
                    Console.WriteLine(EzHack_TextResource.Brute_ConnectionFailed);
                    return; //ip wrong
                }

                foreach (var pw in dict)
                {
                    Thread.Sleep(50);
                    Console.WriteLine(pw);
                    var result = LinkServer.FindHostByAddress(argList[0], pw.Trim('\r'), host);
                    if (result == null) continue;
                    Console.WriteLine(EzHack_TextResource.Brute_SaveMap, pw);
                    Thread.Sleep(5000);
                    Link.OpenRemoteShell(result);
                    return;
                }

                Console.WriteLine(EzHack_TextResource.Brute_Failed);
            },
            EzHack_TextResource.Brute_Help);

        private static readonly Link.RemoteCommand ProbeCommand = new Link.RemoteCommand(
            "probe",
            (argList, remoteHost, connectorHost) =>
            {
                const string commandName = "probe";
                if (argList.Count != 0)
                    throw new CustomException.UnknownArgumentException(commandName);
                foreach (var r in Link.GetRouter(remoteHost)) Console.WriteLine(r.SelfAddress + @"	" + r.Information);
            },
            EzHack_TextResource.Probe_Help);

        internal static readonly Host.FileSystem.Exec EzHackExec = new Host.FileSystem.Exec("EZHack",
            new List<Shell.Command>(new[]
            {
                BruteCommand,
                ProbeCommand
            }), true);
    }
}