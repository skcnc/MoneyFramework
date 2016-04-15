using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class queue_authorized_trade
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
        /// 入队
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool EnQueue(object v)
        {
            if (instance == null)
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

    public class queue_authorized_market
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
        /// 入队
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool EnQueue(object v)
        {
            if (instance == null)
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

    public class queue_authorized_query
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
        /// 入队
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool EnQueue(object v)
        {
            if (instance == null)
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
    /// 入队交易类型：
    /// 1. 显示全部交易  A+
    /// 2. 显示下单交易  O+
    /// 3. 显示未下单交易I+
    /// </summary>
    public class queue_authorized_tradeview
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
        /// 入队
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool EnQueue(object v)
        {
            if (instance == null)
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
}