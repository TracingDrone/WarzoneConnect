using System;
using System.Collections.Generic;
using System.Dynamic;

namespace WarzoneConnect.Player
{
    [Serializable]
    public abstract class Host : DynamicObject
    {
        
        internal string User { get; } //用户名
        internal string Addr { get; } //IP地址
        internal string Info { get; } //系统信息
        internal int System { get; }//0为HackTool，1为UBurst，2为Door，3为Clock
        
        internal FileSystem Fs; //根目录，作为文件系统入口
        internal Shell Sh { get; set; } //自己的Shell

        internal Dictionary<string,object> Prop=new Dictionary<string, object>(); //Properties
        public override bool TryGetMember(GetMemberBinder binder, out object result) 
        {
            return Prop.TryGetValue(binder.Name, out result);
        }
        
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Prop[binder.Name] = value;
            return true;
        }
        
        internal FileSystem.Dir GetRoot()
        {
            return Fs.RootDir;
        }
        

        internal Host(string user, string addr, string info,int system)
        {
            User = user;
            Addr = addr;
            Info = info;
            System = system;
            var bin = new FileSystem.Dir("bin");
            var doc = new FileSystem.Dir("doc");
            var root=new FileSystem.Dir("root");
            root.Add(bin);
            root.Add(doc);
            Fs = new FileSystem(root);
            Sh = new Shell(this);
        }
        internal List<string> InstalledExec { get; set; } = new List<string>(); //已安装的程序

        internal void InstallExec(FileSystem.Exec exec)
        {
            if (this is Target && exec.NeedDll)
                throw new CustomException.DllRequirementException();
            if (InstalledExec.Contains(exec.OriginName))
                throw new CustomException.ExecInstalledException();
            foreach (var command in exec.LinkedCommands)
                command.RegisterCommand(this);
            InstalledExec.Add(exec.OriginName);
        }

        [Serializable]
        internal class FileSystem //文件系统
        {
            internal Dir CurrentDir;
            
            internal Dir RootDir { get; }
            
            [Serializable]
            internal abstract class File : ICloneable
            {
                internal File(string name)
                {
                    Name = name;
                }

                internal string Name { get; set; }

                internal virtual string Output()
                {
                    return Convert.ToString(GetHashCode(), 2);
                }

                public virtual object Clone()
                {
                    return MemberwiseClone();
                }
            }

            [Serializable]
            internal class Exec : File
            {
                internal Exec(string name, List<Shell.Command> commands,bool needDll) : base(name) {
                    LinkedCommands = commands;
                    OriginName = Name;
                    NeedDll = needDll;
                }

                internal List<Shell.Command> LinkedCommands { get; }

                internal string OriginName { get; }
                internal bool NeedDll { get; }

                public override object Clone()
                {
                    return new Exec(OriginName,LinkedCommands,NeedDll);
                }
            }

            [Serializable]
            internal class Doc : File
            {
                internal Doc(string name, string document) : base(name)
                {
                    Document = document;
                }
                
                public override object Clone()
                {
                    return new Doc(Name,Document);
                }

                internal string Document { get; } //文本
                
                internal override string Output()
                {
                    return Document;
                }
            }
            
            [Serializable]
            internal class Video : File
            {
                internal Video(string name, string videoName) : base(name)
                {
                    LinkedVideoName = videoName;
                }

                private string LinkedVideoName { get; } //链接至resx
                
                //尚未完工
            }

            [Serializable]
            internal class Dir : File
            {
                public override object Clone() //DeepCopy
                {
                    var newDir = new Dir(Name);
                    foreach (var file in FileList)
                        if (file is Dir tempOldDirChild)
                        {
                            var tempNewDirChild = (Dir)tempOldDirChild.Clone();
                            tempNewDirChild.ParentDir=newDir;
                            newDir.FileList.Add(tempNewDirChild);
                        }
                        else
                        {
                            newDir.FileList.Add(file);
                        }
                    
                    return newDir;
                }
                
                internal Dir(string name) : base(name)
                {
                }

                internal Dir ParentDir { get; private set; }

                internal List<File> FileList { get; private set; } = new List<File>();

                internal void Add(Dir newDir) //添加目录，在添加文件基础上修改ParentDir
                {
                    if (FileList.Exists(f => f.Name == newDir.Name))
                        throw new CustomException.NameConflictException(); //存在同名文件
                    newDir.ParentDir = this;
                    FileList.Add(newDir);
                }

                internal void Add(File newFile) //当前目录添加文件，与Transfer，Delete，Rename配合使用以实现剪切复制
                {
                    if (newFile is Dir dir)
                    {
                        Add(dir);
                        return;
                    }
                    if (FileList.Exists(f => f.Name == newFile.Name))
                        throw new CustomException.NameConflictException(); //存在同名文件
                    FileList.Add(newFile);
                }

                internal File Transfer(string name) //可能是文件，也可能是null！
                {
                    var returnFile = FileList.Find(f => f.Name == name);
                    return (File) returnFile?.Clone();
                }

                // internal void RebuildFileList(Dir oldDir) //重建文件列表
                // {
                //     foreach (var file in oldDir.FileList)
                //         if (file is Dir tempDir2)
                //         {
                //             var tempDir = new Dir(tempDir2.Name) {ParentDir = this, FileList = tempDir2.FileList};
                //             tempDir.RebuildFileList(tempDir2);
                //             FileList.Add(tempDir);
                //         }
                //         else
                //         {
                //             FileList.Add(file);
                //         }
                //
                //     //此处不应该是简单的复制，而是重建列表（问题已解决（大概））
                // }

                internal void Delete(string name) //如果没出错的话，应该只会删除1个文件吧，大概······
                {
                    if(FileList.RemoveAll(f => f.Name == name) == 0)
                        throw new CustomException.FileNotExistException();
                }

                internal void Rename(string oldName, string newName) //改名，Terminal记得过滤一下名字输入
                {
                    var index = FileList.FindIndex(f => f.Name == oldName);
                    if (FileList.Exists(f => f.Name == newName))
                        throw new CustomException.NameConflictException(); //存在同名文件
                    FileList[index].Name = newName;
                }
                
            }

            internal FileSystem(Dir rootDir)
            {
                RootDir = rootDir;
                CurrentDir = rootDir;
            }
        }
    }

    [Serializable]
    internal class HackTool : Host
    {
        internal HackTool(string user,string addr, string info) : base(user,addr,info,0)
        {
        }
    }
    [Serializable]
    internal class Target : Host
    {
        internal Target(string user, string addr,string info,int system) : base(user, addr,info,system)
        {
        }
    }
}