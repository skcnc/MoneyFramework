using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.AdditionalModule
{
    public class EntrustRecord
    {

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
    }
}