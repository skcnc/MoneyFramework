using Stork_Future_TaoLi.TradeModule;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Stork_Future_TaoLi.Modulars;
using System.Web;
using Stork_Future_TaoLi.Queues;
using CTP_CLI;

namespace Stork_Future_TaoLi
{
    public class FutureTrade
    {
        private LogWirter sublog = new LogWirter();//子线程日志记录
        private CTP_CLI.CCTPClient _client;
        private FutureTradeThreadStatus status = FutureTradeThreadStatus.DISCONNECTED;

        private string BROKER = "2011";
        private string INVESTOR = "1000025";
        private string PASSWORD = "1";
        private string ADDRESS = "ctp24-front1.financial-trading-platform.com:41205"; //"tcp://222.240.130.22:41205";

        #region 委托参数
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID ;
        /// <summary>
        /// 业务单元
        /// </summary>
        public string BusinessUnit;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public byte CombHedgeFlag_0;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public byte CombHedgeFlag_1;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public byte CombHedgeFlag_2;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public byte CombHedgeFlag_3;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public byte CombHedgeFlag_4;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public byte CombOffsetFlag_0;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public byte CombOffsetFlag_1;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public byte CombOffsetFlag_2;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public byte CombOffsetFlag_3;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public byte CombOffsetFlag_4;
        /// <summary>
        /// 触发条件
        /// </summary>
        public byte ContingentCondition;
        /// <summary>
        /// 买卖方向
        /// </summary>
        public byte Direction;
        /// <summary>
        /// 强平原因
        /// </summary>
        public byte ForceCloseReason;
        /// <summary>
        /// GTD日期
        /// </summary>
        public string GTDDate;
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID ;
        /// <summary>
        /// 自动挂起标志
        /// </summary>
        public int IsAutoSuspend;
        ///互换单标志
        public int IsSwapOrder;
        /// <summary>
        /// 价格
        /// </summary>
        public double LimitPrice;
        /// <summary>
        /// 最小成交量
        /// </summary>
        public int MinVolume;
        /// <summary>
        /// 报单价格条件
        /// </summary>
        public byte OrderPriceType;
        /// <summary>
        /// 报单引用
        /// </summary>
        public string OrderRef;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 止损价
        /// </summary>
        public double StopPrice;
        /// <summary>
        /// 有效期类型
        /// </summary>
        public byte TimeCondition;
        /// <summary>
        /// 用户强评标志
        /// </summary>
        public int UserForceClose;
        /// <summary>
        /// 用户代码
        /// </summary>
        public string UserID;
        /// <summary>
        /// 成交量类型
        /// </summary>
        public byte VolumeCondition;
        /// <summary>
        /// 数量
        /// </summary>
        public int VolumeTotalOriginal;

        #endregion



        /// <summary>
        /// 期货交易工作线程
        /// </summary>
        /// <param name="para"></param>
        public void FutureTradeSubThreadProc(object para)
        {
            string ErrorMsg = string.Empty;

            DateTime lastHeartBeat = DateTime.Now; //最近心跳时间

            //令该线程为前台线程
            Thread.CurrentThread.IsBackground = true;

            TradeParaPackage _tpp = (TradeParaPackage)para;

            //当前线程编号
            int _threadNo = _tpp._threadNo;

            sublog.LogEvent("线程 ：" + _threadNo.ToString() + " 开始执行");

            //用作发送心跳包的时间标记
            DateTime _markedTime = DateTime.Now;

            //控制期货交易线程执行
            bool _threadRunControl = true;

            while (_threadRunControl)
            {
                //初始化完成前，不接收实际交易
                queue_future_excuteThread.SetThreadBusy(_threadNo);

                _client.Connect();

                //状态 DISCONNECTED -> CONNECTED
                while (this.status != FutureTradeThreadStatus.CONNECTED)
                {
                    Thread.Sleep(10);
                }

                _client.ReqUserLogin();

                //状态 CONNECTED -> LOGIN
                while (this.status != FutureTradeThreadStatus.LOGIN)
                {
                    Thread.Sleep(10);
                }

                while (true)
                {
                    Thread.Sleep(10);

                    if ((DateTime.Now - lastHeartBeat).TotalSeconds > 30)
                    {
                        sublog.LogEvent("线程 ：" + _threadNo.ToString() + "心跳停止 ， 最后心跳 ： " + lastHeartBeat.ToString());
                        _threadRunControl = false;
                        break;
                    }

                    //TODO: 判断连接是否存在，登陆是否存在，按照设定发送心跳
                    //if((DateTime.Now - lastHeartBeat).TotalSeconds > 5)
                    //{ SendHeartBeat(); 发送心跳
                    //  if(判断链接是否正常)
                    // 正常 lastHeartBeat = DateTime.Now
                    // 非正常 FutureTradeStatus.DISCONNECTED  _threadRunControl = true ; break;



                    if (queue_future_excuteThread.GetQueue(_threadNo).Count < 2)
                    {
                        queue_future_excuteThread.SetThreadFree(_threadNo);
                        status = FutureTradeThreadStatus.FREE;
                    }
                    else
                    {
                        status = FutureTradeThreadStatus.BUSY;
                    }

                    if (queue_future_excuteThread.GetQueue(_threadNo).Count > 0)
                    {
                        List<TradeOrderStruct> trades = (List<TradeOrderStruct>)queue_future_excuteThread.FutureExcuteQueue[_threadNo].Dequeue();

                        if (trades.Count > 0) { sublog.LogEvent("线程 ：" + _threadNo.ToString() + " 执行交易数量 ： " + trades.Count); }

                        lastHeartBeat = DateTime.Now;

                        if (trades.Count == 0) { continue; }


                        CTP_CLI.CThostFtdcInputOrderField_M args = new CThostFtdcInputOrderField_M();
                        //填写委托参数

                        _client.OrderInsert(args);

                        //

                        //填写委托查询参数
                        // 0，代表成功。
                        //-1，表示网络连接失败；
                        //-2，表示未处理请求超过许可数；
                        //-3，表示每秒发送请求数超过许可数。

                        _client.QryOrder();

                        }

                        //此时交易处于orderdone 状态
                        _client.QryTrade();


                        //交易成交查询成功，数据记录入库，一次交易流程完成
                    }
                }
            }


