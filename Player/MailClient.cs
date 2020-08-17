using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WarzoneConnect.Planner;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Player
{
    internal static class MailClient
    {
        private static int _longestTitle, _longestFrom; //最长的邮件标题和发信邮箱，用于收件箱自适应对齐

        internal static readonly Host.FileSystem.Exec MailExec = new Host.FileSystem.Exec("mail",
            new List<Shell.Command>(new[]
            {
                new Shell.Command("mail", (argList, host) =>
                    {
                        if (argList.Count != 0)
                            throw new CustomException.UnknownArgumentException("mail");
                        OpenMailClient(GetMailBox(host));
                    },
                    Mail_TextResource.Mail_Help)
            }),
            false);

        private static void OpenMailClient(MailBox mb)
        {
            Console.CursorVisible = false;
            Console.Clear();
            WriteAnimation.PrintWithBoth("Loading",5,PlotConfig.Mail_ASCII,ConsoleColor.Red);
            Thread.Sleep(2000);
            Console.Clear();
            var currentTPos = 0; //目前光标相对第1封邮件的位置，默认置于第1封邮件处
            while (true)
            {
                void ChangeFocus()
                {
                    if (currentTPos == mb.EMails.Count + 1)
                    {
                        Console.Write(Mail_TextResource.Exit);
                    }
                    else if (currentTPos == mb.EMails.Count)
                    {
                        Console.Write(Mail_TextResource.WriteMail);
                    }
                    else
                    {
                        ShowMail(mb.EMails[currentTPos]);
                    }

                    
                }

                static void ShowMail(MailBox.EMail email)
                {
                    Console.Write(
                        $@"{email.Title.PadRight(_longestTitle+email.Title.Length-UsefulTools.GetLength(email.Title) + 2)}	{email.From.PadRight(_longestFrom + 2)}	{email.Time}	");
                    if (email.IsRead)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Mail_TextResource.Read);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Mail_TextResource.Unread);
                        Console.ResetColor();
                    }
                }
                
                Console.ResetColor();
                Console.WriteLine(PlotConfig.Mail_ASCII);
                _longestTitle = mb.GetLongestTitle();
                _longestFrom = mb.GetLongestFrom();
                Console.WriteLine('\n'+Mail_TextResource.WelcomeBack, mb.User, mb.Unread > 0 ? string.Format(Mail_TextResource.HaveUnread,mb.Unread) : null);
                if (mb.EMails.Count > 0)
                    Console.WriteLine(Mail_TextResource.HeaderTitle.PadRight(_longestTitle+Mail_TextResource.HeaderTitle.Length-UsefulTools.GetLength(Mail_TextResource.HeaderTitle) + 2) + '\t' + 
                                      Mail_TextResource.HeaderFrom.PadRight(_longestFrom + 2)+ '\t' + 
                                      Mail_TextResource.HeaderTime.PadRight(17));
                else
                    Console.WriteLine(Mail_TextResource.EmptyInbox);
                var count0TPos = Console.CursorTop;
                foreach (var eMail in mb.EMails)
                    ShowMail(eMail);

                Console.WriteLine(Mail_TextResource.WriteMail+'\n'+Mail_TextResource.Exit+'\n'+Mail_TextResource.ClientHelp);
                Console.SetCursorPosition(0, count0TPos + currentTPos);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                ChangeFocus();
                Console.ResetColor();
                ConsoleKey key;
                do
                {
                    key = Console.ReadKey(true).Key;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (key)
                    {
                        case ConsoleKey.DownArrow:
                        {
                            if (currentTPos < mb.EMails.Count + 1)
                            {
                                Console.SetCursorPosition(0, count0TPos + currentTPos);
                                ChangeFocus();
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Black;
                                currentTPos++;
                                Console.SetCursorPosition(0, count0TPos + currentTPos);
                                ChangeFocus();
                                Console.ResetColor();
                            }

                            break;
                        }
                        case ConsoleKey.UpArrow:
                        {
                            if (currentTPos > 0)
                            {
                                Console.SetCursorPosition(0, count0TPos + currentTPos);
                                ChangeFocus();
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Black;
                                currentTPos--;
                                Console.SetCursorPosition(0, count0TPos + currentTPos);
                                ChangeFocus();
                                Console.ResetColor();
                            }

                            break;
                        }
                        case ConsoleKey.Enter:
                        {
                            if (currentTPos == mb.EMails.Count + 1)
                            {
                                Console.Clear();
                                return;
                            }

                            if (currentTPos == mb.EMails.Count + 0)
                            {
                                Console.Clear();

                                static void WriteMail(MailBox mb)
                                {
                                    var isToCorrect = false; //验证收件人是否符合格式
                                    Console.Clear();
                                    var text = new List<string>(); //用于保存文本
                                    string to = string.Empty, title = string.Empty;
                                    var currentTPosWm = 1;
                                    while (true)
                                    {
                                        void ChangeFocusWm()
                                        {
                                            Console.SetCursorPosition(0, currentTPosWm);
                                            switch (currentTPosWm)
                                            {
                                                case 1:
                                                    Console.Write(Mail_TextResource.To, to);
                                                    // if (!isToCorrect)
                                                    // {
                                                    //     Console.ForegroundColor = ConsoleColor.Red;
                                                    //     Console.Write(Mail_TextResource.MailAddressUnmatched);
                                                    // }

                                                    break;
                                                case 2:
                                                    Console.Write(Mail_TextResource.Title+title);
                                                    break;
                                                default:
                                                    if (currentTPosWm == text.Count + 5)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.Write(Mail_TextResource.NewLine);
                                                    }
                                                    else if (currentTPosWm == text.Count + 6)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Blue;
                                                        Console.WriteLine(Mail_TextResource.Send);
                                                    }
                                                    else if (currentTPosWm == text.Count + 7)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                                        Console.WriteLine(Mail_TextResource.ExitWriteMail);
                                                    }
                                                    else
                                                    {
                                                        Console.Write(text[currentTPosWm - 5]);
                                                    }

                                                    Console.ResetColor();
                                                    break;
                                            }
                                        }

                                        Console.SetCursorPosition(0, 0);
                                        Console.WriteLine(Mail_TextResource.From+mb.User);
                                        Console.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                                        Console.SetCursorPosition(0, 1);
                                        Console.Write(Mail_TextResource.To+to);
                                        if (isToCorrect)
                                        {
                                            Console.WriteLine();
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine(Mail_TextResource.MailAddressUnmatched);
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                        }

                                        Console.WriteLine(Mail_TextResource.Title + '\n' +
                                                          Mail_TextResource.WriteMailHelp);
                                        Console.WriteLine(string.Empty.PadLeft(Console.WindowWidth - 1, '='));
                                        foreach (var line in text) Console.WriteLine(line);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine(Mail_TextResource.NewLine);
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.WriteLine(Mail_TextResource.Send);
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine(Mail_TextResource.ExitWriteMail);
                                        Console.ResetColor();
                                        Console.SetCursorPosition(0, currentTPosWm);
                                        Console.BackgroundColor = ConsoleColor.Gray;
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        //第0行是发件人
                                        //第1行是收件人
                                        //第2行是标题
                                        //第3行是教程
                                        //倒数3行是“另起一行”
                                        //倒数2行是“发送”
                                        //倒数1行是“离开”
                                        //剩下的是正文

                                        ChangeFocusWm();

                                        Console.ResetColor();
                                        ConsoleKey keyWm;
                                        do
                                        {
                                            keyWm = Console.ReadKey(true).Key;
                                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                                            switch (keyWm)
                                            {
                                                case ConsoleKey.DownArrow:
                                                {
                                                    if (currentTPosWm <= text.Count + 6)
                                                    {
                                                        ChangeFocusWm();

                                                        Console.BackgroundColor = ConsoleColor.Gray;
                                                        Console.ForegroundColor = ConsoleColor.Black;
                                                        currentTPosWm++;
                                                        if (currentTPosWm == 3)
                                                            currentTPosWm++;
                                                        if (currentTPosWm == 4)
                                                            currentTPosWm++;
                                                        if (currentTPosWm == text.Count + 6 && !isToCorrect)
                                                            currentTPosWm++;

                                                        ChangeFocusWm();

                                                        Console.ResetColor();
                                                    }

                                                    break;
                                                }
                                                case ConsoleKey.UpArrow:
                                                {
                                                    if (currentTPosWm > 1)
                                                    {
                                                        ChangeFocusWm();

                                                        Console.BackgroundColor = ConsoleColor.Gray;
                                                        Console.ForegroundColor = ConsoleColor.Black;
                                                        currentTPosWm--;
                                                        if (currentTPosWm == text.Count + 6 && !isToCorrect)
                                                            currentTPosWm--;
                                                        if (currentTPosWm == 4)
                                                            currentTPosWm--;
                                                        if (currentTPosWm == 3)
                                                            currentTPosWm--;
                                                        Console.SetCursorPosition(0, currentTPosWm);

                                                        ChangeFocusWm();
                                                    }

                                                    Console.ResetColor();
                                                    break;
                                                }
                                                case ConsoleKey.Enter:
                                                {
                                                    switch (currentTPosWm)
                                                    {
                                                        case 1:
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(string.Empty.PadRight(Console.WindowWidth - 1,
                                                                ' '));
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(Mail_TextResource.To);
                                                            Console.CursorVisible = true;
                                                            SendKeys.SendWait(to);
                                                            to = Console.ReadLine();
                                                            Console.CursorVisible = false;

                                                            static bool ValidateUser(string acc) //验证邮箱是否符合规格，正则表达式版本
                                                            {
                                                                return new Regex(@"^\w+@\w+\.\w+$").IsMatch(acc);
                                                            }

                                                            isToCorrect = ValidateUser(to);
                                                            break;
                                                        case 2:
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(Mail_TextResource.Title);
                                                            Console.CursorVisible = true;
                                                            SendKeys.SendWait(title);
                                                            title = Console.ReadLine();
                                                            Console.CursorVisible = false;
                                                            break;
                                                        default:
                                                            if (currentTPosWm == text.Count + 5)
                                                            {
                                                                Console.SetCursorPosition(0, currentTPosWm);
                                                                Console.Write(
                                                                    string.Empty.PadLeft(Console.WindowWidth - 1), ' ');
                                                                Console.SetCursorPosition(0, currentTPosWm);
                                                                text.Add(Console.ReadLine());
                                                            }
                                                            else if (currentTPosWm == text.Count + 6)
                                                            {
                                                                var mailText = text.Aggregate(string.Empty,
                                                                    (current, line) => current + (line + '\n'));
                                                                mailText = mailText.TrimEnd('\n');
                                                                WriteAnimation.PrintBarOnly("Sending...", 3);
                                                                Thread.Sleep(2000);
                                                                MailBox.TransferEMail(new MailBox.EMail(mb.User, to,
                                                                    title, mailText));
                                                                //发送时加个动画吧
                                                                Console.Clear();
                                                                return;
                                                            }
                                                            else if (currentTPosWm == text.Count + 7)
                                                            {
                                                                Console.Clear();
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                Console.SetCursorPosition(0, currentTPosWm);
                                                                Console.CursorVisible = true;
                                                                SendKeys.SendWait(text[currentTPosWm - 5]);
                                                                text[currentTPosWm - 5] = Console.ReadLine();
                                                                Console.CursorVisible = false;
                                                            }

                                                            break;
                                                    }

                                                    break;
                                                }
                                                case ConsoleKey.Delete:
                                                {
                                                    switch (currentTPosWm)
                                                    {
                                                        case 1:
                                                            to = string.Empty;
                                                            isToCorrect = false;
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(Mail_TextResource.To);
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.Write(Mail_TextResource.MailAddressUnmatched);
                                                            Console.ResetColor();
                                                            break;
                                                        case 2:
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            title = string.Empty;
                                                            Console.Write(Mail_TextResource.Title);
                                                            break;
                                                        default:
                                                            if (currentTPosWm < text.Count + 5)
                                                                text.RemoveAt(currentTPosWm - 5);
                                                            Console.SetCursorPosition(0, 5);
                                                            foreach (var line in text)
                                                            {
                                                                Console.Write(
                                                                    string.Empty.PadLeft(Console.WindowWidth - 1), ' ');
                                                                Console.CursorLeft = 0;
                                                                Console.WriteLine(line);
                                                            }

                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.CursorLeft = 0;
                                                            Console.WriteLine(Mail_TextResource.NewLine);
                                                            Console.ForegroundColor = ConsoleColor.Blue;
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.CursorLeft = 0;
                                                            Console.WriteLine(Mail_TextResource.Send);
                                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.CursorLeft = 0;
                                                            Console.WriteLine(Mail_TextResource.ExitWriteMail);
                                                            Console.Write(string.Empty.PadLeft(Console.WindowWidth - 1),
                                                                ' ');
                                                            Console.ResetColor();
                                                            Console.SetCursorPosition(0, currentTPosWm);
                                                            break;
                                                    }

                                                    break;
                                                }
                                            }
                                        } while (keyWm != ConsoleKey.Enter && keyWm != ConsoleKey.Delete);
                                    }
                                }

                                WriteMail(mb);
                            }
                            else
                            {
                                Console.Clear();

                                static bool ReadMail(MailBox.EMail eMail)
                                {
                                    Console.WriteLine(
                                        Mail_TextResource.From+eMail.From+'\n'+
                                        Mail_TextResource.To+eMail.To+'\n'+
                                        Mail_TextResource.Time+eMail.Time+'\n'+
                                        Mail_TextResource.Title+eMail.Title);
                                    Console.Write(string.Empty.PadRight(Console.WindowWidth, '='));
                                    var rawText = eMail.Text.Split('\n');
                                    foreach (var row in rawText)
                                        Console.WriteLine(row.Trim());
                                    Console.Write(string.Empty.PadRight(Console.WindowWidth, '='));
                                    Console.WriteLine(Mail_TextResource.ReturnToInbox);
                                    Console.ReadKey();
                                    Console.Clear();
                                    return !eMail.IsRead;
                                }

                                if (ReadMail(mb.EMails[currentTPos]))
                                {
                                    mb.EMails[currentTPos].IsRead = true;
                                    mb.Unread--;
                                }
                            }

                            break;
                        }
                    }
                } while (key != ConsoleKey.Enter);
            }
        }

        internal static MailBox GetMailBox(Host host) //获得邮箱
        {
            try
            {
                return ((dynamic) host).Mailbox;
            }
            catch
            {
                return null;
            }
        }

        // private static bool ValidateUser(string acc) //验证邮箱是否符合规格
        // {
        //     if (acc.Contains(" ")) //不存在空格
        //         return false;
        //     var acc1 = acc.Split('@');
        //     if (acc1.Length != 2 || acc1[0] == string.Empty || acc1[1] == string.Empty) //只有一个@，没有空值
        //         return false;
        //     var acc2 = acc1[1].Split('.');
        //     return acc2.Length >= 2 && acc2.All(block => block != string.Empty);
        // }
    }

    [Serializable]
    internal class MailBox
    {
        internal MailBox(string user)
        {
            User = user + '@' + PlotConfig.Mail_Address;
            Unread = 0;
            EMails = new List<EMail>();
        }

        internal List<EMail> EMails { get; set; }

        internal int Unread { get; set; }

        internal string User { get; }

        internal static void TransferEMail(EMail email)
        {
            MailServer.Send(email);
        }

        public void AddMail(string from, string title, string text, DateTime time) //添加邮件
        {
            EMails.Add(new EMail(from, User, title, text, time));
            Unread++;
        }

        public void AddMail(string from, string title, string text) //开始游戏后实时添加
        {
            AddMail(from, title, text, DateTime.Now);
        }

        public int GetLongestTitle()
        {
            return EMails.Select(t => UsefulTools.GetLength(t.Title)).Concat(new[] {0}).Max();
        }

        public int GetLongestFrom()
        {
            return EMails.Select(t => UsefulTools.GetLength(t.From)).Concat(new[] {0}).Max();
        }

        [Serializable]
        public class EMail //比起struct，还是class好用
        {
            internal EMail(string from, string to, string title, string text, DateTime time)
            {
                From = from;
                To = to;
                Title = title;
                Text = text;
                Time = time;
                IsRead = false;
            }

            internal EMail(string from, string to, string title, string text)
            {
                From = from;
                To = to;
                Title = title;
                Text = text;
                Time = DateTime.Now;
                IsRead = false;
            }

            internal string From { get; }
            internal string To { get; }
            internal string Title { get; }
            internal string Text { get; }
            internal DateTime Time { get; }
            internal bool IsRead { get; set; }
        }
    }
}