using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stork_Future_TaoLi.Database;
using MCStockLib;
using System.Threading;
using Newtonsoft.Json;
using Stork_Future_TaoLi.Account;
using Stork_Future_TaoLi.Variables_Type;


namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 数据库更新操作
    /// 通过线程池完成
    /// </summary>
    public static class DBAccessLayer
    {
        static MoneyEntityEntities3 DbEntity = new MoneyEntityEntities3();
        static object ERtableLock = new object();
        static object DLtableLock = new object();
        static object DBChangeLock = new object();
        //数据库测试标记
        public static bool DBEnable = true;

        public static void Dbsavechage(string type)
        {
            lock (DBChangeLock)
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

                        if (count == 0)
                        {
                            GlobalErrorLog.LogInstance.LogEvent("数据库提交100次失败！");
                            lockdb = true;
                        }
                    }
                }
            }
        }

        #region 风控相关
        public static void AddRiskRecord(string alias, string err, string strid, string code, int amount, double price, string orientation)
        {

            if (DBAccessLayer.DBEnable == false) { return; }

            RISK_TABLE record = new RISK_TABLE()
            {
                ID = Guid.NewGuid(),
                alias = alias,
                date = DateTime.Now.Date,
                strategy_id = strid,
                err = err,
                time = DateTime.Now,
                code = code,
                amount = amount,
                orientation = orientation,
                price = price
            };

            DbEntity.RISK_TABLE.Add(record);
            Dbsavechage("RISK_TABLE");
        }

        /// <summary>
        /// 返回当天风控记录，按照时间降序
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static List<RISK_TABLE> GetRiskRecord(string alias)
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            var risks = (from item in DbEntity.RISK_TABLE where item.alias == alias select item);

            if (risks == null || risks.Count() == 0) { return new List<RISK_TABLE>(); }
            else
            {
                return risks.OrderByDescending(i => i.time).ToList();
            }
        }

        /// <summary>
        /// 获取最新的list列表
        /// </summary>
        /// <returns></returns>
        public static List<RISK_TABLE> GetLatestRiskRecord()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            var risks = (from item in DbEntity.RISK_TABLE select item);

            if (risks == null || risks.Count() == 0) { return null; }
            else
            {
                return risks.OrderByDescending(i => i.time).ToList();
            }
        }
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
        #endregion

        #region 账户相关


        /// <summary>
        /// 记录新股票资金变动
        /// </summary>
        /// <param name="balance">可用资金</param>
        /// <param name="marketvalue">股票市值</param>
        /// <param name="stockvalue">股票成本</param>
        /// <param name="total">股票权益</param>
        /// <param name="frozen">风控+委托冻结资金</param>
        /// <param name="earning">盈亏</param>
        /// <param name="alias">用户名</param>
        public static void InsertStockAccountTable(string balance, string marketvalue, string stockvalue, string total, string earning, string alias, string frozen)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            StockAccountTable item = new StockAccountTable()
            {
                ID = Guid.NewGuid(),
                Balance = balance,
                MarketValue = marketvalue,
                StockValue = stockvalue,
                Total = total,
                StockFrozenValue = frozen,
                Earning = earning,
                Alias = alias,
                UpdateTime = DateTime.Now
            };

            DbEntity.StockAccountTable.Add(item);
            Dbsavechage("InsertStockAccountTable");
        }

        /// <summary>
        /// 获取最新的资金状态
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>最新资金状态</returns>
        public static StockAccountTable GetStockAccount(string alias)
        {
            if (DBAccessLayer.DBEnable == false) return null;

            var records = (from item in DbEntity.StockAccountTable where item.Alias == alias select item).OrderByDescending((item) => item.UpdateTime);

            if (records.Count() > 0)
            {
                return records.ToList()[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 记录新资金变动
        /// </summary>
        /// <param name="staticInterests">静态权益</param>
        /// <param name="OffsetGain">平仓盈亏</param>
        /// <param name="OpsitionGain">持仓盈亏</param>
        /// <param name="DynamicInterests">动态权益</param>
        /// <param name="frozen">风控+委托冻结资金</param>
        /// <param name="CashDeposit">保证金</param>
        /// <param name="ExpendableFund">可用资金</param>
        /// <param name="alias">用户名</param>
        public static void InsertFutureAccountTable(string staticInterests, string OffsetGain, string frozen, string OpsitionGain, string DynamicInterests, string CashDeposit, string ExpendableFund, string alias)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            FutureAccountTable item = new FutureAccountTable()
            {
                ID = Guid.NewGuid(),
                StatisInterests = staticInterests,
                OffsetGain = OffsetGain,
                OpsitionGain = OpsitionGain,
                DynamicInterests = DynamicInterests,
                CashDeposit = CashDeposit,
                FrozenValue = frozen,
                ExpendableFund = ExpendableFund,
                Alias = alias,
                UpdateTime = DateTime.Now
            };

            DbEntity.FutureAccountTable.Add(item);
            Dbsavechage("InsertFutureAccountTable");
        }

        /// <summary>
        /// 获取期货账户
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>期货资金</returns>
        public static FutureAccountTable GetFutureAccount(string alias)
        {
            if (DBAccessLayer.DBEnable == false) return null;

            var records = (from item in DbEntity.FutureAccountTable where item.Alias == alias select item).OrderByDescending((item) => item.UpdateTime);

            if (records.Count() > 0)
            {
                return records.ToList()[0];
            }
            else
            {
                return null;
            }
        }
        

        /// <summary>
        /// 获取用户可用资金量 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>股票|期货</returns>
        public static string GetAccountAvailable(string user)
        {
            if (DBAccessLayer.DBEnable == false) return string.Empty;

            var tmp = from item in DbEntity.UserInfo where item.alias == user select item;
            if (tmp == null || tmp.Count() == 0)
            {
                return string.Empty;
            }

            return tmp.ToList()[0].stockAvailable.ToString() + "|" + tmp.ToList()[0].futureAvailable.ToString();
        }

        /// <summary>
        /// 设置用户资金量
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stock"></param>
        /// <param name="future"></param>
        /// <returns></returns>
        public static bool SetAccountAvailable(string user, string stock, string future)
        {
            if (DBAccessLayer.DBEnable == false) return false;

            var tmp = from item in DbEntity.UserInfo where item.alias == user select item;

            if (tmp.Count() == 0) return false;

            UserInfo info = tmp.ToList()[0];

            info.stockAvailable = stock;
            info.futureAvailable = future;

            Dbsavechage("SetAccountAvailable");
            return true;

        }
        #endregion

        #region 用户信息相关
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

            var tmp = (from item in DbEntity.UserInfo where item.alias == alias select item);

            if (tmp == null || tmp.Count() == 0) return null;
            else return tmp.ToList()[0];
        }

        public static void UpdateUserAccount(string alias, double stockaccount, double futureaccount)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            var user = (from item in DbEntity.UserInfo where item.alias == alias select item);

            if (user == null || user.Count() == 0) return;

            user.ToList()[0].stockAvailable = stockaccount.ToString();
            user.ToList()[0].futureAvailable = futureaccount.ToString();


            //ToDo : 测试account修改能否生效
            Dbsavechage("UpdateUserAccount");

        }


        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="para">参数</param>
        /// <returns></returns>
        public static string InsertUser(registerType para)
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

            //为新用户分配股市资金流动信息
            InsertStockAccountTable(para.StockAccount, "0", "0", para.StockAccount, "0", para.username, "0");

            //为新用户分配期货资金流动信息
            InsertFutureAccountTable(para.FutureAccount, "0", "0", "0", para.FutureAccount, "0", para.FutureAccount, para.username);

            Dbsavechage("InsertUser");

            return "success";
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="para">参数</param>
        /// <returns></returns>
        public static int? Login(loginType para)
        {
            if (DBAccessLayer.DBEnable == false) { return 0; }
            if (para == null) return 0;

            para.password = DESoper.EncryptDES(para.password);

            var tmp = (from item in DbEntity.UserInfo where item.password == para.password && item.alias == para.name select item);

            if (tmp.Count() > 0) { return tmp.ToList()[0].userRight; }
            else return 0;
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

        #endregion

        #region 策略相关

        /// <summary>
        /// 获取匹配策略ID
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string SearchStrategy(SEARCHSTRATEGY info, out int hd)
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

            if (_records != null && _records.Count() == 0)
            {
                return string.Empty;
            }

            SG_IDs = _records.ToList()[0].SG_ID;
            hd = Convert.ToInt16(_records.ToList()[0].SG_HAND_NUM);

            return SG_IDs;

        }

        public static void UpdateStrategyStatusRecord(string str_id, int status)
        {
            if (DBAccessLayer.DBEnable == false) return;
            if (status == 1)
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
        #endregion

        #region 交易相关

        #region 下单
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

            if (selected.Count() > 0)
            {
                DbEntity.OL_TAOLI_LIST_TABLE.Remove(selected.ToList()[0]);
                Dbsavechage("DeleteORDERLIST");
            }
        }
        #endregion

        #region 委托
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
                    ER_ORDER_REF = entrust.OrderRef.ToString(),
                    ER_STRATEGY = entrust.StrategyId,
                    ER_ORDER_TYPE = entrust.SecurityType.ToString(),
                    ER_ORDER_EXCHANGE_ID = entrust.ExchangeID,
                    ER_USER = entrust.User,
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
                    ER_WITHDRAW_AMOUNT = 0,
                    ER_OFFSETFLAG = String.Empty
                };

                DbEntity.ER_TAOLI_TABLE.Add(record);
                Dbsavechage("CreateERRecord");
            }
        }

        public static void CreateFutureERRecord(object item)
        {
            if (DBEnable == false) return;

            if (item == null) return;

            QueryEntrustOrderStruct_M entrust = (QueryEntrustOrderStruct_M)item;

            lock (ERtableLock)
            {
                ER_TAOLI_TABLE record = new ER_TAOLI_TABLE()
                {

                    ER_GUID = Guid.NewGuid(),
                    ER_ID = entrust.OrderSysID,
                    ER_ORDER_REF = entrust.OrderRef.ToString(),
                    ER_STRATEGY = entrust.StrategyId,
                    ER_ORDER_TYPE = entrust.SecurityType.ToString(),
                    ER_ORDER_EXCHANGE_ID = entrust.ExchangeID,
                    ER_USER = entrust.User,
                    ER_CODE = entrust.Code,
                    ER_DIRECTION = entrust.Direction,
                    ER_CANCEL_TIME = new DateTime(1900, 1, 1),
                    ER_COMPLETED = false,
                    ER_DATE = new DateTime(1900, 1, 1),
                    ER_FROZEN_AMOUNT = entrust.Amount,
                    ER_FROZEN_MONEY = entrust.Amount * entrust.OrderPrice,
                    ER_ORDER_STATUS = string.Empty,
                    ER_ORDER_TIME = DateTime.Now,
                    ER_VOLUME_REMAIN = 0,
                    ER_VOLUME_TOTAL_ORIGINAL = entrust.Amount,
                    ER_VOLUME_TRADED = 0,
                    ER_WITHDRAW_AMOUNT = 0,
                    ER_OFFSETFLAG = entrust.OffsetFlag.ToString()
                };

                DbEntity.ER_TAOLI_TABLE.Add(record);
                Dbsavechage("CreateERRecord");
            }
        }

        public static void DeleteERRecord(object Ref)
        {
            if (DBEnable == false) return;

            String OrderRef = ((int)Ref).ToString();

            var record = (from item in DbEntity.ER_TAOLI_TABLE where item.ER_ORDER_REF == OrderRef select item);

            if(record.Count() >0)
            {
                lock(ERtableLock)
                {
                    DbEntity.ER_TAOLI_TABLE.Remove(record.ToList()[0]);
                    Dbsavechage("DeleteERRecord");
                }

                return;
            }

            return;
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
        /// 功能： 获取所有未完成委托
        /// 应用场景： 委托查询线程启动阶段载入委托记录
        /// </summary>
        /// <param name="Type">
        /// 获取类型：
        /// S: 股票
        /// F: 期货
        /// </param>
        /// <returns></returns>
        public static List<ER_TAOLI_TABLE> GetInCompletedERRecord(String Type)
        {
            if (DBEnable == false) return null;
            string sbType = String.Empty;

            if (Type.Trim().ToUpper() == "S")
            {
                sbType = "115";
            }
            else if(Type.Trim().ToUpper() == "F")
            {
                sbType = "1";
            }

            var ERs = (from item in DbEntity.ER_TAOLI_TABLE where item.ER_ORDER_TYPE == sbType select item);

            if (ERs.Count() > 0)
            {
                return ERs.ToList();
            }


            return null;
        }
        #endregion

        #region 成交
        public static List<String> GetDealList(string strId, out decimal totalStockMoney, out decimal futureIndex)
        {
            totalStockMoney = 0;
            futureIndex = 0;

            if (DBAccessLayer.DBEnable)
            {
                var _record = (from item in DbEntity.DL_TAOLI_TABLE where item.DL_STRATEGY == strId select item);

                if (_record == null || _record.Count() == 0)
                {
                    return null;
                }


                List<String> _li = new List<string>();

                foreach (DL_TAOLI_TABLE i in _record.ToList())
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
        /// 获取用户交易
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static List<DL_TAOLI_TABLE> GetUserDeals(string alias)
        {
            if (alias == null) return null;

            alias = alias.Trim();

            List<DL_TAOLI_TABLE> deals_record = (from item in DbEntity.DL_TAOLI_TABLE where item.DL_USER == alias select item).ToList();

            if (deals_record != null && deals_record.Count > 0)
            {
                return deals_record;
            }
            else
            {
                return new List<DL_TAOLI_TABLE>();
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
                        DL_DIRECTION = record.direction.ToString(),
                        DL_OFFSETFLAG = record.offsetflag.ToString(),
                        DL_CODE = record.Security_code,
                        DL_NAME = record.Security_name,
                        DL_STATUS = record.OrderStatus.ToString(),
                        DL_TYPE = type,
                        DL_STOCK_AMOUNT = record.stock_amount,
                        DL_BARGAIN_PRICE = record.bargain_price / 1000,
                        DL_BARGAIN_MONEY = record.bargain_money,
                        DL_BARGAIN_TIME = bargin_time,
                        DL_NO = record.OrderSysID.ToString(),
                        DL_LOAD = true,
                        DL_USER = record.User,
                        DL_MARK = record.OrderMark
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
            string number = record.OrderSysID.Trim();

            if (record != null)
            {
                lock (DLtableLock)
                {
                    var selected = (from row in DbEntity.DL_TAOLI_TABLE where row.DL_NO == number select row);

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
                        DL_DIRECTION = record.Orientation,
                        DL_OFFSETFLAG = record.CombOffsetFlag.ToString(),
                        DL_CODE = record.Code,
                        DL_NAME = record.Code,
                        DL_STATUS = record.Status.ToString(),
                        DL_TYPE = "f",
                        DL_STOCK_AMOUNT = record.VolumeTraded,
                        DL_BARGAIN_PRICE = Convert.ToDouble(record.Price),
                        DL_BARGAIN_MONEY = Convert.ToDouble(record.Price) * record.VolumeTraded,
                        DL_BARGAIN_TIME = record.OrderTime_Start.ToString(),
                        DL_NO = record.OrderSysID.Trim(),
                        DL_USER = record.User,
                        DL_LOAD = false
                    };

                    DbEntity.DL_TAOLI_TABLE.Add(item);

                    Dbsavechage("CreateFutureDLRecord");
                    Thread.Sleep(10);
                }
            }
        }
        #endregion

        #endregion

        #region 持仓操作相关
        /// <summary>
        /// 单笔成交查询回报更新持仓文件
        /// </summary>
        /// <param name="v"></param>
        public static void UpdateCCRecords(object v)
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
            int offsetflag = bargin.offsetflag;
            string user = bargin.User;
            string strategy = bargin.strategyId;
            string sDirection = direction.ToString();

            if (type == "49")
            {
                //股票
                var selectedStock = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_TYPE == type && item.CC_USER == user select item);

                if (selectedStock.Count() == 0)
                {
                    //新股票持仓
                    if (direction == Convert.ToInt32(TradeOrientationAndFlag.StockTradeDirectionBuy))
                    {
                        //买入
                        CC_TAOLI_TABLE record = new CC_TAOLI_TABLE()
                        {
                            ID = Guid.NewGuid(),
                            CC_CODE = code,
                            CC_TYPE = type,
                            CC_AMOUNT = amount,
                            CC_BUY_PRICE = price,
                            CC_USER = user,
                            CC_DIRECTION = sDirection,
                            CC_OFFSETFLAG = 0
                        };

                        DbEntity.CC_TAOLI_TABLE.Add(record);

                        CCRecord ccRecord = new CCRecord()
                        {
                            code = code,
                            type = type,
                            price = price,
                            amount = amount,
                            user = user,
                            direction = sDirection,
                            offsetflag = offsetflag.ToString(),

                        };
                        Dbsavechage("UpdateCCRecords");

                        //修改本地CC列表
                        List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                        accountMonitor.ChangeLocalCC(user.Trim(), records);

                        //修改股票可用资金和股票成本
                        accountMonitor.ChangeStockAccountDuToStockDeal(user.Trim(), price, amount);

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
                    var record = selectedStock.ToList()[0];

                    int? db_amount = record.CC_AMOUNT;
                    double? db_price = record.CC_BUY_PRICE;

                    if (db_amount == null || db_price == null)
                    { return; }

                    //已经持仓的股票
                    if (direction == Convert.ToInt32(TradeOrientationAndFlag.StockTradeDirectionSell))
                    {
                        //卖出
                        if (db_amount < amount)
                        {
                            GlobalErrorLog.LogInstance.LogEvent("持仓小于卖出--策略：" + bargin.strategyId + "， 当前交易代码：" + code + ", 买入：" + amount + "， 价格：" + price);
                            return;
                        }

                        else
                        {
                            if (db_amount == amount)
                            {
                                DbEntity.CC_TAOLI_TABLE.Remove(record);
                                Dbsavechage("UpdateCCRecords");

                                //修改股票可用资金和股票成本
                                accountMonitor.ChangeStockAccountDuToStockDeal(user.Trim(), price, amount * (-1));

                                return;
                            }
                            else
                            {
                                record.CC_BUY_PRICE = (db_amount * db_price - amount * price) / (db_amount - amount);
                            }

                            record.CC_AMOUNT = db_amount - amount;


                            //ToDo : 确认卖出部分股票是否记库成功

                            CCRecord ccrecord = new CCRecord()
                            {
                                code = record.CC_CODE,
                                type = record.CC_TYPE,
                                user = record.CC_USER,
                                amount = Convert.ToInt16(record.CC_AMOUNT),
                                offsetflag = record.CC_OFFSETFLAG.ToString(),
                                direction = record.CC_DIRECTION,
                                price = Convert.ToDouble(record.CC_BUY_PRICE)
                            };

                            Dbsavechage("UpdateCCRecords");

                            //更新本地持仓列表
                            List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                            accountMonitor.ChangeLocalCC(user.Trim(), records);

                            //PositionRecord.UpdateCCRecord(ccrecord);

                            //修改股票可用资金和股票成本
                            accountMonitor.ChangeStockAccountDuToStockDeal(user.Trim(), price, amount * (-1));

                            return;
                        }
                    }
                    else
                    {
                        //买入
                        record.CC_BUY_PRICE = (db_amount * db_price + amount * price) / (db_amount + amount);
                        record.CC_AMOUNT = db_amount + amount;


                        CCRecord ccRecord = new CCRecord()
                        {
                            amount = Convert.ToInt16(record.CC_AMOUNT),
                            code = record.CC_CODE,
                            price = Convert.ToDouble(record.CC_BUY_PRICE),
                            type = record.CC_TYPE,
                            user = record.CC_USER,
                            direction = record.CC_DIRECTION,
                            offsetflag = record.CC_OFFSETFLAG.ToString()
                        };

                        //PositionRecord.UpdateCCRecord(ccRecord);

                        Dbsavechage("UpdateCCRecords");

                        //更新本地持仓列表
                        List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                        accountMonitor.ChangeLocalCC(user.Trim(), records);

                        //修改股票可用资金和股票成本
                        accountMonitor.ChangeStockAccountDuToStockDeal(user.Trim(), price, amount);


                    }
                }
            }
            else
            {
                //期货

                if (offsetflag == 48)
                {
                    //开仓
                    var selectedFuture = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_DIRECTION == sDirection && item.CC_TYPE == type && item.CC_USER == user select item);
                    if (selectedFuture.Count() == 0)
                    {
                        accountMonitor.ChangeFutureAccountDuToFutureDeal(user, code, price, amount, sDirection, 0, 0);

                        //持仓不存在
                        CC_TAOLI_TABLE record = new CC_TAOLI_TABLE()
                        {
                            ID = Guid.NewGuid(),
                            CC_CODE = code,
                            CC_TYPE = type,
                            CC_AMOUNT = amount,
                            CC_BUY_PRICE = price,
                            CC_USER = user,
                            CC_DIRECTION = sDirection,
                            CC_OFFSETFLAG = offsetflag
                        };

                        DbEntity.CC_TAOLI_TABLE.Add(record);

                        CCRecord ccRecord = new CCRecord()
                        {
                            code = code,
                            type = type,
                            price = price,
                            amount = amount,
                            user = user,
                            direction = sDirection,
                            offsetflag = offsetflag.ToString()
                        };

                        Dbsavechage("UpdateCCRecords");


                        //更新本地持仓列表
                        List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                        accountMonitor.ChangeLocalCC(user.Trim(), records);

                        return;
                    }
                    else
                    {
                        //持仓存在
                        var record = selectedFuture.ToList()[0];


                        int? db_amount = record.CC_AMOUNT;
                        double? db_price = record.CC_BUY_PRICE;

                        if (db_amount == null || db_price == null)
                        { return; }

                        record.CC_BUY_PRICE = (db_amount * db_price + amount * price) / (db_amount + amount);
                        record.CC_AMOUNT = db_amount + amount;

                        accountMonitor.ChangeFutureAccountDuToFutureDeal(user, code, Convert.ToDouble(price), Convert.ToInt32(amount), sDirection, Convert.ToDouble(db_price), Convert.ToInt32(db_amount));

                        CCRecord ccRecord = new CCRecord()
                        {
                            amount = Convert.ToInt16(record.CC_AMOUNT),
                            code = record.CC_CODE,
                            price = Convert.ToDouble(record.CC_BUY_PRICE),
                            type = record.CC_TYPE,
                            user = record.CC_USER,
                            offsetflag = record.CC_OFFSETFLAG.ToString(),
                            direction = record.CC_DIRECTION
                        };

                        Dbsavechage("UpdateCCRecords");

                        //更新本地持仓列表
                        List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                        accountMonitor.ChangeLocalCC(user.Trim(), records);

                    }
                }
                else
                {
                    //平仓

                    var selectedFuture = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_DIRECTION != sDirection && item.CC_TYPE == type && item.CC_USER == user select item);

                    if (selectedFuture.Count() == 0)
                    {
                        //不可能持仓列表是负值，肯定有问题
                        GlobalErrorLog.LogInstance.LogEvent("对空仓平仓--策略：" + bargin.strategyId + "， 当前交易代码：" + code + ", 买入：" + amount + "， 价格：" + price);
                        return;
                    }
                    else
                    {
                        var record = selectedFuture.ToList()[0];



                        int? db_amount = record.CC_AMOUNT;
                        double? db_price = record.CC_BUY_PRICE;

                        if (db_amount == null || db_price == null)
                        { return; }

                        if (db_amount < amount)
                        {
                            GlobalErrorLog.LogInstance.LogEvent("持仓小于平仓--策略：" + bargin.strategyId + "， 当前交易代码：" + code + ", 买入：" + amount + "， 价格：" + price);
                            return;
                        }

                        else
                        {
                            if (db_amount == amount)
                            {
                                DbEntity.CC_TAOLI_TABLE.Remove(record); Dbsavechage("UpdateCCRecords");
                            }
                            else
                            {
                                record.CC_BUY_PRICE = (db_amount * db_price - amount * price) / (db_amount - amount);
                            }
                            record.CC_AMOUNT = db_amount - amount;

                            accountMonitor.ChangeFutureAccountDuToFutureDeal(user, code, Convert.ToDouble(price), (-1) * Convert.ToInt32(amount), sDirection, Convert.ToDouble(db_price), Convert.ToInt32(db_amount));

                            CCRecord ccrecord = new CCRecord()
                            {
                                code = record.CC_CODE,
                                type = record.CC_TYPE,
                                user = record.CC_USER,
                                direction = record.CC_DIRECTION,
                                offsetflag = record.CC_OFFSETFLAG.ToString(),
                                price = Convert.ToDouble(record.CC_BUY_PRICE),
                                amount = Convert.ToInt16(record.CC_AMOUNT)
                            };

                            Dbsavechage("UpdateCCRecords");

                            //更新本地持仓列表
                            List<CC_TAOLI_TABLE> records = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                            accountMonitor.ChangeLocalCC(user.Trim(), records);
                            Dbsavechage("UpdateCCRecords");
                            return;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 获取股票持仓列表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="records"></param>
        /// <param name="stockcost"></param>
        public static void LoadCCStockList(string userName, out List<CC_TAOLI_TABLE> records, out double stockcost)
        {


            records = new List<CC_TAOLI_TABLE>();
            stockcost = 0;

            if (DBAccessLayer.DBEnable == false) return;

            if (userName != "*")
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == userName && item.CC_TYPE == "49" select item);
                records = tmp.ToList();
            }
            else
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_TYPE == "49" select item);
                records = tmp.ToList();
            }

            if (records.Count() > 0)
            {
                foreach (var i in records)
                {
                    //records.Add(i);
                    stockcost += Convert.ToDouble(i.CC_AMOUNT * i.CC_BUY_PRICE);
                }
            }

            stockcost /= 1000;
        }

        /// <summary>
        /// 获取期货持仓列表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="records"></param>
        public static void LoadCCFutureList(string userName, out List<CC_TAOLI_TABLE> records)
        {
            records = new List<CC_TAOLI_TABLE>();

            if (DBAccessLayer.DBEnable == false) return;

            if (userName != "*")
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == userName && item.CC_TYPE == "1" select item);
                records = tmp.ToList();
            }
            else
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_TYPE == "1" select item);
                records = tmp.ToList();
            }
        }

        /// <summary>
        /// 获取证券持仓列表
        /// </summary>
        /// <param name="userName">用户名,"*"为输出所有用户的持仓</param>
        /// <param name="records">输出记录</param>
        public static void LoadCCList(string userName, out List<CC_TAOLI_TABLE> records)
        {
            records = new List<CC_TAOLI_TABLE>();

            if (DBAccessLayer.DBEnable == false) return;

            if (userName != "*")
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE select item);
                records = tmp.ToList();
            }
            else
            {
                var tmp = (from item in DbEntity.CC_TAOLI_TABLE where item.CC_USER == userName select item);
                records = tmp.ToList();
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
        #endregion
    }
}