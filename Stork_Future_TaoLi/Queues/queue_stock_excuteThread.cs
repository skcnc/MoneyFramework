using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Stork_Future_TaoLi.Variables_Type;

namespace Stork_Future_TaoLi.Queues
{
    /// <summary>
    /// 由股票交易总线程控制，与每个具体执行交易线程通信的消息队列
    /// </summary>
    public class queue_stock_excuteThread 
    {
        public static List<Queue> StockExcuteQueues = new List<Queue>();
        public static List<bool> StockThreadIsAvailiable = new List<bool>();
        public static List<DateTime> StockThreadUpdateTime = new List<DateTime>();

        /// <summary>
        /// 按照 CONFIG.STOCK_TRADE_THREAD_NUM 要求数量
        /// 初始化消息队列
        /// </summary>
        public static void Init()
        {
            for (int i = 0; i < CONFIG.STOCK_TRADE_THREAD_NUM; i++)
            {
                StockExcuteQueues.Add(new Queue());
                StockThreadIsAvailiable.Add(true);
                StockThreadUpdateTime.Add(DateTime.Now);
            }
        }

        /// <summary>
        /// 获取对应线程最新更新时间
        /// 标记线程最近一次执行交易的时间
        /// </summary>
        /// <param name="_threadNo"></param>
        /// <returns></returns>
        public static DateTime GetUpdateTime(int _threadNo)
        {
            return StockThreadUpdateTime[_threadNo];
        }

        /// <summary>
        /// 更新线程最新更新时间
        /// </summary>
        /// <param name="_threadNo"></param>
        public static void SetUpdateTime(int _threadNo)
        {
            StockThreadUpdateTime[_threadNo] = DateTime.Now;
        }

        /// <summary>
        /// 返回指定线程号的队列
        /// </summary>
        /// <param name="_threadNo">指定的线程号</param>
        /// <returns>相应队列</returns>
        public static Queue GetQueue(int _threadNo)
        {
            if (StockExcuteQueues[_threadNo] == null)
            {
                StockExcuteQueues[_threadNo] = new Queue();
            }

            return StockExcuteQueues[_threadNo];
        }

        /// <summary>
        /// 返回指定线程的状态 
        /// </summary>
        /// <param name="_threadNo">线程编号</param>
        /// <returns>
        /// TRUE ： 线程空闲
        /// FALSE： 线程忙碌
        /// </returns>
        public static bool GetThreadIsAvailiable(int _threadNo)
        {
            return StockThreadIsAvailiable[_threadNo];
        }

        /// <summary>
        /// 放回当前忙碌线程数量
        /// </summary>
        /// <returns>忙碌线程数量</returns>
        public static int GetBusyNum()
        {
            int i = 0;
            lock (StockThreadIsAvailiable)
            {
                foreach (bool b in StockThreadIsAvailiable)
                {
                    if (b == false)
                    {
                        i++;
                    }
                }
            }

            return i;
        }

        /// <summary>
        /// 设定指定线程的工作状态为“忙碌”
        /// </summary>
        /// <param name="_threadNo"></param>
        public static void SetThreadBusy(int _threadNo)
        {
            lock (StockThreadIsAvailiable)
            {
                StockThreadIsAvailiable[_threadNo] = false;
            }
        }

        /// <summary>
        /// 设定指定线程的工作状态为“空闲”
        /// </summary>
        /// <param name="_threadNo"></param>
        public static void SetThreadFree(int _threadNo)
        {
            lock (StockThreadIsAvailiable)
            {
                StockThreadIsAvailiable[_threadNo] = true;
            }
        }

        /// <summary>
        /// 获得指定线程消息数量
        /// </summary>
        /// <param name="_threadNo">线程号</param>
        /// <returns>消息数量</returns>
        public static int GetQueueNumber(int _threadNo)
        {
            if (StockExcuteQueues[_threadNo] != null)
            {
                return StockExcuteQueues[_threadNo].Count;
            }
            else
            {
                return -1;
            }
        }
    }

}