using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Database;
using MCStockLib;
using System.Threading;
using Newtonsoft.Json;
using Stork_Future_TaoLi.Account;


namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 数据库更新操作
    /// 通过线程池完成
    /// </summary>
    public static class DBAccessLayer
    {
        static MoneyEntityEntities1 DbEntity = new MoneyEntityEntities1();
        static object ERtableLock = new object();
        static object DLtableLock = new object();
        static object DBChangeLock = new object();
        //数据库测试标记
        public static bool DBEnable = true;

        /// <summary>
        /// 获取黑白名单
        /// </summary>
        /// <returns>股票黑白名单</returns>
        public static List<BWNameTable> GetWBNamwList()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            var tmp = (from item in DbEntity.BWNameTable select item);

            if (tmp.Count() == 0) return null;
            else return tmp.ToList();
        }

        /// <summary>
        /// 设定黑白名单
        /// </summary>
        /// <param name="records">名单</param>
        /// <returns>成功状态</returns>
        public static bool SetWBNameList(List<BWNameTable> records)
        {
            if (DBAccessLayer.DBEnable == false) return false;

            List<BWNameTable> oldRecords = (from item in DbEntity.BWNameTable select item).ToList();

            for (int i = 0; i < oldRecords.Count; i++)
            {
                DbEntity.BWNameTable.Remove(oldRecords[i]);
            }

            foreach (BWNameTable record in records)
            {
                DbEntity.BWNameTable.Add(record);
            }

            Dbsavechage("BWNameTable");

            return true;
            
        }

        public static void AddRiskRecord(string alias , string err, string strid)
        {

            if (DBAccessLayer.DBEnable == false) { return; }

            RISK_TABLE record = new RISK_TABLE()
            {
                ID = Guid.NewGuid(),
                alias = alias,
                date = DateTime.Now.Date,
                strategy_id = strid,
                err = err,
                time = DateTime.Now
            };

            DbEntity.RISK_TABLE.Add(record);
            Dbsavechage("RISK_TABLE");
        }

        public static List<UserInfo> GetUser()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            var tmp = (from item in DbEntity.UserInfo select item);

            if (tmp.Count() == 0) return null;
            else return tmp.ToList();
        }

        public static UserInfo GetOneUser(string alias)
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            var tmp = (from item in DbEntity.UserInfo select item);

            if (tmp.Count() == 0) return null;
            else return tmp.ToList()[0];
        }

        public static void InsertSGOPEN(object v)
        {

            
            if (DBAccessLayer.DBEnable == false) { return; }
            OPENCREATE open = (OPENCREATE)v;
            //若发现存在相同策略ID的实例未完成，将未完成实例标记为“删除”，替换以当前实例
            //这种情况在启动自检测时出现

            if (!DetectSGOPEN(open.basic.ID)) return;
                
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
            Dbsavechage("InsertSGOPEN");

        }

        public static void DeleteSGOPEN(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return; }
            var _selectedItem = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

            if (_selectedItem.Count() > 0)
            {
                _selectedItem.ToList()[0].SG_STATUS = 1;
                Dbsavechage("DeleteSGOPEN");
            }

        }

        /// <summary>
        /// 判断数据是否已经在库中存在
        /// </summary>
        /// <param name="ID">策略的UUID值</param>
        /// <returns>true: 库中已经存在 false: 库中不存在或者不可以写入</returns>
        public static bool DetectSGOPEN(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return false; }

            var _selectedItem = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

            if (_selectedItem.Count() > 0) return false;
            else return true;
        }

        /// <summary>
        /// 修改开仓记录状态
        /// </summary>
        /// <param name="ID">策略ID</param>
        /// <param name="status">状态</param>
        public static void UpdateSGOPENStatus(string ID, int status)
        {
            if (DBAccessLayer.DBEnable)
            {
                var selectedItem = (from item in DbEntity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

                if (selectedItem.Count() == 0) return;
                else
                {
                    selectedItem.ToList()[0].SG_STATUS = status;
                    Dbsavechage("UpdateSGOPENStatus");
                }
            }
        }

        public static void InsertSGCLOSE(object v)
        {
            if (DBAccessLayer.DBEnable == false) { return; }
            CLOSECREATE close = (CLOSECREATE)v;

            //若发现存在相同策略ID的实例未完成，将未完成实例标记为“删除”，替换以当前实例
            //这种情况在启动自检测时出现
            DeleteSGCLOSE(close.basic.ID);
            if (!(DetectSGCLOSE(close.basic.ID))) return;

            //等待上一步操作完成
            Thread.Sleep(10);

            SG_TAOLI_CLOSE_TABLE item = new SG_TAOLI_CLOSE_TABLE()
            {
                SG_GUID = Guid.NewGuid(),
                SG_ID = close.basic.ID,
                SG_OPEN_ID = close.Open_STR_ID,
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
            Dbsavechage("InsertSGCLOSE");


        }

        public static void DeleteSGCLOSE(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return; }
            var _selectedItem = (from item in DbEntity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == ID && item.SG_STATUS == 0 select item);

            if (_selectedItem.Count() > 0)
            {
                _selectedItem.ToList()[0].SG_STATUS = 1;

                Dbsavechage("DeleteSGCLOSE");
            }


        }


        /// <summary>
        /// 判断数据是否已经在库中存在
        /// </summary>
        /// <param name="ID">策略的UUID值</param>
        /// <returns>true: 库中已经存在 false: 库中不存在或者不可以写入</returns>
        public static bool DetectSGCLOSE(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return false; }
            var _selectedItem = (from item in DbEntity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == ID && item.SG_STATUS == 0 select item);

            if (_selectedItem.Count() > 0) return false;
            else return true;
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
            Dbsavechage("InsertSTATUS");
        }

        public static void InsertORDERLIST(string strategyId, string orderli)
        {
            if (DBAccessLayer.DBEnable == false) { return; }
            OL_TAOLI_LIST_TABLE item = new OL_TAOLI_LIST_TABLE()
            {
                OL_GUID = Guid.NewGuid(),
                SG_ID = strategyId,
                OL_LIST = orderli,
                OL_TIME = DateTime.Now
            };

            DbEntity.OL_TAOLI_LIST_TABLE.Add(item);
            Dbsavechage("InsertORDERLIST");
        }

        public static void DeleteORDERLIST(string strategyId)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            var selected = (from item in DbEntity.OL_TAOLI_LIST_TABLE where item.SG_ID == strategyId select item);

            if(selected.Count() > 0)
            {
                DbEntity.OL_TAOLI_LIST_TABLE.Remove(selected.ToList()[0]);
                Dbsavechage("DeleteORDERLIST");
            }
        }

        /// <summary>
        /// 创建委托记录
        /// </summary>
        /// <param name="strategyId">策略id</param>
        /// <param name="orderId">委托编号</param>
        /// <param name="ordertype">委托类型</param>
        /// <param name="exchangeid">交易所</param>
        public static void CreateERRecord(object item)
        {

            if (DBAccessLayer.DBEnable == false) { return; }
            if (item == null) return;


            QueryEntrustOrderStruct_M entrust = (QueryEntrustOrderStruct_M)item;

            lock (ERtableLock)
            {
                ER_TAOLI_TABLE record = new ER_TAOLI_TABLE()
                {
                    ER_GUID = Guid.NewGuid(),
                    ER_ID = entrust.OrderSysID,
                    ER_STRATEGY = entrust.StrategyId,
                    ER_ORDER_TYPE = entrust.SecurityType.ToString(),
                    ER_ORDER_EXCHANGE_ID = entrust.ExchangeID,

                    ER_CODE = entrust.Code,
                    ER_DIRECTION = entrust.Direction,
                    ER_CANCEL_TIME = new DateTime(1900, 1, 1),
                    ER_COMPLETED = false,
                    ER_DATE = new DateTime(1900, 1, 1),
                    ER_FROZEN_AMOUNT = 0,
                    ER_FROZEN_MONEY = 0,
                    ER_ORDER_STATUS = string.Empty,
                    ER_ORDER_TIME = DateTime.Now,
                    ER_VOLUME_REMAIN = 0,
                    ER_VOLUME_TOTAL_ORIGINAL = 0,
                    ER_VOLUME_TRADED = 0,
                    ER_WITHDRAW_AMOUNT = 0
                };

                DbEntity.ER_TAOLI_TABLE.Add(record);
                Dbsavechage("CreateERRecord");
            }
        }

        public static void CreateFutureERRecord(object item)
        {

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
            lock (ERtableLock)
            {

                var selected = (from item in DbEntity.ER_TAOLI_TABLE where item.ER_ID == record.cOrderSysID select item);

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

                    Dbsavechage("UpdateERRecord");
                }
            }


        }

        /// <summary>
        /// 添加股票交易记录
        /// 成交记录只在委托完成后记录，如果委托未完成，就等它完成
        /// 因此这张表只会增加，不会删除或者修改
        /// --2015.08-07
        /// </summary>
        /// <param name="ret"></param>
        public static void CreateDLRecord(object ret)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            managedBargainreturnstruct record = (managedBargainreturnstruct)ret;

            if (record != null)
            {
                lock (DLtableLock)
                {
                    string type = "s";
                    if (record.OrderType == 115)
                        type = "s";
                    else if (record.OrderType == 102)
                        type = "f";


                    string bargin_time = DateTime.Now.ToString();

                    DL_TAOLI_TABLE item = new DL_TAOLI_TABLE()
                    {
                        DL_GUID = Guid.NewGuid(),
                        DL_STRATEGY = record.strategyId,
                        DL_DIRECTION = record.direction,
                        DL_CODE = record.Security_code,
                        DL_NAME = record.Security_name,
                        DL_STATUS = record.OrderStatus.ToString(),
                        DL_TYPE = type,
                        DL_STOCK_AMOUNT = record.stock_amount,
                        DL_BARGAIN_PRICE = record.bargain_price / 1000,
                        DL_BARGAIN_MONEY = record.bargain_money,
                        DL_BARGAIN_TIME =bargin_time,
                        DL_NO = record.OrderSysID.ToString(),
                        DL_LOAD = true
                    };

                    DbEntity.DL_TAOLI_TABLE.Add(item);

                    Dbsavechage("CreateDLRecord");

                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// 添加期货交易记录
        /// 成交记录只在委托完成后记录，如果委托未完成，就等它完成
        /// 因此这张表只会增加，不会删除或者修改
        /// --2015.12.25
        /// </summary>
        /// <param name="ret"></param>
        public static void CreateFutureDLRecord(object ret)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            RecordItem record = (RecordItem)ret;

            if (record != null)
            {
                lock (DLtableLock)
                {
                    var selected = (from row in DbEntity.DL_TAOLI_TABLE where (row.DL_STRATEGY == record.StrategyId && row.DL_CODE == record.Code && row.DL_TYPE == "f") select row);

                    if (selected.Count() > 0)
                    {
                        foreach (var i in selected.ToList())
                        {
                            DbEntity.DL_TAOLI_TABLE.Remove(i);
                        }
                    }

                    DL_TAOLI_TABLE item = new DL_TAOLI_TABLE()
                    {
                        DL_GUID = Guid.NewGuid(),
                        DL_STRATEGY = record.StrategyId,
                        DL_DIRECTION = Convert.ToInt16(record.Orientation),
                        DL_CODE = record.Code,
                        DL_NAME = record.Code,
                        DL_STATUS = record.Status.ToString(),
                        DL_TYPE = "f",
                        DL_STOCK_AMOUNT = record.VolumeTraded,
                        DL_BARGAIN_PRICE = Convert.ToDouble(record.Price),
                        DL_BARGAIN_MONEY = Convert.ToDouble(record.Price) * record.VolumeTraded,
                        DL_BARGAIN_TIME = record.OrderTime_Start.ToString(),
                        DL_NO = record.OrderSysID.Trim()
                    };

                    DbEntity.DL_TAOLI_TABLE.Add(item);

                    Dbsavechage("CreateFutureDLRecord");
                    Thread.Sleep(10);
                }
            }
        }

        public static void UpdateStrategyStatusRecord(string str_id , int status)
        {
            if (DBAccessLayer.DBEnable == false) return;
            if(status == 1)
            {
                SG_TAOLI_STATUS_TABLE record = new SG_TAOLI_STATUS_TABLE()
                {
                    SG_GUID = Guid.NewGuid(),
                    SG_ID = str_id,
                    SG_STATUS = status,
                    SG_UPDATE_TIME = DateTime.Now
                };

                DbEntity.SG_TAOLI_STATUS_TABLE.Add(record);
            }
            else
            {
                var _rec = (from item in DbEntity.SG_TAOLI_STATUS_TABLE where item.SG_ID == str_id select item);

                if (_rec.Count() == 0)
                {
                    SG_TAOLI_STATUS_TABLE record = new SG_TAOLI_STATUS_TABLE()
                    {
                        SG_GUID = Guid.NewGuid(),
                        SG_ID = str_id,
                        SG_STATUS = 0,
                        SG_UPDATE_TIME = DateTime.Now
                    };

                    _rec = (from item in DbEntity.SG_TAOLI_STATUS_TABLE where item.SG_ID == str_id select item);
                }

                if (_rec.Count() == 0) return;

                var _unit = _rec.ToList()[0];

                switch (status)
                {
                    case 1:
                        _unit.SG_STATUS = 1;
                        break;
                    case 2:
                        _unit.SG_STATUS = 2;
                        break;
                    case 3:
                        _unit.SG_STATUS = 3;
                        break;
                    default:
                        _unit.SG_STATUS = 0;
                        break;
                }
            }
            Dbsavechage("UpdateStrategyStatusRecord");
        }

        /// <summary>
        /// 获得上次退出时未完成的开仓实例
        /// 策略管理线程启动时执行
        /// </summary>
        public static List<OPENCREATE> GetInCompletedOPENStrategy()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

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
            if (DBAccessLayer.DBEnable == false) { return null; }
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

        /// <summary>
        /// 单笔成交查询回报更新持仓文件
        /// </summary>
        /// <param name="v"></param>
        public static void UpdateCCRecords(object v)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            managedBargainreturnstruct bargin = (managedBargainreturnstruct)v;

            if(bargin == null)
            {
                return;
            }

            string code = bargin.Security_code;
            string type = bargin.OrderType.ToString();
            int amount = bargin.stock_amount;
            double price = bargin.bargain_price;
            int direction = bargin.direction;
            string user = bargin.User;
            string strategy = bargin.strategyId;
            string sDirection = direction.ToString();

            var selectedrecord = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_DIRECTION == sDirection && item.CC_TYPE == type && item.CC_USER == user select item);

            if(selectedrecord.Count() == 0)
            {
                //说明数据库中不存在当前票券的持仓
                if (direction == (int)TradeDirection.Buy)
                {
                    CC_TAOLI_TABLE record = new CC_TAOLI_TABLE()
                    {
                        CC_CODE = code,
                        CC_TYPE = type,
                        CC_AMOUNT = amount,
                        CC_BUY_PRICE = price,
                        CC_USER = user,
                        CC_DIRECTION = sDirection
                    };

                    DbEntity.CC_TAOLI_TABLE.Add(record);

                    CCRecord ccRecord = new CCRecord()
                    {
                        code = code,
                        type = type,
                        price = price,
                        amount = amount,
                        user = user,
                        direction = sDirection
                    };
                    PositionRecord.UpdateCCRecord(ccRecord);
                    Dbsavechage("UpdateCCRecords");
                    return;
                }
                else
                {
                    //不可能持仓列表是负值，肯定有问题
                    GlobalErrorLog.LogInstance.LogEvent("对空仓卖出--策略：" + bargin.strategyId + "， 当前交易代码：" + code + ", 买入：" + amount + "， 价格：" + price);
                    return;
                }
            }
            else
            {
                var record = selectedrecord.ToList()[0];

                int? db_amount = record.CC_AMOUNT;
                double? db_price = record.CC_BUY_PRICE;

                if (db_amount == null || db_price == null)
                { return; }

                //对该券有仓位
                if(direction == (int)TradeDirection.Buy)
                {
                    record.CC_BUY_PRICE = (db_amount * db_price + amount * price) / (db_amount + amount);
                    record.CC_AMOUNT = db_amount + amount;


                    CCRecord ccRecord = new CCRecord()
                    {
                        amount = Convert.ToInt16(record.CC_AMOUNT),
                        code = record.CC_CODE,
                        price = Convert.ToDouble(record.CC_BUY_PRICE),
                        type = record.CC_TYPE,
                        user = record.CC_USER
                    };

                    PositionRecord.UpdateCCRecord(ccRecord);

                    Dbsavechage("UpdateCCRecords");
                }
                else
                {
                    if (db_amount < amount)
                    {
                        GlobalErrorLog.LogInstance.LogEvent("持仓小于卖出--策略：" + bargin.strategyId + "， 当前交易代码：" + code + ", 买入：" + amount + "， 价格：" + price);
                        return;
                    }

                    else
                    {
                        record.CC_BUY_PRICE = (db_amount * db_price - amount * price) / (db_amount - amount);
                        record.CC_AMOUNT = db_amount - amount;
                        DbEntity.CC_TAOLI_TABLE.Remove(record);

                        CCRecord ccrecord = new CCRecord()
                        {
                            code = record.CC_CODE,
                            type = record.CC_TYPE,
                            user = record.CC_USER
                        };

                        PositionRecord.DeleteCCRecord(ccrecord);
                        Dbsavechage("UpdateCCRecords");
                        return;
                    }
                }

            }
            
            
        }

        /// <summary>
        /// 更新持仓列表
        /// </summary>
        /// <param name="v"></param>
        public static void UpdatePositionList(object v)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            managedBargainreturnstruct bargin = (managedBargainreturnstruct)v;

            if (bargin == null)
            {
                return;
            }

            string code = bargin.Security_code;
            string type = bargin.OrderType.ToString();
            int amount = bargin.stock_amount;
            double price = bargin.bargain_price;
            int direction = bargin.direction;
            string user = string.Empty;
            string strategy = bargin.strategyId;

            var position = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user && item.CC_CODE == code select item);

            

        }

        public static List<String> GetDealList(string strId, out decimal totalStockMoney, out decimal futureIndex)
        {
            totalStockMoney = 0;
            futureIndex = 0;

            if (DBAccessLayer.DBEnable)
            {
                var _record = (from item in DbEntity.DL_TAOLI_TABLE where item.DL_STRATEGY == strId select item);

                if(_record == null || _record.Count() == 0)
                {
                    return null;
                }


                List<String> _li = new List<string>();

                foreach(DL_TAOLI_TABLE i in _record.ToList())
                {
                    if (i.DL_TYPE == "f")
                    {
                        futureIndex = Convert.ToDecimal(i.DL_BARGAIN_PRICE);
                    }
                    else
                    {
                        totalStockMoney += Convert.ToDecimal(i.DL_BARGAIN_MONEY);
                        _li.Add(i.DL_CODE + ";" + i.DL_TYPE + ";" + i.DL_STOCK_AMOUNT);
                    }
                }

                return _li;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取匹配策略ID
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string SearchStrategy(SEARCHSTRATEGY info,out int hd)
        {
            hd = 0;
            if (DBAccessLayer.DBEnable == false) return string.Empty;


            String SG_IDs = String.Empty;


            double op = Convert.ToDouble(info.BASIS);
            string contract = info.CONTRACT.Trim();
            int index = Convert.ToInt32(info.INDEX);


            var _records = (from item in DbEntity.SG_TAOLI_OPEN_TABLE
                            where ((item.SG_STATUS == 3) &&
                            (item.SG_OP_POINT == op) &&
                            (item.SG_Contract == contract) &&
                            (item.SG_INDEX == index) &&
                            (item.SG_USER == info.basic.USER))
                            select item);

            if(_records != null && _records.Count() == 0)
            {
                return string.Empty;
            }

            SG_IDs = _records.ToList()[0].SG_ID;
            hd = Convert.ToInt16(_records.ToList()[0].SG_HAND_NUM);

            return SG_IDs;

        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="para">参数</param>
        /// <returns></returns>
        public static string  InsertUser(registerType para)
        {
            if (DBAccessLayer.DBEnable == false) { return "数据库未启用"; }
            if (para == null) return "参数有误";

            Database.UserInfo user = new UserInfo()
            {
                ID = Guid.NewGuid(),
                alias = para.username,
                name = para.Realname,
                password = DESoper.EncryptDES(para.Password),
                userRight = Convert.ToInt16(para.right),
                stockAvailable = para.StockAccount,
                futureAvailable = para.FutureAccount
            };

            DbEntity.UserInfo.Add(user);
            Dbsavechage("InsertUser");

            return "success";
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="para">参数</param>
        /// <returns></returns>
        public static bool Login(loginType para)
        {
            if (DBAccessLayer.DBEnable == false) { return false; }
            if (para == null) return false;

            para.password = DESoper.EncryptDES(para.password);

            var tmp = (from item in DbEntity.UserInfo where item.password == para.password && item.alias == para.name select item);

            if (tmp.Count() > 0) { return true; }
            else return false;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static bool ChangePassword(ChangePasswordType para)
        {
            if (DBAccessLayer.DBEnable == false) { return false; }
            if (para == null) return false;

            para.op = DESoper.EncryptDES(para.op);
            para.np = DESoper.EncryptDES(para.np);

            var tmp = (from item in DbEntity.UserInfo where item.password == para.op && item.alias == para.name select item);

            if (tmp.Count() == 0) return false;

            var s = tmp.ToList()[0];
            s.password = para.np;

            Dbsavechage("ChangePassword");
            return true;

        }

        public static void Dbsavechage(string type)
        {
            lock(DBChangeLock)
            {
                bool lockdb = false;
                int count = 100;
                while (lockdb == false)
                {
                    count--;
                    try
                    {
                        DbEntity.SaveChanges();
                        lockdb = true;
                    }
                    catch (Exception ex)
                    {
                        GlobalErrorLog.LogInstance.LogEvent("type = " + type + "\r\n" + ex.InnerException.ToString());
                        Thread.Sleep(10);

                        if(count == 0)
                        {
                            GlobalErrorLog.LogInstance.LogEvent("数据库提交100次失败！");
                            lockdb = true;
                        }
                    }
                }
            }
        }
    }
}