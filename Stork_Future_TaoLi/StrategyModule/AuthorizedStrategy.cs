using marketinfosys;
using Stork_Future_TaoLi.Hubs;
using Stork_Future_TaoLi.Modulars;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.StrategyModule
{
    public class AuthorizedStrategy
    {

        private static AuthorizedStrategy instance = new AuthorizedStrategy();
        private static LogWirter log = new LogWirter();
        private static DateTime DailyDBExchangeMark = new DateTime();

        public static void RUN()
        {
            log.EventSourceName = "授权交易模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 66001;

            DailyDBExchangeMark = new DateTime(1900, 1, 1);
            AuthorizedTradesList.LoadPauseStrategy();

            Thread.Sleep(1000);

            Thread excuteThreadA = new Thread(new ThreadStart(ThreadProc_Check));
            Thread excuteThreadB = new Thread(new ThreadStart(ThreadProc_PushInfo));
            excuteThreadA.Start();
            excuteThreadB.Start();

            
        }

        private static void ThreadProc_Check()
        {
            log.LogEvent("授权交易线程A启动！");

            DateTime dt = DateTime.Now;

            while (true)
            {
                Thread.Sleep(1);

                if (DateTime.Now.Second != dt.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("AuthorizedStrategy_ThreadA", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);

                    dt = DateTime.Now;
                }

                if (DailyDBExchangeMark.Year != DateTime.Now.Year || DailyDBExchangeMark.DayOfWeek != DateTime.Now.DayOfWeek)
                {
                    AuthorizedTradesList.DailyDBExchange();
                    DailyDBExchangeMark = DateTime.Now;
                }

                while(queue_authorized_market.GetQueueNumber() > 0)
                {
                    MarketData info = (MarketData)queue_authorized_market.GetQueue().Dequeue();
                    AuthorizedMarket.UpdateMarketList(info);
                }

               
                // 新指令判断
                if(queue_authorized_trade.GetQueueNumber() > 0)
                {
                    try
                    {
                       var  orders = queue_authorized_trade.GetQueue().Dequeue();

                       if (orders is List<AuthorizedOrder>)
                       {
                           AuthorizedTradesList.SubscribeNewAuhorizedStrategy((List<AuthorizedOrder>)orders);
                       }
                       else
                       {
                           string str = ((String)orders);
                           string type = str.Split('|')[0].Trim();
                           string name = str.Split('|')[1].Trim();
                           string strategy = str.Split('|')[2].Trim();
                           string code = str.Split('|')[3].Trim();

                           switch(type)
                           {
                               case "BSO+":
                                   //策略全部启动
                                   AuthorizedTradesList.StartStrategyTrade(strategy);
                                   break;
                               case "BSS+":
                                   //策略全部暂停
                                   AuthorizedTradesList.SuspendStrategyTrade(strategy);
                                   break;
                               case "BSK+":
                                   //策略全部停止
                                   AuthorizedTradesList.StopStrategyTrade(strategy);
                                   break;
                               case "BSF+":
                                   //策略全部强制交易
                                   AuthorizedTradesList.ForceStrategyTrade(strategy);
                                   break;
                               case "BCO+":
                                   //指定交易启动
                                   AuthorizedTradesList.StartSingleTrade(strategy, code);
                                   break;
                               case "BCS+":
                                   //指定交易暂停
                                   AuthorizedTradesList.SuspendSingleTrade(strategy, code);
                                   break;
                               case "BCK+":
                                   //指定交易停止
                                   AuthorizedTradesList.StopSingleTrade(strategy, code);
                                   break;
                               case "BCF+":
                                   //指定交易强制交易
                                   AuthorizedTradesList.ForceSingleTrade(strategy, code);
                                   break;
                           }
                           
                       }

                    }
                    catch(Exception ex)
                    {
                        DBAccessLayer.LogSysInfo("AuthorizedStrategy", ex.ToString());
                        continue;
                    }

                    
                    
                }

                // 判断交易执行规则

                Dictionary<String, List<AuthorizedOrder>> OrderMap = AuthorizedTradesList.GetOrderList();

                foreach(KeyValuePair<String,List<AuthorizedOrder>> pair in OrderMap)
                {
                    foreach(AuthorizedOrder order in pair.Value)
                    {

                        if (order.Status == 1 || order.Status == 0) continue;

                        if(order.Status == 3)
                        {
                            AuthorizedTradesList.CompleteSpecificTrade(order.belongStrategy, order.cSecurityCode, 0);
                            continue;
                        }

                        bool tradeMark = false;
                        double currentPrice = 0;
                        currentPrice = AuthorizedMarket.GetMarketInfo(order.cSecurityCode.Trim());

                        //开始执行交易规则判断
                        //目前剩下running = 2 , conpleted = 4

                        //强制下单的情况
                        if(order.Status == 4 && order.dOrderPrice != 0 && currentPrice != 0)
                        {
                            tradeMark = true;
                            order.dDealPrice = currentPrice;
                        }

                        try
                        {
                            if (order.Status == 2)
                            {
                                if (order.LossValue == 0 && order.SurplusValue == 0)
                                {
                                    if (order.LimitedPrice == "N")
                                    {
                                        //止损止盈为0，且限价为N
                                        tradeMark = true;
                                        if (order.cTradeDirection == "0")
                                            order.dDealPrice = currentPrice * 1.02;
                                        else
                                            order.dDealPrice = currentPrice * 0.98;
                                    }
                                    else
                                    {
                                        //止损止盈为0，且限价为Y
                                        tradeMark = true;
                                        order.dDealPrice = currentPrice;
                                    }
                                }
                                else
                                {
                                    //止损，止盈价不全为0
                                    if (order.LossValue != 0)
                                    {
                                        if (currentPrice <= order.LossValue)
                                        {
                                            tradeMark = true;
                                        }
                                    }

                                    if (order.SurplusValue != 0)
                                    {
                                        if (currentPrice >= order.SurplusValue)
                                        {
                                            tradeMark = true;
                                        }
                                    }

                                    if (tradeMark == true)
                                    {
                                        if (order.LimitedPrice == "N")
                                        {
                                            if (order.cTradeDirection == "0")
                                                order.dDealPrice = currentPrice * 1.02;
                                            else
                                                order.dDealPrice = currentPrice * 0.98;
                                        }
                                        else
                                        {
                                            order.dDealPrice = currentPrice;
                                        }
                                    }
                                }
                            }

                        }
                        catch(Exception ex)
                        {
                            GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
                        }
                        if (currentPrice == 0 && order.LimitedPrice == "Y")
                        {
                            tradeMark = false;
                        }
                        


                        //结束执行交易规则判断

                        if(tradeMark)
                        {
                            MakeOrder o = new MakeOrder()
                            {
                                belongStrategy = order.belongStrategy,
                                cSecurityCode = order.cSecurityCode,
                                cSecurityType = order.cSecurityType,
                                cTradeDirection = order.cTradeDirection,
                                dOrderPrice = currentPrice,
                                exchangeId = order.exchangeId,
                                nSecurityAmount = order.nSecurityAmount,
                                offsetflag = order.offsetflag,
                                OrderRef = order.OrderRef,
                                User = order.User
                            };

                            List<MakeOrder> orders = new List<MakeOrder>();
                            orders.Add(o);

                            queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue((object)orders);

                            Dictionary<String, String> paras = new Dictionary<string, string>();

                            paras.Add("strno", o.belongStrategy.Trim());
                            paras.Add("code", o.cSecurityCode.Trim());
                            paras.Add("dealprice", currentPrice.ToString());
                            paras.Add("status", order.Status.ToString());

                            ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));

                            AuthorizedTradesList.CompleteSpecificTrade(o.belongStrategy, o.cSecurityCode, currentPrice); 
                        }

                    }
                }

            }
        }

        private static void ThreadProc_PushInfo()
        {
            log.LogEvent("授权交易线程B启动！");

            DateTime dt = DateTime.Now;

            while (true)
            {
                Thread.Sleep(1);

                while (queue_authorized_query.GetQueueNumber() > 0)
                {
                    String name = (String)queue_authorized_query.GetQueue().Dequeue();

                    List<String> strategies = AuthorizedTradesList.GetUserStrategies(name);

                    if (strategies.Count == 0) continue;

                    AuthorizedStrategyMonitor.Instance.UpdateStrategiesList(name, strategies);
                }

                while (queue_authorized_tradeview.GetQueueNumber() > 0)
                {
                    string str = Convert.ToString(queue_authorized_tradeview.GetQueue().Dequeue());

                    string type = str.Split('|')[0].Trim();
                    string name = str.Split('|')[1].Trim();
                    string strategy = str.Split('|')[2].Trim();


                    List<AuthorizedOrder> orders = new List<AuthorizedOrder>();
                    Dictionary<String, List<AuthorizedOrder>> dics = new Dictionary<string, List<AuthorizedOrder>>();
                    String typeView = "0";
                    switch (type)
                    {
                        case "A+":
                            //显示全部交易
                            dics = AuthorizedTradesList.GetAllOrders(name);
                            typeView = "0";
                            break;
                        case "O+":
                            //显示已下单交易
                            dics = AuthorizedTradesList.GetCompletedOrders(name);
                            typeView = "1";
                            break;
                        case "I+":
                            //显示未下单交易
                            dics = AuthorizedTradesList.GetRunningOrders(name);
                            typeView = "2";
                            break;
                    }

                    if (dics != null && dics.Keys.Contains(strategy))
                    {
                        orders = dics[strategy];
                    }
                    AuthorizedStrategyMonitor.Instance.UpdateStrategyOrders(name, strategy, orders);

                    AuthorizedTradesList.ChangeUserView(name, strategy, typeView);
                }

                List<String> Users = AuthorizedTradesList.GetUserList();

                foreach (String User in Users)
                {
                    Dictionary<String, AuthorizedOrderStatus> Codes = AuthorizedTradesList.UpdateViewList(User);

                    if (Codes == null) continue;

                    Dictionary<String, String> PriceMap = new Dictionary<string, string>();
                    Dictionary<String, String> StatusMap = new Dictionary<string, string>();

                    foreach (KeyValuePair<String, AuthorizedOrderStatus> Code in Codes)
                    {
                        float price = 0;
                        if (Code.Value.Status == "0" || Code.Value.Status == "1")
                            price = Convert.ToSingle(AuthorizedMarket.GetMarketInfo(Code.Key) / 10000.0);
                        else if (Code.Value.Status == "2")
                            price = Convert.ToSingle(AuthorizedMarket.GetMarketInfo(Code.Key) / 10000.0);
                        else if (Code.Value.Status == "4")
                            price = Convert.ToSingle(Code.Value.DealPrice);

                        PriceMap.Add(Code.Key, price.ToString());
                        StatusMap.Add(Code.Key, Code.Value.StatusDesc);
                    }

                    AuthorizedStrategyMonitor.Instance.UpdateCurrentPrice(User, PriceMap);
                    AuthorizedStrategyMonitor.Instance.UpdateCurrentStatus(User, StatusMap);

                    int runningNum = 0;
                    int completedNum = 0;

                    AuthorizedTradesList.CalculateOrderNum(User, out runningNum, out completedNum);
                    AuthorizedStrategyMonitor.Instance.UpdateTradeNum(User, runningNum.ToString(), completedNum.ToString(), (runningNum + completedNum).ToString());

                    float earning = 0;
                    float marketvalue = 0;

                    AuthorizedTradesList.GetStrategyAccount(User,out earning,out marketvalue);
                    AuthorizedStrategyMonitor.Instance.UpdateAccountInfo(User, earning.ToString(), marketvalue.ToString());

                }

                if (DateTime.Now.Second != dt.Second)
                {
                    KeyValuePair<string, object> message1 = new KeyValuePair<string, object>("AuthorizedStrategy_ThreadB", (object)true);
                    queue_system_status.GetQueue().Enqueue((object)message1);

                    dt = DateTime.Now;
                }


            }
        }
    }


    public class AuthorizedTradesList
    {

        #region 变量
        private static String ModuleName = "AuthorizedStrategy";

        /// <summary>
        /// 未完成交易列表，如果交易下单或者停止，则转存在CompletedAuthorizedOrderMap
        /// suspend / running 
        /// TKEY : 授权策略号
        /// TValue : 策略对应交易列表
        /// </summary>
        private static Dictionary<String, List<AuthorizedOrder>> AuthorizedOrderMap = new Dictionary<string, List<AuthorizedOrder>>();

        /// <summary>
        /// 已经下单或完成的交易列表
        /// completed / stop 
        /// TKEY：策略编号
        /// TValue : 策略对应已下单或已停止交易列表
        /// </summary>
        private static Dictionary<String, List<AuthorizedOrder>> CompletedAuthorizedOrderMap = new Dictionary<string, List<AuthorizedOrder>>();

        /// <summary>
        /// 授权策略/用户映射表
        /// TKey : 策略号码
        /// TValue : 用户名
        /// </summary>
        private static Dictionary<String, String> AuthorizedUserMap = new Dictionary<string, string>();

        /// <summary>
        /// 模块监听行情列表
        /// </summary>
        private static Dictionary<String, int> ListeningCode = new Dictionary<string, int>();

        /// <summary>
        /// 保存用户正在浏览的策略
        /// Key : 用户名
        /// Value : 正在浏览的策略|浏览方式 （0： 全部，1：运行/暂停，2： 已下单/停止）
        /// </summary>
        private static Dictionary<String, String> ViewStrategyBuffer = new Dictionary<string, string>();
        #endregion

        #region 修改函数
        /// <summary>
        /// 注册新的策略
        /// </summary>
        /// <param name="orders"></param>
        public static void SubscribeNewAuhorizedStrategy(List<AuthorizedOrder> orders)
        {
            if (orders == null || orders.Count == 0)
            {
                GlobalErrorLog.LogInstance.LogEvent("AuthorizedTradeList -> SubscribeNewAuthorizedStrategy -> 传入参数为空！");
                return;
            }

            String User = orders[0].User.Trim();
            String strategyNo = User + DateTime.Now.ToString("yyyyMMddhhmmss");

            for (int i = 0; i < orders.Count; i++)
            {
                AuthorizedOrder order = orders[i];

                order.belongStrategy = strategyNo;

                lock (ListeningCode)
                {
                    if (ListeningCode.Keys.Contains(order.cSecurityCode.Trim()))
                    {
                        ListeningCode[order.cSecurityCode.Trim()] += 1;
                    }
                    else
                    {
                        ListeningCode.Add(order.cSecurityCode.Trim(), 1);

                        MapMarketStratgy.SetMapMS(ModuleName, ListeningCode.Keys.ToList());
                    }
                }
            }


            lock (AuthorizedOrderMap)
            {
                AuthorizedOrderMap.Add(strategyNo, orders);
                CompletedAuthorizedOrderMap.Add(strategyNo, new List<AuthorizedOrder>());
            }


            lock (AuthorizedUserMap)
            {
                AuthorizedUserMap.Add(strategyNo, User);
            }

            DBAccessLayer.InsertAuthorizedStrategy(strategyNo, User, String.Empty, orders);
            FileOperations file = new FileOperations();
            file.CreateFile(orders, strategyNo);
            queue_authorized_tradeview.EnQueue((object)("A+" + "|" + User + "|" + strategyNo));
        }

        /// <summary>
        /// 删除策略
        /// </summary>
        /// <param name="strategyNo"></param>
        public static void DeleteAuthorizedStrategy(String strategyNo)
        {
            strategyNo = strategyNo.Trim();

            if (strategyNo.Trim() == String.Empty)
            {
                GlobalErrorLog.LogInstance.LogEvent("AuthorizedTradesList -> DeleteAuthorizedStrategy -> 删除策略号为空！");
                return;
            }

            if (AuthorizedOrderMap.Keys.Contains(strategyNo))
            {
                if (AuthorizedOrderMap[strategyNo].Count > 0)
                {
                    GlobalErrorLog.LogInstance.LogEvent("AuthorizedTradesList -> DeleteAuthorizedStrategy -> 删除策略失败，AuthorizedOrderMap 仍存在交易！");
                    return;
                }
                lock (AuthorizedOrderMap)
                {
                    AuthorizedOrderMap.Remove(strategyNo);
                }
                lock (CompletedAuthorizedOrderMap)
                {
                    CompletedAuthorizedOrderMap.Remove(strategyNo);
                }
            }

            if (AuthorizedUserMap.Keys.Contains(strategyNo))
            {
                lock (AuthorizedUserMap)
                {
                    AuthorizedUserMap.Remove(strategyNo);
                }
            }

            DBAccessLayer.DeleteAuthorizedStrategy(strategyNo);


        }

        /// <summary>
        /// 发单后更新策略交易列表
        /// </summary>
        /// <param name="strategyNo"></param>
        /// <param name="Code"></param>
        public static void CompleteSpecificTrade(String strategyNo, String Code, double DealPrice)
        {
            Code = Code.Trim();
            strategyNo = strategyNo.Trim();
            String user = String.Empty;

            if (AuthorizedUserMap.Keys.Contains(strategyNo))
            {
                user = AuthorizedUserMap[strategyNo];
            }

            if (AuthorizedOrderMap.Keys.Contains(strategyNo))
            {
                List<AuthorizedOrder> orders = AuthorizedOrderMap[strategyNo];

                var order = (from item in orders where item.cSecurityCode == Code select item);

                if (order == null)
                {
                    GlobalErrorLog.LogInstance.LogEvent("AuthorizedTradeList -> SubscribeNewAuthorizedStrategy -> 未找到交易，策略：" + strategyNo + "代码：" + Code);
                }

                AuthorizedOrder o = order.ToList()[0];

                lock (AuthorizedOrderMap)
                {
                    orders.Remove(o);
                }

                if (CompletedAuthorizedOrderMap.Keys.Contains(strategyNo))
                {
                    lock (CompletedAuthorizedOrderMap)
                    {
                        o.dDealPrice = DealPrice;
                        CompletedAuthorizedOrderMap[strategyNo].Add(o);
                    }
                }

                lock (ListeningCode)
                {
                    if (ListeningCode.Keys.Contains(Code))
                    {
                        ListeningCode[Code]--;
                        if (ListeningCode[Code] == 0)
                        {
                            ListeningCode.Remove(Code);
                            AuthorizedStrategyMonitor.Instance.CompleteTrade(user, strategyNo, Code);
                            FileOperations file = new FileOperations();
                        }
                    }
                }

                if (orders.Count == 0)
                {
                    DeleteAuthorizedStrategy(strategyNo);
                    AuthorizedStrategyMonitor.Instance.DeleteStrategyView(user, strategyNo);
                }
            }
            else
            {
                GlobalErrorLog.LogInstance.LogEvent("AuthorizedTradeList -> SubscribeNewAuthorizedStrategy -> 未找到策略" + strategyNo);
                return;
            }

        }

        /// <summary>
        /// 启动策略交易
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="Orders"></param>
        public static void StartStrategyTrade(String strategy)
        {
            if(AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].Status != (int)AuthorizedTradeStatus.Stop || AOs[i].Status != (int)AuthorizedTradeStatus.Dealed)
                    {
                        AOs[i].Status = (int)AuthorizedTradeStatus.Running;

                        Dictionary<String, String> paras = new Dictionary<string, string>();
      
                        paras.Add("strno", strategy.Trim());
                        paras.Add("code", AOs[i].cSecurityCode.Trim());
                        paras.Add("dealprice", "0");
                        paras.Add("status", AOs[i].Status.ToString());

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                    }
                }
            }
        }

        /// <summary>
        /// 启动单支交易
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="code"></param>
        public static void StartSingleTrade(String strategy,String code)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].cSecurityCode == code)
                    {
                        if (AOs[i].Status != (int)AuthorizedTradeStatus.Stop || AOs[i].Status != (int)AuthorizedTradeStatus.Dealed)
                        {
                            AOs[i].Status = (int)AuthorizedTradeStatus.Running;

                            Dictionary<String, String> paras = new Dictionary<string, string>();

                            paras.Add("strno", strategy.Trim());
                            paras.Add("code", AOs[i].cSecurityCode.Trim());
                            paras.Add("dealprice", "0");
                            paras.Add("status", AOs[i].Status.ToString());

                            ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 暂停策略交易
        /// </summary>
        /// <param name="strategy"></param>
        public static void SuspendStrategyTrade(String strategy)
        {
            if(AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].Status == (int)AuthorizedTradeStatus.Running)
                    {
                        AOs[i].Status = (int)AuthorizedTradeStatus.Pause;

                        Dictionary<String, String> paras = new Dictionary<string, string>();

                        paras.Add("strno", strategy.Trim());
                        paras.Add("code", AOs[i].cSecurityCode.Trim());
                        paras.Add("dealprice", "0");
                        paras.Add("status", AOs[i].Status.ToString());

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                    }
                }
            }
        }

        /// <summary>
        /// 暂停单支交易
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="code"></param>
        public static void SuspendSingleTrade(String strategy, String code)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].cSecurityCode == code && AOs[i].Status == (int)AuthorizedTradeStatus.Running)
                    {
                        AOs[i].Status = (int)AuthorizedTradeStatus.Pause;

                        Dictionary<String, String> paras = new Dictionary<string, string>();

                        paras.Add("strno", strategy.Trim());
                        paras.Add("code", AOs[i].cSecurityCode.Trim());
                        paras.Add("dealprice", "0");
                        paras.Add("status", AOs[i].Status.ToString());

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                    }
                }
            }
        }

        /// <summary>
        /// 停止策略交易
        /// </summary>
        /// <param name="strategy"></param>
        public static void StopStrategyTrade(String strategy)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].Status == (int)AuthorizedTradeStatus.Pause || AOs[i].Status == (int)AuthorizedTradeStatus.Running || AOs[i].Status == (int)AuthorizedTradeStatus.Init)
                    {
                        AOs[i].Status = (int)AuthorizedTradeStatus.Stop;

                        Dictionary<String, String> paras = new Dictionary<string, string>();

                        paras.Add("strno", strategy.Trim());
                        paras.Add("code", AOs[i].cSecurityCode.Trim());
                        paras.Add("dealprice", "0");
                        paras.Add("status", AOs[i].Status.ToString());

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                    }
                }
            }
        }

        /// <summary>
        /// 停止单支交易
        /// </summary>
        /// <param name="strategy"></param>
        public static void StopSingleTrade(String strategy,String code)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].cSecurityCode == code)
                    {
                        if (AOs[i].Status == (int)AuthorizedTradeStatus.Pause || AOs[i].Status == (int)AuthorizedTradeStatus.Running || AOs[i].Status == (int)AuthorizedTradeStatus.Init)
                        {
                            AOs[i].Status = (int)AuthorizedTradeStatus.Stop;

                            Dictionary<String, String> paras = new Dictionary<string, string>();

                            paras.Add("strno", strategy.Trim());
                            paras.Add("code", AOs[i].cSecurityCode.Trim());
                            paras.Add("dealprice", "0");
                            paras.Add("status", AOs[i].Status.ToString());

                            ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.UpdateAuthorizedTrade), (object)(paras));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 强制策略交易
        /// </summary>
        /// <param name="strategy"></param>
        public static void ForceStrategyTrade(String strategy)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];
                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].Status == (int)AuthorizedTradeStatus.Pause || AOs[i].Status == (int)AuthorizedTradeStatus.Running)
                    {
                        AOs[i].Status = (int)AuthorizedTradeStatus.Dealed;
                    }
                }
            }
        }

        /// <summary>
        /// 强制单支交易
        /// </summary>
        /// <param name="strategy"></param>
        public static void ForceSingleTrade(String strategy,String code)
        {
            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                List<AuthorizedOrder> AOs = AuthorizedOrderMap[strategy];

                for (int i = 0; i < AOs.Count; i++)
                {
                    if (AOs[i].cSecurityCode == code)
                    {
                        if (AOs[i].Status == (int)AuthorizedTradeStatus.Pause || AOs[i].Status == (int)AuthorizedTradeStatus.Running)
                        {
                            AOs[i].Status = (int)AuthorizedTradeStatus.Dealed;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 装载数据库中未完成策略
        /// </summary>
        public static void LoadPauseStrategy()
        {
            Dictionary<String, List<AuthorizedOrder>> orders = DBAccessLayer.LoadPauseStrategy();

            if (orders.Count == 0) return;

            foreach (KeyValuePair<String, List<AuthorizedOrder>> order in orders)
            {
                if(order.Value.Count == 0) continue;

                string user = order.Value[0].User;

                AuthorizedUserMap.Add(order.Key, user);

                AuthorizedOrderMap.Add(order.Key, new List<AuthorizedOrder>());
                CompletedAuthorizedOrderMap.Add(order.Key, new List<AuthorizedOrder>());

                foreach(AuthorizedOrder o in order.Value)
                {
                    if((o.Status == (int)AuthorizedTradeStatus.Pause)||(o.Status == (int)AuthorizedTradeStatus.Init)||(o.Status == (int)AuthorizedTradeStatus.Running))
                    {
                        AuthorizedOrderMap[order.Key].Add(o);
                    }
                    else
                    {
                        CompletedAuthorizedOrderMap[order.Key].Add(o);
                    }

                    if(!ListeningCode.Keys.Contains(o.cSecurityCode.Trim()))
                    {
                        ListeningCode.Add(o.cSecurityCode.Trim(), 0);
                    }

                    ListeningCode[o.cSecurityCode.Trim()] += 1;
                }
            }

            MapMarketStratgy.SetMapMS(ModuleName, ListeningCode.Keys.ToList());
        }

        /// <summary>
        /// 数据库日切
        /// </summary>
        public static void DailyDBExchange()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DBAccessLayer.DailyDBExchange));
        }
        #endregion

        #region 查询函数
        /// <summary>
        /// Clone 交易列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<String, List<AuthorizedOrder>> GetOrderList()
        {
            Dictionary<String, List<AuthorizedOrder>> MapClone = new Dictionary<string, List<AuthorizedOrder>>();
            foreach (KeyValuePair<String, List<AuthorizedOrder>> item in AuthorizedOrderMap)
            {
                List<AuthorizedOrder> orders = DuplexOrders(item.Value);
                MapClone.Add(item.Key, orders);
            }

            return MapClone;
        }

        /// <summary>
        /// 复制策略交易列表
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public static List<AuthorizedOrder> DuplexOrders(List<AuthorizedOrder> orders)
        {
            List<AuthorizedOrder> Dup_orders = new List<AuthorizedOrder>();

            foreach (AuthorizedOrder order in orders)
            {
                AuthorizedOrder o = new AuthorizedOrder()
                {
                    belongStrategy = order.belongStrategy,
                    cSecurityCode = order.cSecurityCode,
                    cSecurityType = order.cSecurityType,
                    User = order.User,
                    OrderRef = order.OrderRef,
                    offsetflag = order.offsetflag,
                    nSecurityAmount = order.nSecurityAmount,
                    exchangeId = order.exchangeId,
                    dOrderPrice = order.dOrderPrice,
                    cTradeDirection = order.cTradeDirection,
                    LimitedPrice = order.LimitedPrice,
                    LossValue = order.LossValue,
                    SurplusValue = order.SurplusValue,
                    Status = order.Status
                };

                Dup_orders.Add(o);
            }

            return Dup_orders;
        }

        /// <summary>
        /// 获取订阅列表
        /// </summary>
        /// <returns></returns>
        public static List<String> GetListeningCodeList()
        {
            List<String> list = new List<string>();
            foreach (String code in ListeningCode.Keys)
            {
                list.Add(code);
            }

            return list;
        }

        /// <summary>
        /// 计算用户当前策略运行/结束 交易数量
        /// </summary>
        /// <param name="User"></param>
        /// <param name="runningNum">运行数量</param>
        /// <param name="completedNum">结束数量</param>
        public static void CalculateOrderNum(String User, out int runningNum, out int completedNum)
        {
            User = User.Trim();

            runningNum = 0;
            completedNum = 0;

            String str = GetUserViewStrategy(User);

            if (str == string.Empty) return;

            String strategy = str.Split('|')[0];
            String type = str.Split('|')[1];

            if (strategy == String.Empty) return;

            if (AuthorizedOrderMap.Keys.Contains(strategy)) { runningNum = AuthorizedOrderMap[strategy].Count; }
            if (CompletedAuthorizedOrderMap.Keys.Contains(strategy)) { completedNum = CompletedAuthorizedOrderMap[strategy].Count; }
        }

        /// <summary>
        /// 获取用户当前查看策略所有未完成交易
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static Dictionary<String, AuthorizedOrderStatus> UpdateViewList(String User)
        {
            User = User.Trim();


            String str = GetUserViewStrategy(User);

            if (str == string.Empty) return new Dictionary<string, AuthorizedOrderStatus>();

            String strategy = str.Split('|')[0];
            String type = str.Split('|')[1];

            List<String> Codes = new List<string>();

            if (strategy == String.Empty) return null;

            Dictionary<String, AuthorizedOrderStatus> RunningStatus = new Dictionary<string, AuthorizedOrderStatus>();
            Dictionary<String, AuthorizedOrderStatus> CompletedStatus = new Dictionary<string, AuthorizedOrderStatus>();
            Dictionary<String, AuthorizedOrderStatus> AllStatus = new Dictionary<string, AuthorizedOrderStatus>();

            if (AuthorizedOrderMap.Keys.Contains(strategy))
            {
                foreach(AuthorizedOrder order in AuthorizedOrderMap[strategy])
                {
                    string status = AuthorizedStatus.GetStatus(order.Status);
                    AuthorizedOrderStatus stat = new AuthorizedOrderStatus()
                    {
                        Code = order.cSecurityCode,
                        DealPrice = "0",
                        Status = order.Status.ToString(),
                        StatusDesc = status,
                        Strategy = strategy
                    };

                    RunningStatus.Add(order.cSecurityCode, stat);
                    AllStatus.Add(order.cSecurityCode, stat);
                }
            }

            if (type == "2") return RunningStatus;
           
            if(CompletedAuthorizedOrderMap.Keys.Contains(strategy))
            {
                foreach(AuthorizedOrder order in CompletedAuthorizedOrderMap[strategy])
                {
                    string status = AuthorizedStatus.GetStatus(order.Status);
                    AuthorizedOrderStatus stat = new AuthorizedOrderStatus()
                    {
                        Code = order.cSecurityCode,
                        DealPrice = order.dDealPrice.ToString(),
                        Status = order.Status.ToString(),
                        StatusDesc = status,
                        Strategy = strategy
                    };
                    CompletedStatus.Add(order.cSecurityCode, stat);
                    AllStatus.Add(order.cSecurityCode, stat);
                }
            }

            if (type == "1") return CompletedStatus;

            if (type == "0") return AllStatus;

            return new Dictionary<String, AuthorizedOrderStatus>();

        }

        /// <summary>
        /// 获取全部使用策略用户
        /// </summary>
        /// <returns></returns>
        public static List<String> GetUserList()
        {
            List<String> users = new List<string>();

            foreach (KeyValuePair<String, String> pair in AuthorizedUserMap)
            {
                if (!users.Contains(pair.Value.Trim()))
                {
                    users.Add(pair.Value.Trim());
                }
            }

            return users;
        }

        /// <summary>
        /// 获取用户使用策略列表
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static List<String> GetUserStrategies (String User)
        {
            List<String> Strategies = new List<string>();
            User = User.Trim();


            foreach (KeyValuePair<String, String> pair in AuthorizedUserMap)
            {
                if (pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            return Strategies;
        }

        /// <summary>
        /// 获取用户所有未完成交易
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static Dictionary<String, List<AuthorizedOrder>> GetRunningOrders (String User)
        {
            User = User.Trim();

            List<String> Strategies = new List<string>();

            Dictionary<String, List<AuthorizedOrder>> ListOrders = new Dictionary<string, List<AuthorizedOrder>>();

            foreach (KeyValuePair<String, String> pair in AuthorizedUserMap)
            {
                if (pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            if (Strategies.Count == 0) return null;

            foreach (KeyValuePair<String, List<AuthorizedOrder>> pair in AuthorizedOrderMap)
            {
                if (Strategies.Contains(pair.Key))
                {
                    ListOrders.Add(pair.Key, pair.Value);
                }
            }

            return ListOrders;
        }

        /// <summary>
        /// 获取用户已经完成的交易
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static Dictionary<String,List<AuthorizedOrder>> GetCompletedOrders(String User)
        {
            User = User.Trim();

            List<String> Strategies = new List<string>();

            Dictionary<String, List<AuthorizedOrder>> ListOrders = new Dictionary<string, List<AuthorizedOrder>>();

            foreach (KeyValuePair<String, String> pair in AuthorizedUserMap)
            {
                if (pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            if (Strategies.Count == 0) return null;

            foreach (KeyValuePair<String, List<AuthorizedOrder>> pair in CompletedAuthorizedOrderMap)
            {
                if (Strategies.Contains(pair.Key))
                {
                    ListOrders.Add(pair.Key, pair.Value);
                }
            }

            return ListOrders;
        }

        /// <summary>
        /// 获取用户所有交易
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static Dictionary<String,List<AuthorizedOrder>> GetAllOrders(String User)
        {
            User = User.Trim();

            List<String> Strategies = new List<string>();

            Dictionary<String, List<AuthorizedOrder>> ListOrders = new Dictionary<string, List<AuthorizedOrder>>();

            foreach (KeyValuePair<String, String> pair in AuthorizedUserMap)
            {
                if (pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            if (Strategies.Count == 0) return null;

            foreach(String str in Strategies)
            {
                ListOrders.Add(str, new List<AuthorizedOrder>());
                if(AuthorizedOrderMap.Keys.Contains(str))
                {
                    foreach(AuthorizedOrder order in AuthorizedOrderMap[str])
                    {
                        ListOrders[str].Add(order);
                    }
                }
                if(CompletedAuthorizedOrderMap.Keys.Contains(str))
                {
                    foreach(AuthorizedOrder order in CompletedAuthorizedOrderMap[str])
                    {
                        ListOrders[str].Add(order);
                    }
                }
            }

            return ListOrders;
        }

        /// <summary>
        /// 修改用户当前策略缓存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static void ChangeUserView(String name,String strategy,String type)
        {
            if(ViewStrategyBuffer.Keys.Contains(name))
            {
                ViewStrategyBuffer[name] = strategy + "|" + type;
            }
            else
            {
                ViewStrategyBuffer.Add(name, strategy + "|" + type);
            }
        }

        /// <summary>
        /// 返回用户当前正在浏览的策略
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetUserViewStrategy(String name)
        {
            if(ViewStrategyBuffer.Keys.Contains(name))
            {
                return ViewStrategyBuffer[name];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取策略实时卖出，买入盈亏
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="sellaccount"></param>
        /// <param name="buyaccount"></param>
        public static void GetStrategyAccount(String strategy, out float earning, out float marketvalue)
        {
            earning = marketvalue = 0;
            try
            {

                List<AuthorizedOrder> runningOrder = new List<AuthorizedOrder>();
                if(AuthorizedOrderMap.Keys.Contains(strategy))
                    runningOrder = AuthorizedOrderMap[strategy];

                List<AuthorizedOrder> completedOrder = new List<AuthorizedOrder>();
                if(CompletedAuthorizedOrderMap.Keys.Contains(strategy))
                    completedOrder = CompletedAuthorizedOrderMap[strategy];

                foreach (AuthorizedOrder order in runningOrder)
                {
                    if (order.cTradeDirection == "0" && order.cSecurityType.ToUpper() == "S")
                    {
                        //买入
                        uint currentprice = AuthorizedMarket.GetMarketInfo(order.cSecurityCode);
                        if(currentprice != 0)
                        {
                            marketvalue += Convert.ToSingle(currentprice) / 1000 * order.nSecurityAmount; 
                        }
                    }

                    if(order.cTradeDirection == "1" && order.cSecurityType.ToUpper() == "S")
                    {
                        uint currentprice = AuthorizedMarket.GetMarketInfo(order.cSecurityCode);
                        if(currentprice != 0)
                        {
                            earning += (Convert.ToSingle(currentprice) / 1000 - order.cost) * order.nSecurityAmount; 
                        }
                    }
                }

                foreach(AuthorizedOrder order in completedOrder)
                {
                    if(order.cTradeDirection == "0" && order.cSecurityType.ToUpper() == "S")
                    {
                        if(order.dDealPrice != 0)
                        {
                            marketvalue += Convert.ToSingle(order.dDealPrice * order.nSecurityAmount);
                        }
                    }

                    if(order.cTradeDirection == "1" && order.cSecurityType.ToUpper() == "S")
                    {
                        if(order.dDealPrice != 0)
                        {
                            earning += Convert.ToSingle(order.dDealPrice - order.cost) * order.nSecurityAmount;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                GlobalErrorLog.LogInstance.LogEvent(e.ToString());
            }
        }
        #endregion
    }

    public class AuthorizedMarket
    {

        /// <summary>
        /// 本地行情列表
        /// </summary>
        private static Dictionary<String, MarketData> MarketList = new Dictionary<string, MarketData>();

        /// <summary>
        /// 更新行情列表
        /// </summary>
        /// <param name="info"></param>
        public static void UpdateMarketList(MarketData info )
        {
            lock (MarketList)
            {
                if (MarketList.Keys.Contains(info.Code.Trim()))
                {
                    MarketList.Remove(info.Code.Trim());
                }

                MarketList.Add(info.Code.Trim(), info);
            }
        }

        /// <summary>
        /// 获取最新行情价
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public static uint GetMarketInfo(String Code)
        {
            Code = Code.Trim();
            if(!MarketList.Keys.Contains(Code))
            {
                return 0;
            }
            else
            {
                try
                {
                    return MarketList[Code].Match;
                }
                catch
                {
                    return 0;
                }
            }
        } 

        
    }

    public class AuthorizedStatus
    {
        /// <summary>
        /// 获取交易状态说明
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetStatus(int status)
        {
            string stat = string.Empty;
            switch(status)
            {
                case 0:
                    stat = "暂停";
                    break;
                case 1:
                    stat = "暂停";
                    break;
                case 2:
                    stat = "运行中";
                    break;
                case 3:
                    stat = "停止";
                    break;
                case 4:
                    stat = "已下单";
                    break;
                default:
                    stat = "未知" + status;
                    break;
            }
            return stat;
        }
    }

    public class FileOperations
    {
        /// <summary>
        /// 创建新输出文档
        /// </summary>
        /// <param name="orders">交易信息</param>
        /// <param name="strategyNo">策略名称</param>
        public void CreateFile(List<AuthorizedOrder> orders, String strategyNo )
        {
            String path = CONFIG.AUTHORIZED_BASE_URL + strategyNo;
            String value = String.Empty;

            //a. 用户| 交易所| 代码| 数量| 下单价格|买卖|开平|证券类型|是否限价|止损价|止盈价|成本价|是否交易
            foreach(AuthorizedOrder order in orders)
            {
                value += (order.User + "|" + order.exchangeId + "|" + order.cSecurityCode + "|" + order.nSecurityAmount.ToString() + "|" + order.cTradeDirection + "|" + order.offsetflag + "|" + order.cSecurityType + "|" + order.LimitedPrice + "|" + order.LossValue.ToString() + "|" + order.SurplusValue.ToString() + "|" + order.cost.ToString() + "|" + "N" );
                value += "\r\n";
            }

            if (value == string.Empty) return;

            FileInfo file = new FileInfo(strategyNo);
            try
            {
                if (!file.Exists)
                {

                    FileStream stream = File.Create(path);
                    stream.Flush();
                    stream.Close();
                }

                StreamWriter writer = new StreamWriter(path, false);
                writer.Write(value);
                writer.Flush();
                writer.Close();
            }
            catch(Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("创建文件失败，文件名：" + strategyNo + "\r\n" + ex.ToString());
            }
        }

        /// <summary>
        /// 创建新输出文档
        /// </summary>
        /// <param name="orders">交易信息</param>
        /// <param name="strategyNo">策略名称</param>
        public void RefreshFile(List<AuthorizedOrder> running_orders,List<AuthorizedOrder> completed_orders, String strategyNo)
        {
            String path = System.Environment.CurrentDirectory + strategyNo;
            String value = String.Empty;

            //a. 用户| 交易所| 代码| 数量| 下单价格|买卖|开平|证券类型|是否限价|止损价|止盈价|成本价|是否交易
            foreach (AuthorizedOrder order in running_orders)
            {
                value += (order.User + "|" + order.exchangeId + "|" + order.cSecurityCode + "|" + order.nSecurityAmount.ToString() + "|" + order.dDealPrice.ToString() + "|"  + order.cTradeDirection + "|" + order.offsetflag + "|" + order.cSecurityType + "|" + order.LimitedPrice + "|" + order.LossValue.ToString() + "|" + order.SurplusValue.ToString() + "|" + order.cost.ToString() + "|" + "N");
                value += "\r\n";
            }

            foreach(AuthorizedOrder order in completed_orders)
            {
                value += (order.User + "|" + order.exchangeId + "|" + order.cSecurityCode + "|" + order.nSecurityAmount.ToString() + "|" + order.cTradeDirection + "|" + order.offsetflag + "|" + order.cSecurityType + "|" + order.LimitedPrice + "|" + order.LossValue.ToString() + "|" + order.SurplusValue.ToString() + "|" + order.cost.ToString() + "|" + "N");
                value += "\r\n";
            }

            if (value == string.Empty) return;

            FileInfo file = new FileInfo(strategyNo);

            try
            {
                if (!file.Exists)
                {

                    FileStream stream = File.Create(path);
                    stream.Flush();
                    stream.Close();
                }

                StreamWriter writer = new StreamWriter(path, false);
                writer.Write(value);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("更新文件失败，文件名：" + strategyNo + "\r\n" + ex.ToString());
            }
        }

    }
}