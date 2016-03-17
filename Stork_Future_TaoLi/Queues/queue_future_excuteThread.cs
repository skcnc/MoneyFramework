using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Queues
{
    public class queue_future_excuteThread
    {
        public static List<Queue> FutureExcuteQueue = new List<Queue>();
        public static List<bool> FutureThreadIsAvailiable = new List<bool>();
        public static List<DateTime> FutureThreadUpdateTime = new List<DateTime>();
        private static object SyncRoot = new object();
        /// <summary>
        /// 按照 CONFIG.FUTURE_TRADE_THREAD_NUM 要求数量
        /// 初始化消息队列
        /// </summary>
        public static void Init()
        {
            for (int i = 0; i < CONFIG.FUTURE_TRADE_THREAD_NUM; i++)
            {
                FutureExcuteQueue.Add(new Queue());
                FutureThreadIsAvailiable.Add(true);
                FutureThreadUpdateTime.Add(DateTime.Now);
            }
        }

        /// <summary>
        /// 设置线程最新更新时间
        /// </summary>
        /// <param name="_threadNo"></param>
        public static void SetUpdateTime(int _threadNo)
        {
            lock (SyncRoot)
            {
                FutureThreadUpdateTime[_threadNo] = DateTime.Now;
            }
        }


        /// <summary>
        /// 返回指定线程号的队列
        /// </summary>
        /// <param name="_threadNo">指定线程号</param>
        /// <returns>相应队列</returns>
        public static Queue GetQueue(int _threadNo)
        {
            lock (SyncRoot)
            {
                if (FutureExcuteQueue[_threadNo] == null) { FutureExcuteQueue[_threadNo] = new Queue(); }

                return FutureExcuteQueue[_threadNo];
            }
        }

        public static bool GetThreadIsAvailiable(int _threadNo)
        {
            lock (SyncRoot)
            {
                return FutureThreadIsAvailiable[_threadNo];
            }
        }

        /// <summary>
        /// 获取当前忙碌线程数量
        /// </summary>
        /// <returns></returns>
        public static int GetBusyNum()
        {
            int i = 0;
            lock (SyncRoot)
            {
                foreach (bool b in FutureThreadIsAvailiable)
                {
                    if (b == false) i++;
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
            lock (SyncRoot)
            {
                FutureThreadIsAvailiable[_threadNo] = false;
            }
        }

        /// <summary>
        /// 设定指定线程的工作状态为“空闲”
        /// </summary>
        /// <param name="_threadNo"></param>
        public static void SetThreadFree(int _threadNo)
        {
            lock (SyncRoot)
            {
                FutureThreadIsAvailiable[_threadNo] = true;
            }
        }

        /// <summary>
        /// 获得指定线程消息数量
        /// </summary>
        /// <param name="_threadNo">线程号</param>
        /// <returns>消息数量</returns>
        public static int GetQueueNumber(int _threadNo)
        {
            lock (SyncRoot)
            {
                if (FutureExcuteQueue[_threadNo] != null)
                {
                    return FutureExcuteQueue[_threadNo].Count;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}