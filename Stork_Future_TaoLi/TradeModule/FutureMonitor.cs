using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi;
using Stork_Future_TaoLi.Queues;
using System.Threading.Tasks;
using Stork_Future_TaoLi.Variables_Type;
using System.Threading;
using Stork_Future_TaoLi.TradeModule;
using Stork_Future_TaoLi.Modulars;


namespace Stork_Future_TaoLi
{
    public class FutureMonitor
    {

        private LogWirter log = new LogWirter(); //主线程日志记录
        private LogWirter sublog = new LogWirter();//子线程日志记录

        public void Main()
        {
            //初始化消息队列
            queue_future_excuteThread.Init();

            //创建线程任务
            Thread FutureTradeControl = new Thread(new ThreadStart(ThreadProc));

            //启动主线程
            FutureTradeControl.Start();


            //日志初始化
            log.EventSourceName = "期货交易线程控制模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 62308;

            sublog.EventSourceName = "期货交易线程模块";
            sublog.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            sublog.EventLogID = 62309;
        }

        private void ThreadProc() {

            //初始化子线程
            int futureNum = CONFIG.FUTURE_TRADE_THREAD_NUM;



            List<Task> TradeThreads = new List<Task>();
            log.LogEvent("期货交易控制子线程启动： 初始化交易线程数 :" + futureNum.ToString());

            //启动心跳和交易线程
            Task.Factory.StartNew(() => HeartBeatThreadProc((object)futureNum));
            for (int i = 0; i < futureNum; i++)
            {
                TradeParaPackage tpp = new TradeParaPackage();
                tpp._threadNo = (i);
                object para = (object)tpp;
                FutureTrade trade = new FutureTrade();
                trade.SetLog(sublog);
                TradeThreads.Add(Task.Factory.StartNew(() => trade.FutureTradeSubThreadProc(para)));
            }

            //此时按照配置，共初始化CONFIG.FUTURE_TRADE_THREAD_NUM 数量交易线程
            // 交易线程按照方法 FutureTradeSubThreadProc 执行

            //Loop 完成对于子线程的监控
            //每一秒钟执行一次自检
            //自检包含任务：
            //  1. 对每个线程，判断当前交易执行时间，超过 CONFIG.FUTURE_TRADE_OVERTIME 仍未收到返回将会按照 该参数备注执行
            //  2. 判断当前线程空闲状态，整理可用线程列表
            //  3. 若当前存在可用线程，同时消息队列（queue_prdTrade_FutureTradeMonitor)
            //      也包含新的消息送达，则安排线程处理交易
            //  4. 记录每个交易线程目前处理的交易内容，并写入数据库
            while (true)
            {
                Thread.Sleep(10);

                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 10)
                {
                    log.LogEvent("本模块供血不足，线程即将死亡");
                    break;
                }

                //获取下一笔交易
                List<TradeOrderStruct> next_trade = new List<TradeOrderStruct>();



                if (QUEUE_FUTURE_TRADE.GetQueueNumber() > 0)
                {
                    lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                    {
                        if (QUEUE_FUTURE_TRADE.GetQueue().Count > 0)
                        {
                            next_trade = (List<TradeOrderStruct>)QUEUE_FUTURE_TRADE.GetQueue().Dequeue();
                        }

                        if (next_trade.Count > 0) { log.LogEvent("期货交易所出队交易数量：" + next_trade.Count.ToString()); }
                    }
                }

                if (next_trade.Count == 0) continue;

                //此时next_trade中包含了交易参数
                //判断空闲的线程
                //利用随机选择，保证线程的平均使用

                
                Random ran = new Random();
                bool _bSearch = true;
                int _tNo = 0;

                //默认测试用户下，直接使用0号测试线程
                if (next_trade[0].cUser != DebugMode.TestUser)
                {
                    while (_bSearch)
                    {
                        _tNo = ran.Next(0, futureNum);
                        if (queue_future_excuteThread.GetThreadIsAvailiable(_tNo)) { _bSearch = false; }
                    }
                }

                log.LogEvent("安排线程 ： " + _tNo + " 执行交易 数量： " + next_trade.Count);

                //选择第 _tNo 个线程执行交易
                queue_future_excuteThread.GetQueue(_tNo).Enqueue((object)next_trade);
                queue_future_excuteThread.SetThreadBusy(_tNo);


                //************************************
                //将交易发送到相应执行线程后需要做的事情
                //************************************
                //if (DateTime.Now.Second % 3 == 0)
                //{
                //    //每3秒更新一次线程使用状况
                //    Console.WriteLine(DateTime.Now.ToString());
                //    Console.WriteLine("Thread Busy Rate : " + queue_stock_excuteThread.GetBusyNum().ToString() + "/" + stockNum.ToString());
                //}
            }
            Thread.CurrentThread.Abort();
        }

        private  void HeartBeatThreadProc(object para)
        {
            int threadCount = (int)para;

            DateTime lastSubHeaartBeat = DateTime.Now; //子线程最后发送心跳时间


            while (true)
            {
                Thread.Sleep(10);

                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 10)
                {
                    log.LogEvent("心跳线程即将退出");
                    break;
                }

                //向子线程发送存活心跳，一旦心跳停止，则子线程死亡
                if(DateTime.Now.Second %5 == 0 && DateTime.Now.Second != lastSubHeaartBeat.Second)
                {
                    lastSubHeaartBeat = DateTime.Now;
                    for(int i = 0;i<threadCount;i++)
                    {
                        //如果发送空列表，则为心跳线程
                        List<TradeOrderStruct> o = new List<TradeOrderStruct>();
                        queue_future_excuteThread.GetQueue(i).Enqueue((object)o);
                    }
                }
            }

            Thread.CurrentThread.Abort();
        }

       
    }

}