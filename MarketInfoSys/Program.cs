using MarketInfoSys.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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

            Console.ReadLine();
            host.Close();
            //crossDomainserivceHOST.Close();
        }
    }
}
