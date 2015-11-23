using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;
using Stork_Future_TaoLi.Hubs;
using Newtonsoft.Json;
using MCStockLib;
using Stork_Future_TaoLi.Database;

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
        public static String TestUser = "testor";
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
            bargain.OrderStatus = Convert.ToSByte(EntrustStatus.Dealed);
            bargain.OrderSysID = queryEntrust.OrderSysID;
            bargain.OrderType = queryEntrust.SecurityType;
            bargain.Security_code = queryEntrust.Code;
            bargain.Security_name = "TEST CODE";
            bargain.stock_amount = 0;
            bargain.strategyId = queryEntrust.StrategyId;

            return bargain;
        }
    }

    public class DBExamination
    {
        /// <summary>
        /// 尝试数据库的访问和修改
        /// </summary>
        public static void CheckDB()
        {
             MoneyEntityEntities1 DbEntity = new MoneyEntityEntities1();
            SG_TAOLI_OPEN_TABLE record = new SG_TAOLI_OPEN_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = "Test",
                SG_Contract = "test",
                SG_OP_POINT = 0,
                SG_HAND_NUM = 0,
                SG_INDEX = 0,
                SG_WEIGHT_LIST = "test",
                SG_INIT_TRADE_LIST = "test",
                SG_STATUS = 0,
                SG_CREATE_TIME = DateTime.Now,
                SG_LATEST_TRADE_LIST = "test",
                SG_USER = "test",
            };

            DbEntity.SG_TAOLI_OPEN_TABLE.Add(record);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

             var _selectedItem = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_ID == "Test" select item);
            DbEntity.SG_TAOLI_OPEN_TABLE.Remove((SG_TAOLI_OPEN_TABLE) _selectedItem.ToList()[0]);

            DbEntity.SaveChanges();

            Thread.Sleep(10);
            
            SG_TAOLI_CLOSE_TABLE item2 = new SG_TAOLI_CLOSE_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = "test",
                SG_OPEN_ID = "test",
                SG_INIT_POSITION_LIST = "test",
                SG_LATEST_POSITION_LIST = "test",
                SG_FUTURE_CONTRACT = "test",
                SG_SHORT_POINT = 0,
                SG_HAND = 0,
                SG_COE = 0,
                SG_SD = 0,
                SG_SA = 0,
                SG_PE = 0,
                SG_BAS = 0,
                SG_STATUS = 0,
                SG_CREATE_TIME = DateTime.Now,
                SG_USER = "test"
            };

            DbEntity.SG_TAOLI_CLOSE_TABLE.Add(item2);
            DbEntity.SaveChanges();

            var _selectedItem2 = (from item in DbEntity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == "test" && item.SG_STATUS == 0 select item);
            
            if (_selectedItem.Count() > 0)
            {

                DbEntity.SG_TAOLI_CLOSE_TABLE.Remove(_selectedItem2.ToList()[0]);
                DbEntity.SaveChanges();
            }
        }
    }
    
}