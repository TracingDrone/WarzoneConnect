using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WarzoneConnect.Planner;

namespace WarzoneConnect.Player
{
    [Serializable]
    public class WebApplicationFirewall
    {
        private static readonly List<NetworkFlow> NetworkStatistics = new List<NetworkFlow>();
        private static bool _isDisabled;
        private double _incursionStrength;
        internal bool IsTough;

        public WebApplicationFirewall(double incursionStrength, bool isTough)
        {
            _incursionStrength = incursionStrength;
            IsTough = isTough;
        }

        internal void FlowAnalyser(string source)
        {
            if (_isDisabled)
                return;
            var nf = NetworkStatistics.Find(n => n.Src == source);
            if (nf == null)
            {
                NetworkStatistics.Add(new NetworkFlow(source));
            }
            else
            {
                if (nf.Repeat == 0) nf.Cooldown();
                nf.Repeat++;
                if (nf.Repeat <= 10) return;
                _isDisabled = WafServer.IncursionSetup(source, _incursionStrength);
            }
        }

        internal void DangerZone(string source)
        {
            _isDisabled = WafServer.IncursionSetup(source, _incursionStrength);
        }

        private class NetworkFlow
        {
            internal readonly string Src;
            internal int Repeat;

            internal NetworkFlow(string source)
            {
                Src = source;
                Repeat = 0;
            }

            internal void Cooldown()
            {
                new Task(() =>
                {
                    Thread.Sleep(10000);
                    NetworkStatistics.Remove(this);
                }).Start();
            }
        }
    }
}