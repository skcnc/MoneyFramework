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
        public static int ReduceEntrustPosition(int OrderRef, int count)
        {
            var tmp = (from item in EntrustRecordList where item.OrderRef == OrderRef select item);

            if(tmp.Count() == 0)
            {
                GlobalErrorLog.LogInstance.LogEvent("委托缓存减持失败，原因是委托不存在或已经删除。");
                return 1;
            }



            if (tmp.ToList()[0].Amount < count)
            {
                GlobalErrorLog.LogInstance.LogEvent("委托缓存减持失败，原因是委托剩余量小于成交量。");
                EntrustRecordList.Remove(tmp.ToList()[0]);
                return 2;
            }

            try
            {
                tmp.ToList()[0].Amount -= count;
            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("委托缓存减持失败，原因是：" + ex.ToString());
                return 3;
            }

            return 0;
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
    }
}