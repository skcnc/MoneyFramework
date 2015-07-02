using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Stork_Future_TaoLi.StockInfoService;
using System.Threading.Tasks;



namespace Stork_Future_TaoLi.MarketInfo
{
    public class MarketInfo
    {
        private static void ThreadProc()
        {
            StockInfoService.StockInfoClient client = new StockInfoClient();
            while (true)
            {
                object x = client.DeQueueInfo();
            }
        }
    }
}