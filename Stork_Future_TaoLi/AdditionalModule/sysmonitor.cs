using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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



    class SystemStatusClass
    {
        #region WEB 端用户状态
        public List<WebUserInfo> WebLogInfo { get; set; }
        #endregion

        #region 行情信息
        /// <summary>
        /// 行情分钟频率
        /// </summary>
        public float MarketFrequence { get; set; }

        /// <summary>
        /// 行情分钟平均延时
        /// </summary>
        public float MarketDelay { get; set; }

        /// <summary>
        /// 停盘列表
        /// </summary>
        public List<int> StopStockList { get; set; }
        #endregion

        #region 策略信息

        /// <summary>
        /// 执行策略数量
        /// </summary>
        public int StrategyNum { get; set;  }

        /// <summary>
        /// 策略信息
        /// </summary>
        public Dictionary<string,StrategyInfo> StrategyInfomation { get; set; }

        #endregion

        #region 系统状态
        /// <summary>
        /// 行情系统状态
        /// </summary>
        public bool MarketSystemStatus { get; set; }

        /// <summary>
        /// 心跳发生系统状态
        /// </summary>
        public bool HeartBeatSystemStatus { get; set; }

        /// <summary>
        /// 策略管理系统状态
        /// </summary>
        public bool StrategyManagementSystemStatus { get; set; }

        /// <summary>
        /// 策略工作系统状态
        /// </summary>
        public Dictionary<string, int> StrategyWorkerSystemStatus { get; set; }

        /// <summary>
        /// 交易预处理系统状态
        /// </summary>
        public bool Pre_TradeControlSystemStatus { get; set; }

        /// <summary>
        /// 期货交易管理系统状态
        /// </summary>
        public bool FutureTradeManagementSystemStatus { get; set; }

        /// <summary>
        /// 期货交易工作系统状态
        /// </summary>
        public List<bool> FutureTradeWorkerSystemStatus { get; set; }
        
        /// <summary>
        /// 股票交易管理系统状态
        /// </summary>
        public bool StockTradeManagementSystemStatus { get; set; }

        /// <summary>
        /// 股票交易工作系统状态
        /// </summary>
        public List<bool> StockTradeWorkerSystemStatus { get; set; }

        /// <summary>
        /// 委托查询系统状态
        /// </summary>
        public bool StockEntrustManagementSystemStatus { get; set; }
        #endregion 

    }

    /// <summary>
    /// 用户信息
    /// </summary>
    class WebUserInfo
    {
        /// <summary>
        /// 用户登陆名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登陆时间：分钟
        /// </summary>
        public int Minute { get; set; }
    }

    /// <summary>
    /// 行情信息
    /// </summary>
    class StrategyInfo
    {
        /// <summary>
        /// 策略创建者
        /// </summary>
        public string BelongUser { get; set; }

        /// <summary>
        /// 策略类型
        /// </summary>
        public string StrType { get; set; }

        /// <summary>
        /// 策略合约
        /// </summary>
        public string Contract { get; set; }

        /// <summary>
        /// 指数类型
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 策略状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 线程号
        /// </summary>
        public int ThreadNo { get; set; }

        /// <summary>
        /// 订阅列表
        /// </summary>
        public List<int> SubscribeList { get; set; }
    }


    class ThreadLatestMessage
    {
        public DateTime MarketSystemStatus { get; set; }
        public DateTime HeartBeatSystemStatus { get; set; }
        public DateTime StrategyManagementSystemStatus { get; set; }
        public Dictionary<string, DateTime> StrategyWorkerSystemStatus { get; set; }
        public DateTime Pre_TradeControlSystemStatus { get; set; }
        public DateTime FutureTradeManagementSystemStatus { get; set; }
        public List<DateTime> FutureTradeWorkerSystemStatus { get; set; }
        public DateTime StockTradeManagementSystemStatus { get; set; }
        public List<DateTime> StockTradeWorkerSystemStatus { get; set; }
        public DateTime StockEntrustManagementSystemStatus { get; set; }
    }

    class SystemMonitorClass
    {
        /// <summary>
        /// 线程变量
        /// </summary>
        private Thread excuteThread = new Thread(new ThreadStart(ThreadProc));

        private static SystemMonitorClass instance;

        private static SystemStatusClass status = new SystemStatusClass();

        private static ThreadLatestMessage messageData = new ThreadLatestMessage();

        /// <summary>
        /// 外部执行函数
        /// </summary>
        public void Run()
        {
            excuteThread.Start();
            Thread.Sleep(10);
        }

        public static SystemMonitorClass getInstance()
        {
            if (instance == null)
            {
                instance = new SystemMonitorClass();
            }

            return instance;
        }

        /// <summary>
        /// 线程函数
        /// </summary>
        private static void ThreadProc()
        {
            DateTime lastmessage = DateTime.Now;

            status.FutureTradeWorkerSystemStatus = new List<bool>(CONFIG.FUTURE_TRADE_THREAD_NUM);
            status.StockTradeWorkerSystemStatus = new List<bool>(CONFIG.STOCK_TRADE_THREAD_NUM);


            messageData.FutureTradeWorkerSystemStatus = new List<DateTime>(CONFIG.FUTURE_TRADE_THREAD_NUM);
            messageData.StockTradeWorkerSystemStatus = new List<DateTime>(CONFIG.STOCK_TRADE_THREAD_NUM);

            status.StopStockList = new List<int>();
            status.StrategyInfomation = new Dictionary<string, StrategyInfo>();
            status.StrategyWorkerSystemStatus = new Dictionary<string, int>();
            status.WebLogInfo = new List<WebUserInfo>();

            while (true)
            {
                if(lastmessage.Minute != DateTime.Now.Minute   ){
                    lastmessage = DateTime.Now;

                    //此处需要把信息发出去
                }
                

                if (queue_system_status.GetQueueNumber() == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }


                object obj = queue_system_status.GetQueue().Dequeue();

                KeyValuePair<string, object> news = (KeyValuePair<string, object>)obj;

                switch (news.Key)
                {
                    case "USER":
                        arrive_userinfo(news.Value);
                        break;
                    case "MARKET_F":
                        arrive_market_frequence(news.Value);
                        break;
                    case "MARKET_D":
                        arrive_market_delay(news.Value);
                        break;
                    case "MARKET_STOCK_STOP":
                        arrive_market_stock_stop(news.Value);
                        break;
                    case "STRATEGY_N":
                        arrive_strategy_num(news.Value);
                        break;
                    case "STRATEGY_P":
                        arrive_strategy_info(news.Value);
                        break;
                    case "THREAD_MARKET":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_HEARTBEAT":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_STRATEGY_MANAGEMENT":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_STRATEGY_WORKER":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_PRE_TRADE":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_FUTURE_TRADE_MONITOR":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_FUTURE_TRADE_WORKER":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_STOCK_TRADE_MONITOR":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_STOCK_TRADE_WORKER":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    default:
                        break;

                }


                

            }
        }


        #region 消息处理函数
        /// <summary>
        /// 用户状态消息处理
        /// </summary>
        /// <param name="obj">用户消息</param>
        private static void arrive_userinfo(object obj) { }

        /// <summary>
        /// 行情分钟频率消息处理
        /// </summary>
        /// <param name="obj">行情频率</param>
        private static void arrive_market_frequence(object obj) { }

        /// <summary>
        /// 行情分钟延迟消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_market_delay(object obj) { }

        /// <summary>
        /// 停盘行情消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_market_stock_stop(object obj) { }

        /// <summary>
        /// 策略数量消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_strategy_num(object obj) { }

        /// <summary>
        /// 策略信息消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_strategy_info(object obj) { }

        /// <summary>
        /// 线程状态消息处理
        /// </summary>
        /// <param name="head"></param>
        /// <param name="obj"></param>
        private static void arrive_thread_status(string head, object obj)
        {

            switch (head)
            {
                case "THREAD_MARKET":
                    messageData.MarketSystemStatus = DateTime.Now;
                    break;
                case "THREAD_HEARTBEAT":
                    messageData.HeartBeatSystemStatus = DateTime.Now;
                    break;
                case "THREAD_STRATEGY_MANAGEMENT":
                    messageData.StrategyManagementSystemStatus = DateTime.Now;
                    break;
                case "THREAD_STRATEGY_WORKER":
                    {
                        messageData.StrategyWorkerSystemStatus.Clear();
                        foreach(KeyValuePair<string,int> value in (Dictionary<string,int>)obj)
                        {
                            messageData.StrategyWorkerSystemStatus.Add(value.Key, DateTime.Now);
                        }
                    }
                    break;
                case "THREAD_PRE_TRADE":
                    messageData.Pre_TradeControlSystemStatus = DateTime.Now;
                    break;
                case "THREAD_FUTURE_TRADE_MONITOR":
                    messageData.FutureTradeManagementSystemStatus = DateTime.Now;
                    break;
                case "THREAD_FUTURE_TRADE_WORKER":
                    {
                        //int No = (int)obj;
                        //status.FutureTradeWorkerSystemStatus[No] = DateTime.Now;
                    }
                    break;
                case "THREAD_STOCK_TRADE_MONITOR":
                    messageData.StockTradeManagementSystemStatus = DateTime.Now;
                    break;
                case "THREAD_STOCK_TRADE_WORKER":
                    {
                        //for(int i =0;i<((List<bool>)obj).Count;i++)
                        //{
                        //    status.StockTradeWorkerSystemStatus[i] = ((List<bool>)obj)[i];
                        //}
                    }
                    break;
                default: break;
            }
        }
        #endregion
    }
}