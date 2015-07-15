using MarketInfoSys.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarketInfoSys
{
    class Program
    {
        static void Main(string[] args)
        {
            //启动行情服务
            TDFMain.Run();

            //启动WCF服务
            ServiceHost host = new ServiceHost(typeof(StockInfo));
            host.Open();

            //ServiceHost crossDomainserivceHOST = new ServiceHost(typeof(DomainService));
            //crossDomainserivceHOST.Open();
            DateTime dt = DateTime.Now;
            while(true)
            {
                if((DateTime.Now - dt).TotalSeconds > 10)
                {
                    dt = DateTime.Now;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss" ) + "  股市队列长度:" + Queue_Market_Data.GetQueue().Count);
                }

                if (dt.DayOfWeek == DayOfWeek.Saturday)
                {
                    break;
                }

                Thread.Sleep(100);
            }
            //Console.ReadLine();
            host.Close();
            //crossDomainserivceHOST.Close();
        }
    }
}
