using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using marketinfosys;



namespace Stork_Future_TaoLi
{
    public class MarketInfo 

    {
        private static LogWirter log = new LogWirter();

        private static Dictionary<String, Queue> refStrategyQueue = new Dictionary<String, Queue>();
        private static object lockSync = new object();

        private Dictionary<String, List<String>> subscribeList = new Dictionary<string, List<String>>();


        /// <summary>
        /// 启动行情获取新示例
        /// </summary>
        public void Run()
        {
            Thread excuteThread = new Thread(new ThreadStart(ThreadProc));
            excuteThread.Start();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 设置策略实例行情消息队列的关系
        /// </summary>
        /// <param name="para"></param>
        public static void SetStrategyQueue(KeyValuePair<String, Queue> para)
        {
            lock (lockSync)
            {
                if (refStrategyQueue.ContainsKey(para.Key))
                {
                    refStrategyQueue.Remove(para.Key);
                }
                else
                {
                    refStrategyQueue.Add(para.Key, para.Value);
                }
            }
        }

        public MarketInfo()
        {
            log.EventSourceName = "行情获取模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 63003;
        }

        /// <summary>
        /// 更新本地行情订阅列表
        /// </summary>
        private void updateNewSubscribeList()
        {
            if (MapMarketStratgy.bSubscribeListChangeLabel)
            {
                subscribeList = MapMarketStratgy.GetMapSS();
                MapMarketStratgy.bSubscribeListChangeLabel = false;
            }
        }

        private void ThreadProc()
        {
            //本地股市信息存入stockTable 中
            Hashtable StockTable = new Hashtable();
            StockInfoClient client = new StockInfoClient();
            while (true)
            {
                //更新本地行情列表
                updateNewSubscribeList();

                //从行情应用获取新行情
                Thread.Sleep(1); //线程的喘息时间

                if((DateTime.Now-GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 15)
                {
                    log.LogEvent("行情获取线程即将停止！");
                    break;
                }
                MarketData info = new MarketData();
                try
                {
                    info = client.DeQueueInfo();
                }
                catch(Exception ex)
                {
                    log.LogEvent("未能成功返回； "+ ex.ToString());
                    continue;
                }
                if (info == null)
                    continue;
                else
                {
                    //发现行情有变动，更新本地股市
                    //注册hash键
                    if (StockTable.ContainsKey(info.Code))
                    {
                        StockTable.Remove(info.Code);
                    }
                    StockTable.Add(info.Code, info);

                    if(!subscribeList.Keys.Contains(info.Code))
                    {
                        subscribeList.Add(info.Code, new List<String>());
                    }


                    if (subscribeList.Keys.Contains(info.Code))
                    {
                        //如果没有实例订阅过该股票，就不用管了
                        List<String> _relatedStrategy = subscribeList[info.Code];

                        foreach (String strategy in _relatedStrategy)
                        {
                            if (refStrategyQueue.Keys.Contains(strategy))
                            {
                                refStrategyQueue[strategy].Enqueue((object)info);
                            }
                            else
                            {
                                //如果发现策略实例包含工作列表，却不包含消息队列，则应该报错。
                                continue;
                            }
                        }
                    }
                }
            }

            Thread.CurrentThread.Abort();
        }
    }


    /// <summary>
    /// 股票代码与策略映射关系表
    /// 市场信息更新线程和每个策略执行线程均可访问
    /// </summary>
    public class MapMarketStratgy
    {
        //访问锁定对象
        public static object syncRoot = new object();

        //股票代码与注册该代码的行情映射表
        //public static List<KeyValuePair<String, List<String>>> MapSS = new List<KeyValuePair<string, List<string>>>();

        public static Dictionary<String, List<String>> MapSS = new Dictionary<String, List<String>>();
        public static bool bSubscribeListChangeLabel = false;

        /// <summary>
        /// 获取代码注册的策略
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns>策略编号列表</returns>
        public static List<String> GetRegeditStrategy(String code)
        {

            var t = (from item in MapSS where item.Key == code select item.Value);

            if (t.Count() != 0)
            {
                return (List<String>)t;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 获取策略所注册的代码
        /// </summary>
        /// <param name="Strategy">策略号</param>
        /// <returns>注册代码编号列表</returns>
        public static List<String> GetRegeditCode(String Strategy)
        {
            lock (syncRoot)
            {
                var t = (from item in MapSS where item.Value.Contains(Strategy) select item.Key);

                if (t.Count() != 0)
                {
                    return (List<String>)t;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 输入策略及策略包含代码
        /// 包括添加新的代码和删除老的代码
        /// </summary>
        /// <param name="Strategy">策略名</param>
        /// <param name="Codes">策略包含代码</param>
        public static void SetRegeditStrategy(String Strategy, List<String> Codes)
        {
            //获取已经注册过的代码
            List<String> existRegedit = GetRegeditCode(Strategy);

            lock (syncRoot)
            {
                //获得尚未注册代码
                List<String> ToRegeditCodes = (from item in Codes where existRegedit.Contains(item) == false select item).ToList();

                foreach (String code in ToRegeditCodes)
                {
                    MapSS[code].Add(Strategy);
                }

                List<String> RemoveRegeditCodes = (from item in existRegedit where Codes.Contains(item) == false select item).ToList();

                foreach(String code in RemoveRegeditCodes)
                {
                    MapSS[code].Remove(Strategy);
                }
            }
        }

        /// <summary>
        /// 更新订阅列表
        /// </summary>
        /// <param name="para"></param>
        public static void SetMapSS(Dictionary<String,List<String>> para)
        {
            lock(syncRoot)
            {
                MapSS = para;
                bSubscribeListChangeLabel = true;
            }
        }

        /// <summary>
        /// 获取订阅列表
        /// 该函数由行情模块调用
        /// </summary>
        /// <returns></returns>
        public static Dictionary<String,List<String>> GetMapSS()
        {
            return MapSS;
        }
    }
}