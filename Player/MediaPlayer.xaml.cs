using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Runtime.Serialization;
using WarzoneConnect.Properties;
// ReSharper disable CommentTypo

namespace WarzoneConnect.Player
{
    /// <summary>
    /// MediaPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPlayer
    {
        public MediaPlayer(string name)
        {
            using (var resourceSet = new ResourceSet(@".\Media.resource"))
            {
                var temp = (byte[])resourceSet.GetObject(name);
                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + $@"\{name}.mp4", FileMode.OpenOrCreate);
                fileStream.Write(temp ?? throw new InvalidOperationException(), 0, temp.Length);
                fileStream.Close();
            }
            InitializeComponent();
            PlayerWindow.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + $@"\{name}.mp4");
        }

        private void Play()
        {
            ShowDialog();
        }

        private static readonly Shell.Command PlayCommand = new Shell.Command(
            "play",
            (argList, host) =>
            {
                const string commandName = "play";
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
                                ShellCommandDict.ChangeDirCommand.Execute(argList, host);
                            }
                            catch (CustomException.FileNotExistException)
                            {
                                throw new CustomException.FileNotExistException(commandName);
                            }
                    }

                    if (fileName == string.Empty)
                        throw new CustomException.FileNotExistException(commandName); //文件不存在
                    var video = host.Fs.CurrentDir.Transfer(fileName);
                    switch (video)
                    {
                        case null:
                            throw new CustomException.FileNotExistException(commandName);
                        case MediaFile video1:
                            try
                            {
                                new MediaPlayer(video1.FileName).Play();
                            }
                            catch
                            {
                                throw new BrokenVideoException(commandName); //格式错误
                            }

                            break;
                        default:
                            throw new CustomException.MismatchedFormatException(commandName); //格式错误
                    }
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
            MediaPlayer_TextResource.play);

        internal static readonly Host.FileSystem.Exec PlayExec = new Host.FileSystem.Exec("play",
            new List<Shell.Command>(new[]
            {
                PlayCommand
            }), false);

        internal static void RegisterMediaFile()
        {
            static void IdentifyMedia(Host.FileSystem.File file)
            {
                if (!(file is MediaFile)) return;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.DarkGray;
            }

            ShellCommandDict.FileIdentifier += IdentifyMedia;
        }
        [Serializable]
        internal class MediaFile : Host.FileSystem.File
        {
            internal readonly string FileName;

            public MediaFile(string name) : base(name)
            {
                FileName = name;
            }
        }

        [Serializable]
        public class BrokenVideoException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public BrokenVideoException() : base(CustomException_TextResource.UnknownCommand)
            {
            }

            public BrokenVideoException(string commandName) : base(
                $"{commandName} - {MediaPlayer_TextResource.BrokenVideo}")
            {
            }

            public BrokenVideoException(string message, Exception inner) : base(message, inner)
            {
            }

            protected BrokenVideoException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        private void PlayVideo(object sender, EventArgs e)
        {
            PlayerWindow.Play();
        }

        private void StopVideo(object sender, EventArgs e)
        {
            PlayerWindow.Close();
        }
    }
}
