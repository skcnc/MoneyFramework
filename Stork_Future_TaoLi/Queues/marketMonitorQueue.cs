using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class MarketInfo
    {
        public String Code { get; set; }
        public String Value { get; set; }
    }

    public class marketMonitorQueue
    {
        private static Queue instance;

        public static void EnQueueNew(String code, String value)
        {
            instance.Enqueue((object)(new MarketInfo()
            {
                Code = code,
                Value = value
            }));
        }

        public static MarketInfo DeQueueNew()
        {
            if (instance.Count == 0) return null;
            return (MarketInfo)(instance.Dequeue());
        }
    }
}