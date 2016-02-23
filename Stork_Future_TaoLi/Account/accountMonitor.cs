using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;
using Stork_Future_TaoLi.Hubs;
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

        private static List<AccountInfo> accountList = new List<AccountInfo>();

        private static double factor = 300; //股票对应市值系数

        public static void RUN()
        {
            excuteThread.Start();
            Thread.Sleep(1000);
        }

        private static void WorkThread(){
            while(true)
            {
                Thread.Sleep(1000);

                if (DateTime.Now.Second % 5 == 0)
                {
                    List<UserInfo> users = DBAccessLayer.GetUser();

                    foreach (UserInfo info in users)
                    {
                        if (AccountCalculate.Instance.checkAccountInfo(info.alias))
                        {
                            AccountCalculate.Instance.updateAccountInfo(info.alias, JsonConvert.SerializeObject(UpdateAccount(info)));
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

            account.alias = info.alias;
            account.name = info.name;
            account.account = info.stockAvailable;

            List<CCRecord> positionRecord = new List<CCRecord>();
            double stockcost = 0;

            PositionRecord.LoadCCStockList(info.alias, out positionRecord, out stockcost);

            List<CCRecord> fPositionRecord = new List<CCRecord>();

            PositionRecord.LoadCCFutureList(info.alias, out fPositionRecord);


            double fstock = 0;          //期货对应股票市值
            double fweight = 0;         //期货权益

            foreach (CCRecord record in fPositionRecord)
            {
                if (MarketPrice.market.ContainsKey(record.code))
                {
                    int x = -1;
                    if (record.direction == "0")
                    {
                        x = 1;
                    }

                    fstock += (record.amount) * MarketPrice.market[record.code] * factor * x;
                }
            }

            fstock = Math.Abs(fstock);

            account.fstockvalue = fstock.ToString();

            //期货权益







            account.cost = stockcost.ToString();

            foreach (CCRecord record in positionRecord)
            {
                account.positions.Add(new AccountPosition()
                {
                    amount = record.amount.ToString(),
                    code = record.code,
                    name = record.user,
                    price = record.price.ToString(),
                    type = record.type
                });

            }

            double frozen = 0;

            List<ERecord> entrusts = new List<ERecord>();
            EntrustRecord.GetUserAccountInfo(info.alias, out frozen, out entrusts);

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

            account.frozen = frozen.ToString();

            account.riskfrozen = account.riskfrozen;

            account.balance = (Convert.ToDouble(account.account) - Convert.ToDouble(account.cost) - Convert.ToDouble(account.frozen) - Convert.ToDouble(account.riskfrozen)).ToString();

            account.value = MarketPrice.CalculateCurrentValue(positionRecord).ToString();

            var tmp = (from item in accountList select item.alias);

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

        public static AccountInfo GetTestAccount(string name,out string result)
        {
            result = string.Empty;

            AccountInfo info = new AccountInfo()
            {
                alias = "testor",
                account = "10000000",
                balance = "7000000",
                cost = "3000000",
                faccount = "1000000",
                fbond = "300000",
                frozen = "20000",
                fvalue = "1200000",
                name = "testor",
                value = "1150000",
                positions = new List<AccountPosition>(),
                entrusts = new List<AccountEntrust>()
            };

            info.positions.Clear();
            info.positions.Add(new AccountPosition()
            {
                code = "600012",
                name = "testor",
                amount = "120",
                price = "25.12",
                type = "S"
            });
            info.positions.Add(new AccountPosition()
            {
                code = "600015",
                name = "testor",
                amount = "300",
                price = "1.34",
                type = "S"
            });
            info.positions.Add(new AccountPosition()
            {
                code = "600020",
                name = "testor",
                amount = "100",
                price = "7.18",
                type = "S"
            });
            return info;
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
        /// 可用资金量
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
        /// 风控冻结资金量
        /// 该值在风控模块从剩余资金量中冻结相对于股票购买成本的金额
        /// 获取委托号成功后解冻相应金额，计入冻结资金量(frozen)中
        /// </summary>
        public string riskfrozen { get; set; }

        /// <summary>
        /// 股票预估量
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 股票成本
        /// </summary>
        public string cost { get; set; }

        /// <summary>
        /// 期货资金量
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
}