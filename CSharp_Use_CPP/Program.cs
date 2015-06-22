using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CSharp_Use_CPP
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 之前的测试
            //StockTradeAPIBrage _StockTradeDLLApi = new StockTradeAPIBrage();
          
            //unsafe
            //{
            //    Logininfor vlogin = new Logininfor();

            //    /* ToDu ***********/
            //    /* 登陆参数初始化 */

            //    string str = "Hello Cheng";

            //    byte[] bytes = Encoding.ASCII.GetBytes(str);
            //    sbyte[] sbytes = new sbyte[bytes.Length];

            //    for (int i = 0; i < bytes.Length; i++)
            //    {
            //        if (bytes[i] > 127)
            //            sbytes[i] = (sbyte)(bytes[i] - 256);
            //        else
            //            sbytes[i] = (sbyte)bytes[i];
            //    }
            //    StringBuilder sb = new StringBuilder();

            //    fixed (sbyte* pError = sbytes)
            //    {
            //        _StockTradeDLLApi.TestApi(pError,vlogin);

            //        //for (int i = 0; pError[i] != '\0'; i++)
            //        //{
                        
            //        //}


            //        int i = 0;
            //        StringBuilder str1 = new StringBuilder();
            //        StringBuilder str2 = new StringBuilder();

            //        while (Convert.ToChar(pError[i]) != '\0')
            //        {
            //            i++;
            //            str1.Append(Convert.ToChar(pError[i]));
            //        }

            //        i = 0;

            //        while (Convert.ToChar(vlogin.BROKER_ID[i]) != '\0')
            //        {
            //            i++;
            //            str2.Append(Convert.ToChar(vlogin.BROKER_ID[i]));
            //        }


            //        Console.WriteLine(str1);
            //        Console.WriteLine(str2);
              
                    

            //    }

            //    Console.ReadLine();
            #endregion


            int threadNum = 2;
            List<Task> TradeThreads = new List<Task>();

            Console.WriteLine("测试开始");

            for (int i = 0; i < threadNum; i++)
            {
                TradeThreads.Add(Task.Factory.StartNew(() => ThreadProc((object)i)));
                Thread.Sleep(3000);
            }


            //MCStockLib.managedStockClass stockLib = new MCStockLib.managedStockClass();

            //MCStockLib.managedLogin info = new MCStockLib.managedLogin("172.0.0.1", 80, "abc", "abc", "abc", "abc");

            //string err = "false";
            //stockLib.Init(info, err);

            //Console.WriteLine();

            Console.ReadLine();
          




        }


        static void ThreadProc(object iNo)
        {
            int _threadNo = (int)iNo;
            Console.WriteLine("线程" + iNo + ":  " + "启动完成！");
            Thread.Sleep(3000);

            MCStockLib.managedStockClass stockLib = new MCStockLib.managedStockClass();

            MCStockLib.managedLogin info = new MCStockLib.managedLogin("172.0.0.1", 80, "abc", "abc", "abc", "abc");

            while(true)
            {
                Random ra = new Random();
                int i = ra.Next(15, 100) - _threadNo;
                int j = ra.Next(15, 100) - _threadNo;
                Console.WriteLine("线程" + iNo + ":  " + i + "+" + j + "=");

                int k = stockLib.cal(i, j);

                Console.WriteLine("线程" + iNo + ":  " + i + "+" + j + "=" + k);

            }

        }

       
    }
}
