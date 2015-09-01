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
using System.Collections.Concurrent;

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
            Task FutureTradeControl = new Task(ThreadProc);

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

            DateTime lastHeartBeat = DateTime.Now;//本线程最后发送心跳时间

            List<Task> TradeThreads = new List<Task>();
            log.LogEvent("股票交易控制子线程启动： 初始化交易线程数 :" + futureNum.ToString());

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

                if ((DateTime.Now - lastHeartBeat).TotalSeconds > 10)
                {
                    log.LogEvent("本模块供血不足，线程即将死亡");
                    break;
                }

                //获取下一笔交易
                List<TradeOrderStruct> next_trade = new List<TradeOrderStruct>();

                bool b_get = false;

                if (QUEUE_FUTURE_TRADE.GetQueueNumber() > 0)
                {
                    lock (QUEUE_FUTURE_TRADE.GetQueue().SyncRoot)
                    {
                        if (QUEUE_FUTURE_TRADE.GetQueue().Count > 0)
                        {
                            next_trade = (List<TradeOrderStruct>)QUEUE_FUTURE_TRADE.GetQueue().Dequeue();
                            b_get = true;
                        }

                        if (next_trade.Count > 0) { log.LogEvent("期货交易所出队交易数量：" + next_trade.Count.ToString()); }
                    }
                }

                if (b_get == true)
                {
                    //说明是心跳包    
                    lastHeartBeat = DateTime.Now;   
                }

                if (next_trade.Count == 0) continue;

                //此时next_trade中包含了交易参数
                //判断空闲的线程
                //利用随机选择，保证线程的平均使用

                Random ran = new Random();
                bool _bSearch = true;
                int _tNo = 0;
                while (_bSearch)
                {

                    _tNo = ran.Next(0, futureNum);
                    if (queue_future_excuteThread.GetThreadIsAvailiable(_tNo)) { _bSearch = false; }
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
            DateTime lastHeartBeat = DateTime.Now;//本线程的最后接收心跳时间

            while (true)
            {
                Thread.Sleep(10);
                 
                if((DateTime.Now - lastHeartBeat).TotalSeconds > 10)
                {
                    log.LogEvent("心跳线程即将退出");
                    break;
                }

                //向子线程发送存活心跳，一旦心跳停止，则子线程死亡
                if(DateTime.Now.Second %5 == 0 && DateTime.Now.Second != lastSubHeaartBeat.Second)
                {
                    for(int i = 0;i<threadCount;i++)
                    {
                        //如果发送空列表，则为心跳线程
                        List<TradeOrderStruct> o = new List<TradeOrderStruct>();
                        queue_future_excuteThread.GetQueue(i).Enqueue((object)o);
                    }
                }

                lastHeartBeat = GlobalHeartBeat.GetGlobalTime();
            }

            Thread.CurrentThread.Abort();
        }

       
    }

    public class TradeRecord : ConcurrentDictionary<String, RecordItem>
    {
        private static readonly TradeRecord Instance = new TradeRecord();
        public static TradeRecord GetInstance()
        {
            return Instance;
        }

        public void CreateOrder(String type, String code, String orientation, int amount, decimal price, String StrID)
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

        public void UpdateOrder(int partialAmount, int quitAmount, string key)
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

        public void CompleteOrder(String key, decimal dealPrice, int partialAmount, int quitAmount)
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
        /// 按照机制确认唯一性，并在重启后清0
        /// </summary>
        public int RequestID { get; set; }


        /// <summary>
        /// 交易状态
        /// </summary>
        public TradeDealStatus Status { get; set; }
    }
}