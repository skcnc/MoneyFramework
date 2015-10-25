using MCStockLib;
using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;
using Stork_Future_TaoLi.Hubs;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.Entrust
{
    public class Entrust_Query
    {

        #region 变量
        private static LogWirter log = new LogWirter();
        MCStockLib.managedStockClass _classTradeStock = new managedStockClass();
        MCStockLib.managedLogin login = new managedLogin(CommConfig.Stock_ServerAddr, CommConfig.Stock_Port, CommConfig.Stock_Account, CommConfig.Stock_BrokerID, CommConfig.Stock_Password, CommConfig.Stock_InvestorID);
        string ErrorMsg = string.Empty;
        #endregion

        #region 单例模式
        private static readonly Entrust_Query _instance = new Entrust_Query();
        public static Entrust_Query Instance
        {
            get
            {
                return _instance;
            }
        }
        private Entrust_Query()
        {
            log.EventSourceName = "交易预处理模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 65001;
        }
        #endregion

        #region 启动线程
        public void Run()
        {
            Thread mythread = new Thread(new ThreadStart(threadproc));
            mythread.Start();
        }
        #endregion

        #region 线程执行函数
        private void threadproc()
        {
            while (true)
            {

                Thread.Sleep(1);

                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 5)
                {
                    log.LogEvent("由于供血不足，委托查询线程即将退出。");
                    break;
                }

                //单次循环最多查询100个交易的委托情况
                int maxCount = 100;

                if (!_classTradeStock.getConnectStatus())
                {
                    _classTradeStock.Init(login, ErrorMsg);
                }


                while (maxCount > 0 && queue_query_entrust.GetQueueNumber() > 0)
                {
                    maxCount--;

                    //获取新委托
                    QueryEntrustOrderStruct_M item = (QueryEntrustOrderStruct_M)queue_query_entrust.GetQueue().Dequeue();


                    string err = string.Empty;

                    //查询委托及获取实例
                    var temps = _classTradeStock.QueryEntrust(item, err);

                    if(temps.Length == 0) continue;

                    managedEntrustreturnstruct rets = temps.ToList()[0];

                    String USERNAME = UserRequestMap.GetInstance()[item.OrderRef];
                    OrderViewItem order = new OrderViewItem(
                        item.OrderRef.ToString(), 
                        rets.cOrderSysID, 
                        rets.cSecurity_code, 
                        item.Direction.ToString(), 
                        "NA", 
                        rets.nVolumeTotalOriginal.ToString(), 
                        rets.nVolumeTotal.ToString(), 
                        item.OrderPrice.ToString(),
                        rets.cOrderStatus.ToString(), 
                        rets.cInsertTime);

                    String JSONString = JsonConvert.SerializeObject(order);
                    TradeMonitor.Instance.updateOrderList(USERNAME, JSONString);

                    //目前仅考虑 1对1 返回的情况，不考虑出现1对多 ，类似基金交易的情况
                    //将委托变动返回更新数据库
                    if (DBAccessLayer.DBEnable == true)
                    {
                        //更新数据，记录入数据库
                        ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateERRecord), (object)(rets));

                        //此处判断，相应代码的委托是否完成
                        //此处逻辑需要待返回报文内容确认后修改
                        if ((rets.cOrderStatus.ToString() != "8")||(!(rets.cOrderStatus == '7' && rets.nVolumeTotal == 0)))
                        {
                            queue_query_entrust.GetQueue().Enqueue((object)item);
                            continue;
                        }


                        //委托已经完成，进入成交状态查询
                        var retbargin = _classTradeStock.QueryTrader(item, err).ToList();

                        //将查询信息记录成交表
                        if (retbargin.Count > 0)
                        {
                            managedBargainreturnstruct bargin = retbargin.ToList()[0];
                            bargin.strategyId = item.StrategyId;
                            bargin.direction = item.Direction;
                            DBAccessLayer.CreateDLRecord((object)bargin);

                            //更新持仓列表
                            ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateCCRecords), (object)bargin);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
