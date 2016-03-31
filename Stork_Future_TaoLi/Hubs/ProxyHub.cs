using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using Stork_Future_TaoLi;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Stork_Future_TaoLi
{
    public class ProxyHub : Hub
    {
        /// <summary>
        /// 策略
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strategies"></param>
        public void linkin()
        {
            string strategy = Clients.CallerState.strategyName;
            Join(strategy);
        }

        /// <summary>
        /// 客户端整体建立链接
        /// </summary>
        public void reconnect()
        {
            string strategies = Clients.CallerState.stratgies;

            if (strategies.Trim() == String.Empty) return;

            String[] strs = strategies.Split(';');
            foreach (string s in strs)
            {
                Join(s);
            }
        }

        /// <summary>
        /// 客户端整体断开链接
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            leave();
            return base.OnDisconnected(stopCalled);
        }

        public void Join(string name)
        {
            if (PushStrategyInfo.Instance == null)
            { return; }
            PushStrategyInfo.Instance.join(name, Context.ConnectionId);
        }

        public void leave()
        {
            PushStrategyInfo.Instance.leave(Context.ConnectionId);
        }
    }

    public class PushStrategyInfo
    {
        private IHubContext _context;
        private PushStrategyInfo(IHubContext context) { _context = context; }
        private readonly static PushStrategyInfo _instance = new PushStrategyInfo(GlobalHost.ConnectionManager.GetHubContext<ProxyHub>());
        private static object syncRoot = new object();
        public static PushStrategyInfo Instance
        {
            get { return _instance; }
        }

        //策略名和链接id的字典
        private Dictionary<String, String> RegisterRelation = new Dictionary<string, string>();

        /// <summary>
        /// 客户端连入时添加映射表
        /// </summary>
        /// <param name="strategy_name"></param>
        /// <param name="connectionid"></param>
        public void join(string strategy_name, string connectionid)
        {
            lock (syncRoot)
            {
                if (strategy_name == null || strategy_name == string.Empty) return;
                if (RegisterRelation.ContainsKey(strategy_name))
                {
                    RegisterRelation[strategy_name] = connectionid;
                }
                else
                {
                    RegisterRelation.Add(strategy_name, connectionid);
                }
            }
        }

        /// <summary>
        /// 客户端断链时删除映射表
        /// </summary>
        /// <param name="connectedid"></param>
        public void leave(string connectedid)
        {
            lock (syncRoot)
            {
                if (RegisterRelation.ContainsValue(connectedid))
                {
                    List<string> keys = new List<string>();
                    foreach (KeyValuePair<string, string> item in RegisterRelation)
                    {
                        if (item.Value == connectedid) { keys.Add(item.Key); }
                    }

                    foreach (string key in keys)
                    {
                        RegisterRelation.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="strategy_name"></param>
        /// <returns></returns>
        public string GetConnectedId(string strategy_name)
        {
            if (!RegisterRelation.ContainsKey(strategy_name)) return string.Empty;
            else return RegisterRelation[strategy_name];
        }

        //向客户端传递参数
        public void UpdateStrategyInfo(string name, string Info)
        {


            if (!RegisterRelation.ContainsKey(name))
            {
                //策略实例竟然在页面没有对应的控制面板，此处肯定有问题
                return;
            }

            try
            {
                _context.Clients.Client(RegisterRelation[name]).updatePara(name, Info);
            }
            catch(Exception ex)
            {
                DBAccessLayer.LogSysInfo("ProxyHub-UpdateStrategyInfo", ex.ToString());
                GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
            }
        }

        public string getkey(int k)
        {
            if (RegisterRelation.Count == 0) return string.Empty;
            return RegisterRelation.Keys.ToList()[k];
        }

        public int getnum()
        {
            return RegisterRelation.Count;
        }
    }


}