using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi.Hubs
{
    public class sysMonitorHub : Hub
    {
        public void hello()
        {
            Clients.All.hello();
        }
    }

    public class SysMonitor
    {
        private IHubContext _context;
        private SysMonitor(IHubContext context) { _context = context; }
        private readonly static SysMonitor _instance = new SysMonitor(GlobalHost.ConnectionManager.GetHubContext<sysMonitorHub>());
        private static object syncRoot = new object();
        public static SysMonitor Instance { get { return _instance; } }

        public void updateSysStatus(SystemStatusClass message)
        {
            String Json = JsonConvert.SerializeObject(message);

            _context.Clients.All.updateStatus(Json);

        }
    }
}