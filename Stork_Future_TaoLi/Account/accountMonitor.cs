using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;
using Stork_Future_TaoLi.Hubs;
using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class accountMonitor
    {

        private static Thread excuteThread = new Thread(new ThreadStart(WorkThread));

        /// <summary>
        /// 股票账户资金状态
        /// </summary>
        private static Dictionary<string, StockAccountTable> StockAccountDictionary = new Dictionary<string, StockAccountTable>();

        /// <summary>
        /// 期货账户资金状态
        /// </summary>
        private static Dictionary<string, FutureAccountTable> FutureAccountDictionary = new Dictionary<string, FutureAccountTable>();

        /// <summary>
        /// 持仓缓存，同步数据库持仓信息，每当持仓修改时主动调用ChangeLocalCC函数更改，设计资金和风控的计算都使用此变量，减少数据库访问次数。
        /// </summary>
        private static Dictionary<string, List<CC_TAOLI_TABLE>> CCDictionary = new Dictionary<string, List<CC_TAOLI_TABLE>>();

        /// <summary>
        /// 本地持仓列表
        /// 按照  WorkThread 的循环周期性刷新
        /// 刷新结果记录入数据库
        /// 此变量作为界面刷新数据源
        /// </summary>
        private static Dictionary<String, AccountInfo> AccountInfoDictionary = new Dictionary<string, AccountInfo>();

        /// <summary>
        /// 风控冻结资金列表
        /// 股票交易： 通过风控后写入该表，获取委托后离开该表
        /// 期货交易： 通过风控后写入该表，期货交易成交或者失败后离开该表
        /// </summary>
        private static Dictionary<String, List<RiskFrozenInfo>> RiskFrozenDictionary = new Dictionary<string, List<RiskFrozenInfo>>();

        public static double factor = 300; //股票对应市值系数

        public  static double future_margin_factor = 0.12; //期货保证金系数

        public static void RUN()
        {

            //首次启动需要load全部持仓到本地
            List<UserInfo> users = DBAccessLayer.GetUser();

            foreach(UserInfo user in users)
            {
                List<CC_TAOLI_TABLE> records = new List<CC_TAOLI_TABLE>();
                DBAccessLayer.LoadCCList(user.alias,out records);

                if(records.Count != 0)
                {
                    ChangeLocalCC(user.alias, records);   
                }
            }

            //初始化 stockAccountDictionary futureAccountDictionary
            foreach(UserInfo user in users)
            {
                StockAccountTable stockAccount = DBAccessLayer.GetStockAccount(user.alias);
                StockAccountDictionary.Add(user.alias, stockAccount);

                FutureAccountTable futureAccount = DBAccessLayer.GetFutureAccount(user.alias);
                FutureAccountDictionary.Add(user.alias, futureAccount);
            }

            //初始化 riskFrozenDictionary
            foreach(UserInfo user in users)
            {
                RiskFrozenDictionary.Add(user.alias, new List<RiskFrozenInfo>());
            }


            

            excuteThread.Start();
            Thread.Sleep(1000);
        }

        private static void WorkThread(){
            while(true)
            {
                Thread.Sleep(1000);

                
                if (DateTime.Now.Second % 2 == 0)
                {
                    //计划每5s刷新资金账户情况
                    List<UserInfo> users = DBAccessLayer.GetUser();

                    foreach (UserInfo info in users)
                    {
                        List<RISK_TABLE> risks = DBAccessLayer.GetRiskRecord(info.alias);

                        int count = 0;

                        if (risks.Count > 0)
                        {
                            List<TMRiskInfo> riskinfos = new List<TMRiskInfo>();

                            foreach (RISK_TABLE risk in risks)
                            {
                                count++;
                                if (count > 10) break;
                                riskinfos.Add(new TMRiskInfo() { code = risk.code, hand = risk.amount.ToString(), price = risk.price.ToString(), orientation = risk.orientation, time = risk.time.ToString(), strategy = "00", user = risk.alias, errinfo = risk.err });
                            }


                            TradeMonitor.Instance.updateRiskList(info.alias, JsonConvert.SerializeObject(riskinfos), JsonConvert.SerializeObject(riskmonitor.riskPara));

                        }

            

                        if(info.userRight == 3)
                        {
                            List<AccountInfo> accounts = new List<AccountInfo>();
                            foreach (KeyValuePair<string, AccountInfo> pair in AccountInfoDictionary)
                            {
                                accounts.Add(pair.Value);
                            }
                            TradeMonitor.Instance.updateAuditInfo(accounts);

                            //审计员没有风控和持仓信息
                            continue;

                        }

                        AccountInfo acc = UpdateAccountList(info);

                        if(AccountInfoDictionary.Keys.Contains(info.alias))
                        {
                            lock(AccountInfoDictionary)
                            {
                                AccountInfoDictionary[info.alias] = acc;
                            }
                        }
                        else
                        {
                            AccountInfoDictionary.Add(info.alias, acc);

                        }

                        

                        if (info.userRight == 2)
                        {
                            //交易员显示个人账户信息
                            AccountCalculate.Instance.updateAccountInfo(info.alias, JsonConvert.SerializeObject(acc), false);

                            //更新用户交易列表视图
                            List<DL_TAOLI_TABLE> Deal_records = DBAccessLayer.GetUserDeals(info.alias);
                            if (Deal_records == null) Deal_records = new List<DL_TAOLI_TABLE>();
                            TradeMonitor.Instance.updateTradeList(info.alias, JsonConvert.SerializeObject(Deal_records));

                            //更新持仓列表视图
                            TradeMonitor.Instance.updateOrderList(info.alias, null);
                            TradeMonitor.Instance.updateCCList(info.alias, acc.positions);
                        }
                        else if(info.userRight == 1)
                        {
                            //管理员显示所有用户账户信息
                            AccountCalculate.Instance.updateAccountInfo(info.alias, JsonConvert.SerializeObject(AccountInfoDictionary.Values), true);

                            //更新用户交易列表视图
                            List<DL_TAOLI_TABLE> Deal_records = DBAccessLayer.GetUserDeals(info.alias);
                            if (Deal_records == null) Deal_records = new List<DL_TAOLI_TABLE>();
                            TradeMonitor.Instance.updateTradeList(info.alias, JsonConvert.SerializeObject(Deal_records));

                            //更新持仓列表视图
                            TradeMonitor.Instance.updateOrderList(info.alias, null);
                            TradeMonitor.Instance.updateCCList(info.alias, acc.positions);
                        }
                        
                    }


                }

                
               
            }
        }

        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="result">返回错误说明</param>
        /// <returns>账户信息</returns>
        public static AccountInfo GetAccountInfo(string alias,out string result)
        {
            alias = alias.Trim();
            result = string.Empty;

            if(AccountInfoDictionary.Keys.Contains(alias))
            {
                return AccountInfoDictionary[alias];
            }
            else
            {
                result = GetErrorCode(3,string.Empty);
                return null;
            }
        }

        /// <summary>
        /// 获取所有交易员的账户信息
        /// </summary>
        /// <returns></returns>
        public static List<AccountInfo> GetAccountInfoAll()
        {
            List<AccountInfo> Accounts = new List<AccountInfo>();
           
            List<UserInfo> Users = DBAccessLayer.GetUser();
            String result = String.Empty;
            foreach (UserInfo info in Users)
            {
                if(info.userRight == 2)
                {
                    AccountInfo acc = GetAccountInfo(info.alias, out result);
                    if (acc.positions.Count() > 0)
                    {
                        //交易员，管理员
                        Accounts.Add(acc);
                    }
                }
            }

            return Accounts;
        }

        /// <summary>
        /// 获取错误返回代码
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="content">内容参数</param>
        /// <returns>错误描述</returns>
        public static string GetErrorCode(int code,string content)
        {
            switch (code)
            {
                case 0:
                    return "验证通过";
                case 1:
                    return "股市金额不足";
                case 2:
                    return "期货金额不足";
                case 3:
                    return "账户不存在";
                case 4:
                    return "证券" + content + "不在白名单列表";
                case 5:
                    return "证券" + content + "超过总股本/流通股比例限制";
                case 6:
                    return "单支股票金额超限" + content;
                case 7:
                    return "预计股票成本高于可用资金" + content;
                case 8:
                    return "风险度超过阈值，交易未允许";
                case 9:
                    return "敞口过高，交易未允许";
                case 10:
                    return "单日期货交易过多（超过十次）";
                case 11:
                    {
                        string security = content.Split('|')[0];
                        string amount = content.Split('|')[1];
                        return "卖出股票" + security + "数量低于持仓，当前持仓" + amount;
                    }
                case 12:
                    {
                        string security = content.Split('|')[0];
                        string amount = content.Split('|')[1];
                        string direction = content.Split('|')[2];

                        return "平仓期货" + security + "方向" + direction + "数量低于持仓，当前持仓" + amount;
                    }
                case 13:
                    {
                        return "股票总成本，超过总资产的80%，达到" + content;
                    }
                case 14:
                    {
                        return "风险监控用户未在线，交易禁止！";
                    }
                default :
                    return "验证通过";
            }
        }

        /// <summary>
        /// 更新风控冻结证券信息
        /// </summary>
        /// <param name="alias">用户</param>
        /// <param name="code">证券代码</param>
        /// <param name="amount">数量，正为加入风控列表，负为从风控列表中移除</param>
        /// <param name="cost">成本 (数量 * 买入价)，正为加入风控列表，负为从风控列表移除</param>
        /// <param name="type">类型</param>
        /// <param name="direction">期货交易方向</param>
        /// <returns></returns>
        public static double UpdateRiskFrozonAccount(string alias,string code ,int amount ,double cost,string type,string direction)
        {
            alias = alias.Trim();

            if (RiskFrozenDictionary.Keys.Contains(alias))
            {
                //证券存在于风控列表
                for (int i = 0; i < RiskFrozenDictionary[alias].Count; i++)
                {
                    if (RiskFrozenDictionary[alias][i].Code == code && RiskFrozenDictionary[alias][i].Type == type && RiskFrozenDictionary[alias][i].TradeDirection == direction)
                    {
                        //风控信息已经存在
                        if (amount > 0)
                        {
                            //追加风控信息

                            RiskFrozenInfo riskinfo = RiskFrozenDictionary[alias][i];
                            int totalamount = Convert.ToInt32(riskinfo.FrozenAmount) + amount;
                            double totalcost = Convert.ToDouble(riskinfo.FrozenCost) + cost;


                            RiskFrozenDictionary[alias][i].FrozenAmount = totalamount.ToString();
                            RiskFrozenDictionary[alias][i].FrozenCost = totalcost.ToString();

                            if (type == "S" || type == "s")
                            {
                                //风控中增加的资金，是可用资金里面减少的值
                                //股票的可用资金直接减少，期货需要再通过计算保证金和动态权益获取可用资金，这个在updateaccount中计算
                                StockAccountDictionary[alias].Balance = (Convert.ToDouble(StockAccountDictionary[alias].Balance) - cost).ToString();
                            }

                            return totalcost;
                        }
                        else
                        {
                            //减少风控信息

                            RiskFrozenInfo riskinfo = RiskFrozenDictionary[alias][i];

                            int totalamount = Convert.ToInt32(riskinfo.FrozenAmount) + amount;
                            double totalcost = Convert.ToDouble(riskinfo.FrozenCost) + cost;

                            if (totalamount <= 0 || totalcost <= 0)
                            {
                                //说明该证券已无风控冻结资金
                                RiskFrozenDictionary[alias].Remove(riskinfo);

                            }
                            else
                            {
                                //减少风控后还会剩余风控信息
                                RiskFrozenDictionary[alias][i].FrozenAmount = totalamount.ToString();
                                RiskFrozenDictionary[alias][i].FrozenCost = totalcost.ToString();
                            }

                            return totalcost;
                        }

                    }
                }

                //风控列表存在用户，但是不存在对应的证券冻结情况

                if (amount > 0)
                {
                    //加入风控列表
                    RiskFrozenDictionary[alias].Add(new RiskFrozenInfo()
                    {
                        Code = code,
                        FrozenAmount = amount.ToString(),
                        FrozenCost = cost.ToString(),
                        TradeDirection = direction,
                        Type = type
                    });

                    if (type == "S" || type == "s")
                    {
                        //风控中增加的资金，是可用资金里面减少的值
                        //股票的可用资金直接减少，期货需要再通过计算保证金和动态权益获取可用资金，这个在updateaccount中计算
                        StockAccountDictionary[alias].Balance = (Convert.ToDouble(StockAccountDictionary[alias].Balance) - cost).ToString();
                    }


                    return amount * cost;
                }
                else
                {
                    //列表中本来没有记录，无需再移除
                    return 0;
                }
            }
            else
            {
                //未加入风控列表
                RiskFrozenDictionary.Add(alias, new List<RiskFrozenInfo>());

                if (amount > 0)
                {
                    //加入风控列表
                    RiskFrozenDictionary[alias].Add(new RiskFrozenInfo()
                    {
                        Code = code,
                        FrozenAmount = amount.ToString(),
                        FrozenCost = cost.ToString(),
                        TradeDirection = direction,
                        Type = type
                    });

                    if (type == "S" || type == "s")
                    {
                        //风控中增加的资金，是可用资金里面减少的值
                        //股票的可用资金直接减少，期货需要再通过计算保证金和动态权益获取可用资金，这个在updateaccount中计算
                        StockAccountDictionary[alias].Balance = (Convert.ToDouble(StockAccountDictionary[alias].Balance) - cost).ToString();
                    }

                    return amount * cost;
                }
                else
                {
                    //列表中本来没有记录，无需再移除
                    return 0;
                }
            }
          
        }

        /// <summary>
        ///  股票成交后更新本地股票资金信息
        ///  修改：
        ///     1. 可用资金
        ///     2. 股票成本
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <param name="amount">交易数量，正为买入，负为卖出</param>
        /// <param name="price">价格</param>
        public static void ChangeStockAccountDuToStockDeal(string alias, double price, int amount)
        {
            lock(StockAccountDictionary)
            {
                if (StockAccountDictionary.Keys.Contains(alias.Trim()))
                {
                    StockAccountDictionary[alias.Trim()].StockValue = (Convert.ToDouble(StockAccountDictionary[alias.Trim()].StockValue) + price*amount).ToString();
                    
                    if(amount < 0)
                    {
                        //卖出会增加可用资金
                        StockAccountDictionary[alias.Trim()].Balance = (Convert.ToDouble(StockAccountDictionary[alias.Trim()].Balance) - price * amount).ToString();
                    }
                }
                else
                {
                    StockAccountTable stock = DBAccessLayer.GetStockAccount(alias.Trim());

                    stock.StockValue = (Convert.ToDouble(stock.StockValue) + price * amount).ToString();

                    if(amount < 0)
                    {
                        //卖出修改可用资金量
                        stock.Balance = (Convert.ToDouble(stock.Balance) - amount * price).ToString();
                    }

                    StockAccountDictionary.Add(alias.Trim(), stock);
                }
            }
        }

        /// <summary>
        /// 期货成交后更新本地期货资金信息
        /// 修改：
        ///     平仓盈亏
        ///     保证金
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <param name="hand">手数，正为开仓, 负为平仓</param>
        /// <param name="code">持仓修改的证券代码</param>
        /// <param name="direction">期货的方向</param>
        /// <param name="old_hand">修改前证券仓位总手数</param>
        /// <param name="old_price">修改后证券仓位总手数</param>
        /// <param name="price">价格</param>
        public static void ChangeFutureAccountDuToFutureDeal(string alias , string code , double price, int hand,string direction,double old_price,int old_hand)
        {
            lock (FutureAccountDictionary)
            {
                if(FutureAccountDictionary.Keys.Contains(alias))
                {
                    //保证金改变
                    FutureAccountDictionary[alias.Trim()].CashDeposit = (Convert.ToDouble(FutureAccountDictionary[alias.Trim()].CashDeposit) + AccountPARA.Factor(code) * hand * price * AccountPARA.MarginValue).ToString();

                    //平仓盈亏改变
                    if(hand < 0)
                    {
                        double earning = Convert.ToDouble(FutureAccountDictionary[alias.Trim()].OffsetGain);
                         earning += (price - old_price) * hand;
                        FutureAccountDictionary[alias.Trim()].OffsetGain = earning.ToString();
                    }
                }
                else
                {
                    FutureAccountTable future = DBAccessLayer.GetFutureAccount(alias.Trim());

                    future.CashDeposit = (Convert.ToDouble(FutureAccountDictionary[alias.Trim()].CashDeposit)  + AccountPARA.Factor(code) * hand * price * AccountPARA.MarginValue).ToString();

                    if(hand < 0){
                        double earning = Convert.ToDouble(FutureAccountDictionary[alias.Trim()].OffsetGain);
                        earning += (price - old_price) * hand;
                        FutureAccountDictionary[alias.Trim()].OffsetGain = earning.ToString();
                    }

                    FutureAccountDictionary.Add(alias.Trim(),future);
                }
            }
        }

        /// <summary>
        /// 更新用户仓位信息包含股票和期货
        /// 该函数按照WorkThread的周期执行
        /// </summary>
        /// <param name="info">输入用户信息</param>
        /// <returns>用户仓位信息</returns>
        public static AccountInfo UpdateAccountList(UserInfo info)
        {
             string alias = info.alias.Trim();
           
            //获取全部持仓，该值会拆分成CC_Stock_records 和 CC_Future_records
             List<CC_TAOLI_TABLE> CC_records = new List<CC_TAOLI_TABLE>();
             if (CCDictionary.Keys.Contains(alias))
             {
                 CC_records = CCDictionary[alias];
             }

            //获取股票持仓
            List<CC_TAOLI_TABLE> CC_Stock_records = (from item in CC_records where item.CC_TYPE == "0"  select item).ToList();

            //获取期货持仓
            List<CC_TAOLI_TABLE> CC_Future_records = (from item in CC_records where item.CC_TYPE == "1"  select item).ToList();

            //获取风控预冻结资金
            List<RiskFrozenInfo> Risk_Frozen_records = new List<RiskFrozenInfo>();
            if(RiskFrozenDictionary.Keys.Contains(alias))
            {
                Risk_Frozen_records = RiskFrozenDictionary[alias];
            }

            //获取股票委托冻结资金
            double Entrust_frozen = 0;
            List<ERecord> Erecords = new List<ERecord>();

            //获取委托持仓
            EntrustRecord.GetUserAccountInfo(alias, out Entrust_frozen, out Erecords);

            //内存股票账户，记录其中的可用资金，股票成本，冻结资金不随行情改变，直接使用
            StockAccountTable stockAccount = StockAccountDictionary[alias];

            //股票可用资金
            //1. 经过风控股票买入时改变，从减少可用资金，增加风控冻结资金；
            //2. 经过持仓股票卖出时改变，减少股票成本，增加股票可用资金
            // 股票可用资金，不会因行情改变而改变
            double stock_balance = Convert.ToDouble(stockAccount.Balance.Trim());

            //股票成本
            double stock_cost = Convert.ToDouble(stockAccount.StockValue.Trim());

            //冻结资金 (委托冻结资金 + 风控冻结资金)
            double stock_frozen_value = 0;
            
            foreach(ERecord record in Erecords)
            {
                stock_frozen_value += record.Amount * record.OrderPrice;
            }

            foreach(RiskFrozenInfo record in Risk_Frozen_records)
            {
                if(record.Type == "S" || record.Type =="s")
                {
                    stock_frozen_value += Convert.ToDouble(record.FrozenCost);
                }
            }

            //计算股票市值
            double market_value = 0;

            //股票盈亏
            double stock_earning = 0;

            foreach(CC_TAOLI_TABLE item in CC_Stock_records)
            {
                double price = MarketPrice.market[item.CC_CODE.Trim()];
                if (price != 0)
                {
                    //存在最新市值，采用最新市值计算
                    market_value += (Convert.ToInt32(item.CC_AMOUNT) * price);

                    //知道最新市值才能求盈亏
                    stock_earning += (price - Convert.ToDouble(item.CC_BUY_PRICE)) * Convert.ToInt32(item.CC_AMOUNT);
                }
                else
                {
                    //未找到最新市值，用成交价计算
                    market_value += (Convert.ToInt32(item.CC_AMOUNT) * Convert.ToDouble(item.CC_BUY_PRICE));
                }
            }

            //股票权益 （可用资金 + 股票市值 + 冻结资金量）
            double stock_total = stock_balance + market_value + stock_frozen_value;

            //内存期货账户，记录其中的平仓盈亏，静态权益，保证金，期货冻结资金不随行情改变，直接使用
            FutureAccountTable futureAccount = FutureAccountDictionary[alias];

            //静态权益
            double static_intrests = Convert.ToDouble(futureAccount.StatisInterests.Trim());

            //平仓盈亏
            double offset_gain = Convert.ToDouble(futureAccount.OffsetGain.Trim());

            //保证金
            double cash_deposit = Convert.ToDouble(futureAccount.CashDeposit.Trim());

            //期货冻结保证金
            double frozen_cash_deposit = 0;

            foreach(RiskFrozenInfo record in Risk_Frozen_records)
            {
                if(record.Type == "F" || record.Type =="F")
                {
                    frozen_cash_deposit += Convert.ToDouble(record.FrozenCost) * AccountPARA.Factor(record.Code) * AccountPARA.MarginValue;
                }
             }



            //期货对应股票市值
            double future_stock_marketvalue = 0;

            //持仓盈亏
            double opsition_gain = 0;

            foreach(CC_TAOLI_TABLE item in CC_Future_records)
            {
                double fprice = 0;

                if (MarketPrice.market.Keys.Contains(item.CC_CODE.Trim()))
                {
                    fprice = MarketPrice.market[item.CC_CODE.Trim()];
                }
                else
                {
                    fprice = Convert.ToDouble(item.CC_BUY_PRICE);
                }

                if(item.CC_DIRECTION == TradeOrientationAndFlag.FutureTradeDirectionBuy)
                {
                    if(fprice != 0)
                    {
                        //期货买入仓位
                        //没有实时行情信息，无需再运行持仓盈亏
                        opsition_gain += (fprice - Convert.ToDouble(item.CC_BUY_PRICE)) * Convert.ToInt32(item.CC_AMOUNT);

                        //存在实时行情，用期货行情计算期货对应股票市值
                        future_stock_marketvalue += (AccountPARA.Factor(item.CC_CODE) * Convert.ToInt32(item.CC_AMOUNT) * fprice);
                    }
                    else
                    {
                        //不存在实时行情，用成交价格计算期货对应股票市值
                        future_stock_marketvalue += (AccountPARA.Factor(item.CC_CODE) * Convert.ToInt32(item.CC_AMOUNT) * Convert.ToDouble(item.CC_BUY_PRICE));
                    }

                 
                }
                else if(item.CC_DIRECTION == TradeOrientationAndFlag.FutureTradeDirectionSell)
                {
                    if(fprice != 0)
                    {
                        //期货买入仓位
                        //没有实时行情信息，无需再运行持仓盈亏
                        opsition_gain += (Convert.ToDouble(item.CC_BUY_PRICE) - fprice) * Convert.ToInt32(item.CC_AMOUNT);

                        //存在实时行情，用期货行情计算期货对应股票市值
                        future_stock_marketvalue -= (AccountPARA.Factor(item.CC_CODE) * Convert.ToInt32(item.CC_AMOUNT) * fprice);
                    }
                    else
                    {
                        //不存在实时行情，用成交价格计算期货对应股票市值
                        future_stock_marketvalue -= (AccountPARA.Factor(item.CC_CODE) * Convert.ToInt32(item.CC_AMOUNT) * Convert.ToDouble(item.CC_BUY_PRICE));
                    }
                }
            }

            //动态权益 （静态权益 + 平仓盈亏 + 持仓盈亏）
            double dynamic_interests = static_intrests + offset_gain + opsition_gain;

            //可用资金 （动态权益 - 保证金）
            double expendableFund = dynamic_interests - cash_deposit;

            //期货风险度 （占用保证金 / 动态权益）
            double future_risk = cash_deposit / dynamic_interests ;

            //更新股票资金Dictionary
            lock(StockAccountDictionary)
            {
                StockAccountDictionary[alias].Balance = stock_balance.ToString();
                StockAccountDictionary[alias].Earning = stock_earning.ToString();
                StockAccountDictionary[alias].MarketValue = market_value.ToString();
                StockAccountDictionary[alias].StockFrozenValue = stock_frozen_value.ToString();
                StockAccountDictionary[alias].StockValue = stock_cost.ToString();
                StockAccountDictionary[alias].Total = stock_total.ToString();
                StockAccountDictionary[alias].UpdateTime = DateTime.Now;
            }

            //更新期货资金Dictionary
            lock(FutureAccountDictionary)
            {
                FutureAccountDictionary[alias].CashDeposit = cash_deposit.ToString();
                FutureAccountDictionary[alias].DynamicInterests = dynamic_interests.ToString();
                FutureAccountDictionary[alias].ExpendableFund = expendableFund.ToString();
                FutureAccountDictionary[alias].FrozenValue = frozen_cash_deposit.ToString();
                FutureAccountDictionary[alias].OffsetGain = offset_gain.ToString();
                FutureAccountDictionary[alias].OpsitionGain = opsition_gain.ToString();
                FutureAccountDictionary[alias].StatisInterests = static_intrests.ToString();
                FutureAccountDictionary[alias].UpdateTime = DateTime.Now;
            }

            //返回账户信息
            AccountInfo accinfo = new AccountInfo();

            accinfo.positions = new List<AccountPosition>();
            accinfo.entrusts = new List<AccountEntrust>();
            accinfo.riskFrozenInfo = new List<RiskFrozenInfo>();

            accinfo.account = stock_balance.ToString();
            accinfo.alias = alias;
            accinfo.balance = stock_balance.ToString();
            accinfo.cost = stock_cost.ToString();
            accinfo.earning = stock_earning.ToString();

            foreach (ERecord item in Erecords)
            {
                accinfo.entrusts.Add(new AccountEntrust()
                {
                    code = item.Code,
                    dealAmount = item.DealAmount.ToString(),
                    dealMoney = item.DealFrezonMoney.ToString(),
                    exchange = item.ExchangeId,
                    requestAmount = item.Amount.ToString(),
                    requestPrice = item.OrderPrice.ToString(),
                    direction = item.Direction.ToString(),
                    orderRef = item.OrderRef.ToString(),
                    orderSysRef = item.SysOrderRef.ToString()
                });
            }

            accinfo.faccount = expendableFund.ToString();
            accinfo.fbond = cash_deposit.ToString();
            accinfo.fincome = (opsition_gain + offset_gain).ToString();
            accinfo.frisk = (future_risk * 100).ToString() + "%";
            accinfo.frozen = frozen_cash_deposit.ToString();
            accinfo.fstockvalue = future_stock_marketvalue.ToString();
            accinfo.fvalue = dynamic_interests.ToString();
            accinfo.name = info.name;
            
            foreach(CC_TAOLI_TABLE item in CC_Stock_records)
            {
                accinfo.positions.Add(new AccountPosition()
                {
                    amount = item.CC_AMOUNT.ToString(),
                    code = item.CC_CODE,
                    direction = item.CC_DIRECTION,
                    name = item.CC_USER,
                    price = item.CC_BUY_PRICE.ToString(),
                    type = item.CC_TYPE
                });
            }

            foreach(CC_TAOLI_TABLE item in CC_Future_records)
            {
                accinfo.positions.Add(new AccountPosition()
                {
                    amount = item.CC_AMOUNT.ToString(),
                    code = item.CC_CODE,
                    direction = item.CC_DIRECTION,
                    name = item.CC_USER,
                    price = item.CC_BUY_PRICE.ToString(),
                    type = item.CC_TYPE
                });
            }

            accinfo.risk_exposure = "0";
            accinfo.riskFrozenInfo = new List<RiskFrozenInfo>();

            foreach (RiskFrozenInfo record in Risk_Frozen_records)
            {
                accinfo.riskFrozenInfo.Add(record);
            }

            accinfo.value = future_stock_marketvalue.ToString();

            return accinfo;
           
        }

        /// <summary>
        /// 修改本地持仓列表
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <param name="records">产品列表</param>
        public static void ChangeLocalCC(string alias , List<CC_TAOLI_TABLE> records)
        {
            alias = alias.Trim();

            lock (CCDictionary)
            {
                if(CCDictionary.Keys.Contains(alias))
                {
                    CCDictionary[alias].Clear();

                }
                else
                {
                    CCDictionary.Add(alias, new List<CC_TAOLI_TABLE>());

                }

                foreach(CC_TAOLI_TABLE item in records)
                    {
                        CCDictionary[alias].Add(item);
                    }
            }
        }

        /// <summary>
        /// 获得股票资金对象
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>
        ///     null: 不存在该股票资金信息，这个基本上不可能，如果出现，一定是出现程序问题。
        /// </returns>
        public static StockAccountTable GetStockAccount(string alias)
        {
            alias = alias.Trim();

            if (StockAccountDictionary.Keys.Contains(alias))
            {
                return StockAccountDictionary[alias];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得期货资金对象
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>
        ///     null: 不存在该期货资金信息，这个基本上不可能，如果出现，一定是出现程序问题。
        /// </returns>
        public static FutureAccountTable GetFutureAccount(string alias)
        {
            alias = alias.Trim();

            if(FutureAccountDictionary.Keys.Contains(alias))
            {
                return FutureAccountDictionary[alias];

            }
            else
            {
                return null;
            }
        }

        #region GC_Save_Code
        /// <summary>
        /// 计算账户情况
        /// </summary>
        /// <param name="info"></param>
        //public static AccountInfo UpdateAccount(UserInfo info)
        //{
        //    AccountInfo account = new AccountInfo();
        //    account.positions = new List<AccountPosition>();
        //    account.entrusts = new List<AccountEntrust>();
        //    account.riskFrozenInfo = new List<RiskFrozenInfo>();

        //    StockAccountTable stockAccount = StockAccountDictionary[info.alias.Trim()];
        //    FutureAccountTable futureAccount = FutureAccountDictionary[info.alias.Trim()];

        //    //ToDo ： 1. 创建资金表，股票，期货分开，标记股票和期货资金状态根据交易改变 2. 添加期货结算日结算函数（设定最近结算日并记库）。

        //    double staticBalance = 0;       //静态权益
        //    double closeEarning = 0;        //平仓盈亏
        //    double holdEarning = 0;         //持仓盈亏

        //    account.alias = info.alias;
        //    account.name = info.name;
        //    account.account = info.stockAvailable;

        //    List<CC_TAOLI_TABLE> positionRecord = new List<CC_TAOLI_TABLE>();
        //    double stockcost = 0;

        //    DBAccessLayer.LoadCCStockList(info.alias, out positionRecord, out stockcost);

        //    List<CC_TAOLI_TABLE> fPositionRecord = new List<CC_TAOLI_TABLE>();

        //    DBAccessLayer.LoadCCFutureList(info.alias, out fPositionRecord);

        //    double fstock = 0;          //期货对应股票市值
        //    double fweight = 0;         //期货权益
        //    double fearn = 0;           //期货浮动盈亏

        //    foreach (CC_TAOLI_TABLE record in fPositionRecord)
        //    {
        //        if (MarketPrice.market.ContainsKey(record.CC_CODE))
        //        {
        //            int x = -1;
        //            if (record.CC_DIRECTION == "0")
        //            {
        //                x = 1;
        //            }

        //            fstock += Convert.ToDouble((record.CC_AMOUNT) * MarketPrice.market[record.CC_CODE] * factor * x);

        //            fearn += Convert.ToDouble((record.CC_AMOUNT) * (MarketPrice.market[record.CC_CODE] - record.CC_BUY_PRICE) * x);
        //        }
        //    }

        //    fstock = Math.Abs(fstock);

        //    account.fstockvalue = fstock.ToString();

        //    account.fincome = fearn.ToString();

        //    //期货权益

        //    string accountLimit = DBAccessLayer.GetAccountAvailable(info.alias);

        //    if (accountLimit == string.Empty)
        //    {
        //        return null;

        //    }

        //    stockAccount = Convert.ToDouble(accountLimit.Split('|')[0].Trim());
        //    futureAccount = Convert.ToDouble(accountLimit.Split('|')[1].Trim());

        //    fweight = futureAccount + fearn;

        //    account.fvalue = fweight.ToString();


        //    double future_margin = 0;               //期货保证金

        //    future_margin = fstock * future_margin_factor;

        //    account.fbond = future_margin.ToString();

        //    account.cost = stockcost.ToString();

        //    if (account.positions == null)
        //    {
        //        account.positions = new List<AccountPosition>();
        //    }

        //    foreach (CC_TAOLI_TABLE record in positionRecord)
        //    {
        //        account.positions.Add(new AccountPosition()
        //        {
        //            amount = record.CC_AMOUNT.ToString(),
        //            code = record.CC_CODE,
        //            name = record.CC_USER,
        //            price = record.CC_BUY_PRICE.ToString(),
        //            type = record.CC_TYPE,
        //            direction = record.CC_DIRECTION
        //        });
        //    }

        //    double frozen = 0;

        //    List<ERecord> entrusts = new List<ERecord>();
        //    EntrustRecord.GetUserAccountInfo(info.alias, out frozen, out entrusts);

        //    account.faccount = (futureAccount - future_margin + fearn).ToString();

        //    if (account.entrusts == null) account.entrusts = new List<AccountEntrust>();

        //    foreach (ERecord record in entrusts)
        //    {
        //        account.entrusts.Add(new AccountEntrust()
        //        {
        //            code = record.Code,
        //            dealAmount = record.DealAmount.ToString(),
        //            requestAmount = record.Amount.ToString(),
        //            requestPrice = record.OrderPrice.ToString(),
        //            exchange = record.ExchangeId,
        //            dealMoney = record.DealFrezonMoney.ToString()
        //        });
        //    }

        //    //冻结资金
        //    account.frozen = frozen.ToString();

        //    //风控资金
        //    if (account.riskFrozenInfo == null)
        //    {
        //        account.riskFrozenInfo = new List<RiskFrozenInfo>();
        //    }

        //    int riskFrozenTotalCost = 0;

        //    for (int i = 0; i < account.riskFrozenInfo.Count; i++)
        //    {
        //        riskFrozenTotalCost += Convert.ToInt32(account.riskFrozenInfo[i].FrozenCost);
        //    }

        //    //剩余资金量
        //    account.balance = (Convert.ToDouble(account.account) - Convert.ToDouble(account.cost) - Convert.ToDouble(account.frozen) - Convert.ToDouble(riskFrozenTotalCost)).ToString();

        //    //股票市值
        //    account.value = MarketPrice.CalculateCurrentValue(positionRecord).ToString();

        //    //敞口比例
        //    double risk_exposure = ((Convert.ToDouble(account.value) + fstock) / fstock);

        //    //风险度
        //    account.frisk = (Convert.ToDouble(account.fbond) / Convert.ToDouble(account.fvalue)).ToString();

        //    account.risk_exposure = risk_exposure.ToString();

        //    account.earning = (Convert.ToDouble(account.value) - Convert.ToDouble(account.cost)).ToString();

        //    var tmp = (from item in accountList where item.alias == info.alias select item.alias);

        //    if (tmp.Count() == 0)
        //    {
        //        accountList.Add(account);
        //    }
        //    else
        //    {
        //        if (tmp.ToList().Contains(account.alias))
        //        {
        //            var acc = (from item in accountList where item.alias == account.alias select item).ToList()[0];
        //            accountList.Remove(acc);
        //            accountList.Add(account);
        //        }
        //    }

        //    return account;

        //}
        #endregion
    }

    /// <summary>
    /// 用户资金类型
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 股票可用资金量
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 剩余资金量
        /// </summary>
        public string balance { get; set; }

        /// <summary>
        /// 冻结资金量
        /// </summary>
        public string frozen { get; set; }

        /// <summary>
        /// 股票总盈亏
        /// </summary>
        public string earning { get; set; }

        /// <summary>
        /// 风控冻结资金量
        /// 该值在风控模块从剩余资金量中冻结相对于股票购买成本的金额
        /// 获取委托号成功后解冻相应金额，计入冻结资金量(frozen)中
        /// </summary>
        public List<RiskFrozenInfo> riskFrozenInfo { get; set; }

        /// <summary>
        /// 股票预估量
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 股票成本
        /// </summary>
        public string cost { get; set; }

        /// <summary>
        /// 期货可用资金量
        /// </summary>
        public string faccount { get; set; }

        /// <summary>
        /// 期货权益
        /// </summary>
        public string fvalue { get; set; }

        /// <summary>
        /// 期货保证金
        /// </summary>
        public string fbond { get; set; }

        /// <summary>
        /// 期货对应的股票市值
        /// </summary>
        public string fstockvalue { get; set; }

        /// <summary>
        /// 期货盈亏
        /// </summary>
        public string fincome { get; set; }

        /// <summary>
        /// 期货风险度
        /// </summary>
        public string frisk { get; set; }
        
        /// <summary>
        /// 敞口比例
        /// </summary>
        public string risk_exposure { get; set; }

        /// <summary>
        /// 持仓列表
        /// </summary>
        public List<AccountPosition> positions { get; set; }

        /// <summary>
        /// 委托列表
        /// </summary>
        public List<AccountEntrust> entrusts { get; set; }

    }

    /// <summary>
    /// 持仓信息类型
    /// </summary>
    public class AccountPosition
    {

        /// <summary>
        /// 代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public string amount { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string price { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 方向，仅期货使用
        /// </summary>
        public string direction { get; set; }
    }

    /// <summary>
    /// 委托信息类型
    /// </summary>
    public class AccountEntrust
    {
        /// <summary>
        /// 系统号
        /// </summary>
        public string orderRef { get; set; }
        /// <summary>
        /// 报单编号
        /// </summary>
        public string orderSysRef { get; set; }
        /// <summary>
        /// 交易代码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 交易所
        /// </summary>
        public string exchange { get; set; }

        /// <summary>
        /// 申买量
        /// </summary>
        public string requestAmount { get; set; }

        /// <summary>
        /// 申买价
        /// </summary>
        public string requestPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public string dealAmount { get; set; }

        /// <summary>
        /// 成交价
        /// </summary>
        public string dealMoney { get; set; }

        /// <summary>
        /// 交易方向
        /// </summary>
        public string direction { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string status { get; set; }
    }

    /// <summary>
    /// 冻结证券信息
    /// </summary>
    public class RiskFrozenInfo
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 冻结数量
        /// </summary>
        public string FrozenAmount { get; set; }

        /// <summary>
        /// 冻结金额
        /// </summary>
        public string FrozenCost { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 交易方向
        /// </summary>
        public string TradeDirection { get; set; }

    }
}