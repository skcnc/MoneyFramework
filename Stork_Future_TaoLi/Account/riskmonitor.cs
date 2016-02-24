using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;
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
            UserInfo user = DBAccessLayer.GetOneUser(name);



            double overflow = 0.02; //溢价余量，用于控制金额空间和成本空间的差值。
            result = string.Empty;
            int errCode = 0;
            AccountInfo accInfo = accountMonitor.UpdateAccount(user);

            if (accInfo == null) { return false; }

            

            //判断一：股票资金限制
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


            result = accountMonitor.GetErrorCode(errCode,string.Empty);

            if (errCode != 0) return false;

            

            //判断二：交易股票是否存在白名单，比例是否满足 , 单只股票所占金额比例

            List<BWNameTable> whiteli = new List<BWNameTable>();

            foreach (string s in riskPara.WhiteNameList)
            {
                whiteli.Add(new BWNameTable()
                {
                    Code = s.Split('|')[0].Trim(),
                    Amount = Convert.ToDecimal(s.Split('|')[1].Trim()),
                    PercentageA = Convert.ToDouble(s.Split('|')[2].Trim()),
                    Value = Convert.ToDecimal(s.Split('|')[3].Trim()),
                    PercentageB = Convert.ToDouble(s.Split('|')[4].Trim())
                });

               
            }

            double totalCost = 0;//股票预计成本

            AccountInfo account = accountMonitor.GetAccountInfo(name, out result);

            foreach (TradeOrderStruct tos in orderlist)
            {
                totalCost += (tos.nSecurityAmount * tos.dOrderPrice);
                var tmp = (from item in whiteli where item.Code == tos.cSecurityCode select item);

                if (tmp.Count() == 0)
                {
                    errCode = 4;
                    result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode );
                    return false;
                }

                BWNameTable whiteItem = tmp.ToList()[0];

                long posNum = accountMonitor.GetStockTotalPositionAmount(tos.cSecurityCode.Trim());

                if((Convert.ToDecimal(whiteItem.PercentageA) * whiteItem.Amount < posNum)||(Convert.ToDecimal(whiteItem.PercentageB) * whiteItem.Value < posNum))
                {
                    errCode = 5;
                    result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode );
                    return false;
                }
                
                

                //仅买入时判断单支超限
                if (tos.cTradeDirection == "0")
                {
                    if (tos.dOrderPrice * tos.nSecurityAmount > (Convert.ToDouble(account.value.Trim() + Convert.ToDouble(account.balance.Trim()) * riskPara.PerStockCostPercentage)))
                    {
                        errCode = 6;
                        result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                        return false;
                    }
                }
            }

            //判断三：股票成本是否超限
            if(totalCost < (Convert.ToDouble(account.balance) * 1.02))
            {
                errCode = 7;
                result = accountMonitor.GetErrorCode(errCode, totalCost.ToString());
                return false;
            }

            //判断四： 期货风险度

            //判断五： 敞口比例
            //判断六： 股票占总资金比例

            if (errCode != 0) return false;
            else return true;

        }

        private static riskParameter riskPara = new riskParameter();


        public static void Init()
        {
            //初始化加载风控参数
            List<BWNameTable> BWRecords = DBAccessLayer.GetWBNamwList();

            if (BWRecords == null || BWRecords.Count == 0)
            {

                riskPara.WhiteNameList.Clear();

                riskPara.WhiteNameList.Add("600001|210000|0.05|1000000|0.1");
                riskPara.WhiteNameList.Add("600002|230000|0.05|1000000|0.1");
                riskPara.WhiteNameList.Add("600003|270000|0.05|1000000|0.1");

                riskPara.BlackNameList.Clear();

                riskPara.BlackNameList.Add("600004|210000|0.05|1000000|0.1");
                riskPara.BlackNameList.Add("600005|230000|0.05|1000000|0.1");
                riskPara.BlackNameList.Add("600006|270000|0.05|1000000|0.1");
            }
            else
            {
                riskPara.WhiteNameList.Clear();
                riskPara.BlackNameList.Clear();

                foreach (BWNameTable record in BWRecords)
                {
                    if (record.flag == true)
                    {
                        riskPara.WhiteNameList.Add(record.Code + "|" + record.Amount + "|" + record.PercentageA + "|" + record.Value + "|" + record.PercentageB);
                    }
                    else
                    {
                        riskPara.BlackNameList.Add(record.Code + "|" + record.Amount + "|" + record.PercentageA + "|" + record.Value + "|" + record.PercentageB);
                    }
                }
            }
        }

        /// <summary>
        /// 获取风控参数
        /// </summary>
        /// <returns>风控参数的json字符串</returns>
        public static String GetRiskParaJson(String InputJson)
        {

            
            string err = string.Empty;
            riskPara.account = accountMonitor.GetAccountInfo(InputJson, out err);
            
            return JsonConvert.SerializeObject(riskPara);
        }

        public static String SetRiskParaJson(String InputJson, String WhiteLi)
        {
            try
            {
                riskParameter para = JsonConvert.DeserializeObject<riskParameter>(InputJson);

                List<BWNameTable> Records = new List<BWNameTable>();

                riskPara.WhiteNameList.Clear();


                foreach (string s in WhiteLi.Split('\n'))
                {
                    if (s.Trim() == string.Empty) continue;
                    riskPara.WhiteNameList.Add(s);

                    Records.Add(new BWNameTable()
                    {
                        ID = Guid.NewGuid(),
                        Code = s.Split('|')[0],
                        Amount = Convert.ToDecimal(s.Split('|')[1]),
                        PercentageA = Convert.ToDouble(s.Split('|')[2]),
                        Value = Convert.ToDecimal(s.Split('|')[3]),
                        PercentageB = Convert.ToDouble(s.Split('|')[4]),
                        flag = true

                    });
                }

                
                riskPara.changkouRadio = para.changkouRadio;
                riskPara.PerStockCostPercentage = para.PerStockCostPercentage;
                riskPara.riskLevel = para.riskLevel;
                riskPara.stockRadio = para.stockRadio;


                DBAccessLayer.SetWBNameList(Records);
                

                return "success";
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
    }

    /// <summary>
    /// 风控参数
    /// </summary>
    public class riskParameter
    {
        /// <summary>
        /// 风控白名单
        /// 代码|流通股|比例|总股本|比例
        /// </summary>
        public List<String> WhiteNameList = new List<string>();

        /// <summary>
        /// 风控黑名单
        /// 代码|流通股|比例|总股本|比例
        /// </summary>
        public List<String> BlackNameList = new List<string>();

        /// <summary>
        /// 敞口比例
        /// </summary>
        public double changkouRadio = 0;

        /// <summary>
        /// 风险度限制
        /// </summary>
        public double riskLevel = 0;

        /// <summary>
        /// 股票占总资金比例
        /// 全部股票市值 除以 （证券总资产 加上期货权益）
        /// </summary>
        public double stockRadio = 0;

        /// <summary>
        /// 单只股票所占资金比例
        /// </summary>
        public double PerStockCostPercentage = 0;

        /// <summary>
        /// 持仓信息
        /// </summary>
        public AccountInfo account = new AccountInfo();

    }
}