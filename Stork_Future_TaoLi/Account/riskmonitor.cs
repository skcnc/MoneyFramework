using Stork_Future_TaoLi.Variables_Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 风控模块
    /// </summary>
    public class riskmonitor
    {
        /// <summary>
        /// 查看交易资金余量
        /// </summary>
        public  static bool RiskControl(string name, List<TradeOrderStruct> orderlist, out string result)
        {

            double overflow = 0.02; //溢价余量，用于控制金额空间和成本空间的差值。
            result = string.Empty;
            int errCode = 0;
            AccountInfo accInfo = accountMonitor.GetAccountInfo(name, out result);

            if (accInfo == null) { return false; }


            //判断股票资金限制
            List<TradeOrderStruct> stocks_sh = (from item in orderlist where item.cExhcnageID == ExchangeID.SH select item).OrderBy(i => i.cOrderLevel).ToList();
            List<TradeOrderStruct> stocks_sz = (from item in orderlist where item.cExhcnageID == ExchangeID.SZ select item).OrderBy(i => i.cOrderLevel).ToList();

            //新交易股票成本
            double stock_cost = 0;

            if (stocks_sh.Count > 0)
            {
                foreach (TradeOrderStruct stock in stocks_sh)
                {
                    stock_cost += (stock.nSecurityAmount * stock.dOrderPrice);
                }
            }
            if (stocks_sz.Count > 0)
            {
                foreach (TradeOrderStruct stock in stocks_sz)
                {
                    stock_cost += (stock.nSecurityAmount * stock.dOrderPrice);
                }
            }

            if (Convert.ToDouble(accInfo.balance) > (1 + overflow) * stock_cost)
            {
                errCode = 0;
            }
            else
            {
                errCode = 1;
            }


            result = accountMonitor.GetErrorCode(errCode);

            if (errCode != 0) return false;
            else return true;

        }
    }
}