using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Stork_Future_TaoLi.Hubs
{
    public class TradeMonitorHub : Hub
    {
        public void linkin()
        {
            String name = Clients.CallerState.USERNAME;
            TradeMonitor.Instance.Join(name, Context.ConnectionId);
        }
    }

    public class TradeMonitor
    {
        private IHubContext _context;
        private TradeMonitor(IHubContext context) { _context = context; }
        private readonly static TradeMonitor _instance = new TradeMonitor(GlobalHost.ConnectionManager.GetHubContext<TradeMonitorHub>());
        private static object syncRoot = new object();
        public static TradeMonitor Instance { get { return _instance; } }

        //用户名和链接ID的关系
        private Dictionary<String, String> UserConnectionRelation = new Dictionary<string, string>();

        public void updateOrderList(String name, String JsonString)
        {
            try
            {
                if (!UserConnectionRelation.ContainsKey(name)) { return; }

                _context.Clients.Client(UserConnectionRelation[name]).updateOrderList(JsonString);
            }
            catch (Exception ex) { GlobalErrorLog.LogInstance.LogEvent(ex.ToString()); }
        }

        public void updateTradeList(String name, String JsonString)
        {
            try
            {
                if (!UserConnectionRelation.ContainsKey(name)) { return; }
                _context.Clients.Client(UserConnectionRelation[name]).updateTradeList(JsonString);
            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
            }
        }

        public void Join(String user,String Connectionid)
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
    }
   
}