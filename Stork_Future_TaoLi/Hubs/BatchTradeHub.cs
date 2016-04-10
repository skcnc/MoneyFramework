using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections;
using System.Threading;
using Stork_Future_TaoLi.Queues;
using marketinfosys;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi.Hubs
{
    public class BatchTradeHub : Hub
    {
        public void linkin()
        {
            String name = Clients.CallerState.USERNAME;
            BatchTradeMonitor.Instance.Join(name, Context.ConnectionId);
        }

        public void scribeTradeCode()
        {
            string name = Clients.CallerState.USERNAME;
            String[] codes = Clients.CallerState.CODES.Trim().Split('|');

            BatchTradeMonitor.Instance.MergeSubscribeList(name.Trim(), codes.ToList());
        }
    
    }

    public class BatchTradeMonitor
    {
        private IHubContext _context;
        private String BatchStrategy = "BatchTradeCodes";
        private BatchTradeMonitor(IHubContext context) 
        {
            //向行情模块添加消息列表映射
            MarketInfo.SetStrategyQueue(new KeyValuePair<String, Queue>(BatchStrategy, queue_batchtrade_market.GetQueue()));
            _context = context; 
        }
        private readonly static BatchTradeMonitor _instance = 
            new BatchTradeMonitor(GlobalHost.ConnectionManager.GetHubContext<BatchTradeHub>());
        private static object syncRoot = new object();
        public static BatchTradeMonitor Instance { get { return _instance; } }

        //用户名和链接ID的关系
        private Dictionary<String, String> UserConnectionRelation = new Dictionary<string, string>();
        
        //用户批量交易证券价格监控注册表
        private Dictionary<String, List<string>> UserSubscribeList = new Dictionary<string, List<string>>();

        /// <summary>
        /// 所有批量交易界面订阅股票实时行情列表
        /// 由于该表变化频繁，采用两级提交机制
        /// 后台推送给辅表，保存实时信息
        /// 界面获取正表，每s刷新一次
        /// </summary>
        private Dictionary<String, String> CodeAndPriceList = new Dictionary<string, string>();
        private Dictionary<String, String> CodeAndPriceList_temp = new Dictionary<string, string>();

        /// <summary>
        /// 代码注册账户信息
        /// </summary>
        private Dictionary<String, List<String>> CodeSubscribeList = new Dictionary<String, List<String>>();


        //获取连接ID，并保存在本地
        public void Join(String user, String Connectionid)
        {
            lock (syncRoot)
            {
                if (user == null || user == String.Empty) return;
                if (UserConnectionRelation.ContainsKey(user))
                {
                    UserConnectionRelation[user] = Connectionid;
                }
                else
                {
                    UserConnectionRelation.Add(user, Connectionid);
                }
            }
        }

        /// <summary>
        /// 功能：合并所有批量交易页面订阅列表，生成用户证券订阅列表，证券用户关系列表
        /// 引发marketInfo->MapMarketStratgy 更新
        /// 调用时间： 批量交易页面生成新注册列表
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="codeList">订阅列表</param>
        public void MergeSubscribeList(String user,List<String> codeList)
        {
            user = user.Trim();

            List<String> oldList = new List<string>();
            if (UserSubscribeList.Keys.Contains(user))
            {
                oldList = UserSubscribeList[user];
                UserSubscribeList[user].Clear();
            }
            else
            {
                UserSubscribeList.Add(user, new List<string>());
            }

            foreach(string code in codeList)
            {
                UserSubscribeList[user].Add(code);
            }

            //更新股票-注册者映射


                foreach (string code in oldList)
                {
                    if (CodeSubscribeList.Keys.Contains(code))
                    {
                        if (CodeSubscribeList[code].Contains(user))
                        {
                            CodeSubscribeList[code].Remove(user);

                            if (CodeSubscribeList[code].Count == 0)
                            {
                                CodeSubscribeList.Remove(code);
                            }
                        }
                    }
                }


            foreach(String code in codeList)
            {
                if(!CodeSubscribeList.Keys.Contains(code))
                {
                    CodeSubscribeList.Add(code, new List<string>());
                }

                CodeSubscribeList[code].Add(user);
            }

            List<String> Codes = CodeSubscribeList.Keys.ToList();

            MapMarketStratgy.SetMapMS(BatchStrategy, Codes);
        }

        /// <summary>
        /// 更新行情列表
        /// </summary>
        /// <param name="PriceList"></param>
        public void SetViewPrice(Dictionary<String,String> PriceList)
        {
            foreach(KeyValuePair<String,String> pair in UserConnectionRelation)
            {
                Dictionary<String, String> userPriceList = new Dictionary<string, string>();

                if(!UserSubscribeList.Keys.Contains(pair.Key)) continue;
                List<String> codes = UserSubscribeList[pair.Key];
                foreach(String code in codes)
                {
                    if(PriceList.Keys.Contains(code))
                    {
                        userPriceList.Add(code, PriceList[code]);
                    }
                }

                //userPriceList 中包含用户 pair.key 的所有注册行情最新价格
                _context.Clients.Client(UserConnectionRelation[pair.Key]).updatePriceList(JsonConvert.SerializeObject(userPriceList));
            }
        }
    }


    public class BatchTrade_MarketReciver
    {

        private static DateTime runningMark = new DateTime();

        private static object syncRoot = new object();

        /// <summary>
        /// 证券价格缓存列表
        /// </summary>
        private static Dictionary<String, String> CodeAndPriceList = new Dictionary<string, string>();


        public static void Run()
        {
            Thread newThread = new Thread(TradeProc);
            newThread.Start();
        }

        public static Dictionary<String, String> GetMarketValue()
        {
            lock(syncRoot)
            {
                Dictionary<String, String> RtnValue = new Dictionary<string, string>();

                foreach(KeyValuePair<String,String> pair in CodeAndPriceList)
                {
                    RtnValue.Add(pair.Key, pair.Value);
                }

                return RtnValue;
            }
        }


        /// <summary>
        /// 更新本地行情变动
        /// </summary>
        private static void TradeProc()
        {

            while (true) {

                Thread.Sleep(1);


                if (DateTime.Now.Second != runningMark.Second)
                {
                    runningMark = DateTime.Now;

                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("BatchTrade_MarketReciver", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);

                    if (DateTime.Now.Second % 2 == 0)
                    {
                        Dictionary<String, String> value = GetMarketValue();
                        BatchTradeMonitor.Instance.SetViewPrice(value);
                    }
                }


                if(queue_batchtrade_market.GetQueueNumber() > 0)
                {
                    MarketData data = (MarketData)queue_batchtrade_market.GetQueue().Dequeue();

                    lock(syncRoot)
                    {
                       if(!CodeAndPriceList.Keys.Contains(data.Code.Trim()))
                       {
                           CodeAndPriceList.Add(data.Code.Trim(), data.Match.ToString());
                       }

                       CodeAndPriceList[data.Code.Trim()] = data.Match.ToString();
                    }

                }

            
            }
           
        }
    }
}