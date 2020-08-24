using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Player
{
    [Serializable]
    internal class Shell : DynamicObject
    {
        internal readonly Host ThisHost;

        internal Dictionary<string, object> Prop = new Dictionary<string, object>(); //Properties

        internal Shell(Host host)
        {
            ThisHost = host;
        }

        internal List<Command> CommandList { get; set; } = new List<Command>(); //Shell的指令

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Prop.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Prop[binder.Name] = value;
            return true;
        }

        internal virtual void PreInputPrint()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(@"{0}@{1}", GetUser(), GetAddress());
            Console.ResetColor();
            Console.Write(@":");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(GetCurrentDir());
            Console.ResetColor();
            Console.Write(@" >");
            Console.CursorVisible = true;
        }

        internal string GetUser()
        {
            return ThisHost.User;
        }

        internal string GetAddress()
        {
            return ThisHost is HackTool ? "hacktool" : ThisHost.Addr;
        }

        internal string GetCurrentDir()
        {
            var dirList = new List<string>();
            var thisDir = ThisHost.Fs.CurrentDir;
            dirList.Add(thisDir.Name);
            while (thisDir.ParentDir != null)
            {
                thisDir = thisDir.ParentDir;
                dirList.Add(thisDir.Name);
            }

            dirList.RemoveAt(dirList.Count - 1);
            dirList.Reverse();
            return dirList.Aggregate("root", (current, dir) => current + "/" + dir);
        }

        internal virtual void CommandIdentify(string command) //整理输入内容
        {
            if (command.Trim() == string.Empty)
                return;
            var commandSplit = command.Trim().Split(' ');
            var argList = commandSplit.ToList();
            argList.RemoveAt(0);
            CheckExecList(commandSplit[0], argList);
        }

        internal virtual void
            CheckExecList(string commandString,
                List<string> argList) //识别命令
        {
            try
            {
                var com = CommandList.Find(c => c.Name == commandString);
                if (com == null)
                    throw new UnknownCommandException();
                com.Execute(argList, ThisHost);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // private void HelpAction(IReadOnlyCollection<string> argList) //help
        // {
        //     try
        //     {
        //         if (argList.Count != 0) //只能获取一个参数
        //             throw new UnknownArgumentException();
        //         Console.WriteLine(Shell_TextResource.Help);
        //         foreach (var command in CommandList) Console.WriteLine(command.HelpText);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //     }
        // }

        /*private void ListAction(List<string> argList) //ls
        {
            var backupDir = _thisHost.Fs.CurrentDir;
            try
            {
                switch (argList.Count)
                {
                    case 0: break; //无参数
                    case 1: //1参数，调用cd更改ls执行目录
                    {
                        if (ChangeDirAction(argList))
                            break;
                        return;
                    }
                    default: //多参数
                    {
                        throw new UnknownArgumentException();
                    }
                }
                _thisHost.Fs.CurrentDir.FileList.Sort((x, y) =>string.Compare(x.Name, y.Name, StringComparison.Ordinal) );
                foreach (var file in _thisHost.Fs.CurrentDir.FileList)
                {
                    switch (file)
                    {
                        case Host.FileSystem.Dir _:
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.BackgroundColor = ConsoleColor.Green;
                            break;
                        case Host.FileSystem.Exec _:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            break;
                        case Host.FileSystem.Video _:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.BackgroundColor = ConsoleColor.Blue;
                            break;
                    }

                    Console.Write(file.Name);
                    Console.ResetColor();
                    Console.Write('\t');
                }

                if (_thisHost.Fs.CurrentDir.FileList.Count > 0)
                    Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _thisHost.Fs.CurrentDir = backupDir;
            }
        }*/

        // internal bool ChangeDirAction(List<string> argList) //cd
        // {
        //     var backupDir = thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         switch (argList.Count)
        //         {
        //             case 0: return true; //不调用，直接返回
        //             case 1: break; //可调用
        //             default: throw new UnknownArgumentException(); //参数不正确
        //         }
        //
        //         if (argList[0][0] == '/') argList[0] = argList[0].Remove(0, 1);
        //         var path = argList[0].Split('/');
        //         foreach (var nextDirPath in path)
        //         {
        //             Host.FileSystem.Dir nextDir;
        //             if (nextDirPath == "..")
        //                 nextDir = thisHost.Fs.CurrentDir.ParentDir;
        //             else
        //                 nextDir = (Host.FileSystem.Dir) thisHost.Fs.CurrentDir.FileList
        //                     .Find(d => d.Name == nextDirPath);
        //             thisHost.Fs.CurrentDir = nextDir ?? throw new FileNotExistException();
        //         }
        //
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         thisHost.Fs.CurrentDir = backupDir;
        //         Console.WriteLine(e.Message); //未来写在resx里，还要给不同错误详细分类
        //         return false;
        //     }
        // }

        // private void MakeDirAction(List<string> argList) //mkdir
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 1) //只能获取一个参数
        //             throw new UnknownArgumentException();
        //         var dirName = argList[0];
        //         if (dirName.Contains("/"))
        //         {
        //             var path = dirName.Substring(0, dirName.LastIndexOf('/')); //目录位置
        //             dirName = dirName.Remove(0, dirName.LastIndexOf('/') + 1); //新目录名
        //             argList[0] = path;
        //             if (path != string.Empty)
        //                 if (!ChangeDirAction(argList))
        //                     return;
        //         }
        //
        //         if (dirName == string.Empty)
        //             throw new UnknownArgumentException();
        //         if (_thisHost.Fs.CurrentDir.Add(new Host.FileSystem.Dir(dirName)) == 1)
        //             throw new NameConflictException();
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }

        // private void RemoveAction(List<string> argList) //rm
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 1) //只能获取一个参数
        //             throw new UnknownArgumentException();
        //         var fileName = argList[0];
        //         if (fileName.Contains("/"))
        //         {
        //             var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
        //             fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
        //             argList[0] = path;
        //             if (path != string.Empty)
        //                 if (!ChangeDirAction(argList))
        //                     return;
        //         }
        //
        //         if (fileName == string.Empty)
        //             throw new UnknownArgumentException();
        //         if (_thisHost.Fs.CurrentDir.Delete(fileName) == 2)
        //             throw new FileNotExistException();
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message); //未来写在resx里，还要给不同错误详细分类
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }


        // private void CopyAction(IReadOnlyList<string> argList) //cp
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 2) //只能获取2个参数
        //             throw new UnknownArgumentException();
        //
        //         var oldName = argList[0];
        //         if (oldName.Contains("/"))
        //         {
        //             var oldPath = oldName.Substring(0, oldName.LastIndexOf('/')); //目录位置
        //             oldName = oldName.Remove(0, oldName.LastIndexOf('/') + 1); //新目录名
        //             var oldArg = new List<string> {oldPath};
        //             if (oldPath != string.Empty)
        //                 if (!ChangeDirAction(oldArg))
        //                     return;
        //         }
        //
        //         if (oldName == string.Empty) //没有输入文件名（arg0以斜杠结尾）
        //             throw new UnknownArgumentException();
        //         Host.FileSystem.File cpFile;
        //         if (_thisHost.Fs.CurrentDir.Transfer(oldName) != null)
        //         {
        //             if (_thisHost.Fs.CurrentDir.Transfer(oldName) is Host.FileSystem.Dir)
        //             {
        //                 var cpDir = new Host.FileSystem.Dir(oldName);
        //                 cpDir.RebuildFileList((Host.FileSystem.Dir) _thisHost.Fs.CurrentDir.Transfer(oldName));
        //                 cpFile = cpDir;
        //             }
        //             else
        //             {
        //                 cpFile = _thisHost.Fs.CurrentDir.Transfer(oldName);
        //             }
        //         }
        //         else //没有找到文件
        //         {
        //             throw new FileNotExistException();
        //         }
        //
        //         _thisHost.Fs.CurrentDir = backupDir;
        //
        //         var newName = argList[1];
        //         if (newName.Contains("/"))
        //         {
        //             var newPath = newName.Substring(0, newName.LastIndexOf('/')); //目录位置
        //             newName = newName.Remove(0, newName.LastIndexOf('/') + 1); //新目录名
        //             var newArg = new List<string>
        //             {
        //                 newPath
        //             };
        //             if (newPath != string.Empty)
        //                 if (!ChangeDirAction(newArg))
        //                     return;
        //         }
        //
        //         if (newName == string.Empty) //没有输入文件名（arg1以斜杠结尾）
        //             newName = oldName;
        //         cpFile.Name = newName;
        //         if (_thisHost.Fs.CurrentDir.Add(cpFile) == 1) //存在同名文件
        //             throw new NameConflictException();
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }

        // private void MoveAction(IReadOnlyList<string> argList) //mv
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 2) //只能获取2个参数
        //             throw new UnknownArgumentException();
        //
        //         var oldName = argList[0];
        //         var oldPath = string.Empty;
        //         var oldArg = new List<string>();
        //         if (oldName.Contains("/"))
        //         {
        //             oldPath = oldName.Substring(0, oldName.LastIndexOf('/')); //目录位置
        //             oldName = oldName.Remove(0, oldName.LastIndexOf('/') + 1); //新目录名
        //             oldArg.Add(oldPath);
        //             if (oldPath != string.Empty)
        //                 if (!ChangeDirAction(oldArg))
        //                     return;
        //         }
        //
        //         if (oldName == string.Empty) //没有输入文件名（arg0以斜杠结尾）
        //             throw new ArgumentException();
        //         if (_thisHost.Fs.CurrentDir.Transfer(oldName) != null)
        //         {
        //             var mvFile = _thisHost.Fs.CurrentDir.Transfer(oldName);
        //             _thisHost.Fs.CurrentDir = backupDir;
        //             var newName = argList[1];
        //             var newPath = string.Empty;
        //             if (newName.Contains("/"))
        //             {
        //                 newPath = newName.Substring(0, newName.LastIndexOf('/')); //目录位置
        //                 newName = newName.Remove(0, newName.LastIndexOf('/') + 1); //新目录名
        //                 var newArg = new List<string>
        //                 {
        //                     newPath
        //                 };
        //                 if (newPath != string.Empty)
        //                     if (!ChangeDirAction(newArg))
        //                         return;
        //             }
        //
        //             if (newName == string.Empty) //没有输入文件名（arg1以斜杠结尾）
        //                 throw new UnknownArgumentException(); //没有输入新名称
        //             if (newPath == oldPath)
        //                 switch (_thisHost.Fs.CurrentDir.Rename(oldName, newName))
        //                 {
        //                     case 0: return;
        //                     case 1: throw new NameConflictException(); //存在同名文件
        //                     case 2: throw new FileNotExistException(); //文件不存在(应该不会在这里报错）
        //                 }
        //
        //             mvFile.Name = newName;
        //             _thisHost.Fs.CurrentDir.Add(mvFile);
        //             _thisHost.Fs.CurrentDir = backupDir;
        //             ChangeDirAction(oldArg);
        //             _thisHost.Fs.CurrentDir.Delete(newName);
        //         }
        //         else //没有找到文件
        //         {
        //             throw new FileNotExistException();
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message); //未来写在resx里，还要给不同错误详细分类
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }

        // private void InstallAction(List<string> argList) //install
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 1) //只能获取一个参数
        //             throw new UnknownArgumentException();
        //         var fileName = argList[0];
        //         if (fileName.Contains("/"))
        //         {
        //             var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
        //             fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
        //             argList[0] = path;
        //             if (path != string.Empty)
        //                 if (!ChangeDirAction(argList))
        //                     return;
        //         }
        //
        //         if (fileName == string.Empty)
        //             throw new FileNotExistException(); //文件不存在
        //         var exec = _thisHost.Fs.CurrentDir.Transfer(fileName);
        //         switch (exec)
        //         {
        //             case null:
        //                 throw new FileNotExistException(); //文件不存在
        //             case Host.FileSystem.Exec exec1:
        //                 _thisHost.InstallExec(exec1);
        //                 break;
        //             default:
        //                 throw new FormatException(); //格式错误
        //         }
        //
        //
        //         Console.WriteLine(Shell_TextResource.InstallAction_Finish);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }

        // private void ConcatenateAction(List<string> argList) //cat
        // {
        //     var backupDir = _thisHost.Fs.CurrentDir;
        //     try
        //     {
        //         if (argList.Count != 1) //只能获取一个参数
        //             throw new UnknownArgumentException();
        //         var fileName = argList[0];
        //         if (fileName.Contains("/"))
        //         {
        //             var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
        //             fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
        //             argList[0] = path;
        //             if (path != string.Empty)
        //                 if (!ChangeDirAction(argList))
        //                     return;
        //         }
        //
        //         if (fileName == string.Empty)
        //             throw new FileNotExistException(); //文件不存在
        //         var doc = _thisHost.Fs.CurrentDir.Transfer(fileName);
        //         switch (doc)
        //         {
        //             case null:
        //             case Host.FileSystem.Dir _:
        //                 throw new FileNotExistException(); //文件不存在
        //             default:
        //                 Console.WriteLine(doc.Output());
        //                 break;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //     }
        //     finally
        //     {
        //         _thisHost.Fs.CurrentDir = backupDir;
        //     }
        // }
        [Serializable]
        internal class Command
        {
            private readonly Action<List<string>, Host> _commandAction;

            internal Command(string name, Action<List<string>, Host> commandAction, string helpText)
            {
                Name = name;
                _commandAction = commandAction;
                HelpText = helpText;
            }

            internal string Name { get; }

            internal string HelpText { get; }

            internal virtual void RegisterCommand(Host host)
            {
                host.Sh.CommandList.Add(this);
            }

            internal virtual void Execute(List<string> argList, Host host)
            {
                _commandAction(argList, host);
            }
        }

        private class NameConflictException : Exception
        {
            public override string Message { get; } = Shell_TextResource.NameConflict;
        }

        private class FileNotExistException : Exception
        {
            public override string Message { get; } = Shell_TextResource.FileNotExist;
        }

        internal class UnknownArgumentException : Exception
        {
            public override string Message { get; } = Shell_TextResource.UnknownArgument;
        }

        private class UnknownCommandException : Exception
        {
            public override string Message { get; } = Shell_TextResource.UnknownCommand;
        }

        private class FormatException : Exception
        {
            public override string Message { get; } = Shell_TextResource.MismatchedFormat;
        }

        private class InstalledException : Exception
        {
            public override string Message { get; } = Shell_TextResource.Installed;
        }

        private class RequireDllException : Exception
        {
            public override string Message { get; } = Shell_TextResource.RequireDLL;
        }
    }

    internal static class ShellCommandDict
    {
        internal static readonly Shell.Command ChangeDirCommand = new Shell.Command(
            "cd",
            (argList, host) =>
            {
                const string commandName = "cd";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    switch (argList.Count)
                    {
                        case 0: return; //不调用，直接返回
                        case 1: break; //可调用
                        default: throw new CustomException.UnknownArgumentException(commandName); //参数不正确
                    }

                    if (argList[0][0] == '/') argList[0] = argList[0].Remove(0, 1);
                    var path = argList[0].Split('/');
                    foreach (var nextDirPath in path)
                    {
                        Host.FileSystem.Dir nextDir;
                        if (nextDirPath == "..")
                            nextDir = host.Fs.CurrentDir.ParentDir;
                        else
                            nextDir = (Host.FileSystem.Dir) host.Fs.CurrentDir.FileList
                                .Find(d => d.Name == nextDirPath);
                        host.Fs.CurrentDir = nextDir ?? throw new CustomException.FileNotExistException(commandName);
                    }
                }
                catch (Exception)
                {
                    host.Fs.CurrentDir = backupDir;
                    throw;
                }
            },
            Shell_TextResource.cd);

        internal static readonly Shell.Command ListCommand = new Shell.Command(
            "ls",
            (argList, host) =>
            {
                const string commandName = "ls";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    switch (argList.Count)
                    {
                        case 0: break; //无参数
                        case 1: //1参数，调用cd更改ls执行目录
                        {
                            try
                            {
                                ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }

                            break;
                        }
                        default: //多参数
                        {
                            throw new CustomException.UnknownArgumentException(ListCommand.Name);
                        }
                    }

                    host.Fs.CurrentDir.FileList.Sort((x, y) =>
                        string.Compare(x.Name, y.Name, StringComparison.Ordinal));
                    foreach (var file in host.Fs.CurrentDir.FileList)
                    {
                        switch (file)
                        {
                            case Host.FileSystem.Dir _:
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                Console.BackgroundColor = ConsoleColor.Green;
                                break;
                            case Host.FileSystem.Exec _:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                break;
                            case Host.FileSystem.Video _:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.BackgroundColor = ConsoleColor.Blue;
                                break;
                        }

                        FileIdentifier?.Invoke(file);
                        Console.Write(file.Name);
                        Console.ResetColor();
                        Console.Write('\t');
                    }

                    if (host.Fs.CurrentDir.FileList.Count > 0)
                        Console.WriteLine();
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.ls);

        internal static readonly Shell.Command MkDirCommand = new Shell.Command(
            "mkdir",
            (argList, host) =>
            {
                const string commandName = "mkdir";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 1) //只能获取一个参数
                        throw new CustomException.UnknownArgumentException(commandName);
                    var dirName = argList[0];
                    if (dirName.Contains("/"))
                    {
                        var path = dirName.Substring(0, dirName.LastIndexOf('/')); //目录位置
                        dirName = dirName.Remove(0, dirName.LastIndexOf('/') + 1); //新目录名
                        argList[0] = path;
                        if (path != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (dirName == string.Empty)
                        throw new CustomException.UnknownArgumentException(commandName);
                    try
                    {
                        host.Fs.CurrentDir.Add(new Host.FileSystem.Dir(dirName));
                    }
                    catch (CustomException.NameConflictException)
                    {
                        throw new CustomException.NameConflictException(commandName);
                    }
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.mkdir);

        internal static readonly Shell.Command RemoveCommand = new Shell.Command(
            "rm",
            (argList, host) =>
            {
                const string commandName = "rm";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 1) //只能获取一个参数
                        throw new CustomException.UnknownArgumentException(commandName);
                    var fileName = argList[0];
                    if (fileName.Contains("/"))
                    {
                        var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
                        fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
                        argList[0] = path;
                        if (path != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (fileName == string.Empty)
                        throw new CustomException.UnknownArgumentException(commandName);
                    try
                    {
                        host.Fs.CurrentDir.Delete(fileName);
                    }
                    catch
                    {
                        throw new CustomException.FileNotExistException(commandName);
                    }
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.rm);

        internal static readonly Shell.Command CopyCommand = new Shell.Command(
            "cp",
            (argList, host) =>
            {
                const string commandName = "cp";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 2) //只能获取2个参数
                        throw new CustomException.UnknownArgumentException(commandName);
                    var oldName = argList[0];
                    if (oldName.Contains("/"))
                    {
                        var oldPath = oldName.Substring(0, oldName.LastIndexOf('/')); //目录位置
                        oldName = oldName.Remove(0, oldName.LastIndexOf('/') + 1); //新目录名
                        var oldArg = new List<string> {oldPath};
                        if (oldPath != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(oldArg, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (oldName == string.Empty) //没有输入文件名（arg0以斜杠结尾）
                        throw new CustomException.UnknownArgumentException();

                    if (host.Fs.CurrentDir.Transfer(oldName) == null) //没有找到文件
                        throw new CustomException.FileNotExistException(commandName);

                    var cpFile = host.Fs.CurrentDir.Transfer(oldName);

                    host.Fs.CurrentDir = backupDir;

                    var newName = argList[1];
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
                                ChangeDirCommand.Execute(newArg, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (newName == string.Empty) //没有输入文件名（arg1以斜杠结尾）
                        newName = oldName;
                    cpFile.Name = newName;
                    try
                    {
                        host.Fs.CurrentDir.Add(cpFile);
                    }
                    catch (CustomException.NameConflictException)
                    {
                        throw new CustomException.NameConflictException(commandName);
                    }
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.cp);

        // internal static readonly Shell.Command MoveCommand = new Shell.Command(
        //     "mv",
        //     (argList, host) =>
        //     {
        //         const string commandName = "mv";
        //         var backupDir = host.Fs.CurrentDir;
        //         try
        //         {
        //             if (argList.Count != 2) //只能获取2个参数
        //                 throw new CustomException.UnknownArgumentException(commandName);
        //
        //             var oldName = argList[0];
        //             var oldPath = string.Empty;
        //             var oldArg = new List<string>();
        //             if (oldName.Contains("/"))
        //             {
        //                 oldPath = oldName.Substring(0, oldName.LastIndexOf('/')); //目录位置
        //                 oldName = oldName.Remove(0, oldName.LastIndexOf('/') + 1); //新目录名
        //                 oldArg.Add(oldPath);
        //                 if (oldPath != string.Empty)
        //                     try
        //                     {
        //                         ChangeDirCommand.CommandAction(oldArg, host);
        //                     }
        //                     catch (CustomException.FileNotExistException)
        //                     {
        //                         throw new CustomException.FileNotExistException(commandName);
        //                     }
        //             }
        //
        //             if (oldName == string.Empty) //没有输入文件名（arg0以斜杠结尾）
        //                 throw new CustomException.UnknownArgumentException(commandName);
        //             if (host.Fs.CurrentDir.Transfer(oldName) != null)
        //             {
        //                 var mvFile = host.Fs.CurrentDir.Transfer(oldName);
        //                 host.Fs.CurrentDir = backupDir;
        //                 var newName = argList[1];
        //                 var newPath = string.Empty;
        //                 if (newName.Contains("/"))
        //                 {
        //                     newPath = newName.Substring(0, newName.LastIndexOf('/')); //目录位置
        //                     newName = newName.Remove(0, newName.LastIndexOf('/') + 1); //新目录名
        //                     var newArg = new List<string>
        //                     {
        //                         newPath
        //                     };
        //                     if (newPath != string.Empty)
        //                         try
        //                         {
        //                             ChangeDirCommand.CommandAction(newArg, host);
        //                         }
        //                         catch (CustomException.FileNotExistException)
        //                         {
        //                             throw new CustomException.FileNotExistException(commandName);
        //                         }
        //                 }
        //
        //                 if (newName == string.Empty) //没有输入文件名（arg1以斜杠结尾）
        //                     throw new CustomException.UnknownArgumentException(commandName); //没有输入新名称
        //                 if (newPath == oldPath)
        //                 {
        //                     try
        //                     {
        //                         host.Fs.CurrentDir.Rename(oldName, newName);
        //                     }
        //                     catch (CustomException.NameConflictException)
        //                     {
        //                         throw new CustomException.NameConflictException(commandName);
        //                     }
        //                 }
        //                 else
        //                 {
        //                     try
        //                     {
        //                         mvFile.Name = newName;
        //                         host.Fs.CurrentDir.Add(mvFile);
        //                     }
        //                     catch (CustomException.NameConflictException)
        //                     {
        //                         mvFile.Name = oldName;
        //                         throw new CustomException.NameConflictException(commandName);
        //                     }
        //
        //                     host.Fs.CurrentDir = backupDir;
        //                     ChangeDirCommand.CommandAction(oldArg, host);
        //                     host.Fs.CurrentDir.Delete(newName);
        //                 }
        //             }
        //             else //没有找到文件
        //             {
        //                 throw new CustomException.FileNotExistException(commandName);
        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine(e.Message);
        //         }
        //         finally
        //         {
        //             host.Fs.CurrentDir = backupDir;
        //         }
        //     },
        //     true,
        //     "test");

        internal static readonly Shell.Command MoveCommand = new Shell.Command(
            "mv",
            (argList, host) =>
            {
                const string commandName = "mv";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    CopyCommand.Execute(argList, host);
                    var oldName = argList[0];
                    if (oldName.Contains("/"))
                    {
                        var oldPath = oldName.Substring(0, oldName.LastIndexOf('/')); //目录位置
                        oldName = oldName.Remove(0, oldName.LastIndexOf('/') + 1); //新目录名
                        var oldArg = new List<string> {oldPath};
                        if (oldPath != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(oldArg, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (oldName == string.Empty) //没有输入文件名（arg0以斜杠结尾）
                        throw new CustomException.UnknownArgumentException();
                    host.Fs.CurrentDir.Delete(oldName);
                }
                catch (CustomException.UnknownArgumentException)
                {
                    throw new CustomException.UnknownArgumentException(commandName);
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.mv);

        internal static readonly Shell.Command InstallCommand = new Shell.Command(
            "install",
            (argList, host) =>
            {
                const string commandName = "install";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 1) //只能获取一个参数
                        throw new CustomException.UnknownArgumentException(commandName);
                    var fileName = argList[0];
                    if (fileName.Contains("/"))
                    {
                        var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
                        fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
                        argList[0] = path;
                        if (path != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (fileName == string.Empty)
                        throw new CustomException.FileNotExistException(commandName); //文件不存在
                    var exec = host.Fs.CurrentDir.Transfer(fileName);
                    switch (exec)
                    {
                        case null:
                            throw new CustomException.FileNotExistException(commandName);
                        case Host.FileSystem.Exec exec1:
                            try
                            {
                                host.InstallExec(exec1);
                            }
                            catch (CustomException.DllRequirementException)
                            {
                                throw new CustomException.DllRequirementException(commandName);
                            }
                            catch (CustomException.ExecInstalledException)
                            {
                                throw new CustomException.ExecInstalledException(exec1
                                    .OriginName);
                            }

                            break;
                        default:
                            throw new CustomException.MismatchedFormatException(commandName); //格式错误
                    }

                    Console.WriteLine(Shell_TextResource.InstallAction_Finish);
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.install);

        internal static readonly Shell.Command ConcatenateCommand = new Shell.Command(
            "cat",
            (argList, host) =>
            {
                const string commandName = "cat";
                var backupDir = host.Fs.CurrentDir;
                try
                {
                    if (argList.Count != 1) //只能获取一个参数
                        throw new CustomException.UnknownArgumentException(commandName);
                    var fileName = argList[0];
                    if (fileName.Contains("/"))
                    {
                        var path = fileName.Substring(0, fileName.LastIndexOf('/')); //目录位置
                        fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1); //新目录名
                        argList[0] = path;
                        if (path != string.Empty)
                            try
                            {
                                ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (fileName == string.Empty)
                        throw new CustomException.FileNotExistException(commandName); //文件不存在
                    var doc = host.Fs.CurrentDir.Transfer(fileName);
                    if (doc == null)
                        throw new CustomException.FileNotExistException(commandName); //文件不存在
                    Console.WriteLine(doc.Output());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    host.Fs.CurrentDir = backupDir;
                }
            },
            Shell_TextResource.cat);

        internal static readonly Shell.Command HelpCommand = new Shell.Command(
            "help",
            (argList, host) =>
            {
                const string commandName = "help";
                if (argList.Count != 0) //只能获取一个参数
                    throw new CustomException.UnknownArgumentException(commandName);
                switch (host.System)
                {
                    case 0:
                        Console.WriteLine(Shell_TextResource.Help_HackTool);
                        break;
                    case 1:
                        Console.WriteLine(Shell_TextResource.Help_UBurst);
                        break;
                    case 2:
                        Console.WriteLine(Shell_TextResource.Help_Door);
                        break;
                }

                Console.WriteLine(host.Info + '\n');
                foreach (var command in host.Sh.CommandList)
                    Console.WriteLine(command.HelpText);
            },
            Shell_TextResource.help);

        internal static readonly Shell.Command SaveCommand = new Shell.Command(
            "save",
            (argList, host) =>
            {
                const string commandName = "save";
                if (argList.Count != 0) //无参数
                    throw new CustomException.UnknownArgumentException(commandName);
                GameController.Save();
            },
            Shell_TextResource.save);

        internal static readonly Shell.Command LoadCommand = new Shell.Command(
            "load",
            (argList, host) =>
            {
                const string commandName = "load";
                if (argList.Count != 0) //无参数
                    throw new CustomException.UnknownArgumentException(commandName);
                GameController.Load();
            },
            Shell_TextResource.load);

        internal static event FileIdentify FileIdentifier;

        internal delegate void FileIdentify(Host.FileSystem.File file); //留给PlotObserver执行自动存档
    }
}