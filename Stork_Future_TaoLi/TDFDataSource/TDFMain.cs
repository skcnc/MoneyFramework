using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TDFAPI;
using Stork_Future_TaoLi.Variables_Type;

namespace Stork_Future_TaoLi.TDFDataSource
{
    public class TDFMain
    {
        /// <summary>
        /// 行情订阅主线程
        /// </summary>
        static void MainThread()
        {
            TDFServerInfo[] theServers = new TDFServerInfo[4];
            uint iServerNum = 1;

            theServers[0] = new TDFServerInfo()
            {
                Ip = CHangQingPARA.IP,
                Port = CHangQingPARA.PORT,
                Username = CHangQingPARA.USERNAME,
                Password = CHangQingPARA.PASSWORD
            };

            /******即使不用，也要初始化******/
            theServers[1] = new TDFServerInfo();
            theServers[2] = new TDFServerInfo();
            theServers[3] = new TDFServerInfo();


            /************订阅的类型需要再确认***********/
            var openSetting_ext = new TDFOpenSetting_EXT()
            {
                Servers = theServers,
                ServerNum = iServerNum,
                Subscriptions = "sh;sz;cf;shf;czc;dce",
                ConnectionID = 1,
                Time =0,
                TypeFlags = 0
            };

        }
    }
}