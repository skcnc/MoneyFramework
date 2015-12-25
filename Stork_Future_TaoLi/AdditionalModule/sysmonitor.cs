using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{

    class MarketDelayCalculation
    {
        public static int TotalDelaySecond = 0;
        public static int TotalMarketCount = 0;

        private static bool minute_door = false;
        private static int second_door = 0;

        public static void cal(int time,int i)
        {
            if(time == 80006000)
            {
                return;
            }



            if (DateTime.Now.Second != second_door) { 
                //MarketDelayLog.LogInstance.LogEvent(DateTime.Now.ToString());
                second_door = DateTime.Now.Second;

                MarketDelayLog.LogInstance.LogEvent(i.ToString());
            }

            if (time == 0) return;

            string _time = (time / 1000).ToString();
            int hour = Convert.ToInt16(_time.Substring(0, 2));
            int minute = Convert.ToInt16(_time.Substring(2, 2));
            int second = Convert.ToInt16(_time.Substring(4, 2));

            DateTime _now = DateTime.Now;

            int delay = (_now.Hour - hour) * 3600 + (_now.Minute - minute) * 60 + (_now.Second - second);

            TotalDelaySecond += delay;
            TotalMarketCount += 1;

            if (_now.Second == 0)
            {
                if (minute_door == false)
                {
                    minute_door = true;
                    MarketDelayLog.LogInstance.LogEvent(
                        "时间： " + _now.ToString() + "\r\n" +
                        "平均延时： " + (TotalDelaySecond / TotalMarketCount) + "\r\n" +
                        "行情数量： " + TotalMarketCount
                        );
                }
            }
            else
            {
                minute_door = false;
            }

        }
    }
}