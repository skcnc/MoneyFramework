using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TDFAPI;
using System.Threading;
using System.Text;

namespace MarketInfoSys
{
    public class TDFMain
    {

        public static String ip { get; set; }
        public static String port { get; set; }
        public static String userName { get; set; }
        public static String password { get; set; }
        public static string subscribeList { get; set; }

        public static void Run()
        {
            //启动行情线程
            Thread excuteThread = new Thread(new ThreadStart(MainThread));
            excuteThread.Start();
            Thread.Sleep(1000);

        }

        //delegate String ToString(String[] args);
        /// <summary>
        /// 行情订阅主线程
        /// </summary>
        static void MainThread()
        {
            TDFServerInfo[] theServers = new TDFServerInfo[4];
            uint iServerNum = 1;

            theServers[0] = new TDFServerInfo()
            {
                //Ip = CHangQingPARA.IP,
                //Port = CHangQingPARA.PORT,
                //Username = CHangQingPARA.USERNAME,
                //Password = CHangQingPARA.PASSWORD

                //Ip = "114.80.154.34",
                //Port = "6231",                              //服务器端口
                //Username = "TD1033422002",                        //服务器用户名
                //Password = "27692616"

                Ip = ip,
                Port = port,
                Username = userName,
                Password=password
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
                Markets = "",
                Subscriptions = subscribeList.Replace('\n', ';'),
                ConnectionID = 1,
                Time =0,
                TypeFlags = 0
            };

            using (var dataSource = new TDFSourceImp(openSetting_ext))
            {
                dataSource.SetEnv(EnvironSetting.TDF_ENVIRON_HEART_BEAT_INTERVAL, 0);//环境设置
                dataSource.SetEnv(EnvironSetting.TDF_ENVIRON_MISSED_BEART_COUNT, 0);//环境设置
                dataSource.SetEnv(EnvironSetting.TDF_ENVIRON_OPEN_TIME_OUT, 0);//环境设置

                TDFERRNO nOpenRet = dataSource.Open();

                if (nOpenRet == TDFERRNO.TDF_ERR_SUCCESS)
                {
                    //连接成功
                }
                else
                {
                    //连接失败，告警顶级日志
                    //GlobalErrorLog.LogInstance.LogEvent(String.Format("open returned:{0}, program quit", nOpenRet));
                }

                while (true)
                {
                    Thread.Sleep(20);
                    continue;
                    #region 以下代码是演示订阅功能，真实使用时需要修改
                    //主线程阻塞在这里，等待回调消息通知（其他消息）
//                    String strHelp = @"键入q退出
//    以下命令，请用逗号分隔
//    a 添加订阅
//    d 删除订阅
//    f 清除订阅
//    s 设置订阅
//    hs 显示完全数据
//    hh 显示万得股票名称";
//                    Console.WriteLine(strHelp);
//                    var input = Console.ReadLine();

//                    while (input != "q")
//                    {
//                        var inArgs = input.Split(',');
//                        if (inArgs.Length > 1)
//                        {
//                            ToString convert = (String[] ary) =>
//                            {
//                                System.Text.StringBuilder sb = new StringBuilder();
//                                for (int i = 1; i < ary.Length; ++i)
//                                {
//                                    sb.AppendFormat("{0};", ary[i]);
//                                }

//                                return sb.ToString();
//                            };

//                            switch (inArgs[0])
//                            {
//                                case "a":
//                                    dataSource.SetSubscription(convert(inArgs), SubscriptionType.SUBSCRIPTION_ADD);
//                                    break;
//                                case "d":
//                                    dataSource.SetSubscription(convert(inArgs), SubscriptionType.SUBSCRIPTION_DEL);
//                                    break;
//                                case "s":
//                                    dataSource.SetSubscription(convert(inArgs), SubscriptionType.SUBSCRIPTION_SET);
//                                    break;
//                                case "f":
//                                    dataSource.SetSubscription("", SubscriptionType.SUBSCRIPTION_FULL);
//                                    break;
//                                case "hs":
//                                    dataSource.ShowAllData = true;
//                                    break;
//                                case "hh":
//                                    dataSource.ShowAllData = false;
//                                    break;
//                            }
//                        }
//                        else if (inArgs.Length == 1)
//                        {
//                            switch (inArgs[0])
//                            {
//                                case "f":
//                                    dataSource.SetSubscription("", SubscriptionType.SUBSCRIPTION_FULL);
//                                    break;
//                                case "hs":
//                                    dataSource.ShowAllData = true;
//                                    break;
//                                case "hh":
//                                    dataSource.ShowAllData = false;
//                                    break;
//                            }
//                        }

//                        Console.WriteLine(strHelp);
//                        input = Console.ReadLine();
//                    }
                    #endregion      //演示订阅功能
                    //Thread.Sleep(100);
                }
            }

        }
    }

    public class MarketInfo
    {
        //
        // 摘要: 
        //     原始Code
        public string Code;
        //
        // 摘要: 
        //     时间(HHMMSSmmm)
        public int Time;
        //
        // 摘要: 
        //     状态
        public int Status;
    }
}