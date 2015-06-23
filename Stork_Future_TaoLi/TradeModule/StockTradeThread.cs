using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Stork_Future_TaoLi.Variables_Type;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Modulars;
using MCStockLib;
using Stork_Future_TaoLi.Test;

namespace Stork_Future_TaoLi.TradeModule
{
    public class StockTradeThread
    {
        private static LogWirter log = new LogWirter();
        private static LogWirter sublog = new LogWirter();

        public static void Main()
        {
            //股票交易主控线程名设定
            Thread.CurrentThread.Name = "StockTradeControl";

            //初始化消息队列
            queue_stock_excuteThread.Init();

            //创建线程任务 
            Task StockTradeControl = new Task(ThreadProc);

            //启动主线程
            StockTradeControl.Start();

            //日志初始化
            log.EventSourceName = "交易线程控制模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62302;

            sublog.EventSourceName = "交易线程模块";
            sublog.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            sublog.EventLogID = 62303;

        }

        private static void ThreadProc()
        {
            //初始化子线程
            int stockNum = CONFIG.STOCK_TRADE_THREAD_NUM;


            List<Task> TradeThreads = new List<Task>();

            log.LogEvent("交易控制子线程启动： 初始化交易线程数 :" + stockNum.ToString());
            for (int i = 0; i < stockNum; i++)
            {
                TradeParaPackage tpp = new TradeParaPackage();
                tpp._threadNo = (i);
                object para = (object)tpp;
                TradeThreads.Add(Task.Factory.StartNew(() => StockTradeSubThreadProc(para)));
            }

            //此时按照配置，共初始化CONFIG.STOCK_TRADE_THREAD_NUM 数量交易线程
            // 交易线程按照方法 StockTradeSubThreadProc 执行

            //Loop 完成对于子线程的监控
            //每一秒钟执行一次自检
            //自检包含任务：
            //  1. 对每个线程，判断当前交易执行时间，超过 CONFIG.STOCK_TRADE_OVERTIME 仍未收到返回将会按照 该参数备注执行
            //  2. 判断当前线程空闲状态，整理可用线程列表
            //  3. 若当前存在可用线程，同时消息队列（queue_prdTrade_SH_tradeMonitor，queue_prdTrade_SZ_tradeMonitor)
            //      也包含新的消息送达，则安排线程处理交易
            //  4. 记录每个交易线程目前处理的交易内容，并写入数据库
            while (true)
            {

                //获取下一笔交易
                List<TradeOrderStruct> next_trade = new List<TradeOrderStruct>();
                if (QUEUE_SH_TRADE.GetQueueNumber() > 0)
                {
                    lock (QUEUE_SH_TRADE.GetQueue().SyncRoot)
                    {
                        if (QUEUE_SH_TRADE.GetQueue().Count > 0)
                        {
                            next_trade = (List<TradeOrderStruct>)QUEUE_SH_TRADE.GetQueue().Dequeue();
                        }
                        log.LogEvent("上海交易所出队交易数量：" + next_trade.Count.ToString());
                    }
                }
                else if (QUEUE_SZ_TRADE.GetQueueNumber() > 0)
                {
                    lock (QUEUE_SZ_TRADE.GetQueue().SyncRoot)
                    {
                        if (QUEUE_SZ_TRADE.GetQueue().Count > 0)
                        {
                            next_trade = (List<TradeOrderStruct>)QUEUE_SZ_TRADE.GetQueue().Dequeue();
                        }

                        log.LogEvent("深圳交易所出队交易数量：" + next_trade.Count.ToString());
                    }
                }

                if (next_trade.Count == 0)
                {
                    continue;
                }

                //此时内存中包含了即将被进行的交易

                //判断空闲的线程
                //利用随机选择，保证线程的平均使用
                Random ran = new Random();
                bool _bSearch = true;
                int _tNo = 0;
                while (_bSearch)
                {
                    _tNo = ran.Next(0, stockNum);
                    if (queue_stock_excuteThread.GetThreadIsAvailiable(_tNo))
                    {
                        _bSearch = false;
                    }
                }
                log.LogEvent("安排线程 ： " + _tNo + " 执行交易 数量： " + next_trade.Count);
                //选择第 _tNo 个线程执行交易
                queue_stock_excuteThread.GetQueue(_tNo).Enqueue((object)next_trade);

                //************************************
                //将交易发送到相应执行线程后需要做的事情
                //
                //
                //
                //************************************

            }



        }

