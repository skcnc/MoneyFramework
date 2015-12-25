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


            //初始化行情模拟系统
            simulate_trade.InitSimTable(simulate_trade.SimMarketCode);

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
                    Queue_Data.Connected = true;
                }
                else
                {
                    Queue_Data.Connected = false;
                    //连接失败，告警顶级日志
                    //GlobalErrorLog.LogInstance.LogEvent(String.Format("open returned:{0}, program quit", nOpenRet));
                }

                

                while (true)
                {
                    if (webservice.STOP) { break; }
                    Thread.Sleep(1000);

                    //每隔10s发送一次停盘信息
                    if ((DateTime.Now - RunningTime.CurrentTime).TotalSeconds > 10)
                    {
                        foreach (TDFMarketData data in stop_plate_stocks.GetInstance().GetStopList())
                        {
                            EnQueueType obj = new EnQueueType() { Type = "S", value = (object)data };
                            Queue_Data.GetQueue().Enqueue((object)obj);
                        }
                        RunningTime.CurrentTime = DateTime.Now;

                    }

                    if ((simulate_trade.SimSwitch)&&(Queue_Data.Suspend == false))
                    {
                        for (int i = 0; i < simulate_trade.SimMarketPerSecond; i++)
                        {
                            TDFMarketData objs = simulate_trade.GetSimMarketDate();
                            new EnQueueType() { Type = "S", value = (object)objs };
                            if (Queue_Data.Suspend == false)
                            {
                                Queue_Data.GetQueue().Enqueue((object)(new EnQueueType() { Type = "S", value = (object)objs }));
                            }

                            TDFFutureData objf = simulate_trade.GetSimFutureData();
                            Queue_Data.GetQueue().Enqueue((object)(new EnQueueType() { Type = "F", value = (object)objf }));

                        }
                    }
                    continue;
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