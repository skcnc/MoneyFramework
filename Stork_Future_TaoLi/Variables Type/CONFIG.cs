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
        private static int stock_trade_thread_num = 1;

        /// <summary>
        /// 期货交易最大线程数
        /// </summary>
        public static int FUTURE_TRADE_THREAD_NUM { get { return future_trade_thread_num; } }
        private static int future_trade_thread_num = 0;

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
            return true;
        }
    }

    public class CommConfig
    {
        /// <summary>
        /// 股票通讯地址
        /// </summary>
        private static string stock_serverAddr = "127.0.0.1";
        public static string Stock_ServerAddr { get { return stock_serverAddr; } }

        /// <summary>
        /// 股票通信端口
        /// </summary>
        private static int stock_port = 80;
        public static int Stock_Port { get { return stock_port; } }

        /// <summary>
        /// 资金帐户
        /// </summary>
        private static string stock_account = "admin";
        public static string Stock_Account { get { return stock_account; } }

        /// <summary>
        /// 部门编号
        /// </summary>
        private static string stock_broker_id = "001";
        public static string Stock_BrokerID { get { return stock_broker_id; } }

        /// <summary>
        /// 帐户密码
        /// </summary>
        private static string stock_password = "000000";
        public static string Stock_Password { get { return stock_password; } }

        /// <summary>
        /// 股票帐户
        /// </summary>
        private static string stock_investor_id = "test user";
        public static string Stock_InvestorID { get { return stock_investor_id; } }
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
}