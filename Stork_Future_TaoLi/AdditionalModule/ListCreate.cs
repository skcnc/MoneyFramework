using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Variables_Type;
using System.Threading;
using Stork_Future_TaoLi.Modulars;
using System.Threading.Tasks;
//using MCStockLib;

namespace Stork_Future_TaoLi
{
    public enum  TradeDirection
    {
        Buy = 0,
        Sell = 1
    };

    /// <summary>
    /// 股票交易方向枚举
    /// </summary>
    public enum StockTradeDirection
    {
        Buy = "1",
        Sell = "2"
    }

    /// <summary>
    /// 期货交易方向枚举
    /// </summary>
    public enum FutureTradeDirectionEnum
    {
        Buy = "48",
        Sell = "49"
    }

    /// <summary>
    /// 开平方向枚举
    /// </summary>
    public enum OffsetFlagEnum
    {
        Open = "48",
        Close = "49"
    }

    /// <summary>
    /// 交易线程的流程控制参数
    /// </summary>
    public enum FutureTradeThreadStatus
    {
        DISCONNECTED = 0,   //线程尚未连接到CTP后台
        CONNECTED = 1 ,     //线程连入CTP，未登陆
        LOGIN = 2,          //线程连入CTP，已登陆
        BUSY = 3,
        FREE = 4,
        SYSERROR = 5
    }

    /// <summary>
    /// 交易状态
    /// PREORDER        ：   已经下单
    /// ORDERING        :    正在交易中
    /// ORDERFAILURE    :    交易失败
    /// ORDERCOMPLETED  :    交易完成
    /// </summary>
    public enum TradeDealStatus
    {
        PREORDER = 0,
        ORDERING = 1,
        ORDERFAILURE = 2,
        ORDERCOMPLETED = 3
    }

    
    /// <summary>
    /// 委托查询返回内容
    /// OrderCancel     :   委托取消
    /// UnOrder         :   未申报
    /// PreOrder        :   待申报
    /// Ordered         :   已申报
    /// WrongOrder      :   无效委托
    /// PartCanceled    :   部分撤销
    /// Canceled        :   已撤销
    /// PartDealed      :   部分成交
    /// Dealed          :   成交
    /// </summary>
    public enum EntrustStatus
    {
        OrderCancel = 76,
        UnOrder = 48,
        PreOrder = 49,
        Ordered = 50,
        WrongOrder = 52,
        PartCanceled = 53,
        Canceled = 54,
        PartDealed = 55,
        Dealed = 56
    }

}