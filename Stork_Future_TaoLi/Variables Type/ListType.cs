using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Variables_Type
{
    /// <summary>
    /// 登陆信息
    /// </summary>
    public struct LoginInfo
    {
        //地址
        public string serverAddr { get; set; }
        //端口
        public int PORT { get; set; }
        //资金
        public string ZjAccount { get; set; }
        //部门编号
        public string BROKER_ID { get; set; }
        //账户
        public string INVESTOR_ID { get; set; }
        //密码
        public string PASSWORD { get; set; }
    }

    /// <summary>
    /// 交易报单，（买卖，申购）
    /// </summary>
    public struct TradeOrderStruct
    {
        //交易部分 


        //交易所
        public string cExhcnageID { get; set; }
        //证券代码
        public string cSecurityCode { get; set; }
        //证券名称
        public string SecurityName { get; set; }
        //委托数量
        public long nSecurityAmount { get; set; }
        //委托价格
        public double dOrderPrice { get; set; }
        //买卖类别（见数据字典说明)
        public string cTradeDirection { get; set; }
        //开平标志
        public string cOffsetFlag { get; set; }
        //报单条件（限价，市价）
        public string cOrderPriceType { get; set; }

        //控制部分

        //证券类型
        public string cSecurityType { get; set; }
        //优先级
        public string cOrderLevel { get; set; }
        //报单执行细节
        public string cOrderexecutedetail { get; set; }

    }

    /// <summary>
    /// 交易类别：  b 购买 ， s 期货
    /// </summary>
    public static class TradeDirection
    {
        public static string Buy { get { return "1"; } }
        public static string Sell { get { return "2"; } }
    }

    /// <summary>
    /// 交易所： SH 上海 ， SZ 深圳
    /// </summary>
    public static class ExhcnageID
    {
        //上证
        public static string SH { get { return "SH"; } }
        //深证
        public static string SZ { get { return "SZ"; } }
    }

    /// <summary>
    /// 类型： STOCK 股票， FUTURE 期货
    /// </summary>
    public static class SecurityType
    {
        public static string STOCK { get { return "1"; } }
        public static string FUTURE { get { return "0"; } }
    }
}