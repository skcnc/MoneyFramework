using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Stork_Future_TaoLi.Test
{
    public class TestClass
    {
        static EventLog eventlog = new EventLog();

        public static void Writelog(string msg)
        {
            eventlog.WriteEntry(msg);
        }

        public static bool isRun = true;
    }

    //public class StockTradeTest
    //{
    //    TraderBassClass.CStockTrader _StockTradeDLLApi = new TraderBassClass.CStockTrader();

    //    /// <summary>
    //    /// 连接服务器
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool init()
    //    {
    //        unsafe
    //        {
    //            TraderBassClass.Logininfor vlogin = new TraderBassClass.Logininfor();

    //            /* ToDu ***********/
    //            /* 登陆参数初始化 */

    //            string str = "Hello Cheng";
    //            sbyte[] bytes = new sbyte[1000];

    //            fixed (sbyte* pError = bytes)
    //            {
    //                //_StockTradeDLLApi.init2(pError);
    //            }





    //        }
            
    //        return true;
    //    }

    //    /// <summary>
    //    /// 单个证券交易
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool trader()
    //    {
    //        return true;
    //    }

    //    /// <summary>
    //    /// 多个证券交易
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool Batchstocktrader()
    //    {
    //        return true;
    //    }

    //    /// <summary>
    //    /// 撤单
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool canceltrader()
    //    { return true; }

    //    /// <summary>
    //    /// 查询委托
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool queryorder()
    //    { return true; }

    //    /// <summary>
    //    /// 查询成交
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool querytrader()
    //    {
    //        return true;
    //    }

    //    /// <summary>
    //    /// 查询资金状态
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool queryBalanceSheet()
    //    {
    //        return true;
    //    }

    //    /// <summary>
    //    /// 返回连接情况
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool getconnectstate()
    //    { return true; }


    //    /// <summary>
    //    /// 返回是否被占用
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool getworkstate()
    //    { return true; }


    //    /// <summary>
    //    /// 发送心跳
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool heartBeat()
    //    {
    //        return true;
    //    }

    //}
}