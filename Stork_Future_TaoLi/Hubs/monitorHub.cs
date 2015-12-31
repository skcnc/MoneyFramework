using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi.Hubs
{
    public class monitorHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }

    public class MonitorSys
    {
       private IHubContext _context;
       private MonitorSys(IHubContext context) { _context = context; }
       private readonly static MonitorSys _instance = new MonitorSys(GlobalHost.ConnectionManager.GetHubContext<monitorHub>());
        private static object syncRoot = new object();
        public static MonitorSys Instance { get { return _instance; } }


        public void updateSysStatus(SystemStatusClass message )
        {
            string json = JsonConvert.SerializeObject(message);

            _context.Clients.All.updateStatus(json);
        }

    }
}