        /// <summary>
        /// 设置日志
        /// </summary>
        /// <param name="log"></param>
        public void SetLog(LogWirter log) { this.sublog = log; }


        public FutureTrade()
        {
            _client = new CTP_CLI.CCTPClient(INVESTOR, PASSWORD, BROKER, ADDRESS);

            //CTP后台成功建立连接的回调函数
            _client.FrontConnected += _client_FrontConnected;
            //CTP后台连接丢失回调函数
            _client.FrontDisconnected += _client_FrontDisconnected;
            //登陆成功回调函数
            _client.RspUserLogin += _client_RspUserLogin;
            //
            _client.RspError += _client_RspError;
            //报单变化回调函数
            _client.RtnOrder += _client_RtnOrder;
            //成交变化回调函数
            _client.RtnTrade += _client_RtnTrade;
            //报单修改操作回调函数（暂时不用）
            _client.RspOrderAction += _client_RspOrderAction;
            //报单失败回调函数
            _client.RspOrderInsert += _client_RspOrderInsert;
            //报单查询回调函数
            _client.RspQryOrder += _client_RspQryOrder;
            //成交查询回调函数
            _client.RspQryTrade += _client_RspQryTrade;
          
        }

        /// <summary>
        /// 成交单查询应答。当客户端发出成交单查询指令后，交易托管系统返回响应时，该方法会被调用。
        /// </summary>
        /// <param name="pTrade">成交信息</param>
        /// <param name="pRspInfo">响应信息</param>
        /// <param name="nRequestID">返回用户成交单请求的ID，该ID 由用户在成交单查询时指定</param>
        /// <param name="bIsLast">指示该次返回是否为针对nRequestID的最后一次返回。</param>
        void _client_RspQryTrade(CThostFtdcTradeField_M pTrade, CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            if(pRspInfo.ErrorID == 0 && bIsLast == true)
            {
               

                //记录成交入数据库
            }
        }

        /// <summary>
        /// 报单查询请求。当客户端发出报单查询指令后，交易托管系统返回响应时，该方法会被调用。
        /// </summary>
        /// <param name="pOrder">报单信息结构</param>
        /// <param name="pRspInfo">响应信息</param>
        /// <param name="nRequestID">返回用户报单查询请求的ID，该ID由用户在报单查询时指定。</param>
        /// <param name="bIsLast">指示该次返回是否为针对nRequestID的最后一次返回。</param>
        void _client_RspQryOrder(CThostFtdcOrderField_M pOrder, CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {

        }

        /// <summary>
        /// 连接成功
        /// </summary>
        void _client_FrontConnected()
        {
            this.status = FutureTradeThreadStatus.CONNECTED;
        }

        /// <summary>
        /// 链接失败，记入日志
        /// </summary>
        /// <param name="nReason"></param>
        void _client_FrontDisconnected(int nReason)
        {
            sublog.LogEvent("期货交易所链接失败，失败码：" + nReason.ToString());
        }

        /// <summary>
        /// 交易提交出现问题回掉函数
        /// </summary>
        /// <param name="pInputOrder"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        void _client_RspOrderInsert(CTP_CLI.CThostFtdcInputOrderField_M pInputOrder, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
           
        }

