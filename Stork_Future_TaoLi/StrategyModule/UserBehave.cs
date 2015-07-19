using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    class Basic
    {
        public String USER { get; set; }
        public String ACTIVITY { get; set; }
        public String ORIENTATION { get; set; }
    }

    /// <summary>
    /// 创建开仓
    /// </summary>
    class OPENCREATE
    {
        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP { get; set; }

        /// <summary>
        /// 开仓手数
        /// </summary>
        public int HD { get; set; }

        /// <summary>
        /// 合约数量
        /// </summary>
        public string CT { get; set; }
        public string Index { get; set; }
        public IDictionary<String, float> SNMP { get; set; }
    }

    /// <summary>
    /// 启动开仓运行
    /// </summary>
    class OPENRUN { }

    /// <summary>
    /// 允许开仓交易
    /// </summary>
    class OPENALLOW { }

    /// <summary>
    /// 删除开仓
    /// </summary>
    class OPENDELETE { }

    /// <summary>
    /// 创建平仓
    /// </summary>
    class CLOSECREATE { }

    /// <summary>
    /// 启动平仓运行
    /// </summary>
    class CLOSERUN { }

    /// <summary>
    /// 允许平仓交易
    /// </summary>
    class CLOSEALLOW { }

    /// <summary>
    /// 删除平仓
    /// </summary>
    class CLOSEDELETE { }
}