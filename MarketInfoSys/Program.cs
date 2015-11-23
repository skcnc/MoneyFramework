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

    class simulate_trade
    {
        private static List<Sim_HQ_Struct> _simulate_trade_table = new List<Sim_HQ_Struct>();
        private static string _futureCode = "IH1512";
        private static uint _futurePrice = 4000;
        private static string _indexCode = string.Empty;
        private static uint _indexPrice = 4000;

        public static int SimMarketPerSecond = 10;
        public static string SimMarketCode = "600000;s;16.39|600010;s;3.94|600015;s;10.56|600016;s;8.60|600018;s;7.14|600028;s;5.03|600030;s;15.87|600036;s;17.93|600048;s;8.72|601998;s;6.40";

        //模拟行情开关
        public static bool SimSwitch { get; set; }

        /// <summary>
        /// 初始化模拟行情列表
        /// </summary>
        /// <param name="codes"></param>
        public static void InitSimTable(string codes)
        {
            //code;type;price|code;type;price

            if (codes.Trim() == string.Empty) return;

            List<string> _liCodes = codes.Split('|').ToList();

            _simulate_trade_table.Clear();

            foreach(string s in _liCodes )
            {
                Sim_HQ_Struct unit = new Sim_HQ_Struct()
                {
                    CODE = s.Split(';')[0],
                    TYPE = s.Split(';')[1],
                    PRICE = Convert.ToDecimal(s.Split(';')[2])
                };

                _simulate_trade_table.Add(unit);

                
            }
        }

        //获取股票市场信息
        public static TDFMarketData GetSimMarketDate()
        {
            if (_simulate_trade_table.Count == 0) return null;
            Random seed = new Random();
            Sim_HQ_Struct unit = _simulate_trade_table[seed.Next(_simulate_trade_table.Count - 1)];
            uint price = Convert.ToUInt32(unit.PRICE * 10000);
            double wave = seed.NextDouble(); //涨幅/跌幅

            if(seed.Next(0,1) == 0)
            {
                wave *= -1;
            }

            

            TDFMarketData data = new TDFMarketData()
            {
                AskPrice = new uint[5] { price - 100, price - 200, price, price + 100, price + 200 },
                AskVol = new uint[5] { 100, 100, 100, 100, 100 },
                BidPrice = new uint[5] { price - 100, price - 200, price, price + 100, price + 200 },
                BidVol = new uint[5] { 100, 100, 100, 100, 100 },
                Code = unit.CODE,
                High = price + 1,
                HighLimited = Convert.ToUInt32(price * 1.1),
                IOPV = 0,
                Low = price - 1,
                LowLimited = Convert.ToUInt32(price * 0.9),
                Match = price,
                Time = Convert.ToInt32(DateTime.Now.ToString("HHmmss")) * 1000,
                WindCode = "60000.SH"
            };
           
            return data;

        }

        //获取模拟期货行情
        public static TDFFutureData GetSimFutureData()
        {
            Random seed = new Random();
            double wave = seed.NextDouble(); //涨幅/跌幅

            if (seed.Next(0, 1) == 0)
            {
                wave *= -1;
            }

            TDFFutureData data = new TDFFutureData()
            {
                AskPrice = new uint[5] { _futurePrice - 1, _futurePrice - 2, _futurePrice, _futurePrice + 1, _futurePrice + 2 },
                AskVol = new uint[5] { 100, 100, 100, 100, 100 },
                BidPrice = new uint[5] { _futurePrice - 1, _futurePrice - 2, _futurePrice, _futurePrice + 1, _futurePrice + 2 },
                BidVol = new uint[5] { 100, 100, 100, 100, 100 },
                Code = _futureCode,
                High = _futurePrice + 1,
                HighLimited = Convert.ToUInt32(_futurePrice * 1.1),
                Low = _futurePrice - 1,
                LowLimited = Convert.ToUInt32(_futurePrice * 0.9),
                Match = _futurePrice,
                Time = Convert.ToInt32(DateTime.Now.ToString("HHmmss")) * 1000,
                WindCode = "60000.SH"
            };

            return data;
        }

    }
}
