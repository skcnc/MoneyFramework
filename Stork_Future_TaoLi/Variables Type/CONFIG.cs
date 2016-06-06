using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Variables_Type
{
    /// <summary>
    /// 交易配置参数设置
    /// 本参数提供Init方法初始化参数
    /// 系统启动从配置文件读取
    /// 运行过程中不能修改
    /// </summary>
    public static class CONFIG
    {
        /// <summary>
        /// 股票交易最大线程数
        /// </summary>
        public static int STOCK_TRADE_THREAD_NUM { get { return stock_trade_thread_num; } }
        private static int stock_trade_thread_num = 100;

        /// <summary>货交易最大线程数
        /// 期
        /// 期货交易无需在线程中等待回报，少量交易数即可
        /// 目前期货交易线程数量只能唯一
        /// </summary>
        public static int FUTURE_TRADE_THREAD_NUM { get { return future_trade_thread_num; } }
        private static int future_trade_thread_num = 1;

        /// <summary>
        /// 股票交易等待响应的最大超时时间
        /// 股票总监控线程发现交易执行超过该时间则
        /// 1. 返回超时警告
        /// 2. 记录超时交易信息存入数据库
        /// 3. 向用户页面提出告警
        /// </summary>
        public static int STOCK_TRADE_OVERTIME { get { return stock_trade_overtime; } }
        private static int stock_trade_overtime = 15;

        /// <summary>
        /// 期货交易等待响应的最大超时时间
        /// 期货总监控线程发现交易执行超过该时间则
        /// 1. 返回超时警告
        /// 2. 记录超时交易信息存入数据库
        /// 3. 向用户页面提出告警
        /// </summary>
        public static int FUTURE_TRADE_OVERTIME { get { return future_trade_overtime; } }
        private static int future_trade_overtime = 15;

        /// <summary>
        /// 授权交易列表文件存储路径
        /// </summary>
        public static String AUTHORIZED_BASE_URL { get { return authorized_base_url; } }
        private static String authorized_base_url = "F:\\workbench\\suncheng\\downloads\\";

        /// <summary>
        /// 授权交易列表文件存储路径
        /// </summary>
        public static String AUTHORIZED_ARCHIVE_URL { get { return authorized_archive_url; } }
        private static String authorized_archive_url = "F:\\workbench\\suncheng\\archive\\";

        /// <summary>
        /// 初始化静态类参数
        /// </summary>
        /// <param name="STOCK_TRADE_THREAD_NUM">股票交易最大线程数</param>
        /// <param name="FUTURE_TRADE_THREAD_NUM">期货交易最大线程数</param>
        public static void InitConfig(CONFIG_PARA para)
        {
            stock_trade_thread_num = para.STOCK_TRADE_THREAD_NUM;
            future_trade_thread_num = para.FUTURE_TRADE_THREAD_NUM;
            stock_trade_overtime = para.STOCK_TRADE_OVERTIME;
            future_trade_overtime = para.FUTURE_TRADE_OVERTIME;
        }

        /// <summary>
        /// 控制参数，标记当前是否为测试
        /// </summary>
        /// <returns></returns>
        public static bool IsDebugging()
        {
            return false;
        }

        /// <summary>\
        /// 全局监控用户
        /// </summary>
        public static string GlobalMonitor { get { return globalmonitor; } }
        private static string globalmonitor = "sc";

        /// <summary>
        /// 本地ORDERREF重复性判断
        /// </summary>
        private static List<int> GlobalOrderRefList = new List<int>();
        public static bool CheckOrderRefAvailiable(int Ref)
        {
            if(GlobalOrderRefList.Contains(Ref))
            {
                return false;
            }
            else
            {
                GlobalOrderRefList.Add(Ref);
                return true;
            }
        }
    }

    public class CommConfig
    {
        /// <summary>
        /// 股票通讯地址
        /// </summary>
        private static string stock_serverAddr = "10.65.8.14";
        public static string Stock_ServerAddr { get { return stock_serverAddr; } }

        /// <summary>
        /// 股票通信端口
        /// </summary>
        private static int stock_port = 18887;
        public static int Stock_Port { get { return stock_port; } }

        /// <summary>
        /// 资金帐户
        /// </summary>
        private static string stock_account = "1653043461";
        public static string Stock_Account { get { return stock_account; } }

        /// <summary>
        /// 部门编号
        /// </summary>
        private static string stock_broker_id = "001";
        public static string Stock_BrokerID { get { return stock_broker_id; } }

        /// <summary>
        /// 帐户密码
        /// </summary>
        private static string stock_password = "607178";
        public static string Stock_Password { get { return stock_password; } }

        /// <summary>
        /// 股票帐户
        /// </summary>
        private static string stock_investor_id = "201509";
        public static string Stock_InvestorID { get { return stock_investor_id; } }

        /// <summary>
        /// 期货端口
        /// </summary>
        public static string BROKER { get { return broker; } }
        private static string broker = "8890";

        /// <summary>
        /// 期货账号
        /// </summary>
        public static string INVESTOR { get { return investor; } }
        private static string investor = "17730203";

        /// <summary>
        /// 期货密码
        /// </summary>
        public static string PASSWORD { get { return password; } }
        private static string password = "111111";

        /// <summary>
        /// 期货交易地址
        /// </summary>
        public static string ADDRESS { get { return address; } }
        private static string address = "tcp://119.15.140.81:41205";

    }

    /// <summary>
    /// 初始化配置文件的参数列表
    /// 需要在启动时声明并修改默认值
    /// </summary>
    public class CONFIG_PARA
    {
        public int STOCK_TRADE_THREAD_NUM = 100;
        public int STOCK_TRADE_OVERTIME = 15;
        public int FUTURE_TRADE_THREAD_NUM = 10;
        public int FUTURE_TRADE_OVERTIME = 15;
    }

    /// <summary>
    /// 行情系统全局参数
    /// </summary>
    public class CHangQingPARA
    {
        /// <summary>
        /// 行情服务器IP地址
        /// </summary>
        public static string IP
        {
            get { return _ip; }
        }
        private static string _ip = "127.0.0.1";

        /// <summary>
        /// 行情服务器端口号
        /// </summary>
        public static string PORT { get { return _port; } }
        private static string _port = "80";

        /// <summary>
        /// 行情用户名
        /// </summary>
        public static string USERNAME { get { return _username; } }
        private static string _username = "admin";

        /// <summary>
        /// 行情密码
        /// </summary>
        public static string PASSWORD { get { return _password; } }
        private static string _password = "admin";
    }

    /// <summary>
    /// 账户相关全局参数
    /// </summary>
    public class AccountPARA
    {
        /// <summary>
        /// 期货系数
        /// </summary>
        public static double Factor(string s)
        {
            if (s.Substring(0, 2) == "IC") return factor_200;
            else return factor_300;
        }
        private static double factor_300 = 300;
        private static double factor_200 = 200;

        /// <summary>
        /// 保证金系数
        /// </summary>
        public static double MarginValue { get { return marginValue; } }
        private static double marginValue = 0.3;



    }

}