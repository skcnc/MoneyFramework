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
using System.Data.SqlClient;
using Stork_Future_TaoLi.StrategyModule;


namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 数据库更新操作
    /// 通过线程池完成
    /// </summary>
    public static class DBAccessLayer
    {
        static object ERtableLock = new object();
        static object DLtableLock = new object();
        static object DBChangeLock = new object();
        static object AuthorizedUpdateLock = new object();
        //数据库测试标记
        public static bool DBEnable = true;

       

        #region 风控相关
        public static void AddRiskRecord(string alias, string err, string strid, string code, int amount, double price, string orientation)
        {

            if (DBAccessLayer.DBEnable == false) { return; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
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

                entity.RISK_TABLE.Add(record);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("AddRiskRecord", entity);
            }
        }

        /// <summary>
        /// 返回当天风控记录，按照时间降序
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static List<RISK_TABLE> GetRiskRecord(string alias)
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var risks = (from item in entity.RISK_TABLE where item.alias == alias select item);

                if (risks == null || risks.Count() == 0)
                {
                    return new List<RISK_TABLE>();
                }
                else
                {
                    try
                    {
                        return risks.OrderByDescending(i => i.time).ToList();


                    }
                    catch (Exception ex)
                    {
                        DBAccessLayer.LogSysInfo("DBAccessLayer-GetRiskRecord", ex.ToString());
                        return new List<RISK_TABLE>();
                    }

                }
            }
        }

        /// <summary>
        /// 获取最新的list列表
        /// </summary>
        /// <returns></returns>
        public static List<RISK_TABLE> GetLatestRiskRecord()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var risks = (from item in entity.RISK_TABLE select item);

                Thread.Sleep(1);

                try
                {
                    if (risks == null || risks.Count() == 0) { return null; }
                    else
                    {
                        return risks.OrderByDescending(i => i.time).ToList();
                    }
                }
                catch
                {

                    return null;
                }

            }
        }
        /// <summary>
        /// 获取黑白名单
        /// </summary>
        /// <returns>股票黑白名单</returns>
        public static List<BWNameTable> GetWBNamwList()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = (from item in entity.BWNameTable select item);

                if (tmp.Count() == 0) return null;
                else return tmp.ToList();
            }
        }

        /// <summary>
        /// 设定黑白名单
        /// </summary>
        /// <param name="records">名单</param>
        /// <returns>成功状态</returns>
        public static bool SetWBNameList(List<BWNameTable> records)
        {
            if (DBAccessLayer.DBEnable == false) return false;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                List<BWNameTable> oldRecords = (from item in entity.BWNameTable select item).ToList();

                for (int i = 0; i < oldRecords.Count; i++)
                {
                    entity.BWNameTable.Remove(oldRecords[i]);
                }

                foreach (BWNameTable record in records)
                {
                    entity.BWNameTable.Add(record);
                }

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("BWNameTable", entity);

                return true;
            }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
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

                entity.StockAccountTable.Add(item);

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertStockAccountTable", entity);
                
            }
        }

        /// <summary>
        /// 获取最新的资金状态
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>最新资金状态</returns>
        public static StockAccountTable GetStockAccount(string alias)
        {
            if (DBAccessLayer.DBEnable == false) return null;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var records = (from item in entity.StockAccountTable where item.Alias == alias select item).OrderByDescending((item) => item.UpdateTime);

                if (records.Count() > 0)
                {
                    return records.ToList()[0];
                }
                else
                {
                    return null;
                }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.FutureAccountTable.Add(item);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertFutureAccountTable", entity);
            }
        }

        /// <summary>
        /// 获取期货账户
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <returns>期货资金</returns>
        public static FutureAccountTable GetFutureAccount(string alias)
        {
            if (DBAccessLayer.DBEnable == false) return null;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var records = (from item in entity.FutureAccountTable where item.Alias == alias select item).OrderByDescending((item) => item.UpdateTime);

                if (records.Count() > 0)
                {
                    return records.ToList()[0];
                }
                else
                {
                    return null;
                }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = from item in entity.UserInfo where item.alias == user select item;
                if (tmp == null || tmp.Count() == 0)
                {
                    return string.Empty;
                }

                return tmp.ToList()[0].stockAvailable.ToString() + "|" + tmp.ToList()[0].futureAvailable.ToString();
            }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = from item in entity.UserInfo where item.alias == user select item;

                if (tmp.Count() == 0) return false;

                UserInfo info = tmp.ToList()[0];

                info.stockAvailable = stock;
                info.futureAvailable = future;

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("SetAccountAvailable", entity);
                
            }
            return true;

        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="paraname"></param>
        /// <returns></returns>
        public static String GetParameter(String paraname)
        {
            if (DBAccessLayer.DBEnable == false) return string.Empty;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                paraname = paraname.Trim();

                var tmp = from item in entity.SYSPARAS where item.Paraname == paraname select item.Paravalue;

                try
                {
                    if (tmp.Count() > 0)
                    {
                        return tmp.ToList()[0];
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
                catch (Exception ex)
                {
                    GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
                    LogSysInfo("DBAccessLayer-GetParameter", ex.ToString());
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// 设置具体参数
        /// </summary>
        /// <param name="paraname"></param>
        /// <param name="paravalue"></param>
        public static void SetParameter(PARAS para)
        {
            if (DBAccessLayer.DBEnable == false) return;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = from item in entity.SYSPARAS select item;

                try
                {
                    if (tmp.Count() > 0)
                    {
                        for (int i = 0; i < tmp.ToList().Count; i++)
                        {
                            switch (tmp.ToList()[i].Paraname.Trim())
                            {
                                case "STOCKADDR":
                                    tmp.ToList()[i].Paravalue = para.STOCKADDR.Trim();
                                    break;
                                case "STOCKPORT":
                                    tmp.ToList()[i].Paravalue = para.STOCKPORT.Trim();
                                    break;
                                case "STOCKACCOUNT":
                                    tmp.ToList()[i].Paravalue = para.STOCKACCOUNT.Trim();
                                    break;
                                case "STOCKDEPTNO":
                                    tmp.ToList()[i].Paravalue = para.STOCKDEPTNO.Trim();
                                    break;
                                case "STOCKNO":
                                    tmp.ToList()[i].Paravalue = para.STOCKNO.Trim();
                                    break;
                                case "STOCKPASSWORD":
                                    tmp.ToList()[i].Paravalue = para.STOCKPASSWORD.Trim();
                                    break;
                                case "FUTUREADDR":
                                    tmp.ToList()[i].Paravalue = para.FUTUREADDR.Trim();
                                    break;
                                case "FUTUREBROKER":
                                    tmp.ToList()[i].Paravalue = para.FUTUREBROKER.Trim();
                                    break;
                                case "FUTUREACCOUNT":
                                    tmp.ToList()[i].Paravalue = para.FUTUREACCOUNT.Trim();
                                    break;
                                case "FUTUREPASSWORD":
                                    tmp.ToList()[i].Paravalue = para.FUTUREPASSWORD.Trim();
                                    break;
                                default:
                                    break;
                            }
                        }

                        DBChangeSave save = new DBChangeSave();
                        save.Dbsavechage("SetParameter", entity);

                    }
                    else return;
                }
                catch (Exception ex)
                {
                    GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
                    LogSysInfo("DBAccessLayer-SetParameter", ex.ToString());
                    return;
                }
            }
        }
        #endregion

        #region 用户信息相关
        public static List<UserInfo> GetUser()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {

                var tmp = (from item in entity.UserInfo select item);

                try
                {
                    if (tmp.Count() == 0) return null;
                    else return tmp.ToList();
                }
                catch(Exception ex)
                {
                    GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
                    return null;
                }
            }
        }

        public static UserInfo GetOneUser(string alias)
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = (from item in entity.UserInfo where item.alias == alias select item);

                try
                {
                    if (tmp == null || tmp.Count() == 0) return null;
                    else return tmp.ToList()[0];
                }
                catch (Exception ex)
                {
                    GlobalErrorLog.LogInstance.LogEvent(ex.ToString());
                    return null;
                }
            }
        }

        public static void UpdateUserAccount(string alias, double stockaccount, double futureaccount)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var user = (from item in entity.UserInfo where item.alias == alias select item);

                if (user == null || user.Count() == 0) return;

                user.ToList()[0].stockAvailable = stockaccount.ToString();
                user.ToList()[0].futureAvailable = futureaccount.ToString();

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("UpdateUserAccount", entity);
            }

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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {

                entity.UserInfo.Add(user);

                //为新用户分配股市资金流动信息
                InsertStockAccountTable(para.StockAccount, "0", "0", para.StockAccount, "0", para.username, "0");

                //为新用户分配期货资金流动信息
                InsertFutureAccountTable(para.FutureAccount, "0", "0", "0", para.FutureAccount, "0", para.FutureAccount, para.username);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertUser", entity);
            }

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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = (from item in entity.UserInfo where item.password == para.password && item.alias == para.name select item);
                if (tmp.Count() > 0) { return tmp.ToList()[0].userRight; }
                else return 0;
            }
            
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var tmp = (from item in entity.UserInfo where item.password == para.op && item.alias == para.name select item);

                if (tmp.Count() == 0) return false;

                var s = tmp.ToList()[0];
                s.password = para.np;

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("ChangePassword", entity);
               
            }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var _records = (from item in entity.SG_TAOLI_OPEN_TABLE
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

        }

        public static void UpdateStrategyStatusRecord(string str_id, int status)
        {
            if (DBAccessLayer.DBEnable == false) return;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                if (status == 1)
                {
                    SG_TAOLI_STATUS_TABLE record = new SG_TAOLI_STATUS_TABLE()
                    {
                        SG_GUID = Guid.NewGuid(),
                        SG_ID = str_id,
                        SG_STATUS = status,
                        SG_UPDATE_TIME = DateTime.Now
                    };

                    entity.SG_TAOLI_STATUS_TABLE.Add(record);
                }
                else
                {
                    var _rec = (from item in entity.SG_TAOLI_STATUS_TABLE where item.SG_ID == str_id select item);

                    if (_rec.Count() == 0)
                    {
                        SG_TAOLI_STATUS_TABLE record = new SG_TAOLI_STATUS_TABLE()
                        {
                            SG_GUID = Guid.NewGuid(),
                            SG_ID = str_id,
                            SG_STATUS = 0,
                            SG_UPDATE_TIME = DateTime.Now
                        };

                        _rec = (from item in entity.SG_TAOLI_STATUS_TABLE where item.SG_ID == str_id select item);
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

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("UpdateStrategyStatusRecord", entity);
            }
           
        }

        /// <summary>
        /// 获得上次退出时未完成的开仓实例
        /// 策略管理线程启动时执行
        /// </summary>
        public static List<OPENCREATE> GetInCompletedOPENStrategy()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var selected = (from item in entity.SG_TAOLI_OPEN_TABLE where item.SG_STATUS == 0 select item);

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

        }

        /// <summary>
        /// 获得上次退出时未完成的平仓实例
        /// </summary>
        /// <returns></returns>
        public static List<CLOSECREATE> GetInCompletedCLOSEStrategy()
        {
            if (DBAccessLayer.DBEnable == false) { return null; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var selected = (from item in entity.SG_TAOLI_CLOSE_TABLE where item.SG_STATUS == 0 select item);

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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.SG_TAOLI_OPEN_TABLE.Add(record);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertSGOPEN", entity);
            }

        }

        public static void DeleteSGOPEN(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var _selectedItem = (from item in entity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

                if (_selectedItem.Count() > 0)
                {
                    _selectedItem.ToList()[0].SG_STATUS = 1;

                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("DeleteSGOPEN",entity);
                    
                }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var _selectedItem = (from item in entity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

                if (_selectedItem.Count() > 0) return false;
                else return true;
            }
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

                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    var selectedItem = (from item in entity.SG_TAOLI_OPEN_TABLE where item.SG_ID == ID select item);

                    if (selectedItem.Count() == 0) return;
                    else
                    {
                        selectedItem.ToList()[0].SG_STATUS = status;

                        DBChangeSave save = new DBChangeSave();
                        save.Dbsavechage("UpdateSGOPENStatus", entity);
                        
                    }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.SG_TAOLI_CLOSE_TABLE.Add(item);

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertSGCLOSE", entity);
            }

        }

        public static void DeleteSGCLOSE(string ID)
        {
            if (DBAccessLayer.DBEnable == false) { return; }
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var _selectedItem = (from item in entity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == ID && item.SG_STATUS == 0 select item);

                if (_selectedItem.Count() > 0)
                {
                    _selectedItem.ToList()[0].SG_STATUS = 1;
                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("DeleteSGCLOSE", entity);
                    
                }
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var _selectedItem = (from item in entity.SG_TAOLI_CLOSE_TABLE where item.SG_ID == ID && item.SG_STATUS == 0 select item);

                if (_selectedItem.Count() > 0) return false;
                else return true;
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.SG_TAOLI_STATUS_TABLE.Add(item);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertSTATUS", entity);

            }
        }
        #endregion

        #region 授权策略
        public static void InsertAuthorizedStrategy(Object para)
        {
            if (DBAccessLayer.DBEnable == false) return;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                Dictionary<String, Object> Para = (Dictionary<String, Object>)para;

                String StrNo = Para["strategyNo"].ToString();
                String User = Para["User"].ToString();
                List<AuthorizedOrder> trades = (List<AuthorizedOrder>)Para["orders"];
                String file = String.Empty;

                AuthorizedStrategyTable row = new AuthorizedStrategyTable()
                {
                    ID = Guid.NewGuid(),
                    cUser = User,
                    ImportFile = file,
                    ImportTime = DateTime.Now,
                    Strno = StrNo,
                    TradeNum = trades.Count,
                    FinishFlag = false
                };

                entity.AuthorizedStrategyTable.Add(row);

                for (int i = 0; i < trades.Count; i++)
                {
                    AuthorizedOrder order = trades[i];
                    AuthorizedTradeTable trade = new AuthorizedTradeTable()
                    {
                        ID = Guid.NewGuid(),
                        Code = order.cSecurityCode,
                        dealPrice = 0,
                        dealTime = new DateTime(1900, 1, 1),
                        limitedFlag = order.LimitedPrice,
                        lossPrice = order.LossValue,
                        Offsetflag = order.offsetflag,
                        Orientation = order.cTradeDirection,
                        requestPrice = order.dOrderPrice,
                        startTime = new DateTime(1900, 1, 1),
                        status = order.Status.ToString(),
                        surplusPrice = order.SurplusValue,
                        StrNo = order.belongStrategy,
                        TradeNum = (int?)(order.nSecurityAmount),
                        type = order.cSecurityType,
                        Cost = order.cost,
                        exchangeid = order.exchangeId,
                        describe = AuthorizedStatus.GetStatus(order.Status)
                    };

                    entity.AuthorizedTradeTable.Add(trade);
                }

                entity.SaveChanges();
            }
        }

        public static void  DeleteAuthorizedStrategy(String StrNo)
        {
            if (DBAccessLayer.DBEnable == false) return;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var records = (from item in entity.AuthorizedStrategyTable where item.Strno == StrNo select item);

                if (records.Count() > 0)
                {
                    records.ToList()[0].FinishFlag = true;
                }

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("DeleteAuthorizedStrategy", entity);
            }
        }

        public static void UpdateAuthorizedTrade(Object obj) 
        {

            lock (AuthorizedUpdateLock)
            {
                Dictionary<String, String> paras = (Dictionary<String, String>)obj;

                String StrNo = paras["strno"];
                String Code = paras["code"];
                double dealPrice = Convert.ToDouble(paras["dealprice"].Trim());
                int status = Convert.ToInt16(paras["status"].Trim());

                if (DBAccessLayer.DBEnable == false) return;
                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    var records = (from item in entity.AuthorizedTradeTable where item.StrNo == StrNo && item.Code == Code select item);

                    if (records.Count() == 0) return;

                    AuthorizedTradeTable record = records.ToList()[0];

                    switch (status)
                    {
                        case (int)AuthorizedTradeStatus.Pause:
                            {
                                record.status = status.ToString();
                                record.describe = AuthorizedStatus.GetStatus(status);
                                break;
                            }
                        case (int)AuthorizedTradeStatus.Running:
                            {
                                if (record.status == ((int)AuthorizedTradeStatus.Init).ToString())
                                {
                                    //初始running时间
                                    record.status = status.ToString();
                                    record.describe = AuthorizedStatus.GetStatus(status);
                                    record.startTime = DateTime.Now;
                                }
                                break;
                            }
                        case (int)AuthorizedTradeStatus.Stop:
                            {
                                record.status = status.ToString();
                                record.describe = AuthorizedStatus.GetStatus(status);
                                break;
                            }
                        case (int)AuthorizedTradeStatus.Dealed:
                            {
                                record.status = status.ToString();
                                record.describe = AuthorizedStatus.GetStatus(status);
                                record.dealTime = DateTime.Now;
                                record.dealPrice = dealPrice;
                                break;
                            }
                        default:
                            {
                                break;
                            }

                    }

                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("UpdateAuthorizedTrade", entity);
                    
                }
            }
        }

        public static void BatchUpdateAuthorizedTrade(Object obj)
        {

            lock (AuthorizedUpdateLock)
            {
                List<Dictionary<String, String>> paraDics = (List<Dictionary<String, String>>)obj;
                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    foreach (Dictionary<String, String> paras in paraDics)
                    {
                        String StrNo = paras["strno"];
                        String Code = paras["code"];
                        double dealPrice = Convert.ToDouble(paras["dealprice"].Trim());
                        int status = Convert.ToInt16(paras["status"].Trim());

                        if (DBAccessLayer.DBEnable == false) return;

                        var records = (from item in entity.AuthorizedTradeTable where item.StrNo == StrNo && item.Code == Code select item);

                        if (records.Count() == 0) return;

                        AuthorizedTradeTable record = records.ToList()[0];

                        switch (status)
                        {
                            case (int)AuthorizedTradeStatus.Pause:
                                {
                                    record.status = status.ToString();
                                    record.describe = AuthorizedStatus.GetStatus(status);
                                    break;
                                }
                            case (int)AuthorizedTradeStatus.Running:
                                {
                                    if (record.status == ((int)AuthorizedTradeStatus.Init).ToString())
                                    {
                                        //初始running时间
                                        record.status = status.ToString();
                                        record.describe = AuthorizedStatus.GetStatus(status);
                                        record.startTime = DateTime.Now;
                                    }
                                    break;
                                }
                            case (int)AuthorizedTradeStatus.Stop:
                                {
                                    record.status = status.ToString();
                                    record.describe = AuthorizedStatus.GetStatus(status);
                                    break;
                                }
                            case (int)AuthorizedTradeStatus.Dealed:
                                {
                                    record.status = status.ToString();
                                    record.describe = AuthorizedStatus.GetStatus(status);
                                    record.dealTime = DateTime.Now;
                                    record.dealPrice = dealPrice;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }

                        } 
                    }

                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("UpdateAuthorizedTrade", entity);
                }
            }
        }

        public static Dictionary<String, List<AuthorizedOrder>> LoadPauseStrategy()
        {
            if (DBAccessLayer.DBEnable == false) return new Dictionary<string,List<AuthorizedOrder>>();

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var str_records = (from item in entity.AuthorizedStrategyTable where item.FinishFlag == false select item);

                if (str_records.Count() == 0) return new Dictionary<string, List<AuthorizedOrder>>();

                Dictionary<String, List<AuthorizedOrder>> orders = new Dictionary<string, List<AuthorizedOrder>>();

                foreach (AuthorizedStrategyTable str_record in str_records)
                {
                    orders.Add(str_record.Strno, new List<AuthorizedOrder>());

                    var trade_records = (from item in entity.AuthorizedTradeTable where item.StrNo == str_record.Strno select item);

                    if (trade_records.Count() == 0) continue;

                    foreach (AuthorizedTradeTable trade_record in trade_records)
                    {
                        AuthorizedOrder order = new AuthorizedOrder()
                        {
                            belongStrategy = str_record.Strno,
                            Status = Convert.ToInt16(trade_record.status),
                            cSecurityCode = trade_record.Code,
                            cSecurityType = trade_record.type,
                            nSecurityAmount = (long)trade_record.TradeNum,
                            SurplusValue = Convert.ToSingle(trade_record.surplusPrice),
                            dOrderPrice = (double)trade_record.requestPrice,
                            cTradeDirection = trade_record.Orientation,
                            offsetflag = trade_record.Offsetflag,
                            LossValue = (float)trade_record.lossPrice,
                            LimitedPrice = trade_record.limitedFlag,
                            dDealPrice = (double)trade_record.dealPrice,
                            exchangeId = trade_record.exchangeid,
                            OrderRef = 0,
                            User = str_record.cUser,
                            cost = Convert.ToSingle(trade_record.Cost)

                        };

                        orders[str_record.Strno].Add(order);
                    }
                }

                return orders;
            }
        }

        public static void DailyDBExchange(object obj)
        {
            if(DBAccessLayer.DBEnable == false) return;

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var str_records = (from item in entity.AuthorizedStrategyTable where item.FinishFlag == true select item);

                if (str_records.Count() == 0) return;

                foreach (AuthorizedStrategyTable str_record in str_records.ToList())
                {
                    var trade_records = (from item in entity.AuthorizedTradeTable where item.StrNo == str_record.Strno select item);

                    if (trade_records.Count() == 0) continue;

                    AuthorizedStrategyTable_HIS str_his_record = new AuthorizedStrategyTable_HIS()
                    {
                        cUser = str_record.cUser,
                        FinishFlag = str_record.FinishFlag,
                        ID = str_record.ID,
                        ImportFile = str_record.ImportFile,
                        ImportTime = str_record.ImportTime,
                        Strno = str_record.Strno,
                        TradeNum = str_record.TradeNum
                    };

                    List<AuthorizedTradeTable> trades = new List<AuthorizedTradeTable>();
                    lock (trade_records)
                    {
                        foreach (AuthorizedTradeTable trade_record in trade_records)
                        {
                            AuthorizedTradeTable_HIS trade_his_record = new AuthorizedTradeTable_HIS()
                            {
                                Code = trade_record.Code,
                                dealPrice = trade_record.dealPrice,
                                dealTime = trade_record.dealTime,
                                describe = trade_record.describe,
                                exchangeid = trade_record.exchangeid,
                                ID = trade_record.ID,
                                limitedFlag = trade_record.limitedFlag,
                                lossPrice = trade_record.lossPrice,
                                Offsetflag = trade_record.Offsetflag,
                                Orientation = trade_record.Orientation,
                                requestPrice = trade_record.requestPrice,
                                startTime = trade_record.startTime,
                                status = trade_record.status,
                                StrNo = trade_record.StrNo,
                                surplusPrice = trade_record.surplusPrice,
                                TradeNum = trade_record.TradeNum,
                                type = trade_record.type,
                                Cost = trade_record.Cost
                            };

                            entity.AuthorizedTradeTable_HIS.Add(trade_his_record);
                            //DbEntityAuthorized.AuthorizedTradeTable.Remove(trade_record);
                            trades.Add(trade_record);

                        }

                        // 不直接删除，防止集合改变影响
                        for (int i = 0; i < trades.Count; i++)
                        {
                            entity.AuthorizedTradeTable.Remove(trades[i]);
                        }
                    }

                    entity.AuthorizedStrategyTable_HIS.Add(str_his_record);
                    entity.AuthorizedStrategyTable.Remove(str_record);

                }

                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("DailyDBExchange", entity);
                
            }
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.OL_TAOLI_LIST_TABLE.Add(item);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("InsertORDERLIST", entity);
            }
        }

        public static void DeleteORDERLIST(string strategyId)
        {
            if (DBAccessLayer.DBEnable == false) { return; }

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var selected = (from item in entity.OL_TAOLI_LIST_TABLE where item.SG_ID == strategyId select item);

                if (selected.Count() > 0)
                {
                    entity.OL_TAOLI_LIST_TABLE.Remove(selected.ToList()[0]);
                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("DeleteORDERLIST", entity);

                }
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

                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    entity.ER_TAOLI_TABLE.Add(record);
                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("CreateERRecord", entity);
                }
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

                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    entity.ER_TAOLI_TABLE.Add(record);
                    DBChangeSave save = new DBChangeSave();
                    save.Dbsavechage("CreateERRecord", entity);
                    
                }
            }
        }

        public static void DeleteERRecord(object Ref)
        {
            if (DBEnable == false) return;

            String OrderRef = ((int)Ref).ToString();

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var record = (from item in entity.ER_TAOLI_TABLE where item.ER_ORDER_REF == OrderRef select item);

                try
                {
                    if (record.Count() > 0)
                    {
                        lock (ERtableLock)
                        {
                            entity.ER_TAOLI_TABLE.Remove(record.ToList()[0]);
                            DBChangeSave save = new DBChangeSave();
                            save.Dbsavechage("DeleteERRecord", entity);

                        }

                        return;
                    }
                }
                catch
                {
                    return;
                }
                return;
            }
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
                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    var selected = (from item in entity.ER_TAOLI_TABLE where item.ER_ID == record.cOrderSysID select item);

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

                        DBChangeSave save = new DBChangeSave();
                        save.Dbsavechage("UpdateERRecord", entity);

                    }
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var ERs = (from item in entity.ER_TAOLI_TABLE where item.ER_ORDER_TYPE == sbType select item);

                if (ERs.Count() > 0)
                {
                    return ERs.ToList();
                }


                return null;
            }
        }
        #endregion

        #region 成交
        public static List<String> GetDealList(string strId, out decimal totalStockMoney, out decimal futureIndex)
        {
            totalStockMoney = 0;
            futureIndex = 0;

            if (DBAccessLayer.DBEnable)
            {
                using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                {
                    var _record = (from item in entity.DL_TAOLI_TABLE where item.DL_STRATEGY == strId select item);

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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                List<DL_TAOLI_TABLE> deals_record = (from item in entity.DL_TAOLI_TABLE where item.DL_USER == alias select item).OrderByDescending(i => i.DL_BARGAIN_TIME).ToList();

                if (deals_record != null && deals_record.Count > 0)
                {
                    return deals_record;
                }
                else
                {
                    return new List<DL_TAOLI_TABLE>();
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

                    double price = 0;

                    if(record.OrderType.ToString().ToUpper() == "F")
                    {
                        price = record.bargain_price / 1000;
                    }
                    else
                    {
                        price = record.bargain_price;
                    }

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
                        DL_BARGAIN_PRICE = price,
                        DL_BARGAIN_MONEY = record.bargain_money,
                        DL_BARGAIN_TIME = bargin_time,
                        DL_NO = record.OrderSysID.ToString(),
                        DL_LOAD = true,
                        DL_USER = record.User,
                        DL_MARK = record.OrderMark
                    };

                    using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                    {
                        entity.DL_TAOLI_TABLE.Add(item);

                        DBChangeSave save = new DBChangeSave();
                        save.Dbsavechage("CreateDLRecord", entity);
                    }
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
                    using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
                    {
                        var selected = (from row in entity.DL_TAOLI_TABLE where row.DL_NO == number select row);

                        if (selected.Count() > 0)
                        {
                            foreach (var i in selected.ToList())
                            {
                                entity.DL_TAOLI_TABLE.Remove(i);
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

                        entity.DL_TAOLI_TABLE.Add(item);
                        DBChangeSave save = new DBChangeSave();
                        save.Dbsavechage("CreateFutureDLRecord", entity);

                        Thread.Sleep(10);
                    }
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                if (type == "49")
                {
                    //股票
                    var selectedStock = (from item in entity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_TYPE == type && item.CC_USER == user select item);

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

                            entity.CC_TAOLI_TABLE.Add(record);

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
                            DBChangeSave save = new DBChangeSave();
                            save.Dbsavechage("UpdateCCRecords", entity);


                            //修改本地CC列表
                            List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
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
                                    entity.CC_TAOLI_TABLE.Remove(record);
                                    DBChangeSave save = new DBChangeSave();
                                    save.Dbsavechage("UpdateCCRecords", entity);


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
                                DBChangeSave savechange = new DBChangeSave();
                                savechange.Dbsavechage("UpdateCCRecords", entity);

                                //更新本地持仓列表
                                List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
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

                            DBChangeSave savechange = new DBChangeSave();
                            savechange.Dbsavechage("UpdateCCRecords", entity);

                            //更新本地持仓列表
                            List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
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
                        var selectedFuture = (from item in entity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_DIRECTION == sDirection && item.CC_TYPE == type && item.CC_USER == user select item);
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

                            entity.CC_TAOLI_TABLE.Add(record);

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

                            DBChangeSave savechange = new DBChangeSave();
                            savechange.Dbsavechage("UpdateCCRecords", entity);

                            //更新本地持仓列表
                            List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
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

                            DBChangeSave savechange = new DBChangeSave();
                            savechange.Dbsavechage("UpdateCCRecords", entity);

                            //更新本地持仓列表
                            List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                            accountMonitor.ChangeLocalCC(user.Trim(), records);

                        }
                    }
                    else
                    {
                        //平仓

                        var selectedFuture = (from item in entity.CC_TAOLI_TABLE where item.CC_CODE == code && item.CC_DIRECTION != sDirection && item.CC_TYPE == type && item.CC_USER == user select item);

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
                                    entity.CC_TAOLI_TABLE.Remove(record); DBChangeSave savechange = new DBChangeSave();
                                    savechange.Dbsavechage("UpdateCCRecords", entity);
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



                                //更新本地持仓列表
                                List<CC_TAOLI_TABLE> records = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user select item).ToList();
                                accountMonitor.ChangeLocalCC(user.Trim(), records);
                                DBChangeSave save2 = new DBChangeSave();
                                save2.Dbsavechage("UpdateCCRecords", entity);
                                return;
                            }
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



            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                if (userName != "*")
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == userName && item.CC_TYPE == "49" select item);
                    records = tmp.ToList();
                }
                else
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE where item.CC_TYPE == "49" select item);
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

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                if (userName != "*")
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == userName && item.CC_TYPE == "1" select item);
                    records = tmp.ToList();
                }
                else
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE where item.CC_TYPE == "1" select item);
                    records = tmp.ToList();
                }
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                if (userName != "*")
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE select item);
                    records = tmp.ToList();
                }
                else
                {
                    var tmp = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == userName select item);
                    records = tmp.ToList();
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
            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                var position = (from item in entity.CC_TAOLI_TABLE where item.CC_USER == user && item.CC_CODE == code select item);
            }


        }
        #endregion

        #region 日志相关
        /// <summary>
        /// 录入系统报错日志
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="exception">报错信息</param>
        public static void LogSysInfo(string module,string exception)
        {
            if (DBEnable == false) return;

            SYS_LOG log = new SYS_LOG()
            {
                ID = Guid.NewGuid(),
                logdate = DateTime.Now,
                logtime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
                module = module,
                exceptionWords = exception
            };

            using (MoneyEntityEntities3 entity = new MoneyEntityEntities3())
            {
                entity.SYS_LOG.Add(log);
                DBChangeSave save = new DBChangeSave();
                save.Dbsavechage("LogSysInfo", entity);
                
            }
        }
        #endregion

       
    }

    public class DBChangeSave
    {
        public void Dbsavechage(string type,MoneyEntityEntities3 entity)
        {
            bool lockdb = false;
            int count = 100;
            while (lockdb == false)
            {
                count--;
                try
                {
                    entity.SaveChanges();
                    lockdb = true;
                }
                catch (Exception ex)
                {
                    GlobalErrorLog.LogInstance.LogEvent("type = " + type + "\r\n" + ex.ToString());
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
}