        /// <summary>
        /// 执行交易线程处理逻辑
        /// 包含功能：
        ///     1. 连接股票交易所（上海，深圳）
        ///     2. 心跳包发送/接收
        ///     3. 发送交易，等待响应
        /// </summary>
        private static void StockTradeSubThreadProc(object para)
        {

            /*********************************************
             * 测试数据
             * ******************************************/
            MCStockLib.managedStockClass _classTradeStock = new managedStockClass();
            MCStockLib.managedLogin login = new managedLogin(CommConfig.Stock_ServerAddr, CommConfig.Stock_Port, CommConfig.Stock_Account, CommConfig.Stock_BrokerID, CommConfig.Stock_Password, CommConfig.Stock_InvestorID);
            string ErrorMsg = string.Empty;


            TradeParaPackage _tpp = (TradeParaPackage)para;

            //当前线程编号
            int _threadNo = _tpp._threadNo;

            sublog.LogEvent("线程 ：" + _threadNo.ToString() + " 开始执行");
            //用作发送心跳包的时间标记
            DateTime _markedTime = DateTime.Now;

            //初始化通信
            //功能1
            _classTradeStock.Init(login, ErrorMsg);

            while (true)
            {

                if (_markedTime.Minute != DateTime.Now.Minute)
                {
                    //发送心跳包
                    //功能2
                    //_stockTradeAPI.heartBeat();
                    _classTradeStock.HeartBeat();
                }

                if (queue_stock_excuteThread.GetQueue(_threadNo).Count > 0)
                {
                    //标记线程当前状态为“忙碌”
                    queue_stock_excuteThread.SetThreadBusy(_threadNo);
                }
                else
                {
                    queue_stock_excuteThread.SetThreadFree(_threadNo);
                }

                if (queue_stock_excuteThread.GetQueue(_threadNo).Count > 0)
                {


                    List<TradeOrderStruct> trades = (List<TradeOrderStruct>)queue_stock_excuteThread.StockExcuteQueues[_threadNo].Dequeue();

                    sublog.LogEvent("线程 ：" + _threadNo.ToString() + " 执行交易数量 ： " + trades.Count);


                    if (!_classTradeStock.getConnectStatus())
                    {
                        _classTradeStock.Init(login, ErrorMsg);
                    }

                    //根据消息队列中发来的消息数量判断调用的接口
                    //当内容大于1 ，调用批量接口
                    //当内容等于1， 调用单笔接口
                    queue_stock_excuteThread.SetUpdateTime(_threadNo);


                    if (trades.Count > 1)
                    {
                        //trades
                        Random seed = new Random();
                        if (CONFIG.IsDebugging())
                        {
                            sublog.LogEvent("线程 ：" + _threadNo.ToString() + "进入执行环节 ， 忙碌状态： " + queue_stock_excuteThread.GetThreadIsAvailiable(_threadNo));
                            Thread.Sleep(seed.Next(2000, 5000));
                        }
                        else
                        {
                            //_classTradeStock.BatchTrade(null, 0, null, 0, null);
                        }
                    }
                    else if (trades.Count == 1)
                    {
                        //trades
                        Random seed = new Random();
                        if (CONFIG.IsDebugging())
                            Thread.Sleep(seed.Next(2000, 5000));
                        else
                        {
                            //_classTradeStock.SingleTrade();
                        }
                    }

                    //*********************************
                    //  交易成功后执行的特殊处理
                    //*********************************

                }
            }

        }



    }

    public class TradeParaPackage
    {
        //当前线程的编号
        public int _threadNo { get; set; }
    }
}