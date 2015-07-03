using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class StrategyInterface
    {
        //多线程锁
        public static object rootSync = new object();
        //消息队列映射表
        public static Dictionary<String, Queue> MapStrategyQueue = new Dictionary<string, Queue>();

        /// <summary>
        /// 向指定策略队列中传送消息
        /// </summary>
        /// <param name="Strategy">策略</param>
        /// <param name="Info">策略信息</param>
        /// <returns>
        /// 成功： true
        /// 失败： false 不存在该策略的消息队列
        /// </returns>
        public static bool EnStrategyQueue(String Strategy, object Info)
        {
            lock (rootSync)
            {
                if (MapStrategyQueue.Keys.Contains(Strategy))
                {


                    MapStrategyQueue[Strategy].Enqueue(Info);
                    
                    return true;
                }
                else
                {
                    //不存在这个队列
                    return false;
                }

            }
        }

        /// <summary>
        /// 消息队列中获取消息
        /// </summary>
        /// <param name="Strategy">策略名</param>
        /// <returns>消息内容</returns>
        public static object DeStrategyQueue(String Strategy)
        {
            lock (rootSync)
            {
                if (MapStrategyQueue.Keys.Contains(Strategy) && MapStrategyQueue[Strategy].Count > 0)
                {
                    return MapStrategyQueue[Strategy].Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 注册新的消息队列
        /// </summary>
        /// <param name="Strategy">策略名</param>
        public static void RegeditQueue(String Strategy)
        {
            lock (rootSync)
            {
                if (!MapStrategyQueue.Keys.Contains(Strategy))
                {
                    MapStrategyQueue.Add(Strategy, new Queue());
                }
          
            }
        }
    }
}