using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections;

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
            List<string> codes = Clients.CallerState.CODES.Trim().Split('|');

            


        }
    
    }

    public class BatchTradeMonitor
    {
        private IHubContext _context;
        private String BatchStrategy = "BatchTradeCodes";
        private BatchTradeMonitor(IHubContext context) 
        {
            //向行情模块添加消息列表映射
            MarketInfo.SetStrategyQueue(new KeyValuePair<String, Queue>(BatchStrategy, newWorker.GetRefQueue()));
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

            if (CodeSubscribeList.Keys.Contains(user))
            {
                foreach (string code in oldList)
                {
                    if(CodeSubscribeList[code].Contains(code))
                    {
                        CodeSubscribeList[code].Remove(user);

                        if(CodeSubscribeList[code].Count == 0)
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


            
            MapMarketStratgy.SetRegeditStrategy(BatchStrategy, Codes);
        }
    }
}