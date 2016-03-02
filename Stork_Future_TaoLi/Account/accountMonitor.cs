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

        private static List<AccountInfo> accountList = new List<AccountInfo>();

        public static double factor = 300; //股票对应市值系数

        public  static double future_margin_factor = 0.12; //期货保证金系数

        public static void RUN()
        {
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
                        if(info.userRight == 3)
                        {

                            //审计员没有风控和持仓信息
                            continue;

                        }

                        AccountInfo acc = UpdateAccount(info);

                        if (info.userRight == 2)
                        {
                            //交易员显示个人账户信息
                            AccountCalculate.Instance.updateAccountInfo(info.alias, JsonConvert.SerializeObject(acc), false);
                        }
                        else if(info.userRight == 1)
                        {
                            //管理员显示所有用户账户信息
                            AccountCalculate.Instance.updateAccountInfo(info.alias, JsonConvert.SerializeObject(accountList), true);
                        }
                    }


                }

                
               
            }
        }

        /// <summary>
        /// 计算账户情况
        /// </summary>
        /// <param name="info"></param>
        public static AccountInfo UpdateAccount(UserInfo info)
        {
            AccountInfo account = new AccountInfo();
            account.positions = new List<AccountPosition>();
            account.entrusts = new List<AccountEntrust>();
            account.riskFrozenInfo = new List<RiskFrozenInfo>();

            StockAccountTable stockAccount = StockAccountDictionary[info.alias.Trim()];
            FutureAccountTable futureAccount = FutureAccountDictionary[info.alias.Trim()];

            //ToDo ： 1. 创建资金表，股票，期货分开，标记股票和期货资金状态根据交易改变 2. 添加期货结算日结算函数（设定最近结算日并记库）。

            double staticBalance = 0;       //静态权益
            double closeEarning = 0;        //平仓盈亏
            double holdEarning = 0;         //持仓盈亏

            account.alias = info.alias;
            account.name = info.name;
            account.account = info.stockAvailable;

            List<CC_TAOLI_TABLE> positionRecord = new List<CC_TAOLI_TABLE>();
            double stockcost = 0;

            DBAccessLayer.LoadCCStockList(info.alias, out positionRecord, out stockcost);

            List<CC_TAOLI_TABLE> fPositionRecord = new List<CC_TAOLI_TABLE>();

            DBAccessLayer.LoadCCFutureList(info.alias, out fPositionRecord);

            double fstock = 0;          //期货对应股票市值
            double fweight = 0;         //期货权益
            double fearn = 0;           //期货浮动盈亏

            foreach (CC_TAOLI_TABLE record in fPositionRecord)
            {
                if (MarketPrice.market.ContainsKey(record.CC_CODE))
                {
                    int x = -1;
                    if (record.CC_DIRECTION == "0")
                    {
                        x = 1;
                    }

                    fstock += Convert.ToDouble((record.CC_AMOUNT) * MarketPrice.market[record.CC_CODE] * factor * x);

                    fearn += Convert.ToDouble((record.CC_AMOUNT) * (MarketPrice.market[record.CC_CODE] - record.CC_BUY_PRICE) * x);
                }
            }

            fstock = Math.Abs(fstock);

            account.fstockvalue = fstock.ToString();

            account.fincome = fearn.ToString();

            //期货权益

            string accountLimit = DBAccessLayer.GetAccountAvailable(info.alias);

            if (accountLimit == string.Empty)
            {
                return null;

            }

            stockAccount = Convert.ToDouble(accountLimit.Split('|')[0].Trim());
            futureAccount = Convert.ToDouble(accountLimit.Split('|')[1].Trim());

            fweight = futureAccount + fearn;

            account.fvalue = fweight.ToString();


            double future_margin = 0;               //期货保证金

            future_margin = fstock * future_margin_factor;

            account.fbond = future_margin.ToString();

            account.cost = stockcost.ToString();

            if (account.positions == null)
            {
                account.positions = new List<AccountPosition>();
            }

            foreach (CC_TAOLI_TABLE record in positionRecord)
            {
                account.positions.Add(new AccountPosition()
                {
                    amount = record.CC_AMOUNT.ToString(),
                    code = record.CC_CODE,
                    name = record.CC_USER,
                    price = record.CC_BUY_PRICE.ToString(),
                    type = record.CC_TYPE,
                    direction = record.CC_DIRECTION
                });
            }

            double frozen = 0;

            List<ERecord> entrusts = new List<ERecord>();
            EntrustRecord.GetUserAccountInfo(info.alias, out frozen, out entrusts);

            account.faccount = (futureAccount - future_margin + fearn).ToString();

            if (account.entrusts == null) account.entrusts = new List<AccountEntrust>();

            foreach (ERecord record in entrusts)
            {
                account.entrusts.Add(new AccountEntrust()
                {
                    code = record.Code,
                    dealAmount = record.DealAmount.ToString(),
                    requestAmount = record.Amount.ToString(),
                    requestPrice = record.OrderPrice.ToString(),
                    exchange = record.ExchangeId,
                    dealMoney = record.DealFrezonMoney.ToString()
                });
            }

            //冻结资金
            account.frozen = frozen.ToString();

            //风控资金
            if(account.riskFrozenInfo == null)
            { 
                account.riskFrozenInfo = new List<RiskFrozenInfo>(); 
            }

            int riskFrozenTotalCost = 0;

            for (int i = 0; i < account.riskFrozenInfo.Count;i++ )
            {
                riskFrozenTotalCost += Convert.ToInt32(account.riskFrozenInfo[i].FrozenCost);
            }

            //剩余资金量
            account.balance = (Convert.ToDouble(account.account) - Convert.ToDouble(account.cost) - Convert.ToDouble(account.frozen) - Convert.ToDouble(riskFrozenTotalCost)).ToString();

            //股票市值
            account.value = MarketPrice.CalculateCurrentValue(positionRecord).ToString();

            //敞口比例
            double risk_exposure = ((Convert.ToDouble(account.value) + fstock) / fstock);

            //风险度
            account.frisk = (Convert.ToDouble(account.fbond) / Convert.ToDouble(account.fvalue)).ToString();

            account.risk_exposure = risk_exposure.ToString();

            account.earning = (Convert.ToDouble(account.value) - Convert.ToDouble(account.cost)).ToString();

            var tmp = (from item in accountList where item.alias == info.alias select item.alias);

            if(tmp.Count() == 0)
            {
                accountList.Add(account);
            }
            else
            {
                 if(tmp.ToList().Contains(account.alias))
                 {
                     var acc = (from item in accountList where item.alias  == account.alias select item).ToList()[0];
                     accountList.Remove(acc);
                     accountList.Add(account);
                 }
            }

            return account;

        }

        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="result">返回错误说明</param>
        /// <returns>账户信息</returns>
        public static AccountInfo GetAccountInfo(string name,out string result)
        {
            result = string.Empty;
            var acc = (from item in accountList where item.alias == name select item);

            if(acc.Count() == 0)
            {
               //没有查到关于该用户的风控信息，直接返回失败
                result = GetErrorCode(3,string.Empty);
                return null;
            }

            return acc.ToList()[0];
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
                default :
                    return "验证通过";
            }
        }

        /// <summary>
        /// 获得单只股票的全局总持仓
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>股票总数</returns>
        public static long GetStockTotalPositionAmount(string code)
        {
            long totalnum = 0;

            foreach (AccountInfo acc in accountList)
            {
                var tmpA = (from item in acc.positions where item.code == code select item);

                if(tmpA.Count() != 0)
                {
                    totalnum += Convert.ToInt16(tmpA.ToList()[0].amount);
                }

                var tmpB = (from item in acc.entrusts where item.code == code select item);

                if (tmpB.Count() != 0)
                {
                    totalnum += Convert.ToInt16(tmpB.ToList()[0].requestAmount);
                }
            }

            return totalnum;
        }


        /// <summary>
        /// 更新风控冻结证券信息
        /// </summary>
        /// <param name="alias">用户</param>
        /// <param name="code">证券代码</param>
        /// <param name="amount">数量</param>
        /// <param name="cost">成本</param>
        /// <param name="type">类型</param>
        /// <param name="direction">期货交易方向</param>
        /// <returns></returns>
        public static double UpdateRiskFrozonAccount(string alias,string code ,int amount ,double cost,string type,string direction)
        {
            for (int i = 0; i < accountList.Count; i++)
            {
                if (accountList[i].alias.Trim() == alias.Trim())
                {

                    if (accountList[i].riskFrozenInfo == null) accountList[i].riskFrozenInfo = new List<RiskFrozenInfo>();

                    // 先判断已经有存量风控资金的情况
                    for (int j = 0; j < accountList[i].riskFrozenInfo.Count; j++)
                    {
                        if (accountList[i].riskFrozenInfo[j].Code.Trim() == code.Trim())
                        {
                            if (type == "F" || type == "f")
                            {
                                if (accountList[i].riskFrozenInfo[j].TradeDirection.Trim() == direction.Trim())
                                {
                                    int camount = Convert.ToInt32(accountList[i].riskFrozenInfo[j].FrozenAmount);
                                    double ccost = Convert.ToInt32(accountList[i].riskFrozenInfo[j].FrozenCost);

                                    accountList[i].riskFrozenInfo[j].FrozenAmount = (camount + amount).ToString();
                                    accountList[i].riskFrozenInfo[j].FrozenCost = (ccost + cost).ToString();

                                    return (ccost + cost);
                                }
                            }
                            else if (type == "S" || type == "s")
                            {

                                int camount = Convert.ToInt32(accountList[i].riskFrozenInfo[j].FrozenAmount);
                                double ccost = Convert.ToInt32(accountList[i].riskFrozenInfo[j].FrozenCost);


                                //买入
                                accountList[i].riskFrozenInfo[j].FrozenAmount = (camount + amount).ToString();
                                accountList[i].riskFrozenInfo[j].FrozenCost = (ccost + cost).ToString();

                                return (ccost + cost);
                            }

                        

                        }
                    }
                    
                    //判断无风控资金存量的情况
                    accountList[i].riskFrozenInfo.Add(new RiskFrozenInfo()
                    {
                        Code = code.Trim(),
                        FrozenAmount = amount.ToString().Trim(),
                        FrozenCost = cost.ToString().Trim(),
                        TradeDirection = direction.ToString().Trim(),
                        Type = type.ToString().Trim()
                    });

                    return cost;
                 }
            }

            return 0;
        }

        /// <summary>
        /// 更新可用资金
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <param name="type">类型</param>
        /// <param name="money">变动金额</param>
        /// <returns></returns>
        public static double UpdateAccountMoney(string alias, string type, double money)
        {

            string accountLimit = DBAccessLayer.GetAccountAvailable(alias);
            if (accountLimit == null || accountLimit == string.Empty) return 0;

            double futureAccount = Convert.ToDouble(accountLimit.Split('|')[0].Trim());
            double stockAccount = Convert.ToDouble(accountLimit.Split('|')[1].Trim());

            if (type == "F" || type == "f")
            {
                
            }
            else if (type == "S" || type == "s")
            {
                stockAccount += money;
                return stockAccount;
            }

            return 0;
        }


        /// <summary>
        ///  股票成交后更新本地股票资金信息
        ///  修改：
        ///     1. 可用资金
        ///     2. 股票成本
        ///     3. 冻结资金
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
                        //卖出会变动可用资金
                        StockAccountDictionary[alias.Trim()].Balance = (Convert.ToDouble(StockAccountDictionary[alias.Trim()].Balance) - price * amount).ToString();
                    }

                    if (amount > 0)
                    {
                        //买入才涉及冻结金额
                        StockAccountDictionary[alias.Trim()].StockFrozenValue = (Convert.ToDouble(StockAccountDictionary[alias.Trim()].StockFrozenValue) - price * amount).ToString();
                    }
                }
                else
                {
                    StockAccountTable stock = DBAccessLayer.GetStockAccount(alias.Trim());

                    stock.StockValue = (Convert.ToDouble(stock.StockValue) + price * amount).ToString();

                    if (amount > 0)
                    {
                        //买入才涉及冻结金额
                        stock.StockFrozenValue = (Convert.ToDouble(stock.StockFrozenValue) - price * amount).ToString();
                    }

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
        ///     期货冻结资金
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
                    FutureAccountDictionary[alias.Trim()].CashDeposit = (Convert.ToDouble(FutureAccountDictionary[alias.Trim()].CashDeposit) + AccountPARA.Factor * hand * price * AccountPARA.MarginValue).ToString();

                    //平仓盈亏改变

                    if(hand < 0)
                    {
                        double earning = Convert.ToDouble(FutureAccountDictionary[alias.Trim()].OffsetGain);
                         earning += (price - old_price) * hand;
                        FutureAccountDictionary[alias.Trim()].OffsetGain = earning.ToString();
                    }



                    //冻结资金改变
                    if (hand > 0)
                    {
                        //平仓不涉及冻期货结资金的改变
                        FutureAccountDictionary[alias.Trim()].FrozenValue = (Convert.ToDouble(FutureAccountDictionary[alias.Trim()].FrozenValue) - hand * price).ToString();
                    }
                }
                else
                {
                    FutureAccountTable future = DBAccessLayer.GetFutureAccount(alias.Trim());

                    future.CashDeposit = ((Convert.ToDouble(FutureAccountDictionary[alias.Trim()].CashDeposit)  + AccountPARA.Factor * hand * price * AccountPARA.MarginValue).ToString();

                    if(hand < 0){
                        double earning = Convert.ToDouble(FutureAccountDictionary[alias.Trim()].OffsetGain);
                        earning += (price - old_price) * hand;
                        FutureAccountDictionary[alias.Trim()].OffsetGain = earning.ToString();
                    }


                      if (hand > 0)
                      {
                          future.FrozenValue = (Convert.ToDouble(future.FrozenValue) - hand * price).ToString();
                      }

                    FutureAccountDictionary.Add(alias.Trim(),future);
                }
            }
        }


        public static AccountInfo UpdateAccountList(string alias)
        {

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