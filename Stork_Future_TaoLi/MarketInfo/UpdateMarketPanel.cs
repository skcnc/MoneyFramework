using Stork_Future_TaoLi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class UpdateMarketPanel
    {
        public void Run()
        {
            Thread excutedThread = new Thread(new ThreadStart(ThreadProc));
            excutedThread.Start();
        }

        private void ThreadProc()
        {
            while (true)
            {
                Thread.Sleep(1);

                while (marketMonitorQueue.GetQueueLength() > 0)
                {
                    MarketValue value = marketMonitorQueue.DeQueueNew();
                    if (value == null) continue;
                    MarketMonitor.Instance.Send(value);
                }
            }
        }
    }
}