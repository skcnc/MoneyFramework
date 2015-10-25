using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            TestFuture trade = new TestFuture();
            //TestFuture trade1 = new TestFuture();
            trade.name = "name1";
            //trade1.name = "name2";
            Thread mythread = new Thread(new ThreadStart(trade.FutureTradeSubThreadProc));
            //Thread mythread1 = new Thread(new ThreadStart(trade1.FutureTradeSubThreadProc));

            mythread.Start();
            //mythread1.Start();

            //Console.WriteLine(Json.GetJson());

            Console.ReadLine();
        }
    }
}
