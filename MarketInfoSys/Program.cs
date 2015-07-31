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
        public static bool STOP
        {
            set
            {
                stop = value;
            }
        }


        public static int GetLength()
        {
            return Queue_Data.GetQueue().Count;
        }

        public static void run()
        {
            Thread excuteThread = new Thread(new ThreadStart(() =>
            {
                //启动行情服务
                TDFMain.Run();

                //启动WCF服务
                ServiceHost host = new ServiceHost(typeof(StockInfo));
                host.Open();

                //ServiceHost crossDomainserivceHOST = new ServiceHost(typeof(DomainService));
                //crossDomainserivceHOST.Open();
                DateTime dt = DateTime.Now;
                while (!stop)
                {
                    if ((DateTime.Now - dt).TotalSeconds > 10)
                    {
                        dt = DateTime.Now;

                        //Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "  股市队列长度:" + Queue_Market_Data.GetQueue().Count);
                    }

                    if (dt.DayOfWeek == DayOfWeek.Saturday)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
                //Console.ReadLine();
                host.Close();

            }));

            excuteThread.Start();
        }
    }
}
