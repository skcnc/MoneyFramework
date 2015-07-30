using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.MarketInfo
{
    public class UpdateMarketPanel
    {
        private void Run()
        {
            Thread excutedThread = new Thread(new ThreadStart(ThreadProc));
            excutedThread.Start();
        }

        private void ThreadProc()
        {

        }
    }
}