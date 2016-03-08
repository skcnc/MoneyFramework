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

            for (int i = 0; i < mytraderoder.Count(); i++)
            {
                Thread.Sleep(1000);
                if (mytraderoder[i] == null) break;
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
            MoneyEntityEntities3 DbEntity = new MoneyEntityEntities3();
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
            DbEntity.SG_TAOLI_OPEN_TABLE.Remove((SG_TAOLI_OPEN_TABLE)_selectedItem.ToList()[0]);

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



            DbEntity.SG_TAOLI_CLOSE_TABLE.Remove(_selectedItem2.ToList()[0]);
            DbEntity.SaveChanges();

            SG_TAOLI_STATUS_TABLE item3 = new SG_TAOLI_STATUS_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = "test",
                SG_STATUS = 0,
                SG_UPDATE_TIME = DateTime.Now
            };

            DbEntity.SG_TAOLI_STATUS_TABLE.Add(item3);
            DbEntity.SaveChanges();

            var _selectedItem3 = (from item in DbEntity.SG_TAOLI_STATUS_TABLE where item.SG_ID == "test" select item);

            DbEntity.SG_TAOLI_STATUS_TABLE.Remove(_selectedItem3.ToList()[0]);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

            OL_TAOLI_LIST_TABLE item4 = new OL_TAOLI_LIST_TABLE()
            {
                OL_GUID = Guid.NewGuid(),
                SG_ID = "test",
                OL_LIST = "test",
                OL_TIME = DateTime.Now
            };

            DbEntity.OL_TAOLI_LIST_TABLE.Add(item4);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

            var selected4 = (from item in DbEntity.OL_TAOLI_LIST_TABLE where item.SG_ID == "test" select item);

            DbEntity.OL_TAOLI_LIST_TABLE.Remove(selected4.ToList()[0]);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

            ER_TAOLI_TABLE record5 = new ER_TAOLI_TABLE()
            {
                ER_GUID = Guid.NewGuid(),
                ER_ID = "test",
                //ER_STRATEGY = entrust.StrategyId,
                ER_ORDER_TYPE = "test",
                ER_ORDER_EXCHANGE_ID = "test",

                //ER_CODE = entrust.Code,
                //ER_DIRECTION = entrust.Direction
            };

            DbEntity.ER_TAOLI_TABLE.Add(record5);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

            var selected5 = (from item in DbEntity.ER_TAOLI_TABLE where item.ER_ID == "test" select item);

            DbEntity.ER_TAOLI_TABLE.Remove(selected5.ToList()[0]);
            DbEntity.SaveChanges();


            Thread.Sleep(10);

            DL_TAOLI_TABLE item6 = new DL_TAOLI_TABLE()
            {
                DL_GUID = Guid.NewGuid(),
                DL_STRATEGY = "test",
                DL_DIRECTION = "0",
                DL_CODE = "test",
                DL_NAME = "test",
                DL_STATUS = "test",
                DL_TYPE = "test",
                DL_STOCK_AMOUNT = 0,
                DL_BARGAIN_PRICE = 0,
                DL_BARGAIN_MONEY = 0,
                DL_BARGAIN_TIME = "test",
                DL_NO = "test",
                DL_LOAD = true
            };

            DbEntity.DL_TAOLI_TABLE.Add(item6);

            DbEntity.SaveChanges();


            Thread.Sleep(10);

            var _selecteditem6 = (from item in DbEntity.DL_TAOLI_TABLE where item.DL_STRATEGY == "test" select item);

            DbEntity.DL_TAOLI_TABLE.Remove(_selecteditem6.ToList()[0]);
            DbEntity.SaveChanges();

            Thread.Sleep(10);

            CC_TAOLI_TABLE item7 = new CC_TAOLI_TABLE()
            {
                CC_CODE = "test",
                CC_TYPE = "test",
                CC_AMOUNT = 0,
                CC_BUY_PRICE = 0,
                CC_USER = "test"
            };

            DbEntity.CC_TAOLI_TABLE.Add(item7);
            DbEntity.SaveChanges();

            Thread.Sleep(10);
            var _selecteditem7 = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_CODE == "test" select item);

            DbEntity.CC_TAOLI_TABLE.Remove(_selecteditem7.ToList()[0]);
            DbEntity.SaveChanges();

            
        }
    }
    
}