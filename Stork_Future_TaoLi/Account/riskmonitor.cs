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
            //riskPara.account = accountMonitor.GetAccountInfo(InputJson, out err);
            riskPara.account = accountMonitor.GetTestAccount(InputJson, out err);

            return JsonConvert.SerializeObject(riskPara);
        }

        public static String SetRiskParaJson(String InputJson, String WhiteLi,String BlackLi)
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

                riskPara.BlackNameList.Clear();
                foreach (string s in BlackLi.Split('\n'))
                {
                    if (s.Trim() == string.Empty) continue;
                    riskPara.BlackNameList.Add(s);
                    Records.Add(new BWNameTable()
                    {
                        ID = Guid.NewGuid(),
                        Code = s.Split('|')[0],
                        Amount = Convert.ToDecimal(s.Split('|')[1]),
                        PercentageA = Convert.ToDouble(s.Split('|')[2]),
                        Value = Convert.ToDecimal(s.Split('|')[3]),
                        PercentageB = Convert.ToDouble(s.Split('|')[4]),
                        flag = false

                    });
                }

                riskPara.changkouRadio = para.changkouRadio;
                riskPara.PerStockCostPercentage = para.PerStockCostPercentage;
                riskPara.riskLevel = para.riskLevel;


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
        /// 代码|流通股|比例
        /// </summary>
        public List<String> WhiteNameList = new List<string>();

        /// <summary>
        /// 风控黑名单
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
        /// 单只股票所占资金比例
        /// </summary>
        public double PerStockCostPercentage = 0;

        /// <summary>
        /// 持仓信息
        /// </summary>
        public AccountInfo account = new AccountInfo();

    }
}