using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Hubs
{
    public class Marketmonitorhub : Hub
    {
        //public void Send(string name ,string value)
        //{
        //    // Call the addNewMessageToPage method to update clients.
        //    Clients.All.updateMarket(name, value);
        //}

        public void aa()
        {
            int a = 0;
            a = 5;
        }
    }

    public class MarketMonitor
    {
        private IHubContext _context;
        private MarketMonitor(IHubContext context) { _context = context; }
        private readonly static MarketMonitor _instance = new MarketMonitor(GlobalHost.ConnectionManager.GetHubContext<Marketmonitorhub>());
        private static object syncRoot = new object();
        public static MarketMonitor Instance { get { return _instance; } }

        public void Send(MarketValue value)
        {
            _context.Clients.All.updatevalue(
                value.Code,
                value.Time,
                value.Type,
                ((float)value.Match)/10000,
                ((float)value.HighLimit) / 10000,
                ((float)value.LowLimit) / 10000,
                value.isStop,
                ((float)value.PreClose) / 10000
                );
        }
    }
}