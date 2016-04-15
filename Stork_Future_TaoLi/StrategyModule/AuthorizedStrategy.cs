using marketinfosys;
using Stork_Future_TaoLi.Hubs;
using Stork_Future_TaoLi.Modulars;
using Stork_Future_TaoLi.Queues;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi.StrategyModule
{
    public class AuthorizedStrategy
    {

        private static AuthorizedStrategy instance = new AuthorizedStrategy();


        private static LogWirter log = new LogWirter();

        public static void RUN()
        {
            log.EventSourceName = "授权交易模块";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 66001;

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

                while(queue_authorized_market.GetQueueNumber() > 0)
                {
                    MarketData info = (MarketData)queue_authorized_market.GetQueue().Dequeue();
                    AuthorizedMarket.UpdateMarketList(info);
                }

               
                // 判断是否添加新策略
                if(queue_authorized_trade.GetQueueNumber() > 0)
                {
                    List<AuthorizedOrder> orders = null;

                    try
                    {
                        orders = (List<AuthorizedOrder>)queue_authorized_trade.GetQueue().Dequeue();
                    }
                    catch(Exception ex)
                    {
                        DBAccessLayer.LogSysInfo("AuthorizedStrategy", ex.ToString());
                        continue;
                    }

                    AuthorizedTradesList.SubscribeNewAuhorizedStrategy(orders);
                    
                }

                // 判断交易执行规则

                Dictionary<String, List<AuthorizedOrder>> OrderMap = AuthorizedTradesList.GetOrderList();

                foreach(KeyValuePair<String,List<AuthorizedOrder>> pair in OrderMap)
                {
                    foreach(AuthorizedOrder order in pair.Value)
                    {
                        bool tradeMark = false;
                        double currentPrice = AuthorizedMarket.GetMarketInfo(order.cSecurityCode.Trim());
                        //开始执行交易规则判断

                        if (order.LossValue != 0 && currentPrice <= order.LossValue)
                        {
                            //低于止损价，立即发单
                            order.dOrderPrice = order.dOrderPrice * 0.98;
                            tradeMark = true;
                        }

                        if (order.SurplusValue != 0 && currentPrice >= order.SurplusValue)
                        {
                            //高于止盈价，立即发单
                            order.dOrderPrice = order.dOrderPrice * 1.02;
                            tradeMark = true;
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
                                dOrderPrice = order.dOrderPrice,
                                exchangeId = order.exchangeId,
                                nSecurityAmount = order.nSecurityAmount,
                                offsetflag = order.offsetflag,
                                OrderRef = order.OrderRef,
                                User = order.User
                            };

                            queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue((object)o);

                            AuthorizedTradesList.CompleteSpecificTrade(o.belongStrategy, o.cSecurityCode);
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

                while(queue_authorized_query.GetQueueNumber() > 0)
                {
                    String name = (String)queue_authorized_query.GetQueue().Dequeue();

                    List<String> strategies = AuthorizedTradesList.GetStrategiesForSpecificUser(name);

                    if (strategies.Count == 0) continue;

                    AuthorizedStrategyMonitor.Instance.UpdateStrategiesList(name, strategies);
                }

                while(queue_authorized_tradeview.GetQueueNumber() > 0)
                {
                    string str = Convert.ToString(queue_authorized_tradeview.GetQueue().Dequeue());

                    string name = str.Split('|')[0].Trim();
                    string strategy = str.Split('|')[1].Trim();

                    Dictionary<String, List<AuthorizedOrder>> ordersDic = AuthorizedTradesList.GetOrdersForSpecificUser(name);

                    if(ordersDic.Keys.Contains(strategy))
                    {
                        AuthorizedStrategyMonitor.Instance.UpdateStrategyOrders(name, strategy, ordersDic[strategy]);
                    }
                    else
                    {
                        continue ;
                    }

                }

                List<String> Users = AuthorizedTradesList.GetUserList();

                foreach (String User in Users)
                {
                    List<String> Codes = AuthorizedTradesList.GetCodeList(User);
                    Dictionary<String, String> PriceMap = new Dictionary<string, string>();
                    foreach (String Code in Codes)
                    {
                        float price = AuthorizedMarket.GetMarketInfo(Code) / 1000;
                        PriceMap.Add(Code, price.ToString());
                    }

                    AuthorizedStrategyMonitor.Instance.UpdateCurrentPrice(User, PriceMap);
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


        private static String ModuleName = "AuthorizedStrategy";

        /// <summary>
        /// 授权交易列表
        /// TKEY : 授权策略号
        /// TValue : 策略对应交易列表
        /// </summary>
        private static Dictionary<String, List<AuthorizedOrder>> AuthorizedOrderMap = new Dictionary<string, List<AuthorizedOrder>>();


        /// <summary>
        /// 源策略交易信息
        /// 策略创建时添加
        /// 策略全部完成后删除
        /// </summary>
        private static Dictionary<String, List<AuthorizedOrder>> SourceAuthorizedOrderMap = new Dictionary<string, List<AuthorizedOrder>>();

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

            for(int i = 0;i<orders.Count ;i++){
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
            

            lock(AuthorizedOrderMap)
            {
                AuthorizedOrderMap.Add(strategyNo, orders);

                List<AuthorizedOrder> dup_orders = DuplexOrders(orders);
                SourceAuthorizedOrderMap.Add(strategyNo, dup_orders);
            }

            
            lock(AuthorizedUserMap)
            {
                AuthorizedUserMap.Add(strategyNo, User);
            }

            queue_authorized_tradeview.EnQueue((object)("A+" + User + "|" + strategyNo));
        }

        /// <summary>
        /// 删除策略
        /// </summary>
        /// <param name="strategyNo"></param>
        public static void DeleteAuthorizedStrategy(String strategyNo)
        {
            strategyNo = strategyNo.Trim();

            if(strategyNo.Trim() == String.Empty)
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
                    SourceAuthorizedOrderMap.Remove(strategyNo);
                }
            }

            if(AuthorizedUserMap.Keys.Contains(strategyNo))
            {
                lock (AuthorizedUserMap)
                {
                    AuthorizedUserMap.Remove(strategyNo);
                }
            }

           
        }

        /// <summary>
        /// 发单后更新策略交易列表
        /// </summary>
        /// <param name="strategyNo"></param>
        /// <param name="Code"></param>
        public static void CompleteSpecificTrade(String strategyNo , String Code)
        {
            Code = Code.Trim();
            strategyNo = strategyNo.Trim();
            String user = String.Empty;

            if(AuthorizedUserMap.Keys.Contains(strategyNo))
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

                lock (AuthorizedOrderMap)
                {
                    orders.Remove((AuthorizedOrder)order);
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
                        }
                    }
                }

                if(orders.Count == 0)
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
        /// Clone 交易列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<String, List<AuthorizedOrder>> GetOrderList()
        {
            Dictionary<String, List<AuthorizedOrder>> MapClone = new Dictionary<string, List<AuthorizedOrder>>();
            foreach(KeyValuePair<String,List<AuthorizedOrder>> item in AuthorizedOrderMap)
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

            foreach(AuthorizedOrder order in orders)
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
            foreach(String code in ListeningCode.Keys)
            {
                list.Add(code);
            }

            return list;
        }

        /// <summary>
        /// 获取用户订阅交易列表
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static List<String> GetCodeList(String User)
        {
            List<String> Strategies = new List<string>();
            User = User.Trim();

            foreach(KeyValuePair<String,String> pair in AuthorizedUserMap)
            {
                if (pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            List<String> Codes = new List<string>();

            if(Strategies.Count > 0)
            {                
                foreach(String strategy in Strategies)
                {
                    List<AuthorizedOrder> orders = AuthorizedOrderMap[strategy];

                    foreach(AuthorizedOrder order in orders)
                    {
                        if(!Codes.Contains(order.cSecurityCode.Trim()))
                        {
                            Codes.Add(order.cSecurityCode.Trim());
                        }
                    }
                }
            }

            return Codes;
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
                if(!users.Contains(pair.Value.Trim()))
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
        public static List<String> GetStrategiesForSpecificUser(String User)
        {
            List<String> Strategies = new List<string>();
            User = User.Trim();


            foreach(KeyValuePair<String,String> pair in AuthorizedUserMap )
            {
                if(pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            return Strategies;
        }

        /// <summary>
        /// 获取用户所有策略内容
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static Dictionary<String, List<AuthorizedOrder>> GetOrdersForSpecificUser(String User)
        {
            User = User.Trim();

            List<String> Strategies = new List<string>();

            Dictionary<String, List<AuthorizedOrder>> ListOrders = new Dictionary<string, List<AuthorizedOrder>>();

            foreach(KeyValuePair<String,String> pair in AuthorizedUserMap)
            {
                if(pair.Value == User)
                {
                    Strategies.Add(pair.Key);
                }
            }

            if(Strategies.Count == 0) return null;

            foreach(KeyValuePair<String,List<AuthorizedOrder>> pair in AuthorizedOrderMap)
            {
                if(Strategies.Contains(pair.Key))
                {
                    ListOrders.Add(pair.Key, pair.Value);
                }
            }

            return ListOrders;
        }
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
}