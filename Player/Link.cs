using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

// ReSharper disable CommentTypo

namespace WarzoneConnect.Player
{
    public static class Link //实现connect,tf,dc
    {
        [Serializable]
        internal class Router //不同计算机之间连接的情况
        {
            internal readonly string SelfAddress;
            internal readonly string Information;

            internal Router(string sa, string info)
            {
                SelfAddress = sa;
                Information = info;
                ConnectedAddress=new List<Router>();
            }

            internal List<Router> ConnectedAddress { get; }
        }

        [Serializable]
        internal class RemoteCommand : Shell.Command
        {
            internal RemoteCommand(string name, Action<List<string>, Host, Host> remoteCommandAction, string helpText)
                : base(name, ((list, host) =>
                {
                    Console.WriteLine(Link_TextResource.InvokeError);
                }), helpText)
            {
                _remoteCommandAction = remoteCommandAction;
            }

            private readonly Action<List<string>, Host, Host> _remoteCommandAction;
            
            internal override void RegisterCommand(Host host)
            {
                ((List<RemoteCommand>)((dynamic)host.Sh).RemoteCommandList).Add(this);
                _connectorHost = host;
            }

            internal override void Execute(List<string> argList, Host host)
            {
                _remoteCommandAction(argList, host, _connectorHost);
            }

            private Host _connectorHost;
        }

        private static readonly Shell.Command ConnectCommand=new Shell.Command("connect",
            (argList, host) =>
            {
                const string commandName = "connect";
                if (argList.Count != 1)
                    throw new CustomException.UnknownArgumentException(commandName);
                if(!LinkServer.FindHostByAddress(argList[0],host))
                {
                    throw new ConnectionFailedException(commandName,argList[0]);
                }
                Console.Write(Link_TextResource.InputPassword,argList[0]);
                var cki=Console.ReadKey(true);
                var password = string.Empty;
                while (cki.Key != ConsoleKey.Enter)
                { 
                    if (cki.Key == ConsoleKey.Backspace)
                    { 
                        if (password.Length != 0)
                        {
                            password=password.Remove(password.Length - 1);
                            Console.Write(@" ");
                        }
                    }
                    else
                    { 
                        Console.Write('*');
                        password += cki.KeyChar.ToString();
                    }
                    cki = Console.ReadKey(true);
                }
                Console.WriteLine();
                var result = LinkServer.FindHostByAddress(argList[0], password,host);
                if (result == null)
                {
                    throw new WrongPasswordException(commandName);
                }
                OpenRemoteShell(result);
            },
            Link_TextResource.Connect_Help);
        
        private static readonly RemoteCommand DisconnectCommand=new RemoteCommand("dc",
            (argList, remoteHost, connectorHost) =>
            {
                const string commandName = "dc";
                if (argList.Count != 0) 
                    throw new CustomException.UnknownArgumentException(commandName);
                Thread.CurrentThread.Abort(string.Format(Link_TextResource.Disconnect, commandName));
            },
            Link_TextResource.Dc_Help);
        
        private static readonly RemoteCommand TransferCommand=new RemoteCommand("tf",
            (argList, remoteHost, connectorHost) =>
            {
                const string commandName = "tf";
                var backupDir = remoteHost.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 1 && argList.Count != 2)
                        throw new CustomException.UnknownArgumentException(commandName);
                    if (argList[0].Contains("/"))
                    {
                        var oldPath = argList[0].Substring(0, argList[0].LastIndexOf('/')); //目录位置
                        argList[0] = argList[0].Remove(0, argList[0].LastIndexOf('/') + 1); //新目录名
                        var oldArg = new List<string> {oldPath};
                        if (oldPath != string.Empty)
                            try
                            {
                                ShellCommandDict.ChangeDirCommand.Execute(oldArg, remoteHost);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (argList[0] == string.Empty) //没有输入文件名（arg0以斜杠结尾）
                        throw new CustomException.UnknownArgumentException(commandName);

                    if (remoteHost.Fs.CurrentDir.Transfer(argList[0]) == null) //没有找到文件
                    {
                        throw new CustomException.FileNotExistException(commandName);
                    }
                    try
                    {
                        LinkServer.FileTransfer(argList, remoteHost.Fs.CurrentDir.Transfer(argList[0]),connectorHost);
                    }
                    catch (CustomException.UnknownArgumentException)
                    {
                        throw new CustomException.UnknownArgumentException(commandName);
                    }
                    catch (CustomException.NameConflictException)
                    {
                        throw new CustomException.NameConflictException(commandName);
                    }

                }
                finally
                {
                    remoteHost.Fs.CurrentDir = backupDir;
                }
            },
            Link_TextResource.Tf_Help);
        
        internal static readonly Host.FileSystem.Exec LinkExec = new Host.FileSystem.Exec("link", 
            new List<Shell.Command>(new[]
            {
                ConnectCommand,
                DisconnectCommand,
                TransferCommand
            }),false);
        
        internal static void OpenRemoteShell(RemoteShell rs)
        {
            Console.Clear();
            var remoteConnectionTask=new Task(new Terminal(rs).Open);
            try
            {
                remoteConnectionTask.Start();
            }
            catch (ThreadAbortException tbe)
            {
                Console.WriteLine((string)tbe.ExceptionState);
            }
            Task.WaitAny(remoteConnectionTask);
        }

        internal static List<Router> GetRouter(Host host)
        {
            return ((Router)((dynamic) host).Rt).ConnectedAddress;
        }

        internal class RemoteShell : Shell
        {
            private List<RemoteCommand> RemoteCommandList { get; }

            internal RemoteShell(Host ch,Host remoteHost) : base(remoteHost)
            { 
                RemoteCommandList = (List<RemoteCommand>)((dynamic)ch.Sh).RemoteCommandList;
                CommandList = remoteHost.Sh.CommandList;
            }

            
            
            internal override void PreInputPrint()
            {
                
                var currentTPos = Console.CursorTop;
                Console.CursorTop = 0;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Link_TextResource.RemoteShell_Connected, ThisHost.Sh.GetAddress());
                Console.ResetColor();
                if (currentTPos == 0)
                    currentTPos++;
                Console.CursorTop = currentTPos;
                base.PreInputPrint();
            }

            internal override void CheckExecList(string commandString, List<string> argList)
            {
                try
                {
                    if (!RemoteCommandList.Exists(rc =>
                        rc.Name == commandString))
                    {
                        if (commandString == ShellCommandDict.HelpCommand.Name)
                        {
                            ShellCommandDict.HelpCommand.Execute(argList, ThisHost);
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine('\n'+Link_TextResource.RemoteTerminal_ExtraExec);
                            foreach (var rcl in RemoteCommandList)
                            {
                                Console.WriteLine(rcl.HelpText);
                            }
                            Console.ResetColor();
                        }
                        else
                            base.CheckExecList(commandString, argList);
                    }
                    else
                    {
                        RemoteCommandList.Find(rc => rc.Name == commandString).Execute(argList, ThisHost);
                    }

                }
                catch (ThreadAbortException abortException)
                {
                    Console.WriteLine(abortException.ExceptionState);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private class WrongPasswordException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public WrongPasswordException(string commandName) : base(
                $"{commandName} - {Link_TextResource.WrongPassword}")
            {
            }
        }
        
        private class ConnectionFailedException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public ConnectionFailedException(string commandName, string address) : base(
                string.Format(Link_TextResource.ConnectionFailed, commandName, address))
            {
            }
        }
    }
}

