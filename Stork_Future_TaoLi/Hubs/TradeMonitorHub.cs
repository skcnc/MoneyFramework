using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

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

        public static Dictionary<string, List<string>> OrderLists = new Dictionary<string, List<string>>();

        //用户名和链接ID的关系
        private Dictionary<String, String> UserConnectionRelation = new Dictionary<string, string>();

        public void updateOrderList(String name, String JsonString)
        {
            try
            {
                if (!OrderLists.Keys.Contains(name))
                {
                    List<string> ss = new List<string>();
                    ss.Add(JsonString);
                    OrderLists.Add(name, ss);
                }

                if (!UserConnectionRelation.ContainsKey(name)) { return; }
                List<string> jsons = OrderLists[name];


                _context.Clients.Client(UserConnectionRelation[name]).updateOrderList(JsonConvert.SerializeObject(jsons));
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

        public void updateRiskList(String name,String JsonStringRisk, String JsonStringRiskPara)
        {
            try
            {
                if (!UserConnectionRelation.ContainsKey(name)) { return; }

                _context.Clients.Client(UserConnectionRelation[name]).updateRiskList(JsonStringRisk);
                _context.Clients.Client(UserConnectionRelation[name]).updateRiskPara(JsonStringRiskPara);
            }
            catch (Exception ex) { GlobalErrorLog.LogInstance.LogEvent(ex.ToString()); }
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

    /// <summary>
    /// 风控信息
    /// </summary>
    public class TMRiskInfo
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 手数
        /// </summary>
        public string hand { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string price { get; set; }

        /// <summary>
        /// 交易方向
        /// </summary>
        public string orientation { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public string user { get; set; }

        /// <summary>
        /// 策略号
        /// </summary>
        public string strategy { get; set; }

        /// <summary>
        /// 风控信息
        /// </summary>
        public string errinfo { get; set; }
        
    }

    /// <summary>
    /// 委托信息
    /// </summary>
    public class EntrustInfo
    {
        /// <summary>
        /// 系统号
        /// </summary>
        public string sysNo { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>
        public string entrustNo { get; set; }

        /// <summary>
        /// 合约代码
        /// </summary>
        public string contract { get; set; }

        /// <summary>
        /// 买卖
        /// </summary>
        public string direction { get; set; }

        /// <summary>
        /// 开平
        /// </summary>
        public string offsetflag { get; set; }

        /// <summary>
        /// 报单手数
        /// </summary>
        public string amount { get; set; }

        /// <summary>
        /// 未成交手数
        /// </summary>
        public string undealamount { get; set; }

        /// <summary>
        /// 报单价格
        /// </summary>
        public string price { get; set; }

        /// <summary>
        /// 报单状态
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 报单时间
        /// </summary>
        public string time { get; set; }
    }

}