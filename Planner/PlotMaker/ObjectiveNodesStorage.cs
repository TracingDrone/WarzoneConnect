using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WarzoneConnect.Player;
using WarzoneConnect.Properties;

namespace WarzoneConnect.Planner.PlotMaker
{
    internal static class ObjectiveNodesStorage //任务节点的编辑位置，在此处编辑任务详情
    {
        private static readonly string BlazeMail = PlotConfig.blaze + '@' + PlotConfig.Mail_Address;
        private static readonly string BlazeBotMail = PlotConfig.blaze_bot + '@' + PlotConfig.Mail_Address;
        
        private static class ToBeContinued
        {
            private static readonly PlotObserver.ObjectiveNode Side02 = new PlotObserver.ObjectiveNode(() =>
                { },BlazeMail,
                email =>
                {
                    if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                        return new[] {Side02};
                    if (!email.Text.Contains(PlotConfig.flag3))
                        return new[] {Side02};
                    MailClient.GetMailBox(GameController.HostList[0])
                        .AddMail(BlazeMail, PlotConfig.Side02_Ending_Mail01_Name,
                            PlotConfig.Side02_Ending_Mail01_Name.Trim());
                    return null;
                });
            
            private static readonly PlotObserver.ObjectiveNode Side01 = new PlotObserver.ObjectiveNode(() =>
                {
                },BlazeBotMail,
                email =>
                {
                    if (email.From != GameController.HostList[2].User + '@' + PlotConfig.Mail_Address)
                        return new[] {Side01};
                    MailClient.GetMailBox(GameController.HostList[2])
                        .AddMail(BlazeBotMail, PlotConfig.Side01_Ending_Mail01_Name,
                            PlotConfig.Side01_Ending_Mail01_Text.Trim());
                    // ReSharper disable once StringLiteralTypo
                    GameController.HostList[1].GetRoot().Add(new AutoSploit.ExpFile("UnionBurst","（暂时性的）UBurst系统通杀",new DateTime(2020,7,30),0,"olaolaolaolamudamudamudamuda",1 ));
                    return null;
                });
            
            internal static PlotObserver.ObjectiveNode[] StartPoint()
            {
                return new []{Side01,Side02};
            }
        }
        
        
        private static class Prologue
        {
            private static class Main02
            {
                private static readonly PlotObserver.ObjectiveNode Sub02 = new PlotObserver.ObjectiveNode(() =>
                {
                },BlazeMail,
                email =>
                {
                    if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                        return new[] {Main};
                    return !email.Text.Contains(PlotConfig.flag2) ? new[] {Sub02} : Ending();
                });
            
            private static readonly PlotObserver.ObjectiveNode Sub01 = new PlotObserver.ObjectiveNode(() =>
                {
                    MailClient.GetMailBox(GameController.HostList[0])
                        .AddMail(BlazeMail, PlotConfig.Main02_Sub01_Mail01_Name,
                            PlotConfig.Main02_Sub01_Mail01_Text.Trim());
                },BlazeMail,
                email =>
                {
                    if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                        return new[] {Main};
                    return !email.Text.Contains(PlotConfig.flag2) ? new[] {Sub02} : Ending();
                });
            
            internal static readonly PlotObserver.ObjectiveNode Main = new PlotObserver.ObjectiveNode(() =>
                {
                    MailClient.GetMailBox(GameController.HostList[0])
                        .AddMail(BlazeMail, PlotConfig.Main02_Mail01_Name,
                            PlotConfig.Main02_Mail01_Text.Trim().
                                Replace("[hacktool_storage_address]",GameController.HostList[1].Addr).
                                Replace("[hacktool_storage_password]",((dynamic)GameController.HostList[1]).Pw));
                },BlazeMail,
                email =>
                {
                    if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                        return new[] {Main};
                    return !email.Text.Contains(PlotConfig.flag2) ? new[] {Sub01} : Ending();
                });
            }
            
            private static class Main01
            {
                private static readonly PlotObserver.ObjectiveNode Sub02 = new PlotObserver.ObjectiveNode(() =>
                    {
                    },BlazeMail,
                    email =>
                    {
                        if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                            return new[] {Main};
                        return email.Text.Contains(PlotConfig.flag1) ? new []{Main02.Main} : new[] {Sub02};
                    });
            
