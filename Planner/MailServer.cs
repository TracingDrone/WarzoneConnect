using System.Linq;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    public static class MailServer
    {
        internal delegate void MailInspection(MailBox.EMail email); //留给PlotObserver执行触发点监控
        internal static event MailInspection Trigger;
        
        internal static void RebuildMails() //重建邮箱，此处填入初始化的邮箱信息
        {
            
            ((dynamic) GameController.HostList[0]).Mailbox = new MailBox(GameController.HostList[0].User);
            ((dynamic) GameController.HostList[1]).Mailbox = new MailBox(GameController.HostList[1].User);
            ((dynamic) GameController.HostList[2]).Mailbox = new MailBox(GameController.HostList[2].User);
            ((dynamic) GameController.HostList[3]).Mailbox = new MailBox(GameController.HostList[3].User);
        }
        
        internal static void Send(MailBox.EMail email)
        {
            // foreach (var r in PlotObserver.RspList.Where(r => r.User == email.To)) 
            // { 
            //     r.AddMail(email.From, email.Title, email.Text);
            //     PlotObserver.Trigger(email);
            //     return;
            // }

            Trigger?.Invoke(email);

            foreach (var mb in GameController.HostList.Select(MailClient.GetMailBox).Where(mb => mb != null && mb.User == email.To))
            {
                mb.AddMail(email.From, email.Title, email.Text);
                return;
            }
        }
    }
}