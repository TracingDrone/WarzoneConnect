using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Planner
{
    public static class BigFirework
    {
        internal static void YouDied()
        {
            // GameController.Save(); //挂前存个档
            var incursionDetect = DateTime.Now;
            
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            var random = new Random();
            Console.SetWindowSize(120, 30);
            var isWritten = new bool[Console.WindowWidth / 2, Console.WindowHeight];
            for (var i = 0; i < Console.WindowHeight; i++)
            {
                for (var j = 0; j < Console.WindowWidth / 2; j++)
                {
                    int h;
                    int w;
                    do
                    {
                        h = random.Next(Console.WindowHeight);
                        w = random.Next(Console.WindowWidth / 2);
                    } while (isWritten[w, h]);

                    Console.SetCursorPosition(w * 2, h);
                    Console.Write(random.Next(2)+@" ");
                    isWritten[w, h] = true;
                }

                Thread.Sleep(100);
            }
            
            var failSave = DateTime.Now;
            
            GameController.Save();
            if (File.Exists(@".\Save.resx")&&!File.Exists(@".\Save.bak"))
            {
                File.Move(@".\Save.resx",@".\Save.bak");
            }
            using var resx = new ResXResourceWriter(@".\Save.resx");
            resx.AddResource("RootKit", "BootUpHijacking");
            resx.Dispose();
            

            var asciiLength=0;
            var calculateAsciiTask = Task.Run(() =>
            {
                asciiLength = BigFirework_TextResource.YouDied.Replace("\r\n", "\n").Split('\n').Select(str => str.Length).Concat(new[] {asciiLength}).Max();
            });
            Thread.Sleep(2000);
            calculateAsciiTask.Wait();

            var basicTPos = 5;
            foreach (var str in BigFirework_TextResource.YouDied.Replace("\r\n", "\n").Split('\n'))
            {
                Console.SetCursorPosition((Console.WindowWidth-asciiLength)/2,basicTPos);
                Console.Write(str);
                basicTPos++;
            }
            basicTPos++;
            Console.SetCursorPosition((Console.WindowWidth-asciiLength)/2,basicTPos);
            Console.Write(string.Empty.PadLeft(asciiLength,'#'));
            basicTPos += 2;
            
            Console.SetCursorPosition((Console.WindowWidth-asciiLength)/2,basicTPos);
            Console.Write(string.Empty.PadLeft(asciiLength,'#'));
            
            Console.ResetColor();
            Thread.Sleep(1000);
            var freesia = DateTime.Now;
            
            using (var fileStream = new FileStream(@".\log.txt", FileMode.Append))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(BigFirework_TextResource.FailedLog, incursionDetect, failSave, freesia);
                    streamWriter.Flush();
                }
            }
            Environment.Exit(0);
        }
    }
}