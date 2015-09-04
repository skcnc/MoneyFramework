using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;
using Stork_Future_TaoLi.Hubs;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi
{
    public class TestClass
    {
        public void Run()
        {
            Thread excutedThread = new Thread(new ThreadStart(ThreadProc));
            excutedThread.Start();

            Thread.Sleep(1000);
        }

        private void ThreadProc()
        {
            Random ra= new Random();
            int k = 0;
            while(true)
            {

                int num = PushStrategyInfo.Instance.getnum();
                int next = ra.Next(num);
                string name = PushStrategyInfo.Instance.getkey(next);

                if (name == string.Empty)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                PushStrategyInfo.Instance.UpdateStrategyInfo(name, DateTime.Now.ToString());
                Thread.Sleep(1000);

                if(k >= 10)
                {
                    List<OrderViewItem> orders = new List<OrderViewItem>();
                    orders.Add(new OrderViewItem("20001", "37", "AD1509", "0", "0", "100", "20", "18.08", "部分提交", DateTime.Now.ToString()));
                    orders.Add(new OrderViewItem("20002", "37", "AD1509", "0", "0", "100", "20", "18.08", "部分提交", DateTime.Now.ToString()));
                    orders.Add(new OrderViewItem("20003", "37", "AD1509", "0", "0", "100", "20", "18.08", "部分提交", DateTime.Now.ToString()));
                    orders.Add(new OrderViewItem("20004", "37", "AD1509", "0", "0", "100", "20", "18.08", "部分提交", DateTime.Now.ToString()));
                    orders.Add(new OrderViewItem("20005", "37", "AD1509", "0", "0", "100", "20", "18.08", "部分提交", DateTime.Now.ToString()));

                    foreach (OrderViewItem item in orders)
                    {
                        String JSONString = JsonConvert.SerializeObject(item);
                        TradeMonitor.Instance.updateOrderList("sa", JSONString);
                    }
                   
                }

                k++;
            }
        }
    }

    public class DebugMode
    {
        public static bool debug = false;
    }

    
}