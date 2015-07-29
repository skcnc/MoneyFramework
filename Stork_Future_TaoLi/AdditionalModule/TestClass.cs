using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;

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
            }
        }
    }

    public class DebugMode
    {
        public static bool debug = false;
    }

    
}