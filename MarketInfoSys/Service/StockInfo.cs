using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MarketInfoSys
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“Service1”。
    public class StockInfo : IStockInfo
    {
        public int DoWork(int a , int b)
        {
            return a + b;
        }

        public int DoWork2(int a, int b)
        {
            return b;
        }

        public MarketData DeQueueInfo()
        {
            lock (Queue_Market_Data.GetQueue().SyncRoot)
            {
                if (Queue_Market_Data.GetQueue().Count > 0)
                {
                    return (MarketData)(Queue_Market_Data.GetQueue().Dequeue());
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
