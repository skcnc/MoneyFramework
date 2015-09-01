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
    /// 期货交易RequestID 分发类
    /// </summary>
    public class REQUEST_ID
    {
        private static object _syncRoot = new object();
        private static int _id = 0;

        /// <summary>
        /// 申请新的ID值
        /// </summary>
        /// <returns>id值</returns>
        public int ApplyNewID()
        {
            lock (_syncRoot)
            {
                _id++;
                return _id;
            }
        }

    }
}