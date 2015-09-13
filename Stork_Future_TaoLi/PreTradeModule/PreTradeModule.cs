using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using Stork_Future_TaoLi.Modulars;
using System.Threading;
using Stork_Future_TaoLi;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 模块名称：   交易预处理模块 
    /// 模块函数：
    ///             getInstance()          返回模块实例，并保持在整个应用程序执行期间，不会同时运行两个交易预处理模块
    ///             DeQueue()               从消息队列中获取数据
    /// </summary>
    public class PreTradeModule
    {
        private static PreTradeModule instance;
        private Thread excuteThread  = new Thread(new ThreadStart(ThreadProc));
        private static LogWirter log = new LogWirter();

        private static DateTime isRunning = new DateTime(1900, 01, 01);
        /// <summary>
        /// 判断预处理交易线程当前是否正常运行
        /// 该值代表了最后一次正常运行的时间
        /// </summary>
        public DateTime ISRUNNING
        {
            get { return isRunning; }
        }

        /// <summary>
        /// 单例模式下获取模块实例
        /// </summary>
        /// <returns>模块单例实例</returns>
        public static PreTradeModule getInstance()
        {
            if (instance == null)
            {
                instance = new PreTradeModule();
            }
           
            return instance;
        }

        /// <summary>
        /// 构造函数，私有
        /// </summary>
        private PreTradeModule()
        {
            log.EventSourceName = "交易预处理模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62301;
        }

        /// <summary>
        /// 从消息队列中策略实例生成交易列表
        /// </summary>
        /// <returns>
        /// NULL : 说明队列中无值
        /// 其他 ：返回队列中首值
        /// </returns>
        private List<TradeOrderStruct> DeQueue()
        {
            List<TradeOrderStruct> tos ;
            lock (queue_prd_trade.GetQueue().SyncRoot)
            {
                if (queue_prd_trade.GetQueue().Count == 0)
                    return null;
                 tos = (List<TradeOrderStruct>)queue_prd_trade.GetQueue().Dequeue();
            }
          
            if (tos != null) return tos;
            else return null;
        }

        /// <summary>
        /// 从消息队列中获取交易管理页面生成的交易
        /// </summary>
        /// <returns>
        /// NULL : 说明队列中无值
        /// 其他 ：返回队列中首值</returns>
        private MakeFutureOrder DeQueue2()
        {
            MakeFutureOrder tos;

            lock(queue_prd_trade_from_tradeMonitor_future.GetQueue().SyncRoot)
            {
                if (queue_prd_trade_from_tradeMonitor_future.GetQueue().Count == 0) return null;
                tos = (MakeFutureOrder)queue_prd_trade_from_tradeMonitor_future.GetQueue().Dequeue();
            }

            if (tos != null) return tos;
            else return null;
        }



        private static TradeOrderStruct CreateNewTrade(TradeOrderStruct tos)
        {
            TradeOrderStruct t = new TradeOrderStruct();
            t.cExhcnageID = tos.cExhcnageID;
            t.cOffsetFlag = tos.cOffsetFlag;
            t.cOrderexecutedetail = tos.cOrderexecutedetail;
            t.cOrderLevel = tos.cOrderLevel;
            t.cOrderPriceType = tos.cOrderPriceType;
            t.cSecurityCode = tos.cSecurityCode;
            t.cSecurityType = tos.cSecurityType;
            t.cTradeDirection = tos.cTradeDirection;
            t.dOrderPrice = tos.dOrderPrice;
            t.nSecurityAmount = tos.nSecurityAmount;
            t.SecurityName = tos.SecurityName;
            t.belongStrategy = tos.belongStrategy;
            t.OrderRef = tos.OrderRef;
            return t;
        }

        private static List<TradeOrderStruct> CreateList(List<TradeOrderStruct> _li)
        {
            List<TradeOrderStruct> _lii = new List<TradeOrderStruct>();
            for (int i = 0; i < _li.Count; i++)
            {
                _lii.Add(CreateNewTrade(_li[i]));
            }

            return _lii;
        }

        /// <summary>
        /// 启动预处理线程
        /// </summary>
        /// <returns>如果成功返回true,否则返回false</returns>
        public bool Run()
        {
            excuteThread.Start();
            Thread.Sleep(1000);

            return true;
        }

        /// <summary>
        /// 预处理线程启动
        /// </summary>
        private static void ThreadProc()
        {
            log.LogEvent("交易预处理线程开始执行");

            DateTime lastHeartBeat = DateTime.Now;

            while (true)
            {
                Thread.Sleep(10);

                /*****************************
                 * 生成交易List之前的例行工作
                 * **************************/

                

                //发送心跳
                if (DateTime.Now.Second % 5 == 0 && DateTime.Now.Second != lastHeartBeat.Second)
                {

                    List<TradeOrderStruct> o = new List<TradeOrderStruct>();
                    QUEUE_SH_TRADE.GetQueue().Enqueue((object)o);
                    lastHeartBeat = DateTime.Now;
                }


                #region 策略生成交易队列
                List<TradeOrderStruct> tos = PreTradeModule.instance.DeQueue();
                if (tos != null)
                {


                    log.LogEvent("来自策略的交易数：" + tos.Count.ToString());

                    //获取到新的list
                    List<TradeOrderStruct> stocks_sh = (from item in tos where item.cExhcnageID == ExchangeID.SH select item).OrderBy(i => i.cOrderLevel).ToList();

                    List<TradeOrderStruct> stocks_sz = (from item in tos where item.cExhcnageID == ExchangeID.SZ select item).OrderBy(i => i.cOrderLevel).ToList();

                    List<TradeOrderStruct> future = (from item in tos select item).OrderBy(i => i.cOrderLevel).ToList();

                    //将新的list推送到对应的线程控制器
                    #region 交易送入队列

                    List<TradeOrderStruct> unit = new List<TradeOrderStruct>();

                    #region SH股票交易送入队列
                    if (stocks_sh.Count > 0)
                    {
                        log.LogEvent("上海交易所入队交易数量：" + stocks_sh.Count.ToString());

                        foreach (TradeOrderStruct stu in stocks_sh)
                        {

                            TradeOrderStruct _tos = CreateNewTrade(stu);
                            unit.Add(_tos);

                            if (unit.Count == 15)
                            {
                                List<TradeOrderStruct> _li = CreateList(unit);
                                unit.Clear();

                                lock (QUEUE_SH_TRADE.GetQueue().SyncRoot)
                                {

                                    QUEUE_SH_TRADE.GetQueue().Enqueue((object)_li);
                                }

                            }
                        }

                        if (unit.Count != 0)
                        {
                            List<TradeOrderStruct> _li = CreateList(unit);
                            unit.Clear();

                            lock (QUEUE_SH_TRADE.GetQueue().SyncRoot)
                            {
                                QUEUE_SH_TRADE.GetQueue().Enqueue((object)_li);
                            }

                        }

                    }
                    #endregion

                    #region SZ股票交易送入队列
                    if (stocks_sz.Count > 0)
                    {
                        log.LogEvent("深圳交易所入队交易数量：" + stocks_sz.Count.ToString());
                        foreach (TradeOrderStruct stu in stocks_sz)
                        {

                            TradeOrderStruct _tos = CreateNewTrade(stu);
                            unit.Add(_tos);

                            if (unit.Count == 15)
                            {
                                List<TradeOrderStruct> _li = CreateList(unit);
                                unit.Clear();

                                lock (QUEUE_SZ_TRADE.GetQueue().SyncRoot)
                                {
                                    QUEUE_SZ_TRADE.GetQueue().Enqueue((object)_li);
                                }

                                unit.Clear();
                            }
                        }

                        if (unit.Count != 0)
                        {
                            List<TradeOrderStruct> _li = CreateList(unit);
                            unit.Clear();

                            lock (QUEUE_SZ_TRADE.GetQueue().SyncRoot)
                            {
                                QUEUE_SZ_TRADE.GetQueue().Enqueue((object)_li);
                            }

                            //unit.Clear();
                        }

                    }
                    #endregion

                    #region 期货交易送入队列
                    if (future.Count > 0)
                    {
                        log.LogEvent("期货交易入队交易数量：" + future.Count.ToString());
                        foreach (TradeOrderStruct stu in future)
                        {
                            TradeOrderStruct _tos = stu;
                            unit.Add(_tos);

                            if (unit.Count == 15)
                            {
                                lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                                {
                                    QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)unit);
                                }

                                unit.Clear();
                            }
                        }

                        if (unit.Count != 0)
                        {
                            lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                            {
                                QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)unit);
                            }

                            unit.Clear();
                        }

                    }
                    #endregion

                    #endregion


                }

                #endregion

                #region 交易管理界面直接发起交易
                MakeFutureOrder mo = PreTradeModule.instance.DeQueue2();
                if (mo != null)
                {
                    List<TradeOrderStruct> _TradeList = new List<TradeOrderStruct>();
                    TradeOrderStruct _tradeUnit = new TradeOrderStruct()
                    {
                        cExhcnageID = mo.exchangeId,
                        cSecurityCode = mo.cSecurityCode,
                        nSecurityAmount = mo.nSecurityAmount,
                        dOrderPrice = mo.dOrderPrice,
                        cTradeDirection = mo.cTradeDirection,
                        cOffsetFlag = mo.cOffsetFlag,

                        cSecurityType = mo.cSecurityType,
                        cOrderLevel = "1",
                        cOrderexecutedetail = String.Empty,
                        belongStrategy = mo.belongStrategy,
                        OrderRef = REQUEST_ID.ApplyNewID()
                    };

                    UserRequestMap.GetInstance().AddOrUpdate(_tradeUnit.OrderRef, mo.User,(key,oldValue) => oldValue = mo.User);

                    _TradeList.Add(_tradeUnit);

                    log.LogEvent("来自交易管理页面的交易");
                    if (mo.cSecurityType == "s" || mo.cSecurityType == "S")
                    {
                        if (mo.exchangeId == ExchangeID.SH)
                        {
                            lock (QUEUE_SH_TRADE.GetQueue().SyncRoot)
                            {
                                QUEUE_SH_TRADE.GetQueue().Enqueue((object)_TradeList);
                            }
                        }
                        else if (mo.exchangeId == ExchangeID.SZ)
                        {
                            lock (QUEUE_SZ_TRADE.GetQueue().SyncRoot)
                            {
                                QUEUE_SZ_TRADE.GetQueue().Enqueue((object)_TradeList);
                            }
                        }

                    }
                    else if (mo.cSecurityType == "f" || mo.cSecurityType == "F")
                    {
                        lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                        {
                            QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)_TradeList);
                        }
                    }
                }
                
                #endregion

                if (DateTime.Now.Second != PreTradeModule.isRunning.Second)
                {
                    PreTradeModule.isRunning = DateTime.Now;
                }

            }

            Thread.CurrentThread.Abort();
        }
        
    }
}