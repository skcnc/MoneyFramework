using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TDFAPI;
using Stork_Future_TaoLi;

namespace Stork_Future_TaoLi.TDFDataSource
{
    public class TDFSourceImp : TDFAPI.TDFDataSource
    {
        private static LogWirter log = new LogWirter();

        public TDFSourceImp(TDFOpenSetting_EXT openSetting_ext)
            : base(openSetting_ext)
        {
            ShowAllData = false;
        }
        public bool ShowAllData { get; set; }

        //重载 OnRecvSysMsg 方法，接收系统消息通知
        // 请注意：
        //  1. 不要在这个函数里做耗时操作
        //  2. 只在这个函数里做数据获取工作 -- 将数据复制到其它数据缓存区，由其它线程做业务逻辑处理
        public override void OnRecvSysMsg(TDFMSG msg)
        {
            //throw new NotImplementedException();

        }
    }
}