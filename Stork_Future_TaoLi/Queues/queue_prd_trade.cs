using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Stork_Future_TaoLi.Variables_Type;

namespace Stork_Future_TaoLi.Modulars
{
    /// <summary>
    /// 交易预处理模块与主控模块通讯的消息队列
    /// </summary>
    public class queue_prd_trade
    {
        private static  Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }
           
            return instance;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool EnQueue(object v)
        {
            if(instance == null)
            {
                instance = new Queue();
            }
            try
            {
                instance.Enqueue(v);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// 交易预处理模块与上海交易所股票交易线程控制模块的消息队列
    /// </summary>
    public class QUEUE_SH_TRADE
    {
        private static Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }

            return instance;
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }

    }

    /// <summary>
    /// 交易预处理模块与上海交易所股票交易线程控制模块的消息队列
    /// </summary>
    public class QUEUE_SZ_TRADE
    {
        private static Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }

            return instance;
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// 交易预处理模块与期货交易线程控制模块的消息队列
    /// </summary>
    public class QUEUE_FUTURE_TRADE
    {
        private static Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }

            return instance;
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }
    }


    /// <summary>
    /// 交易管理线程和心跳发起线程之间的消息队列
    /// </summary>
    public class Queue_Trade_Heart_Beat
    {
        private static Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }

            return instance;
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }

    }


    public class Queue_Future_Heart_Beat
    {
        private static Queue instance;

        /// <summary>
        /// 获取队列的实例
        /// </summary>
        /// <returns>队列实例</returns>
        public static Queue GetQueue()
        {
            if (instance == null)
            {
                instance = new Queue();
            }

            return instance;
        }

        /// <summary>
        /// 获取该队列中的消息数量
        /// </summary>
        /// <returns>
        /// -1      队列未初始化或状态异常
        /// 其他    队列包含消息数量
        /// </returns>
        public static int GetQueueNumber()
        {
            if (instance != null)
            {
                return instance.Count;
            }
            else
            {
                return -1;
            }
        }

    }
}