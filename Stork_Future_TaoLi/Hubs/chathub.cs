using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Hubs
{
    public class Marketmonitorhub : Hub
    {
        public void Send(string name ,string value)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.updateMarket(name, value);
        }
    }

    public class MarketMonitor
    {
        private IHubContext _context;
        private MarketMonitor(IHubContext context) { _context = context; }
        private readonly static MarketMonitor _instance = new MarketMonitor(GlobalHost.ConnectionManager.GetHubContext<Marketmonitorhub>());
        private static object syncRoot = new object();
        public static MarketMonitor Instance { get { return _instance; } }
    }
}