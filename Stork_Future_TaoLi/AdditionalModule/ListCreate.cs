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

    public enum FutureTradeStatus
    {
        DISCONNECTED = 0,
        CONNECTED = 1 ,
        LOGIN = 2,
        ORDERINSERT = 3,
        ORDERWRONG = 4,
        ORDERDONE = 5,
        TRADEINSERT = 6,
        TRADEDONE = 7,
        SYSERROR = 8
    }

    public class LocalMarketPrice
    {
    }
}