        /// <summary>
        /// 提交报单请求响应处理函数， 当报单内容有问题时，通过该函数响应
        /// </summary>
        /// <param name="pInputOrderAction">报单请求内容</param>
        /// <param name="pRspInfo">返回信息</param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        void _client_RspOrderAction(CTP_CLI.CThostFtdcInputOrderActionField_M pInputOrderAction, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 成交回报（私有回报）
        /// </summary>
        /// <param name="pTrade"></param>
        void _client_RtnTrade(CTP_CLI.CThostFtdcTradeField_M pTrade)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 报单回报（私有回报）
        /// </summary>
        /// <param name="pOrder"></param>
        static void _client_RtnOrder(CTP_CLI.CThostFtdcOrderField_M pOrder)
        {
            //报单内容发生变更时推送给客户端，如部分成交，撤单等情况
            int tradeCompleted = pOrder.VolumeTraded;
            int tradeRemain = pOrder.VolumeTotal;
            int requestID = pOrder.RequestID;
            String localRequestID = pOrder.OrderLocalID;


            //TradeRecord.GetInstance().UpdateOrder(tradeCompleted,)

        }


        
        /// <summary>
        /// 登陆成功回掉函数
        /// </summary>
        /// <param name="pRspUserLogin">用户登录信息结构</param>
        /// <param name="pRspInfo">返回用户响应信息</param>
        /// <param name="nRequestID">返回用户登录请求的ID，该ID 由用户在登录时指定。</param>
        /// <param name="bIsLast">指示该次返回是否为针对nRequestID的最后一次返回。</param>
        void _client_RspUserLogin(CTP_CLI.CThostFtdcRspUserLoginField_M pRspUserLogin, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo.ErrorID == 0 && bIsLast == true)
            {
                status = FutureTradeThreadStatus.LOGIN;
            }
            //throw new NotImplementedException();
        }

        void _client_RspError(CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            throw new NotImplementedException();
        }
    }

    public class TradeRecord : ConcurrentDictionary<String,RecordItem>
    {
        private static readonly TradeRecord Instance = new TradeRecord();
        public static TradeRecord GetInstance()
        {
            return Instance;
        }

        public void CreateOrder(String type, String code,String orientation,int amount,decimal price,String StrID)
        {
            RecordItem _record = new RecordItem()
            {
                StrategyId = StrID,
                LocalRequestID = StrID + code,
                OrderTime_Start = DateTime.Now,
                Type = type,
                Code = code,
                Orientation = orientation,
                Amount = amount,
                Price = price,
                ParialDealAmount = 0,
                QuitAmount = 0,
                Status = TradeDealStatus.PREORDER
            };

            if (this.Keys.Contains(_record.LocalRequestID))
            {
                //已经存在Key，采用新的记录
                RecordItem _oldRecord = new RecordItem();
                this.TryRemove(_record.LocalRequestID, out _oldRecord);
            }

            this.TryAdd(_record.LocalRequestID, _record);

        }
        
        public void UpdateOrder(int partialAmount,int quitAmount,string key)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);
            _record.ParialDealAmount = partialAmount;
            _record.QuitAmount = quitAmount;
            _record.Status = TradeDealStatus.ORDERING;

            this.TryAdd(_record.LocalRequestID, _record);
        }

        public void MarkFailure(String key, String Err)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);
            _record.ErrMsg = Err;
            _record.Status = TradeDealStatus.ORDERFAILURE;
        }

        public void CompleteOrder(String key,decimal dealPrice,int partialAmount,int quitAmount)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);

            _record.DealPrice = dealPrice;
            _record.ParialDealAmount = partialAmount;
            _record.QuitAmount = quitAmount;
            _record.Status = TradeDealStatus.ORDERCOMPLETED;
        }
    }

    public class RecordItem
    {
        /// <summary>
        /// 策略号
        /// </summary>
        public String StrategyId { get; set; }

        /// <summary>
        ///KEY 策略ID号+CODE 
        /// </summary>
        public String LocalRequestID { get; set; }

        /// <summary>
        /// 交易开始时间
        /// </summary>
        public DateTime OrderTime_Start { get; set; }

        /// <summary>
        /// 交易完成时间
        /// </summary>
        public DateTime OrderTime_Completed { get; set; }

        /// <summary>
        /// 交易类型 ： 0 股票 1： 期货
        /// </summary>
        public String Type { get; set; }

        /// <summary>
        /// 交易代码
        /// </summary>
        public String Code { get; set; }

        /// <summary>
        /// 交易方向 0：买入 1：卖出
        /// </summary>
        public String Orientation { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 设定价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public decimal DealPrice { get; set; }

        /// <summary>
        /// 部分成交量
        /// </summary>
        public int ParialDealAmount { get; set; }

        /// <summary>
        /// 撤销量
        /// </summary>
        public int QuitAmount { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public String ErrMsg { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public int RequestID { get; set; }


        /// <summary>
        /// 交易状态
        /// </summary>
        public TradeDealStatus Status { get; set; }
    }
}