using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    class Basic
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public String USER { get; set; }

        /// <summary>
        /// 通讯类型
        ///OPENCREATE  创建开仓实例
        ///OPENMODIFY  修改开仓实例
        ///OPENRUN     启动开仓实例运行
        ///OPENALLOW   允许开仓实例交易
        ///OPENDELETE  删除开仓实例
        ///CLOSECREATE 创建平仓实例
        ///CLOSEMODIFY 修改平仓实例
        ///CLOSERUN    启动平仓实例运行
        ///CLOSEALLOW  允许平仓实例交易
        ///CLOSEDELETE 删除平仓实例
        /// </summary>
        public String ACTIVITY { get; set; }

        /// <summary>
        /// 交易类型
        /// 0： 平仓类交易
        /// 1： 开仓类交易
        /// </summary>
        public String ORIENTATION { get; set; }
    }

    /// <summary>
    /// 创建开仓
    /// </summary>
    class OPENCREATE
    {
        public Basic basic { get; set; }
        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 合约
        /// </summary>
        public string CT { get; set; }

        /// <summary>
        /// 指数类型
        /// </summary>
        public string INDEX { get; set; }
        /// <summary>
        /// 权重参数
        /// 股票代码;类型;权重值
        /// </summary>
        public IList<String> WEIGHTING { get; set; }
    }

    /// <summary>
    /// 修改开仓实例
    /// </summary>
    class OPENMODIFY
    {
        public Basic basic { get; set; }

        /// <summary>
        /// 被修改实例ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 持仓列表参数
        /// 股票代码;类型;持仓数量
        /// </summary>
        public IList<String> POSITION { get; set; }

        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }
    }

    /// <summary>
    /// 启动开仓运行
    /// </summary>
    class OPENRUN {
        public Basic basic { set; get; }

        public String ID { get; set; }
    }

    /// <summary>
    /// 允许开仓交易
    /// </summary>
    class OPENALLOW {
        public Basic basic { set; get; }

        public String ID { get; set; }
    }

    /// <summary>
    /// 删除开仓
    /// </summary>
    class OPENDELETE {
        public Basic basic { set; get; }

        public String ID { get; set; }
    }

    /// <summary>
    /// 创建平仓
    /// </summary>
    class CLOSECREATE {
        public Basic basic { get; set; }


        /// <summary>
        /// 合约
        /// </summary>
        public string CT { get; set; }

        /// <summary>
        /// 空头点位
        /// Short of Point
        /// </summary>
        public float SP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 持仓列表参数
        /// 股票代码;类型;持仓数量
        /// </summary>
        public IList<String> POSITION { get; set; }

        /// <summary>
        /// 股票成本
        /// </summary>
        public decimal COSTOFEQUITY { get; set; }

        /// <summary>
        /// 股票分红
        /// </summary>
        public decimal STOCKDIVIDENDS { get; set; }

        /// <summary>
        /// 股票配股
        /// </summary>
        public decimal STOCKALLOTMENT { get; set; }

        /// <summary>
        /// 预期收益
        /// Prospective yield
        /// </summary>
        public decimal PY { get; set; }

        /// <summary>
        /// 开仓基差
        /// Open Basis
        /// </summary>
        public float OB { get; set; }
    }

    class CLOSEMODIFY
    {
        public Basic basic { get; set; }

        /// <summary>
        /// 修改平仓实例ID
        /// </summary>
        public String ID { get; set; }

        /// <summary>
        /// 空头点位
        /// Short of Point
        /// </summary>
        public float SP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 持仓列表参数
        /// 股票代码;类型;持仓数量
        /// </summary>
        public IList<String> POSITION { get; set; }

        /// <summary>
        /// 股票成本
        /// </summary>
        public decimal COSTOFEQUITY { get; set; }

        /// <summary>
        /// 股票分红
        /// </summary>
        public decimal STOCKDIVIDENDS { get; set; }

        /// <summary>
        /// 股票配股
        /// </summary>
        public decimal STOCKALLOTMENT { get; set; }

        /// <summary>
        /// 预期收益
        /// Prospective yield
        /// </summary>
        public decimal PY { get; set; }

        /// <summary>
        /// 开仓基差
        /// Open Basis
        /// </summary>
        public float OB { get; set; }
    }

    /// <summary>
    /// 启动平仓运行
    /// </summary>
    class CLOSERUN {
        public Basic basic { get; set; }

        public String ID { get; set; }
    }

    /// <summary>
    /// 允许平仓交易
    /// </summary>
    class CLOSEALLOW
    {
        public Basic basic { get; set; }

        public String ID { get; set; }
    }

    /// <summary>
    /// 删除平仓
    /// </summary>
    class CLOSEDELETE
    {
        public Basic basic { get; set; }

        public String ID { get; set; }
    }
}