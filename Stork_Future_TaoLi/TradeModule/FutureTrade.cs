using Stork_Future_TaoLi.TradeModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class FutureTrade
    {
        private LogWirter sublog = new LogWirter();//子线程日志记录
        private CTP_CLI.CCTPClient _client;
        private FutureTradeStatus status = FutureTradeStatus.DISCONNECTED;

        private string BROKER = "0292";
        private string INVESTOR = "";
        private string PASSWORD = "";
        private string ADDRESS = "tcp://222.240.130.22:41205";

        

        /// <summary>
        /// 期货交易工作线程
        /// </summary>
        /// <param name="para"></param>
        public void FutureTradeSubThreadProc(object para)
        {

            string ErrorMsg = string.Empty;

            DateTime lastHeartBeat = DateTime.Now; //最近心跳时间

            //令该线程为前台线程
            Thread.CurrentThread.IsBackground = true;

            TradeParaPackage _tpp = (TradeParaPackage)para;

            //当前线程编号
            int _threadNo = _tpp._threadNo;

            sublog.LogEvent("线程 ：" + _threadNo.ToString() + " 开始执行");

            //用作发送心跳包的时间标记
            DateTime _markedTime = DateTime.Now;

            _client.Connect();

            //状态 DISCONNECTED -> CONNECTED
            while(this.status != FutureTradeStatus.CONNECTED)
            {
                Thread.Sleep(10);
            }

            _client.ReqUserLogin();

            //状态 CONNECTED -> LOGIN
            while (this.status != FutureTradeStatus.LOGIN)
            {
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 设置日志
        /// </summary>
        /// <param name="log"></param>
        public void SetLog(LogWirter log) { this.sublog = log; }


        public FutureTrade()
        {
            _client = new CTP_CLI.CCTPClient(INVESTOR, PASSWORD, BROKER, ADDRESS);
            _client.FrontConnected += _client_FrontConnected;
            _client.FrontDisconnected += _client_FrontDisconnected;
            _client.RspUserLogin += _client_RspUserLogin;
            _client.RspError += _client_RspError;
            _client.RtnOrder += _client_RtnOrder;
            _client.RtnTrade += _client_RtnTrade;
            _client.RspOrderAction += _client_RspOrderAction;
            _client.RspOrderInsert += _client_RspOrderInsert;
          
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        void _client_FrontConnected()
        {
            this.status = FutureTradeStatus.CONNECTED;
        }

        /// <summary>
        /// 链接失败，记入日志
        /// </summary>
        /// <param name="nReason"></param>
        void _client_FrontDisconnected(int nReason)
        {
            sublog.LogEvent("期货交易所链接失败，失败码：" + nReason.ToString());
        }

        void _client_RspOrderInsert(CTP_CLI.CThostFtdcInputOrderField_M pInputOrder, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
                throw new NotImplementedException();
        }

        void _client_RspOrderAction(CTP_CLI.CThostFtdcInputOrderActionField_M pInputOrderAction, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            throw new NotImplementedException();
        }

        void _client_RtnTrade(CTP_CLI.CThostFtdcTradeField_M pTrade)
        {
            throw new NotImplementedException();
        }

        void _client_RtnOrder(CTP_CLI.CThostFtdcOrderField_M pOrder)
        {
            throw new NotImplementedException();
        }


        

        void _client_RspUserLogin(CTP_CLI.CThostFtdcRspUserLoginField_M pRspUserLogin, CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            //throw new NotImplementedException();
        }

        void _client_RspError(CTP_CLI.CThostFtdcRspInfoField_M pRspInfo, int nRequestID, bool bIsLast)
        {
            throw new NotImplementedException();
        }
    }
}