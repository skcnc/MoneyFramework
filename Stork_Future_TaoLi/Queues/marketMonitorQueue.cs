using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class MarketValue
    {
        public String Code { get; set; }
        public String Value { get; set; }
    }

    public class marketMonitorQueue
    {
        private static Queue instance;

        public static void EnQueueNew(String code, String value)
        {
            instance.Enqueue((object)(new MarketValue()
            {
                Code = code,
                Value = value
            }));
        }

        public static MarketValue DeQueueNew()
        {
            if (instance.Count == 0) return null;
            return (MarketValue)(instance.Dequeue());
        }

        public static int GetQueueLength()
        {
            return instance.Count;
        }
    }
}