using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using System.Threading;
using Stork_Future_TaoLi.Modulars;
using System.Threading.Tasks;

namespace Stork_Future_TaoLi.Test
{
    /// <summary>
    /// 测试使用
    /// 每隔ListSendInter时间生成list用于处理
    /// 每个list中间包含10 ~100个交易不等
    /// 每个交易内容自动生成
    /// </summary>
    public class ListCreate
    {
        //测试数据
        private static int loop = 1; //生成轮数


        /// <summary>
        /// 每秒生成一个list
        /// </summary>
        static int _ListSendInter = 1;
        private static LogWirter log = new LogWirter();

        /// <summary>
        /// 每个list中的交易数量
        /// </summary>
        static int _ListNum = 100;

        static TradeOrderStruct createRandomTrade()
        {
            TradeOrderStruct tos = new TradeOrderStruct();
            Random seed = new Random();

            if (DateTime.Now.Millisecond % 2 == 0)
            {
                tos.cExhcnageID = ExhcnageID.SH;
                tos.cTradeDirection = TradeDirection.STORK; 
            }
            else
            {
                tos.cExhcnageID = ExhcnageID.SZ;
                tos.cTradeDirection = TradeDirection.STORK; 
            }

            int code = seed.Next(60000, 70000);
            tos.cSecurityCode = code.ToString();

            tos.SecurityName = "tcode" + DateTime.Now.Millisecond.ToString();

            tos.nSecurityAmount = seed.Next(1, 100);

            tos.dOrderPrice = seed.NextDouble() * 100;

            tos.cOrderPriceType = (tos.dOrderPrice * 1.09).ToString();

            tos.cSecurityType = "stock";

            tos.cOrderLevel = seed.Next(1, 2).ToString();

            tos.cOrderexecutedetail = "Test";

            return tos;





        }

        public static void Main()
        {
            log.EventSourceName = "测试List发送模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62300;

            Task SendListTask = new Task(ThreadProc);
            SendListTask.Start();
        }

        static void ThreadProc()
        {
            Thread.CurrentThread.Name = "SendListThread";
            Thread.Sleep(3000);

            bool isRun = true;

            while (isRun)
            {
                Thread.Sleep(_ListSendInter * 1000);

                List<TradeOrderStruct> tradeList = new List<TradeOrderStruct>();

                Random seed = new Random();

                _ListNum = seed.Next(1, 100);

                for (int i = 0; i < _ListNum; i++)
                {
                    tradeList.Add(createRandomTrade());
                }

                log.LogEvent("入队交易数量：" + tradeList.Count.ToString());

                queue_prd_trade.GetQueue().Enqueue((object)tradeList);

                //只执行一次
                if (loop == 3)
                {
                    isRun = false;
                }
                else
                {
                    loop++;
                    isRun = true;
                }

            }
        }
    }

    
}