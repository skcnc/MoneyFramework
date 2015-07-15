using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stork_Future_TaoLi.Test;
using Stork_Future_TaoLi.PreTradeModule;
using Stork_Future_TaoLi.TradeModule;
using System.Threading;


namespace Stork_Future_TaoLi
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            //模块初始化工作
            ListCreate.Main();


            PreTradeModule.PreTradeModule.getInstance().Run();

            StockTradeThread.Main();

            StrategyMonitorClass strategyMonitor = new StrategyMonitorClass();
            strategyMonitor.Run();

            MarketInfo marketInfo = new MarketInfo();
            marketInfo.Run();

        }
    }

    /// <summary>
    /// Global 心跳发射线程
    /// </summary>
    public class ThreadHeartBeatControl
    {
        private static Thread HeartThread = new Thread(new ThreadStart(threadProc));

        public static void Run()
        {
            HeartThread.Start();
            Thread.Sleep(1000);
        }

        private static void threadProc()
        {
            while(true)
            {
                Thread.Sleep(1000);
                if (DateTime.Now.Minute % 2 == 0)
                {
                    GlobalHeartBeat.SetGlobalTime();
                }
            }
        }
    }
}