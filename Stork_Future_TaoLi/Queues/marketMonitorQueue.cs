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
        public uint Match { get; set; }
        public DateTime Time { get; set; }
        public int Type { get; set; }
        public bool isStop { get; set; }
        public uint HighLimit { get; set; }
        public uint LowLimit { get; set; }
        public uint PreClose { get; set; }

    }

    public class marketMonitorQueue
    {
        private static Queue instance = new Queue();

        //类型 时间 价格  是否停牌 涨停价格 跌停价格  昨收价
        public static void EnQueueNew(String code, int Time, uint Match, int status , uint HighLimit, uint LowLimit,  uint preClose,int type)
        {
            {
                v.isStop = true;
            }
            else { v.isStop = false; }

            v.HighLimit = HighLimit;
            v.LowLimit = LowLimit;
            v.PreClose = preClose;
            v.Code = code;


            instance.Enqueue((object)v);
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