using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.StrategyModule
{
    public class AuthorizedStrategy
    {

        private static AuthorizedStrategy instance = new AuthorizedStrategy();
        private static Thread excuteThreadA = new Thread(new ThreadStart(ThreadProc_Check));
        private static Thread excuteThreadB = new Thread(new ThreadStart(ThreadProc_Check));
        private static LogWirter log = new LogWirter();

        private AuthorizedStrategy()
        {
            log.EventSourceName = "授权交易模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 66001;
        }

        public static void RUN()
        {
            excuteThreadA.Start();
            excuteThreadB.Start();
        }

        private static void ThreadProc_Check()
        {
            log.LogEvent("授权交易线程A启动！");

            while (true)
            {

            }
        }

        private static void ThreadProc_PushInfo()
        {
            log.LogEvent("授权交易线程B启动！");

            while (true)
            {

            }
        }
    }
}