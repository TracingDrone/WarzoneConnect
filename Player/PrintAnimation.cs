using System;
using System.Collections.Generic;
using System.Threading;

namespace WarzoneConnect.Player
{
    internal static class WriteAnimation //弄动画特效的
    {
        internal static void PrintWithBoth(string sen, int second, string art, params ConsoleColor[] color)
        {
            var rpb = new RandomProgressBar();
            var aa = new AsciiAnimation();

            aa.DrawOutline(art);
            rpb.TopPosAfterAa = aa.InitTPos;
            rpb.Pa += aa.PrintColor;
            rpb.Start(sen, second, color);
        }

        internal static void PrintBarOnly(string sen, int second, params ConsoleColor[] color)
        {
            new RandomProgressBar().Start(sen, second, color);
        }

        private class RandomProgressBar
        {
            internal int TopPosAfterAa;
            internal event PrintAsc Pa;

            internal void Start(string sen, int second, params ConsoleColor[] color)
            {
                Console.ResetColor();

                var printColor = Console.ForegroundColor;
                var progress = 0;

                if (color.Length == 1)
                    printColor = color[0];
                var tPos = Console.CursorTop;
                if (Pa != null)
                    tPos = TopPosAfterAa + 2;

                Console.SetCursorPosition(0, tPos);
                Console.Write($@"{sen} [");
                var curLPos = sen.Length + 2;

                var random = new Random();
                while (progress <= second * 100)
                {
                    for (var i = 0; i < progress / second / 2; i++)
                    {
                        Console.SetCursorPosition(curLPos + i, tPos);
                        Console.ForegroundColor = printColor;
                        Console.Write(@"#");
                        Console.ResetColor();
                    }

                    Console.SetCursorPosition(curLPos + 50, tPos);
                    Console.Write($@"] {progress / second}%");
                    Console.SetCursorPosition(curLPos, tPos);
                    progress += random.Next(6, 15);

                    Pa?.Invoke(progress / second, printColor);

                    Thread.Sleep(100);
                }

                Console.SetCursorPosition(curLPos, tPos);
                for (var i = 0; i < 50; i++)
                {
                    Console.ForegroundColor = printColor;
                    Console.Write(@"#");
                    Console.ResetColor();
                }

                Console.Write(@"] Completed!");
            }

            internal delegate void PrintAsc(int progress, params ConsoleColor[] color); //RGP为主，ASC为辅
        }

        private class AsciiAnimation
        {
            private readonly List<List<char>> _charSheet = new List<List<char>> {new List<char>()};
            private int _v, _h;
            internal int InitTPos;

            internal void DrawOutline(string art)
            {
                foreach (var ch in art)
                    if (ch == '\n')
                    {
                        if (_charSheet[_v].Count > _h) _h = _charSheet[_v].Count;
                        _v++;
                        _charSheet.Add(new List<char>());
                    }
                    else
                    {
                        _charSheet[_v].Add(ch);
                    }

                _v++;

                for (var i = 0; i < _h; i++)
                for (var j = 0; j < _v; j++)
                {
                    Console.SetCursorPosition(i, j + InitTPos);
                    if (i >= _charSheet[j].Count) continue;
                    Console.Write(_charSheet[j][i] == '#' ? ' ' : _charSheet[j][i]);
                }

                InitTPos = Console.CursorTop;
            }

            internal void PrintColor(int progress, params ConsoleColor[] color)
            {
                Console.ResetColor();

                var printColor = Console.ForegroundColor;

                if (color.Length == 1)
                    printColor = color[0];

                for (var i = 0; i + 1 / _h < progress / 2; i++)
                for (var j = 0; j < _v; j++)
                    if (i < _charSheet[j].Count)
                    {
                        Console.SetCursorPosition(i, j);
                        if (_charSheet[j][i] == '#')
                        {
                            Console.ForegroundColor = printColor;
                            Console.Write(@"#");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write(_charSheet[j][i]);
                        }
                    }
            }
        }

