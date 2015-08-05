using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Database;
using MCStockLib;
using System.Threading;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 数据库更新操作
    /// 通过线程池完成
    /// </summary>
    public static class DBAccessLayer
    {
        static MoneyEntityEntities1 DbEntity = new MoneyEntityEntities1();
       
        //数据库测试标记
        public static bool DBEnable = false;

        public static void InsertSGOPEN(object v)
        {
            OPENCREATE open = (OPENCREATE)v;
            //若发现存在相同策略ID的实例未完成，将未完成实例标记为“删除”，替换以当前实例
            //这种情况在启动自检测时出现

            DeleteSGOPEN(open.basic.ID);
                
            //等待上一步操作完成
            Thread.Sleep(10);


            SG_TAOLI_OPEN_TABLE record = new SG_TAOLI_OPEN_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = open.basic.ID,
                SG_Contract = open.CT,
                SG_OP_POINT = (double)open.OP,
                SG_HAND_NUM = (int)open.HD,
                SG_INDEX = int.Parse(open.INDEX.Trim()),
                SG_WEIGHT_LIST = open.weightli,
                SG_INIT_TRADE_LIST = open.orderli,
                SG_STATUS = 0,
                SG_CREATE_TIME = DateTime.Now,
                SG_LATEST_TRADE_LIST = open.orderli,
                SG_USER = open.basic.USER
            };

            DbEntity.SG_TAOLI_OPEN_TABLE.Add(record);
            DbEntity.SaveChanges();

        }

        public static void DeleteSGOPEN(string ID)
        {
            var _selectedItem = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

            if (_selectedItem.Count() > 0)
            {
                _selectedItem.ToList()[0].SG_STATUS = 1;
                DbEntity.SaveChanges();
            }

        }

        public static void InsertSGCLOSE(object v)
        {
            CLOSECREATE close = (CLOSECREATE)v;

            //若发现存在相同策略ID的实例未完成，将未完成实例标记为“删除”，替换以当前实例
            //这种情况在启动自检测时出现
            DeleteSGCLOSE(close.basic.ID);

            //等待上一步操作完成
            Thread.Sleep(10);

            SG_TAOLI_CLOSE_TABLE item = new SG_TAOLI_CLOSE_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = close.basic.ID,
                SG_OPEN_ID = close.OpenStraID,
                SG_INIT_POSITION_LIST = close.POSITION,
                SG_LATEST_POSITION_LIST = close.POSITION,
                SG_FUTURE_CONTRACT = close.CT,
                SG_SHORT_POINT = (int)close.SP,
                SG_HAND = close.HD,
                SG_COE = (double)close.COSTOFEQUITY,
                SG_SD = (double)close.STOCKDIVIDENDS,
                SG_SA = (double)close.STOCKALLOTMENT,
                SG_PE = (double)close.PROSPECTIVEARNINGS,
                SG_BAS = (double)close.OB,
                SG_STATUS = 0,
                SG_CREATE_TIME = DateTime.Now,
                SG_USER = close.basic.USER
            };

            DbEntity.SG_TAOLI_CLOSE_TABLE.Add(item);
            DbEntity.SaveChanges();


        }

        public static void DeleteSGCLOSE(string ID)
        {
            var _selectedItem = (from item in DbEntity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == ID && item.SG_STATUS == 0 select item);

            if (_selectedItem.Count() > 0)
            {
                _selectedItem.ToList()[0].SG_STATUS = 1;

                DbEntity.SaveChanges();
            }


        }

        public static void InsertSTATUS(string Id, int status)
        {
            SG_TAOLI_STATUS_TABLE item = new SG_TAOLI_STATUS_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = Id,
                SG_STATUS = status,
                SG_UPDATE_TIME = DateTime.Now
            };

            DbEntity.SG_TAOLI_STATUS_TABLE.Add(item);
            DbEntity.SaveChanges();
        }

        public static void InsertORDERLIST(string strategyId, string orderli)
        {
            OL_TAOLI_LIST_TABLE item = new OL_TAOLI_LIST_TABLE()
            {
                OL_GUID = Guid.NewGuid(),
                SG_ID = strategyId,
                OL_LIST = orderli,
                OL_TIME = DateTime.Now
            };

            DbEntity.OL_TAOLI_LIST_TABLE.Add(item);
            DbEntity.SaveChanges();
        }

        public static void DeleteORDERLIST(string strategyId)
        {
            var selected = (from item in DbEntity.OL_TAOLI_LIST_TABLE where item.SG_ID == strategyId select item);

            if(selected.Count() > 0)
            {
                DbEntity.OL_TAOLI_LIST_TABLE.Remove(selected.ToList()[0]);
                DbEntity.SaveChanges();
            }
        }

        /// <summary>
        /// 创建委托记录
        /// </summary>
        /// <param name="strategyId">策略id</param>
        /// <param name="orderId">委托编号</param>
        /// <param name="ordertype">委托类型</param>
        /// <param name="exchangeid">交易所</param>
        //public static void CreateERRecord(string strategyId,string orderId,string ordertype,string exchangeid,string code)
        public static void CreateERRecord(object item)
        {
            if (item == null) return;

            managedQueryEntrustorderstruct entrust = (managedQueryEntrustorderstruct)item;

            ER_TAOLI_TABLE record = new ER_TAOLI_TABLE()
            {
                ER_GUID = Guid.NewGuid(),
                ER_ID = entrust.cOrderSysID,
                ER_STRATEGY = entrust.StrategyId,
                ER_ORDER_TYPE = entrust.cSecurityType.ToString(),
                ER_ORDER_EXCHANGE_ID = entrust.cExchangeID,
                ER_CODE = entrust.Code
            };

            DbEntity.ER_TAOLI_TABLE.Add(record);
            DbEntity.SaveChanges();
        }

        /// <summary>
        /// 修改委托记录
        /// </summary>
        /// <param name="orderId">委托编号</param>
        /// <param name="orderStatus">委托状态</param>
        /// <param name="nVolumeTotalOriginal">委托数量</param>
        /// <param name="nVolumeTraded">今成交委托量</param>
        /// <param name="nVolumeTotal">剩余委托量</param>
        /// <param name="withdraw_ammount">撤单数量</param>
        /// <param name="frozen_money">冻结金额</param>
        /// <param name="frozen_amount">冻结数量</param>
        public static void UpdateERRecord(object ret)
        {
            managedEntrustreturnstruct record = (managedEntrustreturnstruct)ret;
            var selected = (from item in DbEntity.ER_TAOLI_TABLE where item.ER_ID == record.cOrderSysID && item.ER_CODE == record.cSecurity_code select item);

            if (selected.Count() > 0)
            {
                
                var item = selected.ToList()[0];
                item.ER_ORDER_STATUS = record.cOrderStatus.ToString();
                item.ER_VOLUME_TOTAL_ORIGINAL = record.nVolumeTotalOriginal;
                item.ER_VOLUME_TRADED = record.nVolumeTraded;
                item.ER_VOLUME_REMAIN = record.nVolumeTotal;
                item.ER_WITHDRAW_AMOUNT = record.withdraw_ammount;
                item.ER_FROZEN_MONEY = record.frozen_money;
                item.ER_FROZEN_AMOUNT = record.frozen_amount;
                
                //日期时间填写，待返回内容确认后完成
                //item.ER_DATE = record.cInsertDate;
                //item.ER_ORDER_TIME = record.cInsertTime;
                //item.ER_CANCEL_TIME = record.cCancelTime;

                DbEntity.SaveChanges();
            }


        }

        /// <summary>
        /// 添加交易记录
        /// </summary>
        /// <param name="ret"></param>
        public static void CreateDLRecord(object ret)
        {

        }

        /// <summary>
        /// 更新持仓
        /// </summary>
        /// <param name="change"></param>
        public static void UpdateCCRecord(object change) { }


        /// <summary>
        /// 获得上次退出时未完成的开仓实例
        /// 策略管理线程启动时执行
        /// </summary>
        public static List<OPENCREATE> GetInCompletedOPENStrategy()
        {
            var selected = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_STATUS == 0 select item);

            List<OPENCREATE> IncompletedStrategies = new List<OPENCREATE>();

            foreach (var item in selected.ToList())
            {
                IncompletedStrategies.Add(new OPENCREATE()
                {
                    basic = new Basic()
                    {
                        USER = item.SG_USER,
                        ACTIVITY = "OPENCREATE",
                        ORIENTATION = "1",
                        ID = item.SG_ID
                    },
                    OP = (float)item.SG_OP_POINT,
                    HD = (int)item.SG_HAND_NUM,
                    CT = item.SG_Contract,
                    INDEX = item.SG_INDEX.ToString(),
                    orderli = item.SG_LATEST_TRADE_LIST,
                    weightli = item.SG_WEIGHT_LIST
                });
            }

            return IncompletedStrategies;
        }


        /// <summary>
        /// 获得上次退出时未完成的平仓实例
        /// </summary>
        /// <returns></returns>
        public static List<CLOSECREATE> GetInCompletedCLOSEStrategy()
        {
            var selected = (from item in DbEntity.SG_TAOLI_CLOSE_TABLE where item.SG_STATUS == 0 select item);

            List<CLOSECREATE> IncompletedStrategies = new List<CLOSECREATE>();

            foreach (var item in selected.ToList())
            {
                IncompletedStrategies.Add(new CLOSECREATE()
                {
                    basic = new Basic()
                    {
                        USER = item.SG_USER,
                        ACTIVITY = "CLOSECREATE",
                        ORIENTATION = "0",
                        ID = item.SG_ID
                    },
                    CT = item.SG_FUTURE_CONTRACT,
                    SP = (float)item.SG_SHORT_POINT,
                    HD = (int)item.SG_HAND,
                    POSITION = item.SG_LATEST_POSITION_LIST,
                    COSTOFEQUITY = (decimal)item.SG_COE,
                    STOCKDIVIDENDS = (decimal)item.SG_SD,
                    STOCKALLOTMENT = (decimal)item.SG_SA,
                    PROSPECTIVEARNINGS = (decimal)item.SG_PE,
                    OB = (float)item.SG_BAS
                });
            }

            return IncompletedStrategies;
        }

    }
}