using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using Stork_Future_TaoLi.Modulars;
using System.Threading;
using Stork_Future_TaoLi;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Hubs;
using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;

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
        /// 从消息队列中获取交易管理页面生成的交易(期货 & 股票)
        /// </summary>
        /// <returns>
        /// NULL : 说明队列中无值
        /// 其他 ：返回队列中首值</returns>
        private List<MakeOrder> DeQueueMonitorOrder()
        {
            List<MakeOrder> tos;

            lock(queue_prd_trade_from_tradeMonitor.GetQueue().SyncRoot)
            {
                if (queue_prd_trade_from_tradeMonitor.GetQueue().Count == 0) return null;
                tos = (List<MakeOrder>)queue_prd_trade_from_tradeMonitor.GetQueue().Dequeue();
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
            t.cUser = tos.cUser;
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


            while (true)
            {
                Thread.Sleep(10);

                /*****************************
                 * 生成交易List之前的例行工作
                 * **************************/

                #region 策略生成交易队列
                List<TradeOrderStruct> tos = PreTradeModule.instance.DeQueue();
                

                if (tos != null)
                {

                    log.LogEvent("来自策略的交易数：" + tos.Count.ToString());

                    if(tos.Count == 0)
                    {
                        continue;
                    }

                    string user = tos[0].cUser;
                    string strategyid = tos[0].belongStrategy;

                    //风控检测
                    string result = string.Empty;
                    bool brisk = riskmonitor.RiskDetection(user, tos, out result);

                    //风控结果记入数据库
                    DBAccessLayer.AddRiskRecord(user, result, strategyid, "00", 0, 0, "0");

                    List<RISK_TABLE> risks = DBAccessLayer.GetRiskRecord(user);

                    int count = 0;

                    if (risks.Count > 0)
                    {
                        List<TMRiskInfo> riskinfos = new List<TMRiskInfo>();

                        foreach (RISK_TABLE risk in risks)
                        {
                            count++;
                            if (count > 10) break;
                            riskinfos.Add(new TMRiskInfo() { code = risk.code, hand = risk.amount.ToString(), price = risk.price.ToString(), orientation = risk.orientation, time = risk.time.ToString(), strategy = "00", user = risk.alias, errinfo = risk.err });
                        }


                        TradeMonitor.Instance.updateRiskList(user, JsonConvert.SerializeObject(riskinfos), JsonConvert.SerializeObject(riskmonitor.riskPara));

                    }



                    if (!brisk)
                    {
                        continue;
                    }

                    //获取到新的list
                    List<TradeOrderStruct> stocks_sh = (from item in tos where item.cExhcnageID == ExchangeID.SH select item).OrderBy(i => i.cOrderLevel).ToList();

                    List<TradeOrderStruct> stocks_sz = (from item in tos where item.cExhcnageID == ExchangeID.SZ select item).OrderBy(i => i.cOrderLevel).ToList();

                    List<TradeOrderStruct> future = (from item in tos where item.cExhcnageID == ExchangeID.CF select item).OrderBy(i => i.cOrderLevel).ToList();

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

                            List<TradeOrderStruct> _li = CreateList(unit);
                            unit.Clear();

                            if (_li.Count == 15)
                            {
                                lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                                {
                                    QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)_li);
                                }
                            }

                            if (_li.Count != 0)
                            {
                                lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                                {
                                    QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)_li);
                                }
                            }
                        }

                        

                    }
                    #endregion

                    #endregion


                }

                #endregion

                #region 交易管理界面直接发起交易
                List<MakeOrder> mos = PreTradeModule.instance.DeQueueMonitorOrder();


                if (mos != null)
                {
                    if (mos.Count == 0) continue;

                    List<TradeOrderStruct> _TradeList = new List<TradeOrderStruct>();
                    string User = String.Empty;
                    foreach (MakeOrder mo in mos)
                    {
                        User = mo.User;
                        TradeOrderStruct _tradeUnit = new TradeOrderStruct()
                        {
                            cExhcnageID = mo.exchangeId,
                            cSecurityCode = mo.cSecurityCode,
                            nSecurityAmount = mo.nSecurityAmount,
                            dOrderPrice = mo.dOrderPrice,
                            cTradeDirection = mo.cTradeDirection,
                            cOffsetFlag = mo.offsetflag,
                            SecurityName = String.Empty,
                            cOrderPriceType = "0",
                            cUser = mo.User,
                            cSecurityType = mo.cSecurityType,
                            cOrderLevel = "1",
                            cOrderexecutedetail = "0",
                            belongStrategy = mo.belongStrategy,
                            OrderRef = REQUEST_ID.ApplyNewID()
                        };

                        if (mo.cSecurityType.ToUpper() == "F")
                        {
                            _tradeUnit.cTradeDirection = ((_tradeUnit.cTradeDirection == "0") ? "48" : "49");
                            _tradeUnit.cOffsetFlag = (_tradeUnit.cOffsetFlag == "0" ? "48" : "49");
                        }
                        if (mo.cSecurityType.ToUpper() == "S")
                        {
                            _tradeUnit.cTradeDirection = ((_tradeUnit.cTradeDirection == "0") ? "1" : "2");
                        }

                        UserRequestMap.GetInstance().AddOrUpdate(_tradeUnit.OrderRef, mo.User, (key, oldValue) => oldValue = mo.User);

                        _TradeList.Add(_tradeUnit);
                    }

                    //风控检测
                    string result = string.Empty;
                    bool brisk = riskmonitor.RiskDetection(User, _TradeList, out result);

                    //风控结果记入数据库
                    if (_TradeList.Count == 1)
                    {
                        DBAccessLayer.AddRiskRecord(_TradeList[0].cUser, result, "00", _TradeList[0].cSecurityCode, Convert.ToInt32(_TradeList[0].nSecurityAmount), _TradeList[0].dOrderPrice, _TradeList[0].cTradeDirection);
                    }
                    else
                    {
                        foreach (TradeOrderStruct tradeUnit in _TradeList)
                        {
                            DBAccessLayer.AddRiskRecord(tradeUnit.cUser, result, "00", tradeUnit.cSecurityCode, Convert.ToInt32(tradeUnit.nSecurityAmount), tradeUnit.dOrderPrice, tradeUnit.cTradeDirection);
                        }
                    }

                    List<RISK_TABLE> risks = DBAccessLayer.GetRiskRecord(User);

                    int count = 0;

                    if (risks.Count > 0)
                    {
                        List<TMRiskInfo> riskinfos = new List<TMRiskInfo>();

                        foreach (RISK_TABLE risk in risks)
                        {
                            count++;
                            if (count > 10) break;
                            riskinfos.Add(new TMRiskInfo() { code = risk.code, hand = risk.amount.ToString(), price = risk.price.ToString(), orientation = risk.orientation, time = risk.time.ToString(), strategy = "00", user = risk.alias, errinfo = risk.err });
                        }


                        TradeMonitor.Instance.updateRiskList(User, JsonConvert.SerializeObject(riskinfos), JsonConvert.SerializeObject(riskmonitor.riskPara));

                    }



                    if (!brisk)
                    {
                        continue;
                    }

                    log.LogEvent("来自交易管理页面的交易");

                    List<TradeOrderStruct> shTradeList = new List<TradeOrderStruct>();
                    List<TradeOrderStruct> szTradeList = new List<TradeOrderStruct>();
                    List<TradeOrderStruct> futureTradeList = new List<TradeOrderStruct>();
                    foreach (TradeOrderStruct tradeUnit in _TradeList)
                    {
                        if (tradeUnit.cSecurityType.ToUpper() == "S")
                        {
                            if (tradeUnit.cExhcnageID.ToUpper() == ExchangeID.SH)
                            {
                                shTradeList.Add(tradeUnit);
                                continue;
                            }
                            else if (tradeUnit.cExhcnageID.ToUpper() == ExchangeID.SZ)
                            {
                                szTradeList.Add(tradeUnit);
                                continue;
                            }

                        }
                        else if (tradeUnit.cSecurityType.ToUpper() == "F")
                        {
                            futureTradeList.Add(tradeUnit);
                        }
                    }

                    if (shTradeList.Count > 0)
                    {
                        lock (QUEUE_SH_TRADE.GetQueue().SyncRoot)
                        {
                            QUEUE_SH_TRADE.GetQueue().Enqueue((object)shTradeList);
                        }
                    }
                    if (szTradeList.Count > 0)
                    {
                        lock (QUEUE_SZ_TRADE.GetQueue().SyncRoot)
                        {
                            QUEUE_SZ_TRADE.GetQueue().Enqueue((object)szTradeList);
                        }
                    }
                    if (futureTradeList.Count > 0)
                    {
                        lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                        {
                            QUEUE_FUTURE_TRADE.GetQueue().Enqueue((object)futureTradeList);
                        }
                    }
                }
                
                #endregion

                if (DateTime.Now.Second != PreTradeModule.isRunning.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("THREAD_PRE_TRADE", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);
                    PreTradeModule.isRunning = DateTime.Now;
                } 

            }
        }
        
    }
}