                private static readonly PlotObserver.ObjectiveNode Sub01 = new PlotObserver.ObjectiveNode(() =>
                    {
                        MailClient.GetMailBox(GameController.HostList[0])
                            .AddMail(BlazeMail, PlotConfig.Main01_Sub01_Mail01_Name,
                                PlotConfig.Main01_Sub01_Mail01_Text.Trim());
                    },BlazeMail,
                    email =>
                    {
                        if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                            return new[] {Main};
                        return email.Text.Contains(PlotConfig.flag1) ? new []{Main02.Main} : new[] {Sub02};
                    });

                internal static readonly PlotObserver.ObjectiveNode Main = new PlotObserver.ObjectiveNode(NotifyImportantMail,BlazeMail,
                    email =>
                    {
                        if (email.From != GameController.HostList[0].User + '@' + PlotConfig.Mail_Address)
                            return new[] {Main};
                        return email.Text.Contains(PlotConfig.flag1) ? new []{Main02.Main} : new[] {Sub01};
                    });
            }

            internal static IEnumerable<PlotObserver.ObjectiveNode> StartPoint()
            {
                return new []{Main01.Main};
            }
            private static PlotObserver.ObjectiveNode[] Ending()
            {
                MailClient.GetMailBox(GameController.HostList[0])
                    .AddMail(BlazeMail, PlotConfig.Prologue_Ending_Mail01_Name,
                        PlotConfig.Prologue_Ending_Mail01_Text.Trim());
                GameController.HostList[1].GetRoot().Add(WaFirecracker.WafcExec);
                return ToBeContinued.StartPoint();
            }
        }

        
        internal static List<PlotObserver.ObjectiveNode> GetStartPoint()
        {
            LivelyMail();
            return Prologue.StartPoint().ToList();
        } //定义开始的节点及行为

        private static void NotifyImportantMail()
        {
            Thread.Sleep(500);
            var currentTopPos = Console.CursorTop;
            var currentLeftPos = Console.CursorLeft;
            Console.CursorLeft = 0;
            Console.CursorTop = Console.WindowHeight - 1;
            Console.Write(PlotConfig.ImportantMail);
            Console.CursorTop = currentTopPos;
            Console.CursorLeft = currentLeftPos;
        }

        //生成日常邮件
        private static void LivelyMail()
        {//TODO 发挥想象力
            //游戏开局，告知M01的两封邮件
            MailClient.GetMailBox(GameController.HostList[0])
                .AddMail(BlazeMail, 
                    PlotConfig.Main01_Mail01_Name,
                    PlotConfig.Main01_Mail01_Text.Trim().Replace("[dummy1_address]",GameController.HostList[4].Addr),
                    new DateTime(2020,8,7,9,22,23));
            MailClient.GetMailBox(GameController.HostList[0])
                .AddMail(BlazeMail, 
                    PlotConfig.Main01_Mail02_Name,
                    PlotConfig.Main01_Mail02_Text.Trim(),
                    new DateTime(2020,8,14,16,11,05));
            
            //Side01提示邮件
            MailClient.GetMailBox(GameController.HostList[2])
                .AddMail(BlazeMail, 
                    PlotConfig.Side01_Mail01_Name,
                    PlotConfig.Side01_Mail01_Text.Trim().Replace("[BlazeBotMail]",BlazeBotMail),
                    new DateTime(2020,8,1,12,24,39));
            
            //storage提示邮件
            MailClient.GetMailBox(GameController.HostList[2])
                .AddMail(BlazeMail, PlotConfig.Lively_Mail01_Name,
                    PlotConfig.Lively_Mail01_Text.Trim(),new DateTime(2020,8,3,12,24,39));
            MailClient.GetMailBox(GameController.HostList[2])
                .AddMail(BlazeMail, PlotConfig.Lively_Mail02_Name,
                    PlotConfig.Lively_Mail02_Text.Trim(),new DateTime(2020,8,5,10,00,54));
            
            //周报广告
            MailClient.GetMailBox(GameController.HostList[2])
                .AddMail(PlotConfig.daily_world, PlotConfig.Lively_Mail03_Name,
                    PlotConfig.Lively_Mail03_Text.Trim(),new DateTime(2020,8,2,9,00,05));
            MailClient.GetMailBox(GameController.HostList[2])
                .AddMail(PlotConfig.daily_world, PlotConfig.Lively_Mail04_Name,
                    PlotConfig.Lively_Mail04_Text.Trim(),new DateTime(2020,8,2,9,00,06));
        }
    }
}