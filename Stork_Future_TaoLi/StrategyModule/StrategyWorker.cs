using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using managedSTY;

namespace Stork_Future_TaoLi.StrategyModule
{
    /// <summary>
    /// 开仓参数
    /// </summary>
    public class OPENPARA
    {
        /// <summary>
        /// 权重文件列表
        /// </summary>
        public Dictionary<String, float> WeightList { get; set; }

        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP { get; set; }

        /// <summary>
        /// 开仓指数类型
        /// </summary>
        public String INDEX { get; set; }
    }

    /// <summary>
    /// 平仓参数
    /// </summary>
    public class CLOSEPARA
    {
        /// <summary>
        /// 空头点位
        /// </summary>
        public float SP { set; get; }

        /// <summary>
        /// 股票成本
        /// </summary>
        public decimal COE { get; set; }

        /// <summary>
        /// 股票分红
        /// </summary>
        public decimal SD { get; set; }

        /// <summary>
        /// 股票配股
        /// </summary>
        public decimal SA { get; set; }

        /// <summary>
        /// 预期收益
        /// </summary>
        public decimal PE { get; set; }

        /// <summary>
        /// 开仓基差
        /// </summary>
        public float BASIS { get; set; }
    }

    public class StrategyWorker
    {

        #region public variables
        /// <summary>
        /// 策略实例ID
        /// </summary>
        public String StrategyInstanceID
        {
            get;
            set;
        }

        /// <summary>
        /// 使用用户
        /// </summary>
        public String User
        { get; set; }

        /// <summary>
        /// 开仓参数
        /// </summary>
        public OPENPARA open_para { get; set; }

        /// <summary>
        /// 平仓参数
        /// </summary>
        public CLOSEPARA close_para { get; set; }

        /// <summary>
        /// 交易类型 
        /// OPEN:开仓
        /// CLOSE：平仓
        /// </summary>
        public String Type { get; set; }

        /// <summary>
        /// 期货合约
        /// </summary>
        public String CT { get; set; }

        /// <summary>
        /// 手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 允许实例运行
        /// </summary>
        public bool bRun { get; set; }

        /// <summary>
        /// 允许实例交易
        /// </summary>
        public bool bAllow { get; set; }

        /// <summary>
        /// 股票交易量列表
        /// </summary>
        public Dictionary<string, int> LiStockOrder = new Dictionary<string,int>();

        /// <summary>
        /// 策略实例结束标志
        /// </summary>
        public bool breaklabel = false;

        /// <summary>
        /// 获取订阅列表
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
        /// 开仓库
        /// </summary>
        private Strategy_OPEN m_strategy_open = new Strategy_OPEN();

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
                _subscribe.Add("600104.sh");
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
            bSubscribeChange = false;
            return _subscribe;
        }

        public void SetSubscribeList(List<String> markets)
        {
            foreach(String s in markets)
            {
                _subscribe.Add(s);
            }
            bSubscribeChange = true;
        }

        /// <summary>
        /// 修改实例运行参数
        /// </summary>
        /// <param name="para">开仓参数</param>
        /// <param name="tradeList">开仓交易列表</param>
        public void UpdateBaseParas(object v)
        {
            lock(lockSync)
            {
                if(v is OPENMODIFY)
                {
                    OPENMODIFY value = (OPENMODIFY)v;
                    HD = value.HD;

                    Dictionary<string, int> oli = new Dictionary<string, int>();

                    foreach (var item in value.POSITION.Split('\n'))
                    {
                        if (item.Trim() == string.Empty) { continue; }
                        else
                        {
                            oli.Add(item.Split(';')[0], Convert.ToInt32(item.Split(';')[1]));
                        }
                    }

                    LiStockOrder = oli;

                    open_para.OP = value.OP;

                    Dictionary<string, float> wli = new Dictionary<string, float>();

                    foreach (var item in value.weightli.Split('\n'))
                    {
                        if (item.Trim() == String.Empty) { continue; }
                        else
                        {
                            wli.Add(item.Split('\n')[0], Convert.ToSingle(item.Split('\n')[2]));
                        }
                    }

                    open_para.WeightList = wli;
                }
                else
                {
                    CLOSEMODIFY value = (CLOSEMODIFY)v;
                    HD = value.HD;

                    Dictionary<string, int> oli = new Dictionary<string, int>();

                    foreach (var item in value.POSITION.Split('\n'))
                    {
                        if (item.Trim() == string.Empty) { continue; }
                        else
                        {
                            oli.Add(item.Split(';')[0], Convert.ToInt32(item.Split(';')[1]));
                        }
                    }

                    LiStockOrder = oli;

                    close_para.BASIS = value.OB;
                    close_para.COE = value.COSTOFEQUITY;
                    close_para.SA = value.STOCKALLOTMENT;
                    close_para.SD = value.STOCKDIVIDENDS;
                    close_para.SP = value.SP;
                    close_para.PE = value.PROSPECTIVEARNINGS;
                }
            }
        }

        public void Init(OPENCREATE para)
        {
            open_args args = new open_args();
            args.weightlist
            
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

                Thread.Sleep(1);
            }
        }
        #endregion
    }
}