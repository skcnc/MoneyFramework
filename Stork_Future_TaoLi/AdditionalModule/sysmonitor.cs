using Stork_Future_TaoLi.Hubs;
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

        
        private static DateTime updatetime = DateTime.Now;

        public static void cal(int time)
        {
            if(time == 80006000)
            {
                return;
            }


            if (time == 0) return;

            try
            {
                string _time = (time / 1000).ToString();

                if (_time.Length == 5) _time = "0" + _time;

                int hour = Convert.ToInt16(_time.Substring(0, 2));
                int minute = Convert.ToInt16(_time.Substring(2, 2));
                int second = Convert.ToInt16(_time.Substring(4, 2));

                DateTime _now = DateTime.Now;

                int delay = (_now.Hour - hour) * 3600 + (_now.Minute - minute) * 60 + (_now.Second - second);

                if (delay > 100) 
                {
                    return; 
                }


                TotalDelaySecond += delay;
                TotalMarketCount += 1;

                if (_now.Minute != updatetime.Minute)
                {
                    updatetime = _now;

                    MarketDelayLog.LogInstance.LogEvent(
                        "时间： " + _now.ToString() + "\r\n" +
                        "平均延时： " + (TotalDelaySecond / TotalMarketCount) + "\r\n" +
                        "行情数量： " + TotalMarketCount
                        );

                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("MARKET_F", (object)TotalMarketCount);
                    queue_system_status.GetQueue().Enqueue((object)message1);

                    KeyValuePair<string, object> message2 = new KeyValuePair<string, object>("MARKET_D", (object)(TotalDelaySecond / TotalMarketCount));
                    queue_system_status.GetQueue().Enqueue((object)message2);

                }

            }
            catch
            {
                return;
            }

        }
    }



    public class SystemStatusClass
    {
        #region WEB 端用户状态
        //public List<WebUserInfo> WebLogInfo { get; set; }
        #endregion

        #region 行情信息
        /// <summary>
        /// 行情分钟频率
        /// </summary>
        public float MarketFrequence { get; set; }

        /// <summary>
        /// 行情分钟平均延时
        /// </summary>
        public double MarketDelay { get; set; }

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
        public List<StrategyInfo> StrategyInformation { get; set; }

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

        /// <summary>
        /// 退货控制系统状态
        /// </summary>
        public bool RefundControlSystemStatus { get; set; }

        /// <summary>
        /// 股票退货系统状态
        /// </summary>
        public bool RefundStockSystemStatus { get; set; }

        /// <summary>
        /// 期货退货系统状态
        /// </summary>
        public bool RefundFutureSystemStatus { get; set; }
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
    public class StrategyInfo
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
        //public string Index { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        //public DateTime CreateTime { get; set; }

        /// <summary>
        /// 策略状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 线程号
        /// </summary>
        public string StrategyInstanceID { get; set; }

        /// <summary>
        /// 订阅列表
        /// </summary>
        public List<string> SubscribeList { get; set; }

        /// <summary>
        /// 手数
        /// </summary>
        public int HandNum { get; set; }
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
        public DateTime RefundControlSystemStatus { get; set; }
        public DateTime RefundStockSystemStatus { get; set; }
        public DateTime RefundFutureSystemStatus { get; set; }
        public double MarketDelay { get; set; }
        public int MarketFrequence { get; set; }
        public Dictionary<string, StrategyInfo> StrategyInfomation { get; set; }
        public int StrategyNum { get; set; }
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
            messageData.FutureTradeWorkerSystemStatus = new List<DateTime>(CONFIG.FUTURE_TRADE_THREAD_NUM);
            for (int i = 0; i < CONFIG.FUTURE_TRADE_THREAD_NUM; i++)
            {
                messageData.FutureTradeWorkerSystemStatus.Add(DateTime.Now);
            }
            messageData.StockTradeWorkerSystemStatus = new List<DateTime>(CONFIG.STOCK_TRADE_THREAD_NUM);

            for (int i = 0; i < CONFIG.STOCK_TRADE_THREAD_NUM; i++)
            {
                messageData.StockTradeWorkerSystemStatus.Add(DateTime.Now);
            }

            messageData.StrategyWorkerSystemStatus = new Dictionary<string, DateTime>();

            status.FutureTradeWorkerSystemStatus = new List<bool>(CONFIG.FUTURE_TRADE_THREAD_NUM);

            for (int i = 0; i < CONFIG.FUTURE_TRADE_THREAD_NUM; i++)
            {
                status.FutureTradeWorkerSystemStatus.Add(false);
            }

            status.StockTradeWorkerSystemStatus = new List<bool>(CONFIG.STOCK_TRADE_THREAD_NUM);

            for (int i = 0; i < CONFIG.STOCK_TRADE_THREAD_NUM; i++)
            {
                status.StockTradeWorkerSystemStatus.Add(false);
            }

            status.StopStockList = new List<int>();
            status.StrategyInformation = new List<StrategyInfo>(); 
            status.StrategyWorkerSystemStatus = new Dictionary<string, int>();

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

            
            //status.WebLogInfo = new List<WebUserInfo>();

            while (true)
            {
                if ((lastmessage.Second != DateTime.Now.Second) && (DateTime.Now.Second % 2 == 0))
                {
                    lastmessage = DateTime.Now;

                    //此处需要把信息发出去
                    SystemStatusClass message = new SystemStatusClass();
                    DateTime current = DateTime.Now;

                    message.MarketDelay = messageData.MarketDelay;
                    message.MarketFrequence = messageData.MarketFrequence;

                    message.StrategyNum = messageData.StrategyNum;
                    message.StrategyInformation = new List<StrategyInfo>();

                    if(messageData.StrategyInfomation == null)
                    {
                        messageData.StrategyInfomation = new Dictionary<string, StrategyInfo>();
                    }

                    foreach(KeyValuePair<String,StrategyInfo> info in messageData.StrategyInfomation)
                    {
                        message.StrategyInformation.Add(info.Value);
                    }
                    

                    message.StrategyWorkerSystemStatus = new Dictionary<string, int>();
                    message.FutureTradeWorkerSystemStatus = new List<bool>(CONFIG.FUTURE_TRADE_THREAD_NUM);
                    message.StockTradeWorkerSystemStatus = new List<bool>(CONFIG.STOCK_TRADE_THREAD_NUM);

                    for (int i = 0; i < CONFIG.FUTURE_TRADE_THREAD_NUM; i++)
                    {
                        message.FutureTradeWorkerSystemStatus.Add(false);
                    }

                    for (int i = 0; i < CONFIG.STOCK_TRADE_THREAD_NUM; i++)
                    {
                        message.StockTradeWorkerSystemStatus.Add(false);
                    }

                    if ((current - messageData.MarketSystemStatus).TotalSeconds > 5) { message.MarketSystemStatus = false; } else { message.MarketSystemStatus = true; }
                    if ((current - messageData.HeartBeatSystemStatus).TotalSeconds > 5) { message.HeartBeatSystemStatus = false; } else { message.HeartBeatSystemStatus = true; }
                    if ((current - messageData.StrategyManagementSystemStatus).TotalSeconds > 5) { message.StrategyManagementSystemStatus = false; } else { message.StrategyManagementSystemStatus = true; }
                    
                    foreach(KeyValuePair<string,DateTime> value in messageData.StrategyWorkerSystemStatus)
                    {
                        if ((current - value.Value).TotalSeconds > 5) { message.StrategyWorkerSystemStatus.Add(value.Key.Substring(0, value.Key.Length - 1), 0); } else { message.StrategyWorkerSystemStatus.Add(value.Key.Substring(0, value.Key.Length - 1), Convert.ToInt16(value.Key.Substring(value.Key.Length - 1, 1))); }
                    }

                    if ((current - messageData.Pre_TradeControlSystemStatus).TotalSeconds > 5) { message.Pre_TradeControlSystemStatus = false; } else { message.Pre_TradeControlSystemStatus = true; }

                    if ((current - messageData.FutureTradeManagementSystemStatus).TotalSeconds > 5) { message.FutureTradeManagementSystemStatus = false; } else { message.FutureTradeManagementSystemStatus = true; }

                    int count = 0;
                    foreach(DateTime time in messageData.FutureTradeWorkerSystemStatus)
                    {
                        if ((current - messageData.FutureTradeWorkerSystemStatus[count]).TotalSeconds > 5) { message.FutureTradeWorkerSystemStatus[count] = false; } else { message.FutureTradeWorkerSystemStatus[count] = true; }
                        count++;
                    }


                    if ((current - messageData.StockTradeManagementSystemStatus).TotalSeconds > 5) { message.StockTradeManagementSystemStatus = false; } else { message.StockTradeManagementSystemStatus = true; }

                    count = 0;
                    foreach (DateTime time in messageData.StockTradeWorkerSystemStatus)
                    {
                        if ((current - messageData.StockTradeWorkerSystemStatus[count]).TotalSeconds > 5) { message.StockTradeWorkerSystemStatus[count] = false; } else { message.StockTradeWorkerSystemStatus[count] = true; }
                        count++;
                    }


                    if ((current - messageData.StockEntrustManagementSystemStatus).TotalSeconds > 5) { message.StockEntrustManagementSystemStatus = false; } else { message.StockEntrustManagementSystemStatus = true; }

                    if ((current - messageData.RefundControlSystemStatus).TotalSeconds > 5) { message.RefundControlSystemStatus = false; } else { message.RefundControlSystemStatus = true; }

                    if ((current - messageData.RefundFutureSystemStatus).TotalSeconds > 5) { message.RefundFutureSystemStatus = false; } else { message.RefundFutureSystemStatus = true; }

                    if ((current - messageData.RefundStockSystemStatus).TotalSeconds > 5) { message.RefundStockSystemStatus = false; } else { message.RefundStockSystemStatus = true; }


                    MonitorSys.Instance.updateSysStatus(message);
                }
                

                if (queue_system_status.GetQueueNumber() == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }


                object obj = queue_system_status.GetQueue().Dequeue();

                if (obj == null) continue;

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
                    case "THREAD_ENTRUST_WORKER":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_Refund_Control_MONITOR":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_Future_Refund_Control_MONITOR":
                        arrive_thread_status(news.Key, news.Value);
                        break;
                    case "THREAD_Stock_Refund_Control_MONITOR":
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
        private static void arrive_market_frequence(object obj) {

            messageData.MarketFrequence = Convert.ToInt32(obj);
               
        }

        /// <summary>
        /// 行情分钟延迟消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_market_delay(object obj) {

            messageData.MarketDelay = Convert.ToDouble(obj) * 1.0;
        }

        /// <summary>
        /// 停盘行情消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_market_stock_stop(object obj) { }

        /// <summary>
        /// 策略数量消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_strategy_num(object obj) {
            messageData.StrategyNum = (int)obj;
        }

        /// <summary>
        /// 策略信息消息处理
        /// </summary>
        /// <param name="obj"></param>
        private static void arrive_strategy_info(object obj) {

            messageData.StrategyInfomation = (Dictionary<string, StrategyInfo>)(obj);
        }

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
                        try
                        {
                            messageData.StrategyWorkerSystemStatus.Clear();
                            Dictionary<string, int> workers = (Dictionary<string, int>)obj;
                            foreach (KeyValuePair<string, int> value in workers)
                            {
                                messageData.StrategyWorkerSystemStatus.Add(value.Key + value.Value.ToString(), DateTime.Now);
                            }
                        }
                        catch(Exception ex)
                        {
                            GlobalErrorLog.LogInstance.LogEvent("sysmonitor-arrive_thread_status-THREAD_STRATEGY_WORKER:" + ex.ToString());
                            DBAccessLayer.LogSysInfo("sysmonitor", ex.ToString());
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
                        int No = (int)obj;
                        messageData.FutureTradeWorkerSystemStatus[No] = DateTime.Now;
                    }
                    break;
                case "THREAD_STOCK_TRADE_MONITOR":
                    messageData.StockTradeManagementSystemStatus = DateTime.Now;
                    break;
                case "THREAD_STOCK_TRADE_WORKER":
                    {
                        int No = (int)obj;
                        messageData.StockTradeWorkerSystemStatus[No] = DateTime.Now;
                    }
                    break;
                case "THREAD_ENTRUST_WORKER":
                    {
                        messageData.StockEntrustManagementSystemStatus = DateTime.Now;
                    }
                    break;
                case "THREAD_Stock_Refund_Control_MONITOR":
                    {
                        messageData.RefundStockSystemStatus = DateTime.Now;
                    }
                    break;
                case "THREAD_Future_Refund_Control_MONITOR":
                    {
                        messageData.RefundFutureSystemStatus = DateTime.Now;
                    }
                    break;
                case "THREAD_Refund_Control_MONITOR":
                    {
                        messageData.RefundControlSystemStatus = DateTime.Now;
                    }
                    break;
                default: break;
            }
        }
        #endregion
    }
}