using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Stork_Future_TaoLi
{
    public class TestClass
    {
        static EventLog eventlog = new EventLog();

        public static void Writelog(string msg)
        {
            eventlog.WriteEntry(msg);
        }

        public static bool isRun = true;
    }

    public class DebugMode
    {
        public static bool debug = true;
    }
}