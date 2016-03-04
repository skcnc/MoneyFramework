using Stork_Future_TaoLi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 本地委托变量，保存正在查询成交，尚未成交的委托
    /// </summary>
    public class EntrustRecord
    {
        public static List<ERecord> EntrustRecordList = new List<ERecord>();


        /// <summary>
        /// 添加新委托
        /// </summary>
        /// <param name="record"></param>
        /// <returns>
        /// 0： 添加成功
        /// 1： record orderRef 冲突， 可能该委托已经存在
        /// 2： log中查看错误具体信息 
        /// </returns>
        public static int AddEntrustRecord(ERecord record)
        {
            var tmp = (from item in EntrustRecordList where item.OrderRef == record.OrderRef select item);

            if(tmp.Count() == 0)
            {
                GlobalErrorLog.LogInstance.LogEvent("添加委托失败，原因是该委托已经存在，策略号： " + record.StrategyNo + "  代码： " + record.Code + "  本地编号： " + record.OrderRef);
                return 1;
            }

            try
            {
                EntrustRecordList.Add(record);
            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("添加委托失败，原因：" + ex.ToString());
                return 2;
            }

            return 0;
        }


        /// <summary>
        /// 减持委托数量
        /// </summary>
        /// <param name="OrderRef">本地编号</param>
        /// <param name="count">减持数量</param>
        /// <returns>
        /// 0：减持成功
        /// 1：委托不存在
        /// 2：委托量小于成交量
        /// 3：其他失败原因
        /// </returns>
        public static int ModifyEntrustPosition(int OrderRef, int dealAmount, double FrozenMoney)
        {
            var tmp = (from item in EntrustRecordList where item.OrderRef == OrderRef select item);

            if(tmp.Count() == 0)
            {
                GlobalErrorLog.LogInstance.LogEvent("委托缓存减持失败，原因是委托不存在或已经删除。");
                return 1;
            }

            tmp.ToList()[0].DealAmount = dealAmount;
            tmp.ToList()[0].DealFrezonMoney = FrozenMoney;
            
            return 0;
        }

        /// <summary>
        /// 删除委托缓存
        /// </summary>
        /// <param name="OrderRef"></param>
        /// <returns>
        /// 0： 删除成功
        /// 1： 删除失败
        /// </returns>
        public static int DeleteEntrustRecord(int OrderRef)
        {
            var tmp = (from item in EntrustRecordList where item.OrderRef == OrderRef select item);

            if (tmp.Count() == 0)
            {
                GlobalErrorLog.LogInstance.LogEvent("删除委托失败，原因是原委托不存在，委托号： " + OrderRef );
                return 1;
            }

            if (tmp.ToList()[0].DealAmount != tmp.ToList()[0].Amount)
            {
                GlobalErrorLog.LogInstance.LogEvent("删除委托报警，原因是成交量和申报量不符，委托号： " + OrderRef + "   股票代码： " + tmp.ToList()[0].Code + "   申报量：" + tmp.ToList()[0].Amount + "   成交量：" + tmp.ToList()[0].DealAmount);
            }

            EntrustRecordList.Remove(tmp.ToList()[0]);

            return 0;
        }

        /// <summary>
        /// 获取委托中冻结资金量
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="frozenMoney">冻结资金量</param>
        public static void GetUserAccountInfo(string user, out double frozenMoney, out List<ERecord> record)
        {
            frozenMoney = 0;

            if (user == "*")
            {
                var tmp = (from item in EntrustRecordList select item);
                record = tmp.ToList();
            }
            else
            {
                var tmp = (from item in EntrustRecordList where item.UserName == user select item);
                record = tmp.ToList();
            }

           

            if (record.Count() > 0)
            {
                foreach (var i in record.ToList())
                {
                    frozenMoney += i.DealFrezonMoney;
                }
            }
        }
    }

    /// <summary>
    /// 尚未完成委托记录
    /// </summary>
    public class ERecord
    {
        /// <summary>
        /// 交易所属用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 交易所属策略
        /// </summary>
        public string StrategyNo { get; set; }

        /// <summary>
        /// 交易代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 交易所
        /// </summary>
        public string ExchangeId { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public double OrderPrice { get; set; }

        /// <summary>
        /// 本地委托编号
        /// </summary>
        public int OrderRef { get; set; }

        /// <summary>
        /// 交易所返回委托编号
        /// </summary>
        public string SysOrderRef { get; set; }

        /// <summary>
        /// 已成交数量
        /// </summary>
        public int DealAmount { get; set; }

        /// <summary>
        /// 冻结金额
        /// </summary>
        public double DealFrezonMoney { get; set; }

        /// <summary>
        /// 股票买入卖出标记
        /// </summary>
        public string Direction { get; set; }
    }

    /// <summary>
    /// 持仓记录
    /// </summary>
    public class PositionRecord
    {
        ///// <summary>
        ///// 持仓列表记录
        ///// </summary>
        //public static List<CCRecord> CCRecordList = new List<CCRecord>();


        ///// <summary>
        ///// 更新持仓列表
        ///// </summary>
        ///// <param name="record">新持仓记录</param>
        //public static void UpdateCCRecord(CCRecord record)
        //{

        //    if(record.type == "0")
        //    {
        //        //股票
        //        var tmp = (from item in CCRecordList where item.code == record.code && item.type == record.type && item.user == record.user  select item);

        //        if (tmp.Count() == 0)
        //        {
        //            CCRecordList.Add(record);
        //        }
        //        else
        //        {
        //            tmp.ToList()[0].amount = record.amount;
        //            tmp.ToList()[0].price = record.price;
        //        }

        //    }
        //    else
        //    {
        //        //期货
        //        var tmp = (from item in CCRecordList where item.code == record.code && item.type == record.type && item.user == record.user && item.direction == record.direction select item);

        //        if (tmp.Count() == 0)
        //        {
        //            CCRecordList.Add(record);
        //        }
        //        else
        //        {
        //            if (record.amount > 0)
        //            {
        //                tmp.ToList()[0].amount = record.amount;
        //                tmp.ToList()[0].price = record.price;
        //            }
        //            else
        //            {
        //                DeleteCCRecord(record);
        //            }
                   
        //        }
        //    }
        //}

        //public static void DeleteCCRecord(CCRecord record)
        //{
        //    var tmp = (from item in CCRecordList where item.user == record.user && item.code == record.code && item.type == record.type && item.direction == record.direction select item);

        //    if (tmp.Count() > 0)
        //    {
        //        CCRecordList.Remove(tmp.ToList()[0]);
        //    }
        //}

        /// <summary>
        /// 获取用户股票持仓
        /// </summary>
        /// <param name="userName">用户名,如果为'*'则查看所有</param>
        /// <param name="records">股票持仓记录</param>
        /// <param name="stockcost">股票成本</param>
        //public static void LoadCCStockList(string userName, out List<CCRecord> records , out double stockcost)
        //{
        //    records = new List<CCRecord>();
        //    stockcost = 0;


         
        //    if (userName != "*")
        //    {
        //        var tmp = (from item in CCRecordList where item.user == userName && item.type == "49" select item);
        //        records = tmp.ToList();
        //    }
        //    else
        //    {
        //        var tmp = (from item in CCRecordList where item.type == "49"  select item);
        //        records = tmp.ToList();
        //    }



        //    if (records.Count() > 0)
        //    {

        //        foreach (var i in records)
        //        {
        //            records.Add(i);
        //            stockcost += (i.amount * i.price);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取期货成交列表
        /// </summary>
        /// <param name="userName">用户名，如果为'*'则查看所有</param>
        /// <param name="records">期货持仓记录</param>
        //public static void LoadCCFutureList(string userName, out List<CCRecord> records)
        //{
        //    records = new List<CCRecord>();

        //    if (userName != "*")
        //    {
        //        var tmp = (from item in CCRecordList where item.user == userName && item.type == "49" select item);
        //        records = tmp.ToList();
        //    }
        //    else
        //    {
        //        var tmp = (from item in CCRecordList where item.type== "49" select item);
        //        records = tmp.ToList();
        //    }



        //    if (records.Count() > 0)
        //    {

        //        foreach (var i in records)
        //        {
        //            records.Add(i);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// 持仓记录类型
    /// </summary>
    public class CCRecord
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 持仓数量
        /// </summary>
        public int amount { get; set; }

        /// <summary>
        /// 持仓单价
        /// </summary>
        public double price { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string user { get; set; }

        /// <summary>
        /// 交易方向
        /// </summary>
        public string direction { get; set; }

        /// <summary>
        /// 开平标志
        /// </summary>
        public string offsetflag { get; set; }
    }

    /// <summary>
    /// 最新行情记录
    /// </summary>
    public class MarketPrice
    {
        /// <summary>
        /// 行情列表
        /// </summary>
        public static Dictionary<string, double> market = new Dictionary<string, double>();

        /// <summary>
        /// 计算当前股票市值
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        public static double CalculateCurrentValue(List<CC_TAOLI_TABLE> records)
        {
            double value = 0;

                foreach (CC_TAOLI_TABLE i in records)
                {

                    if (market.Keys.Contains(i.CC_CODE))
                    {
                        if (market[i.CC_CODE] == 0)
                        {
                            value += Convert.ToDouble(i.CC_BUY_PRICE * i.CC_AMOUNT);
                        }
                        else
                        {
                            value += Convert.ToDouble(market[i.CC_CODE] / 1000 * i.CC_AMOUNT);
                        }
                    }
                    else
                    {
                        value += Convert.ToDouble(i.CC_BUY_PRICE * i.CC_AMOUNT);
                    }

                }

                return value;


        }
    }
}