        // internal static void
        //     old_RandomProgressBar(object package) //new object[] {string sen,int second,bool withAA(, ConsoleColor color)}
        // {
        //     _progressPercentage = 0;
        //     var objects = (object[]) package;
        //     var sen = objects[0].ToString();
        //     var second = int.Parse(objects[1].ToString());
        //     var withAscArt = bool.Parse(objects[2].ToString());
        //     var color = ConsoleColor.Gray;
        //     if (objects.Length == 4) Enum.TryParse(objects[3].ToString(), out color);
        //     var progress = 0;
        //     if (withAscArt)
        //     {
        //         MreLockPgBar.Reset();
        //         MreLockPgBar.WaitOne(); //等待AA绘画
        //         MreLockAscArt.Reset(); //开始PB绘画
        //     }
        //     else
        //     {
        //         _tPosAfterAscArt = Console.CursorTop;
        //     }
        //     Console.SetCursorPosition(0, _tPosAfterAscArt);
        //     Console.Write($@"{sen} [");
        //     var curLPos = sen.Length + 2;
        //     if (withAscArt)
        //         MreLockAscArt.Set();
        //     var random = new Random();
        //     while (progress <= second * 100)
        //     {
        //         if (withAscArt)
        //         {
        //             MreLockPgBar.WaitOne(); // 等待AA绘图完成
        //             MreLockAscArt.Reset(); //锁AA，直至PB绘图完成
        //         }
        //
        //         _progressPercentage = progress / second;
        //         for (var i = 0; i < _progressPercentage / 2; i++)
        //         {
        //             Console.SetCursorPosition(curLPos + i, _tPosAfterAscArt);
        //             Console.ForegroundColor = color;
        //             Console.Write(@"#");
        //             Console.ResetColor();
        //         }
        //
        //         Console.SetCursorPosition(curLPos + 50, _tPosAfterAscArt);
        //         Console.Write($@"] {_progressPercentage}%");
        //         Console.SetCursorPosition(curLPos, _tPosAfterAscArt);
        //         progress += random.Next(6, 15);
        //         if (withAscArt) MreLockAscArt.Set(); //进度条绘制完后通知图画
        //         Thread.Sleep(100);
        //     }
        //
        //     Console.SetCursorPosition(curLPos, _tPosAfterAscArt);
        //     for (var i = 0; i < 50; i++)
        //     {
        //         Console.ForegroundColor = color;
        //         Console.Write(@"#");
        //         Console.ResetColor();
        //     }
        //
        //     Console.Write(@"] Completed!");
        //     if (withAscArt) _progressPercentage += 100; //避免联动时出现死锁（？）
        //     Thread.Sleep(2000);
        //     // MreLockPgBar.Set();
        // }
        //
        // internal static void old_AsciiAnimation(object package) //new object[] {string sen, bool isFollowPB(， ConsoleColor color)}
        // {
        //     var objects = (object[]) package;
        //     var input = objects[0].ToString();
        //     var isFollowPgBar = bool.Parse(objects[1].ToString());
        //     var color = ConsoleColor.Gray;
        //     if (objects.Length == 3) Enum.TryParse(objects[2].ToString(), out color);
        //     var charSheet = new List<List<char>> {new List<char>()};
        //    
        //     int v = 0, h = 0;
        //     if (isFollowPgBar)
        //         MreLockAscArt.Reset();
        //     foreach (var ch in input)
        //         if (ch == '\n')
        //         {
        //             if (charSheet[v].Count > h) h = charSheet[v].Count;
        //             v++;
        //             charSheet.Add(new List<char>());
        //         }
        //         else
        //         {
        //             charSheet[v].Add(ch);
        //         }
        //
        //     v++;
        //     var initTPos = Console.CursorTop;
        //     if (isFollowPgBar)
        //         MreLockPgBar.Reset(); //先画AA
        //     for (var i = 0; i < h; i++)
        //     for (var j = 0; j < v; j++)
        //     {
        //         Console.SetCursorPosition(i, j + initTPos);
        //         if (i >= charSheet[j].Count) continue;
        //         Console.Write(charSheet[j][i] == '#' ? ' ' : charSheet[j][i]);
        //     }
        //
        //     _tPosAfterAscArt = Console.CursorTop + 2;
        //     if (isFollowPgBar)
        //         MreLockPgBar.Set(); //AA绘画完成
        //
        //     for (var i = 0; i < h;)
        //         if (isFollowPgBar && (i + 1) * 100 / h > _progressPercentage) //就是这句话锁了（怒）
        //         {
        //             MreLockPgBar.Set(); //PB开始绘画，直至pP足够大
        //         }
        //         else
        //         {
        //             if (isFollowPgBar)
        //             {
        //                 MreLockAscArt.WaitOne(); //等待PB绘画完成
        //                 MreLockPgBar.Reset(); //AA开始绘画
        //             }
        //
        //             for (var j = 0; j < v; j++)
        //                 if (i < charSheet[j].Count)
        //                 {
        //                     Console.SetCursorPosition(i, j + initTPos);
        //                     if (charSheet[j][i] == '#')
        //                     {
        //                         Console.ForegroundColor = color;
        //                         Console.Write(@"#");
        //                         Console.ResetColor();
        //                     }
        //                     else
        //                     {
        //                         Console.Write(charSheet[j][i]);
        //                     }
        //                 }
        //
        //             i++;
        //             if (isFollowPgBar)
        //                 MreLockPgBar.Set(); //AA绘画完成
        //             else
        //                 Thread.Sleep(50);
        //         }
        //
        //     Thread.Sleep(2000);
        //     MreLockAscArt.Set();
        // }
    }
}