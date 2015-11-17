using MarketInfoSys.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TDFAPI;

namespace MarketInfoSys
{
    /// <summary>
    /// 从远端获取到信息，先转换成该类型
    /// 应用服务获取时会根据类型转换成通用型参数
    /// </summary>
    class EnQueueType
    {
        public String Type { get; set; }
        public object value { get; set; }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            ControlPanel panel = new ControlPanel();

            Application.Run(panel);
        }
    }

    class webservice
    {
        private static  bool stop = true;
        private static ServiceHost host = new ServiceHost(typeof(StockInfo));

        public static bool STOP
        {
            set
            {
                stop = value;
            }
            get
            {
                return stop;
            }
        }


        public static int GetLength()
        {
            return Queue_Data.GetQueue().Count;
        }

        public static Thread excuteThread = new Thread(new ThreadStart(() =>
        {
            //启动行情服务
            TDFMain.Run();

            //启动WCF服务
            DateTime dt = DateTime.Now;
            while (!stop)
            {
                if ((DateTime.Now - dt).TotalMinutes > 10)
                {
                    dt = DateTime.Now;
                }

                if (dt.DayOfWeek == DayOfWeek.Saturday)
                {
                    break;
                }

                Thread.Sleep(100);
            }
        }));

        public static void run()
        {
            excuteThread.Start();
            host.Open();
            Queue_Data.Suspend = false;
        }

        public static void suspend()
        {
            Queue_Data.Suspend = true;
        }

        public static void resume()
        {
            Queue_Data.Suspend = false;
        }

        public static void abort()
        {
            stop = true;
            host.Close();
            Queue_Data.Suspend = true;
        }
    }

    class stop_plate_stocks
    {
        //停盘列表
        private static List<TDFMarketData> stop_stocks = new List<TDFMarketData>();
        private static stop_plate_stocks instance = new stop_plate_stocks();

        public List<TDFMarketData> GetStopList() { return stop_stocks; }

        public static stop_plate_stocks GetInstance()
        {
            if (instance == null) instance = new stop_plate_stocks();
            return instance;
        }

        /// <summary>
        /// 更新停盘列表
        /// </summary>
        /// <param name="data"></param>
        public void updateStopList(TDFMarketData data)
        {
            if(data.Status != 68)
            {
                return;
            }

            if (stop_stocks.Count != 0)
            {
                var temp = (from item in stop_stocks where item.Code == data.Code select item);
                if (temp.Count() > 0)
                {
                    //已经添加 
                    return;
                }
            }

            lock (stop_stocks)
            {
                stop_stocks.Add(data);
            }
        }
    }
}
