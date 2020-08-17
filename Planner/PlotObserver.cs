using System;
using System.Collections.Generic;
using System.Resources;
using System.Threading.Tasks;
using WarzoneConnect.Planner.PlotMaker;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    public static class PlotObserver
    {
        private static List<ObjectiveNode> _currentNode;
        
        [Serializable]
        internal class ObjectiveNode //任务节点，定义该任务的剧情行为，对邮件的监听行为以及在不同条件下通向的下个剧情点
        {
            // 可以做到一对多（建立多线任务），但还没做到多对一（剧情线收束）
            private Action _intro; //开场的剧情行为
            private string _triggerAddress; //触发检查信号的收件人地址
            private Func<MailBox.EMail,ObjectiveNode[]> _condition; //由trigger触发，判断是否满足任务的条件并指引到下一个节点

            internal MailServer.MailInspection TriggerPlay; //触发器，不要修改
            internal void FreshStart()
            {
                var introTask = new Task(_intro);
                introTask.Start();
                introTask.Wait();
                introTask.Dispose();
                HalfwayStart();
            }

            internal void HalfwayStart()
            {
                TriggerPlay = email => {
                    if (email.To != _triggerAddress) return;
                    var conditionTask = new Task<ObjectiveNode[]>(() => _condition.Invoke(email));
                    conditionTask.Start();
                    conditionTask.Wait();
                    MailServer.Trigger -= TriggerPlay;
                    foreach (var nextNode in conditionTask.Result)
                    {
                        _currentNode.Add(nextNode); 
                        new Task(() => nextNode.FreshStart()).Start();
                    }
                    _currentNode.Remove(this);
                    conditionTask.Dispose();
                };
                MailServer.Trigger += TriggerPlay;
            }

            internal ObjectiveNode(Action intro, string triggerAddress, Func<MailBox.EMail,ObjectiveNode[]> condition)
            {
                _intro = intro;
                _triggerAddress = triggerAddress;
                _condition = condition;
            }
        }

        internal static void InitializePlot()
        {
            GameController.SaveLoadList.Add(new GameController.SaveLoadActions(Save,Load));
            _currentNode = ObjectiveNodesStorage.GetStartPoint();
        }

        internal static void StartObserve()
        {
            foreach (var node in _currentNode)
            {
                new Task(() => node.FreshStart()).Start();
            }
        }

        private static void HalfwayObserve()
        {
            foreach (var node in _currentNode)
            {
                new Task(() => node.HalfwayStart()).Start();
            }
        }

        private static void Save(ResXResourceWriter resx)
        {
            resx.AddResource("Objectives",_currentNode);
        }

         private static bool Load(ResXResourceSet resxSet)
         {
             try
             {
                 // StopObserve();
                 if (_currentNode != null)
                     foreach (var node in _currentNode)
                         MailServer.Trigger -= node.TriggerPlay;
                 
                 _currentNode = (List<ObjectiveNode>) resxSet.GetObject("Objectives");

                 HalfwayObserve();
                 return true;
             }
             catch
             {
                 return false;
             }
             finally
             {
                 AutoSave();
             }
         }
         
         private static void AutoSave() //连接成功后会AutoSave
         {
             LinkServer.UpperInspection += GameController.Save;
         }
    }
}