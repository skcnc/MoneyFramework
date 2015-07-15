using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.StrategyModule
{
    public class StrategyWorker
    {
        #region public variables
        /// <summary>
        /// 策略实例名称
        /// MMDDHHMMSS + 指数名 + 期货名
        /// </summary>
        public String StrategyInstanceName
        {
            get;
            set;
        }

        /// <summary>
        /// 策略实例ID
        /// </summary>
        public Guid StrategyInstanceID
        {
            get;
            set;
        }

        /// <summary>
        /// 策略实例初始化参数
        /// </summary>
        public InitParameters BaseNumber = new InitParameters();

        /// <summary>
        /// 权重信息
        /// </summary>
        public Dictionary<string, float> LiStockWeight = new Dictionary<string,float>();

        /// <summary>
        /// 股票交易量列表
        /// </summary>
        public Dictionary<string, int> LiStockOrder = new Dictionary<string,int>();

        /// <summary>
        /// 策略实例结束标志
        /// </summary>
        public bool breaklabel = false;

        /// <summary>
        /// 允许实例运行
        /// </summary>
        public bool bRun = false;

        /// <summary>
        /// 允许实例交易
        /// </summary>
        public bool bAllow = false;

        /// <summary>
        /// 获取交易列表
        /// </summary>
        public List<string> SubscribeList
        {
            get { return _subscribe; }
        }

        /// <summary>
        /// 行情订阅内容修改标志
        /// </summary>
        public bool bSubscribeChange = true;

        #endregion

        #region private variables
        /// <summary>
        /// 定义线程
        /// </summary>
        private Thread excutedThread;
        private delegate void ThreadProc();

        /// <summary>
        /// 行情获取队列
        /// 行情模块发送行情，本线程收取
        /// </summary>
        private Queue _marketQueue = new Queue();

        /// <summary>
        /// 行情订阅信息
        /// </summary>
        private List<string> _subscribe = new List<string>();


        /// <summary>
        /// 同步锁 
        /// </summary>
        private object lockSync = new object();

        /// <summary>
        /// DLL 的接口
        /// </summary>
        //private strategyDLLInterface StrategyDLL = new strategyDLLInterface();

        /// <summary>
        /// 线程运行状态计时
        /// 如果线程运行超过24小时，则强制关闭。
        /// </summary>
        private DateTime RunningTime = new DateTime();
        #endregion


        #region public methods

        /// <summary>
        /// 启动策略执行线程
        /// </summary>
        public void RUN()
        {

            ThreadProc _thread = new ThreadProc(_threadProc);
            excutedThread = new Thread(new ThreadStart(_thread));
            excutedThread.Start();
            RunningTime = DateTime.Now;

            Thread.Sleep(100);

            //Test Code Begin
            if(DebugMode.debug)
            {
              
                _subscribe.Add("600005.sh");
                _subscribe.Add("600651.sh");
                bSubscribeChange = true;
            }
            //Test Code End

        }

        /// <summary>
        /// 向本地传送市场行情信息
        /// </summary>
        /// <param name="mInfo">市场行情信息</param>
        /// <returns>当前队列长度</returns>
        public int EnqueueMarketInfo(object mInfo)
        {
            if(_marketQueue != null)
            {
                _marketQueue.Enqueue(mInfo); 
            }

            return _marketQueue.Count;
        }

        /// <summary>
        /// 获取实例的行情队列
        /// </summary>
        /// <returns></returns>
        public Queue GetRefQueue()
        {
            return _marketQueue;
        }

        /// <summary>
        /// 获取当前策略订阅列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetSubscribeList()
        {
            lock (lockSync)
            {
                return _subscribe;
            }
        }

        /// <summary>
        /// 修改实例运行参数
        /// </summary>
        /// <param name="para">开仓参数</param>
        /// <param name="tradeList">开仓交易列表</param>
        public void UpdateBaseParas(InitParameters para,   Dictionary<string, int> tradeList)
        {
            lock(lockSync)
            {
                this.BaseNumber = para;
                this.LiStockOrder = tradeList;
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// 线程工作函数
        /// </summary>
        private void _threadProc()
        {
            while (!breaklabel)
            { 
                if(bRun)
                {
                    //策略实例运算
                }

                if(bRun && bAllow)
                {
                    // 生成交易列表


                    // 列表只会生成一次
                    breaklabel = true;
                }

            }
        }
        #endregion
    }

    /// <summary>
    /// 策略初始参数
    /// </summary>
    public class InitParameters
    {
        /// <summary>
        /// 期货编号
        /// </summary>
        public string CT;
        /// <summary>
        /// 手数
        /// </summary>
        public int HD;
        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP;
        public float BP;
        /// <summary>
        /// 指数
        /// </summary>
        public string Index;


    }
}