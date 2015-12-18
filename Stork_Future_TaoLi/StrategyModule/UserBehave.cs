using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class Basic
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

        /// <summary>
        /// 调试标志位
        /// 如果为true，生成的List不会用于真实的交易
        /// </summary>
        public bool DEBUGMODE {
            get
            {
                return true;
            }
        }

        public String ID { get; set; }
    }

    /// <summary>
    /// 创建开仓
    /// </summary>
    public class OPENCREATE
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
        public String orderli { get; set; }

        /// <summary>
        /// 权重文件
        /// </summary>
        public String weightli { get; set; }
    }

    /// <summary>
    /// 修改开仓实例
    /// </summary>
    public class OPENMODIFY
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
        public String POSITION { get; set; }

        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 权重文件
        /// </summary>
        public String weightli { get; set; }
    }

    /// <summary>
    /// 启动开仓运行
    /// </summary>
    public class OPENRUN
    {
        public Basic basic { set; get; }
        public bool RUN { get; set; }
    }

    /// <summary>
    /// 允许开仓交易
    /// </summary>
    class OPENALLOW {
        public Basic basic { set; get; }
        public bool ALLOW { get; set; }
    }

    /// <summary>
    /// 删除开仓
    /// </summary>
    class OPENDELETE {
        public Basic basic { set; get; }
    }

    /// <summary>
    /// 创建平仓
    /// </summary>
    public class CLOSECREATE
    {
        public Basic basic { get; set; }

        /// <summary>
        /// 权重文件
        /// </summary>
        public String WEIGHT { get; set; }

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
        public String POSITION { get; set; }

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
        /// prospective earnings
        /// </summary>
        public decimal PROSPECTIVEARNINGS { get; set; }

        /// <summary>
        /// 开仓基差
        /// Open Basis
        /// </summary>
        public float OB { get; set; }

        /// <summary>
        /// 预付费率
        /// </summary>
        public float CHARGE { get; set; }

        /// <summary>
        /// 平仓对应的开仓ID
        /// </summary>
        public string Open_ID { get; set; }
    }

    public class CLOSEMODIFY
    {

        /// <summary>
        /// 修改平仓实例ID
        /// </summary>
        public String ID { get; set; }

        public Basic basic { get; set; }

        /// <summary>
        /// 权重文件
        /// </summary>
        public String WEIGHT { get; set; }

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
        public String POSITION { get; set; }

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
        /// prospective earnings
        /// </summary>
        public decimal PROSPECTIVEARNINGS { get; set; }

        /// <summary>
        /// 开仓基差
        /// Open Basis
        /// </summary>
        public float OB { get; set; }

        /// <summary>
        /// 预付费率
        /// </summary>
        public float CHARGE { get; set; }
    }

    /// <summary>
    /// 启动平仓运行
    /// </summary>
    public class CLOSERUN
    {
        public Basic basic { get; set; }
        public bool RUN { get; set; }
    }

    /// <summary>
    /// 允许平仓交易
    /// </summary>
    public class CLOSEALLOW
    {
        public Basic basic { get; set; }
        public bool ALLOW { get; set; }
    }

    /// <summary>
    /// 删除平仓
    /// </summary>
    public class CLOSEDELETE
    {
        public Basic basic { get; set; }
    }

    public class MakeOrder
    {
        //用户名
        public String User { get; set; }
        //交易所代码
        public string exchangeId { get; set; }
        //证券代码
        public string cSecurityCode { get; set; }
        //委托数量
        public long nSecurityAmount { get; set; }
        //委托价格
        public double dOrderPrice { get; set; }
        //买卖类别（见数据字典说明)
        public string cTradeDirection { get; set; }
        //开平标志
        public string cOffsetFlag { get; set; }
        //证券类型
        public string cSecurityType { get; set; }
        // 列表所属策略实例ID
        public string belongStrategy { get; set; }
        // 本地委托编号
        public int OrderRef { get; set; }
    }



    /// <summary>
    /// 平仓实例匹配
    /// </summary>
    public class SEARCHSTRATEGY
    {
        public Basic basic { get; set; }

        public String CONTRACT { get; set; }

        public String OPENPOINT { get; set; }

        public String INDEX { get; set; }
    }

}