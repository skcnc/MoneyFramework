using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ConsoleApplication2
{
    class Json
    {
        

        public static String GetJson()
        {
            TradeViewItem item = new TradeViewItem("4324", "45");
            return JsonConvert.SerializeObject(item);
        }
    }

    public class TradeViewItem
    {

        //public TradeViewItem(String _OrderRef, String _TradeID, String _Code, String _Direction, String _CombOff, String _Price, String _Volume, String _OrderSysID)
        //{
        //    OrderRef = _OrderRef;
        //    TradeID = _TradeID;
        //    Code = _Code;
        //    Direction = _Direction;
        //    CombOff = _CombOff;
        //    Price = _Price;
        //    Volume = _Volume;
        //    OrderSysID = _OrderSysID;
        //}

        public TradeViewItem(string orderref,string tradeId)
        {
            OrderRef = orderref;
            TradeID = tradeId;
        }
        /// <summary>
        /// 标记成交回报
        /// </summary>
        public String TYPE
        {
            get
            {
                return "TRADE";
            }
        }
        /// <summary>
        /// 系统号
        /// </summary>
        public String OrderRef { get; set; }

        /// <summary>
        /// 成交编号
        /// </summary>
        public String TradeID { get; set; }

        ///// <summary>
        ///// 合约/代码
        ///// </summary>
        //String Code { get; set; }

        ///// <summary>
        ///// 买卖
        ///// </summary>
        //String Direction { get; set; }

        ///// <summary>
        ///// 开平
        ///// </summary>
        //String CombOff { get; set; }

        ///// <summary>
        ///// 成交价格
        ///// </summary>
        //String Price { get; set; }

        ///// <summary>
        ///// 成交手数
        ///// </summary>
        //String Volume { get; set; }

        ///// <summary>
        ///// 报单编号
        ///// </summary>
        //String OrderSysID { get; set; }
    }
}
