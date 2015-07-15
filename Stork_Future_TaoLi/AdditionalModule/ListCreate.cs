using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using System.Threading;
using Stork_Future_TaoLi.Modulars;
using System.Threading.Tasks;
//using MCStockLib;

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
            string cExhcnageID = "0";
            string cTradeDirection = "0";
            string cSecurityCode = "0";
            string SecurityName = "0";
            int nSecurityAmount = 0;
            double dOrderPrice = 0;
            string cOrderPriceType = "0";
            string cSecurityType = "0";
            string cOrderLevel = "0";
            string cOrderexecutedetail = "0";
            string cOffsetFlag = "0";

            
            Random seed = new Random();

            if (DateTime.Now.Millisecond % 2 == 0)
            {
                cExhcnageID = ExhcnageID.SH;
                cSecurityCode = "600269";
                dOrderPrice = 7.5;
                nSecurityAmount = 100;
                
                
            }
            else
            {
                cExhcnageID = ExhcnageID.SZ;
                cSecurityCode = "000001";
                dOrderPrice = 15;
                nSecurityAmount = 100;
            }


            cTradeDirection = TradeDirection.Buy; 
            

            SecurityName = "tcode" + DateTime.Now.Millisecond.ToString();


            cOrderPriceType = string.Empty;

            cSecurityType = SecurityType.STOCK;

            cOrderLevel = seed.Next(1, 2).ToString();

            cOrderexecutedetail = "test";


            //return new managedTraderorderstruct(cExhcnageID, cSecurityCode, SecurityName, nSecurityAmount, dOrderPrice, Convert.ToSByte(cTradeDirection), Convert.ToSByte(cOffsetFlag), Convert.ToSByte(cOrderPriceType), Convert.ToSByte(cSecurityType), Convert.ToSByte(cOrderLevel), Convert.ToSByte(cOrderexecutedetail));
            TradeOrderStruct tos = new TradeOrderStruct();
            tos.cExhcnageID = cExhcnageID;
            tos.cOffsetFlag = cOffsetFlag;
            tos.cOrderexecutedetail = cOrderexecutedetail;
            tos.cOrderLevel = cOrderLevel;
            tos.cOrderPriceType = cOrderPriceType;
            tos.cSecurityCode = cSecurityCode;
            tos.cSecurityType = cSecurityType;
            tos.cTradeDirection = cTradeDirection;
            tos.dOrderPrice = dOrderPrice;
            tos.nSecurityAmount = nSecurityAmount;
            tos.SecurityName = SecurityName;
            return tos;





        }

        public static void Main()
        {
            log.EventSourceName = "测试List发送模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62300;

            Task SendListTask = new Task(ThreadProc);
            //SendListTask.Start();
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
                if (loop == 1)
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