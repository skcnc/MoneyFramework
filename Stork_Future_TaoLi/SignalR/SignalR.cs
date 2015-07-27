using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using Stork_Future_TaoLi;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(Stork_Future_TaoLi.Startup))]
namespace Stork_Future_TaoLi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app) { app.MapSignalR(); }
    }

    public class ChatHub : Hub
    {
        public void Send(String name, String message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }


}