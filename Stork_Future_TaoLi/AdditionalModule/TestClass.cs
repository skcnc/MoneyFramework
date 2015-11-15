using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;
using Stork_Future_TaoLi.Hubs;
using Newtonsoft.Json;
using MCStockLib;

namespace Stork_Future_TaoLi
{
    public class TestClass
    {
        public void Run()
        {
            Thread excutedThread = new Thread(new ThreadStart(ThreadProc));
            excutedThread.Start();

            Thread.Sleep(1000);
        }

        private void ThreadProc()
        {
            
        }
    }

    public class DebugMode
    {
        public static bool debug = false;
        public static String TestUser = "Testor";
    }

    public class StockTradeTest
    {
        public bool SingleTradeTest(TradeOrderStruct_M mytraderoder, out QueryEntrustOrderStruct_M myEntrust, out string Errormsg)
        {
            myEntrust = new QueryEntrustOrderStruct_M();

            myEntrust.Code = mytraderoder.SecurityCode;
            myEntrust.Direction = mytraderoder.TradeDirection;
            myEntrust.ExchangeID = mytraderoder.ExchangeID;
            myEntrust.OrderPrice = mytraderoder.OrderPrice;
            myEntrust.SecurityType = mytraderoder.SecurityType;
            Random rando = new Random();

            myEntrust.OrderSysID = "T" + rando.Next(1, 99999).ToString();

            Errormsg = "success";

            return true;
        }

        public bool BatchTradeTest(TradeOrderStruct_M[] mytraderoder, int nSize, out QueryEntrustOrderStruct_M[] myEntrust, out string Errormsg)
        {
            myEntrust = new QueryEntrustOrderStruct_M[nSize];

            for (int i = 0; i < nSize; i++)
            {
                myEntrust[i] = new QueryEntrustOrderStruct_M();

                myEntrust[i].Code = mytraderoder[i].SecurityCode;
                myEntrust[i].Direction = mytraderoder[i].TradeDirection;
                myEntrust[i].ExchangeID = mytraderoder[i].ExchangeID;
                myEntrust[i].OrderPrice = mytraderoder[i].OrderPrice;
                myEntrust[i].SecurityType = mytraderoder[i].SecurityType;

                Random rando = new Random();

                myEntrust[i].OrderSysID = "T" + rando.Next(1, 99999).ToString();
            }

            Errormsg = "success";

            return true;
        }

        public managedEntrustreturnstruct QueryEntrust(QueryEntrustOrderStruct_M queryEntrust)
        {
            managedEntrustreturnstruct entrust = new managedEntrustreturnstruct();

            entrust.cCancelTime = String.Empty;
            entrust.cInsertDate = DateTime.Now.Year.ToString() + DateTime.Now.Month + DateTime.Now.Day;
            entrust.cInsertTime = DateTime.Now.Hour.ToString() + DateTime.Now.Minute + DateTime.Now.Second;
            entrust.cOrderStatus = ((int)(EntrustStatus.Dealed));
            entrust.cOrderSysID = queryEntrust.OrderSysID;
            entrust.cOrderType = queryEntrust.SecurityType;
            entrust.frozen_amount = 0;
            entrust.frozen_money = 0;
            entrust.nVolumeTotal = 0;
            entrust.nVolumeTotalOriginal = 0;
            entrust.nVolumeTraded = 0;
            entrust.security_name = "TEST CODE";
            entrust.withdraw_ammount = 0;

            return entrust;
        }

        public managedBargainreturnstruct QueryTrader(QueryEntrustOrderStruct_M queryEntrust)
        {
            managedBargainreturnstruct bargain = new managedBargainreturnstruct();

            bargain.bargain_money = 0;
            bargain.bargain_price = 0;
            bargain.bargain_time = DateTime.Now.Hour.ToString() + DateTime.Now.Minute + DateTime.Now.Second;
            bargain.bargain_no = queryEntrust.OrderRef;
            bargain.direction = queryEntrust.Direction;
            bargain.OrderStatus = Convert.ToSByte(300);
            bargain.OrderSysID = queryEntrust.OrderSysID;
            bargain.OrderType = queryEntrust.SecurityType;
            bargain.Security_code = queryEntrust.Code;
            bargain.Security_name = "TEST CODE";
            bargain.stock_amount = 0;
            bargain.strategyId = queryEntrust.StrategyId;

            return bargain;
        }
    }
    
}