using MCStockLib;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Stork_Future_TaoLi.TradeModule
{
    /// <summary>
       /// 退货数据结构
       /// </summary>
    public class RefundStruct
    {
        /// <summary>
        /// 证券类型
        /// </summary>
        public string SecurityType { get; set; }

        /// <summary>
        /// 交易所
        /// </summary>
        public string ExchangeId { get; set; }

        /// <summary>
        /// 报单系统编号
        /// </summary>
        public string OrderSysId { get; set; }

        /// <summary>
        /// 报单本地编号
        /// </summary>
        public string OrderRef { get; set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; set; }

        /// <summary>
        /// 交易方向
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// 开平标志：仅期货使用
        /// </summary>
        public string OffSetFlag { get; set; }
    }

    /// <summary>
    /// 退货模块，总共包含三个线程
    /// 1. 退货控制线程，筛选所有退货请求，并向股票和退货线程传递。
    /// 2. 股票退货线程
    /// 3. 期货退货行情
    /// </summary>
    public class RefundTrade
    {
        private static LogWirter log = new LogWirter();  //退货线程记录
        private static FutureTradeThreadStatus status = FutureTradeThreadStatus.DISCONNECTED;

        public static void Main()
        {
            //初始化消息队列
            queue_stock_excuteThread.Init();

            //创建线程任务 
            Task RefundTradeControl = new Task(ThreadProc);

            //启动主线程
            RefundTradeControl.Start();

            //日志初始化
            log.EventSourceName = "证券退货线程模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62315;



        }

        /// <summary>
        /// 股票总控线程函数
        /// </summary>
        private static void StockThreadProc() {

            log.LogEvent("股票退货线程启动！");

            MCStockLib.managedStockClass _classTradeStock = new managedStockClass();
            MCStockLib.managedLogin login = new managedLogin(CommConfig.Stock_ServerAddr, CommConfig.Stock_Port, CommConfig.Stock_Account, CommConfig.Stock_BrokerID, CommConfig.Stock_Password, CommConfig.Stock_InvestorID);

            //标记心跳包发送时间
            DateTime _markedTime = DateTime.Now;

            DateTime lastmessage = DateTime.Now;

            string ErrorMsg = string.Empty;

            //初始化通信
            //功能1
            _classTradeStock.Init(login, ErrorMsg);

            while (true)
            {
                Thread.Sleep(10);
                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 10)
                {
                    log.LogEvent("股票退货线程心跳停止 ， 最后心跳 ： " + GlobalHeartBeat.GetGlobalTime().ToString());
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Stock_Refund_Control_MONITOR", (object)false);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    break;
                }

                if (lastmessage.Second != DateTime.Now.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Stock_Refund_Control_MONITOR", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    lastmessage = DateTime.Now;
                }

                if (_markedTime.Minute != DateTime.Now.Minute)
                {
                    //发送心跳包
                    //功能2
                    //_stockTradeAPI.heartBeat();
                    _classTradeStock.HeartBeat();
                    _markedTime = DateTime.Now;
                }

                if(queue_stock_refund_thread.GetQueueNumber() > 0)
                {
                    RefundStruct refundItem = (RefundStruct)queue_stock_refund_thread.GetQueue().Dequeue();

                    QueryEntrustOrderStruct_M item = new QueryEntrustOrderStruct_M()
                    {
                        Code = refundItem.SecurityCode,
                        Direction = Convert.ToInt32(refundItem.Direction),
                        ExchangeID = (refundItem.ExchangeId == "SH" ? "1" : "2"),
                        OrderPrice = 0,                                             //撤单不用考虑价格
                        OrderRef = Convert.ToInt32(refundItem.OrderRef),
                        OrderSysID = refundItem.OrderSysId,
                        SecurityType = (sbyte)115,
                        StrategyId = string.Empty                                   //撤单不考虑策略编号
                    };

                    if (!_classTradeStock.getConnectStatus())
                    {
                        _classTradeStock.Init(login, ErrorMsg);
                    }

                    String err = String.Empty;

                    //发出撤单交易
                    _classTradeStock.CancelTrade(item, err);

                    log.LogEvent("股票撤单已发出，本地编号：" + refundItem.OrderRef + "  系统编号：" + refundItem.OrderSysId);

                }
            }
        }

        /// <summary>
        /// 期货总控线程函数
        /// </summary>
        private static void FutureThreadProc() {

            log.LogEvent("期货退货线程启动！");

            string ErrorMsg = string.Empty;

            DateTime lastmessage = DateTime.Now;

            CTP_CLI.CCTPClient _client = new CTP_CLI.CCTPClient(CommConfig.INVESTOR, CommConfig.PASSWORD, CommConfig.BROKER, CommConfig.ADDRESS);

            _client.FrontConnected += _client_FrontConnected;
            _client.FrontDisconnected += _client_FrontDisconnected;
            _client.RspUserLogin += _client_RspUserLogin;

            //报单变化回调函数
            _client.RtnOrder += FutureTrade._client_RtnOrder;
            //成交变化回调函数
            _client.RtnTrade += FutureTrade._client_RtnTrade;
            //报单修改操作回调函数（暂时不用）
            _client.RspOrderAction += FutureTrade._client_RspOrderAction;
            //报单失败回调函数
            _client.RspOrderInsert += FutureTrade._client_RspOrderInsert;
            //报单问题回调函数
            _client.ErrRtnOrderInsert += FutureTrade._client_ErrRtnOrderInsert;
            _client.Connect();

            //状态 DISCONNECTED -> CONNECTED
            while (status != FutureTradeThreadStatus.CONNECTED)
            {
                Thread.Sleep(10);
            }
                    
            _client.ReqUserLogin();

            //状态 CONNECTED -> LOGIN
            while (status != FutureTradeThreadStatus.LOGIN)
            {
                Thread.Sleep(10);
            }

            while(true)
            {

                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 10)
                {
                    log.LogEvent("期货退货线程心跳停止 ， 最后心跳 ： " + GlobalHeartBeat.GetGlobalTime().ToString());
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Future_Refund_Control_MONITOR", (object)false);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    break;
                }

                if (lastmessage.Second != DateTime.Now.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Future_Refund_Control_MONITOR", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    lastmessage = DateTime.Now;
                }


                Thread.Sleep(10);
                if (queue_future_refund_thread.GetQueueNumber() > 0)
                {
                    RefundStruct refundItem = (RefundStruct)queue_future_refund_thread.GetQueue().Dequeue();

                    RecordItem record = TradeRecord.GetInstance().getOrderInfo(Convert.ToInt32(refundItem.OrderRef));

                    refundItem.ExchangeId = record.ExchangeID;

                    CTP_CLI.CThostFtdcInputOrderActionField_M item = new CTP_CLI.CThostFtdcInputOrderActionField_M()
                    {
                        BrokerID = CommConfig.BROKER,
                        ExchangeID = refundItem.ExchangeId,
                        OrderSysID = refundItem.OrderSysId.PadLeft(12),
                        InvestorID = CommConfig.INVESTOR,
                        ActionFlag = Convert.ToByte('0'),        //删除标志 THOST_FTDC_AF_Delete 
                        InstrumentID = record.Code
                    };

                    _client.ReqOrderAction(item);
                }

                
            }
        }

        static void _client_RspUserLogin(CTP_CLI.CThostFtdcRspUserLoginField_M pRspUserLogin, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            //throw new NotImplementedException();
            status = FutureTradeThreadStatus.LOGIN;
        }

        static void _client_FrontDisconnected(int nReason)
        {
            //throw new NotImplementedException();
            status = FutureTradeThreadStatus.DISCONNECTED;
        }

        static void _client_FrontConnected()
        {
            //throw new NotImplementedException();
            status = FutureTradeThreadStatus.CONNECTED;
        }

        /// <summary>
        /// 退货总控线程函数
        /// </summary>
        private static void ThreadProc()
        {
            //初始化股票 和 期货退货线程

            DateTime lastmessage = DateTime.Now;

            log.LogEvent("退货控制线程启动");

            Task StockRefundThread = new Task(StockThreadProc);
            Task FutureRefundThread = new Task(FutureThreadProc);

            StockRefundThread.Start();
            FutureRefundThread.Start();

            while (true)
            {
                Thread.Sleep(10);
                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 10)
                {
                    log.LogEvent("本模块供血不足，线程即将死亡");
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Refund_Control_MONITOR", (object)false);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    break;
                }

                if (lastmessage.Second != DateTime.Now.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_Refund_Control_MONITOR", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    lastmessage = DateTime.Now;
                }

                //获取下一笔交易
                RefundStruct next_trade = new RefundStruct();
                if (queue_refund_thread.GetQueueNumber() > 0)
                {
                    lock (queue_refund_thread.GetQueue().SyncRoot)
                    {
                        if (queue_refund_thread.GetQueue().Count > 0)
                        {
                            next_trade = (RefundStruct)queue_refund_thread.GetQueue().Dequeue();
                        }

                        if(next_trade != null && next_trade.OrderRef != string.Empty && next_trade.OrderSysId != string.Empty)
                        {
                            log.LogEvent("退货控制线程收到退货请求，交易编号：" + next_trade.OrderRef);
                        }
                    }
                }


                if (next_trade.OrderRef == null || next_trade.OrderRef == String.Empty || next_trade.OrderSysId == String.Empty)
                {
                    continue;
                }

                //此时内存中包含了即将被进行的交易

                next_trade.Direction = next_trade.Direction.Trim();
                next_trade.ExchangeId = next_trade.ExchangeId.Trim();
                next_trade.OffSetFlag = next_trade.OffSetFlag.Trim();
                next_trade.OrderRef = next_trade.OrderRef.Trim();
                next_trade.OrderSysId = next_trade.OrderSysId.Trim();
                next_trade.SecurityCode = next_trade.SecurityCode.Trim();
                next_trade.SecurityType = next_trade.SecurityType.Trim();

                if (next_trade.SecurityType.ToUpper() == "S")
                {
                    //股票退货
                    queue_stock_refund_thread.GetQueue().Enqueue(next_trade);
                }
                else if(next_trade.SecurityType.ToUpper() == "F")
                {
                    //期货退货
                    queue_future_refund_thread.GetQueue().Enqueue(next_trade);
                }
            }

            Thread.CurrentThread.Abort();
        }

    }
}