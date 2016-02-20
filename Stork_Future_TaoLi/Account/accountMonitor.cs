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
        private static AccountInfo UpdateAccount(UserInfo info)
        {
            AccountInfo account = new AccountInfo();

            account.alias = info.alias;
            account.name = info.name;
            account.account = info.stockAvailable;

            List<CCRecord> positionRecord = new List<CCRecord>();
            double stockcost = 0;

            PositionRecord.LoadCCList(info.alias, out positionRecord, out stockcost);

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

            account.balance = (Convert.ToDouble(account.account) - Convert.ToDouble(account.cost) - Convert.ToDouble(account.frozen)).ToString();

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
    }


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
        /// 股票预估量
        /// </summary>
        public string value { get; set; }


        /// <summary>
        /// 股票成本
        /// </summary>
        public string cost { get; set; }

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
    public class AccountPosition{
        /// <summary>
        /// 代码
        /// </summary>
        public string code {get;set;}

        /// <summary>
        /// 类型
        /// </summary>
        public string type{get;set;}

        /// <summary>
        /// 数量
        /// </summary>
        public string amount {get;set;}

        /// <summary>
        /// 价格
        /// </summary>
        public string price {get;set;}

        /// <summary>
        /// 用户名
        /// </summary>
        public string name{get;set;}
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