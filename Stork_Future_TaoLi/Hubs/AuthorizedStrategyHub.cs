using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi.Hubs
{
    public class AuthorizedStrategyHub : Hub
    {
        public void linkin()
        {
            String name = Clients.CallerState.USERNAME;
            AuthorizedStrategyMonitor.Instance.Join(name, Context.ConnectionId);
        }

        public void setTradeQueue()
        {
            string name = Clients.CallerState.USERNAME;
            string type = Clients.CallerState.TYPE;
            string strategy = Clients.CallerState.STRATEGY;
            string code = Clients.CallerState.CODE;
            queue_authorized_trade.EnQueue((object)(type + "|" + name + "|" + strategy + "|" + code));
        }

        /// <summary>
        /// 页面初始化加载
        /// 返回策略列表和策略信息
        /// </summary>
        public void load()
        {
            string name = Clients.CallerState.USERNAME;
            queue_authorized_query.EnQueue((object)name);
        }

        /// <summary>
        /// 获取策略内容
        /// </summary>
        public void setStatusQueue()
        {
            string strategy = Clients.CallerState.STRATEGY;
            string name = Clients.CallerState.USERNAME;
            //展示类型 A+ 全部  O+ 已下单 I+ 未下单
            string type = Clients.CallerState.SHOWTYPE;
            queue_authorized_tradeview.EnQueue((object)(type + "|" + name + "|" + strategy));
        }
    }

    public class AuthorizedStrategyMonitor
    {
        private IHubContext _context;
        private static String ModuleName = "AuthorizedStrategy";
        private AuthorizedStrategyMonitor(IHubContext context) 
        {
            //向行情模块添加消息列表映射
            MarketInfo.SetStrategyQueue(new KeyValuePair<String, Queue>(ModuleName, queue_authorized_market.GetQueue()));
            _context = context; 
        }
        private readonly static AuthorizedStrategyMonitor _instance =
            new AuthorizedStrategyMonitor(GlobalHost.ConnectionManager.GetHubContext<AuthorizedStrategyHub>());
        private static object syncRoot = new object();
        public static AuthorizedStrategyMonitor Instance { get { return _instance; } }

        //用户名和链接ID的关系
        private Dictionary<String, String> UserConnectionRelation = new Dictionary<string, string>();

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

        public void AddNewStrategyView(String user, String Strategy, List<AuthorizedOrder> orders)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).AddStrategyView(Strategy, JsonConvert.SerializeObject(orders));
            }
        }

        public void DeleteStrategyView(String user,String Strategy)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).DeleteStrategyView(Strategy);
            }
        }

        public void CompleteTrade(String user, String Strategy, String Code)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).CompleteTradeView(Strategy, Code);
            }
        }

        public void UpdateStrategiesList(String user, List<String> Strategies)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).updateStrategies(JsonConvert.SerializeObject(Strategies));
            }
        }

        public void UpdateStrategyOrders(String user, String strategy, List<AuthorizedOrder> orders)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).UpdateStrategyOrders(strategy, JsonConvert.SerializeObject(orders));
            }
        }

        public void UpdateCurrentPrice(String user, Dictionary<String, String> prices)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).updatePrice(JsonConvert.SerializeObject(prices));
            }
        }

        public void UpdateCurrentStatus(String user, Dictionary<String, String> status)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).updateStatus(JsonConvert.SerializeObject(status));
            }
        }

        public void UpdateTradeNum(String user, String running, String completed, String total)
        {
            if(UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).updateNum(running, completed, total);
            }
        }

        public void UpdateAccountInfo(String user, String earning, String marketValue)
        {
            if (UserConnectionRelation.Keys.Contains(user))
            {
                String ConnectionID = UserConnectionRelation[user];
                _context.Clients.Client(ConnectionID).updateAccount(earning, marketValue);
            }
        }
    }
}
