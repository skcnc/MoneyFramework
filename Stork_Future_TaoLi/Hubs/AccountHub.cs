using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Stork_Future_TaoLi.Hubs
{
    public class accountHub : Hub
    {
        public void linkin(String name)
        {
            AccountCalculate.Instance.Join(name, Context.ConnectionId);
        }

        public void leave(String name)
        {
            AccountCalculate.Instance.Leave(name);
        }

    }

    public class AccountCalculate
    {
        private IHubContext _context;
        private AccountCalculate(IHubContext context) { _context = context; }
        private readonly static AccountCalculate _instance = new AccountCalculate(GlobalHost.ConnectionManager.GetHubContext<accountHub>());
        private static object syncRoot = new object();
        public static AccountCalculate Instance { get { return _instance; } }

        /// <summary>
        /// 已开行情监控页面用户和链接ID的字典
        /// </summary>
        private Dictionary<String, String> UserConnectionRelation = new Dictionary<string, string>();

        public void updateAccountInfo(String name, String JsonString,bool admin)
        {
            try
            {
                if (!UserConnectionRelation.ContainsKey(name)) { return; }
                _context.Clients.Client(UserConnectionRelation[name]).updateAccountinfo(admin,JsonString);
            }
            catch (Exception ex)
            {
                DBAccessLayer.LogSysInfo("AccountHub-updateAccountInfo", ex.ToString());
                GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
            }
        }

        public bool checkAccountInfo(String name)
        {
            if (!UserConnectionRelation.ContainsKey(name)) return false;
            else return true;
        }

        /// <summary>
        /// 客户端连入映射表
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="Connectionid">连接号</param>
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
        /// 删除用户链接
        /// </summary>
        /// <param name="User"></param>
        public void Leave(String User)
        {
            lock (syncRoot)
            {
                if (User == null || User == string.Empty) return;

                if (UserConnectionRelation.ContainsKey(User))
                {
                    UserConnectionRelation.Remove(User);
                }
            }
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public string GetConnectionId(String name)
        {
            if (!UserConnectionRelation.ContainsKey(name)) return string.Empty;
            else return UserConnectionRelation[name];
        }

        public string getkey(int k)
        {
            if (UserConnectionRelation.Count == 0) return string.Empty;
            return UserConnectionRelation.Keys.ToList()[k];
        }

        public int getnum()
        {
            return UserConnectionRelation.Count;
        }
    }
}