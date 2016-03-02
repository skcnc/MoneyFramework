using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TDFAPI;

namespace MarketInfoSys
{
    public class TDFSourceImp : TDFAPI.TDFDataSource
    {
        private static LogWirter logA = new LogWirter();
        private static LogWirter logB = new LogWirter();

        public TDFSourceImp(TDFOpenSetting_EXT openSetting_ext)
            : base(openSetting_ext)
        {
            ShowAllData = false;

            logA.EventSourceName = "行情获取日志A";
            logA.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            logA.EventLogID = 63001;

            logB.EventSourceName = "行情获取日志B";
            logB.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            logB.EventLogID = 63002;
        }
        public bool ShowAllData { get; set; }

        //重载 OnRecvSysMsg 方法，接收系统消息通知
        // 请注意：
        //  1. 不要在这个函数里做耗时操作
        //  2. 只在这个函数里做数据获取工作 -- 将数据复制到其它数据缓存区，由其它线程做业务逻辑处理
        public override void OnRecvSysMsg(TDFMSG msg)
        {
            //throw new NotImplementedException();
            if (msg.MsgID == TDFMSGID.MSG_SYS_CONNECT_RESULT)
            {
                //连接结果
                TDFConnectResult connectResult = msg.Data as TDFConnectResult;
                string strPrefix = connectResult.ConnResult ? "连接成功" : "连接失败";
                logA.LogEvent("系统服务器连接情况 ：" + strPrefix);
            }
            else if (msg.MsgID == TDFMSGID.MSG_SYS_LOGIN_RESULT)
            {
                //登陆结果
                TDFLoginResult loginResult = msg.Data as TDFLoginResult;
                if (loginResult.LoginResult)
                {
                    logA.LogEvent("系统服务器登陆成功");
                    for (int i = 0; i < loginResult.Markets.Length; i++)
                    {
                        logA.LogEvent(String.Format("market:{0},dyn-date:{1}", loginResult.Markets[i], loginResult.DynDate[i]));
                    }
                }
                else
                {
                    logA.LogEvent(String.Format("登陆失败！ info:{0}", loginResult.Info));
                }
            }
            else if (msg.MsgID == TDFMSGID.MSG_SYS_CODETABLE_RESULT)
            {
                //接收代码表结果
                TDFCodeResult codeResult = msg.Data as TDFCodeResult;
                logA.LogEvent(String.Format("获取到代码表, info:{0}，市场个数:{1}", codeResult.Info, codeResult.Markets.Length));

                //代码表是什么鸟玩意？？
                
                TDFCode[] codeArrSZ;
                GetCodeTable("SZ", out codeArrSZ);

                TDFCode[] codeArrSH;
                GetCodeTable("SH", out codeArrSH);

                //for (int i = 0; i < 100 && i < codeArr.Length; i++)
                //{
                //    if (codeArr[i].Type >= 0x90 && codeArr[i].Type <= 0x95)
                //    {
                //        // 期权数据
                //        TDFOptionCode code = new TDFOptionCode();
                //        var ret = GetOptionCodeInfo(codeArr[i].WindCode, ref code);
                //        PrintHelper.PrintObject(code);
                //    }
                //    else
                //    {
                //        PrintHelper.PrintObject(codeArr[i]);
                //    }
                //}
            }
            else if (msg.MsgID == TDFMSGID.MSG_SYS_QUOTATIONDATE_CHANGE)
            {
                //行情日期变更。
                TDFQuotationDateChange quotationChange = msg.Data as TDFQuotationDateChange;
                logA.LogEvent(String.Format("接收到行情日期变更通知消息，market:{0}, old date:{1}, new date:{2}", quotationChange.Market, quotationChange.OldDate, quotationChange.NewDate));
            }
            else if (msg.MsgID == TDFMSGID.MSG_SYS_MARKET_CLOSE)
            {
                //闭市消息
                TDFMarketClose marketClose = msg.Data as TDFMarketClose;
                logA.LogEvent(String.Format("接收到闭市消息, 交易所:{0}, 时间:{1}, 信息:{2}", marketClose.Market, marketClose.Time, marketClose.Info));
            }
            else if (msg.MsgID == TDFMSGID.MSG_SYS_HEART_BEAT)
            {
                //心跳消息
            }
        }


        //重载OnRecvDataMsg方法，接收行情数据
        // 请注意：
        //  1. 不要在这个函数里做耗时操作
        //  2. 只在这个函数里做数据获取工作 -- 将数据复制到其它数据缓存区，由其它线程做业务逻辑处理
        public override void OnRecvDataMsg(TDFMSG msg)
        {
            if (msg.MsgID == TDFMSGID.MSG_DATA_MARKET)
            {
                //行情消息
                TDFMarketData[] marketDataArr = msg.Data as TDFMarketData[];

                foreach (TDFMarketData data in marketDataArr)
                {
                    EnQueueType obj = new EnQueueType() { Type = "S", value = (object)data };
                    if (Queue_Data.Suspend == false)
                    {
                        if (!simulate_trade.MarketRecorder)
                        {
                            Queue_Data.GetQueue().Enqueue((object)obj);
                        }
                        if (data.Status == 68)
                        {
                            stop_plate_stocks.GetInstance().updateStopList(data);
                        }

                        if (simulate_trade.MarketRecorder)
                        {
                            MarketInfoQueue.EnQueueNew(data);
                        }
                    }
                }
            }
            else if (msg.MsgID == TDFMSGID.MSG_DATA_FUTURE)
            {
                //期货行情数据
                TDFFutureData[] futureDataArr = msg.Data as TDFFutureData[];

                foreach (TDFFutureData data in futureDataArr)
                {
                        EnQueueType obj = new EnQueueType() { Type = "F", value = (object)data };
                        if (Queue_Data.Suspend == false)
                        {
                            Queue_Data.GetQueue().Enqueue((object)obj);
                        }
                    
                }

            }
            else if (msg.MsgID == TDFMSGID.MSG_DATA_INDEX)
            {
                //指数消息
                TDFIndexData[] indexDataArr = msg.Data as TDFIndexData[];

                foreach (TDFIndexData data in indexDataArr)
                {
                    EnQueueType obj = new EnQueueType() { Type = "I", value = (object)data };
                    if (Queue_Data.Suspend == false)
                    {
                        Queue_Data.GetQueue().Enqueue((object)obj);
                    }
                }
            }
        }
    }
}