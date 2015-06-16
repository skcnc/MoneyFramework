using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using Stork_Future_TaoLi.Modulars;
using System.Threading;

namespace Stork_Future_TaoLi.PreTradeModule
{
    /// <summary>
    /// 模块名称：   交易预处理模块
    /// 模块函数：
    ///             getInstance()          返回模块实例，并保持在整个应用程序执行期间，不会同事运行两个交易预处理模块
    ///             DeQueue()               从消息队列中获取数据
    /// </summary>
    public class PreTradeModule
    {
        private static PreTradeModule instance;
        private Thread excuteThread  = new Thread(new ThreadStart(ThreadProc));

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

        }

        /// <summary>
        /// 从消息队列中获取数据
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
                 tos = (List<TradeOrderStruct>)queue_prd_trade.GetQueue().Dequeue();
            }
          
            if (tos != null) return tos;
            else return null;
        }

        /// <summary>
        /// 启动预处理线程
        /// </summary>
        /// <returns>如果成功返回true,否则返回false</returns>
        public bool Run()
        {
            excuteThread.Start();
            Thread.Sleep(1000);

            if (excuteThread.ThreadState == ThreadState.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 预处理线程启动
        /// </summary>
        private static void ThreadProc()
        {

            while (true)
            {
                Thread.Sleep(10);
                List<TradeOrderStruct> tos = PreTradeModule.instance.DeQueue();
                if (tos == null)
                {
                    continue;
                }

                //获取到新的list
                List<TradeOrderStruct> stocks_sh = (from item in tos where item.cTradeDirection == TradeDirection.STORK && item.cExhcnageID == ExhcnageID.SH select item).OrderBy(i => i.cOrderLevel).ToList();

                List<TradeOrderStruct> stocks_sz = (from item in tos where item.cTradeDirection == TradeDirection.STORK && item.cExhcnageID == ExhcnageID.SZ select item).OrderBy(i => i.cOrderLevel).ToList();

                List<TradeOrderStruct> future = (from item in tos where item.cTradeDirection == TradeDirection.FUTURE select item).OrderBy(i => i.cOrderLevel).ToList();

                //将新的list推送到对应的线程控制器
                #region 
                List<TradeOrderStruct> unit = new List<TradeOrderStruct>();


                if (stocks_sh.Count > 0)
                {
                    foreach (TradeOrderStruct stu in stocks_sh)
                    {
                        unit.Add(stu);
                        if (unit.Count == 15)
                        {
                            lock (queue_prdTrade_SH_tradeMonitor.GetQueue().SyncRoot)
                            {
                                queue_prdTrade_SH_tradeMonitor.GetQueue().Enqueue((object)unit);
                            }

                            unit.Clear();
                        }
                    }

                    if (unit.Count != 0)
                    {
                        lock (queue_prdTrade_SH_tradeMonitor.GetQueue().SyncRoot)
                        {
                            queue_prdTrade_SH_tradeMonitor.GetQueue().Enqueue((object)unit);
                        }

                        unit.Clear();
                    }

                }

                if (stocks_sz.Count > 0)
                {
                    foreach (TradeOrderStruct stu in stocks_sz)
                    {
                        unit.Add(stu);
                        if (unit.Count == 15)
                        {
                            lock (queue_prdTrade_SZ_tradeMonitor.GetQueue().SyncRoot)
                            {
                                queue_prdTrade_SZ_tradeMonitor.GetQueue().Enqueue((object)unit);
                            }

                            unit.Clear();
                        }
                    }

                    if (unit.Count != 0)
                    {
                        lock (queue_prdTrade_SZ_tradeMonitor.GetQueue().SyncRoot)
                        {
                            queue_prdTrade_SZ_tradeMonitor.GetQueue().Enqueue((object)unit);
                        }

                        unit.Clear();
                    }

                }

                if (future.Count > 0)
                {
                    foreach (TradeOrderStruct stu in future)
                    {
                        unit.Add(stu);
                        if (unit.Count == 15)
                        {
                            lock (queue_prdTrade_FutureTradeMonitor.GetQueue().SyncRoot)
                            {
                                queue_prdTrade_FutureTradeMonitor.GetQueue().Enqueue((object)unit);
                            }

                            unit.Clear();
                        }
                    }

                    if (unit.Count != 0)
                    {
                        lock (queue_prdTrade_FutureTradeMonitor.GetQueue().SyncRoot)
                        {
                            queue_prdTrade_FutureTradeMonitor.GetQueue().Enqueue((object)unit);
                        }

                        unit.Clear();
                    }

                }

                #endregion
                if (DateTime.Now.Second != PreTradeModule.isRunning.Second)
                {
                    PreTradeModule.isRunning = DateTime.Now;
                }

            }
        }
        
    }
}