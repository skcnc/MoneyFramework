using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using MarketInfoSys;
using System.Threading.Tasks;
using System.Collections;



namespace Stork_Future_TaoLi.MarketInfo
{
    public class MarketInfo
    {
        private static void ThreadProc()
        {
            Hashtable StockTable = new Hashtable();
            StockInfoClient client = new StockInfoClient();
            while (true)
            {
                //从行情应用获取新行情
                MarketData info = client.DeQueueInfo();
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

                    

                }
            }
        }
    }

    public class MarketPool
    {
        MarketData currentData = new MarketData();

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

        public static Dictionary<String, List<String>> MapSS = new Dictionary<string, List<string>>();

        /// <summary>
        /// 获取代码注册的策略
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns>策略编号列表</returns>
        public static List<String> GetRegeditStrategy(String code)
        {
            lock (syncRoot)
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
    }
}