using Newtonsoft.Json;
using Stork_Future_TaoLi.Database;
using Stork_Future_TaoLi.Hubs;
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

        private static Dictionary<String, int> FutureTradeTimes = new Dictionary<string, int>();
        private static DateTime FutureTradeTimes_reInitDate = new DateTime(2000, 1, 1);


        /// <summary>
        /// 本地缓存风控信息，本地风控信息逐秒更新
        /// </summary>
        public static Dictionary<String, TMRiskInfo> LocalRiskInfo = new Dictionary<string, TMRiskInfo>();

        /// <summary>
        /// 股票风控检测
        /// </summary>
        public  static bool RiskControl(string name, List<TradeOrderStruct> orderlist, out string result)
        {
            name = name.Trim();
            if(!FutureTradeTimes.Keys.Contains(name))
            {
                FutureTradeTimes.Add(name, 10);
            }

            if((DateTime.Now - FutureTradeTimes_reInitDate).TotalDays > 1)
            {
                FutureTradeTimes_reInitDate = DateTime.Now;

                for(int i =0;i<FutureTradeTimes.Count;i++)
                {
                    FutureTradeTimes[FutureTradeTimes.Keys.ToList()[i]] = 10;
                }
            }

            UserInfo user = DBAccessLayer.GetOneUser(name);

            double overflow = 0.02; //溢价余量，用于控制金额空间和成本空间的差值。
            result = string.Empty;
            int errCode = 0;
            AccountInfo accInfo = accountMonitor.UpdateAccount(user);

            if (accInfo == null)
            {
                result = "未能获得实时账户";
                return false;
            }

            

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

            List<BWNameTable> whiteli = DBAccessLayer.GetWBNamwList();

            foreach (TradeOrderStruct tos in orderlist)
            {
                if (tos.cSecurityType == "s" || tos.cSecurityType == "S")
                {
                    if (!((from item in whiteli select item.Code).ToList()).Contains(tos.cSecurityCode.Trim()))
                    {
                        errCode = 4;
                        result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                        return false;
                    }
                    else
                    {
                        BWNameTable bw = (from item in whiteli where item.Code == tos.cSecurityCode select item).ToList()[0];

                        if ((from item in accInfo.positions where item.code == bw.Code select item).Count() > 0)
                        {
                            AccountPosition ap = (from item in accInfo.positions where item.code == bw.Code select item).ToList()[0];

                            if ((Convert.ToDouble(ap.amount) + Convert.ToDouble(tos.nSecurityAmount)) > (Convert.ToDouble(bw.Amount) * bw.PercentageA))
                            {
                                errCode = 5;
                                result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                                return false;
                            }
                        }
                    }
                }
            }

            double totalCost = 0;//股票预计成本


            foreach (TradeOrderStruct tos in orderlist)
            {
                if(tos.cSecurityType != "S" && tos.cSecurityType != "s")
                {
                    continue;
                }

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
                    if (tos.dOrderPrice * tos.nSecurityAmount > (Convert.ToDouble(accInfo.value.Trim() + Convert.ToDouble(accInfo.balance.Trim()))) * riskPara.PerStockCostPercentage)
                    {
                        errCode = 6;
                        result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                        return false;
                    }
                }
            }

            //判断三：股票成本是否超限
            if (totalCost * (1 + overflow) > Convert.ToDouble(accInfo.balance))
            {
                errCode = 7;
                result = accountMonitor.GetErrorCode(errCode, totalCost.ToString());
                return false;
            }


            double fearning = Convert.ToDouble(accInfo.fincome);        //期货盈亏
            double fstock = Convert.ToDouble(accInfo.fstockvalue);      //期货对应股票市值
            double fweight = Convert.ToDouble(accInfo.fvalue);          //期货权益


            string accountLimit = DBAccessLayer.GetAccountAvailable(user.alias);
            double stockAccount = 0;                //股票资金量
            double futureAccount = 0;               //期货原始权益

            if (accountLimit == string.Empty)
            {
                result = "无用户可用资金和用户权益";
                return false;
            }


            stockAccount = Convert.ToDouble(accountLimit.Split('|')[0].Trim());
            futureAccount = Convert.ToDouble(accountLimit.Split('|')[0].Trim());

            //预估交易后的期货风控参数
            //客户权益 = 当日结存 + 浮动盈亏 ，其中新交易不受浮动盈亏影响，所以客户权益不变
            //期货保证金 = 期货对应股票市值 * 系数 

            foreach (TradeOrderStruct record in orderlist)
            {
                if(record.cSecurityType != "f" && record.cSecurityType != "F")
                {
                    continue;
                }
                FutureTradeTimes[name] -= Convert.ToInt16(record.nSecurityAmount);
                if (MarketPrice.market.ContainsKey(record.cSecurityCode))
                {
                    int x = -1;
                    if (record.cTradeDirection == "48")
                    {
                        x = 1;
                    }

                    // 模拟新期货开仓后
                    fstock += Convert.ToDouble((record.nSecurityAmount) * MarketPrice.market[record.cSecurityCode] * accountMonitor.factor * x);

                    fearning += Convert.ToDouble((record.nSecurityAmount) * (MarketPrice.market[record.cSecurityCode] - record.dOrderPrice) * x);
                    futureAccount += Convert.ToDouble((record.nSecurityAmount) * (MarketPrice.market[record.cSecurityCode] - record.dOrderPrice) * x);
                }
            }

            fstock = Math.Abs(fstock);

            fweight = futureAccount + fearning;

            double future_margin = fstock * accountMonitor.future_margin_factor;

            double frisk = future_margin / fweight;                     //期货风险度

            double fchangkou = (fstock + totalCost) / fstock;           //敞口比例


            foreach (TradeOrderStruct record in orderlist)
            {
                if (record.cSecurityType != "f" && record.cSecurityType != "F")
                {
                    continue;
                }
                //单日期货不超过十笔
                if (FutureTradeTimes[name] < 0)
                {
                    errCode = 10;
                    result = accountMonitor.GetErrorCode(errCode, String.Empty);
                    return false;
                }

                //期货风险度判断
                if (Convert.ToDouble(frisk) > riskPara.riskLevel)
                {
                    errCode = 8;
                    result = accountMonitor.GetErrorCode(errCode, string.Empty);
                    return false;
                }



            }


            //敞口比例
            if (fchangkou > riskPara.changkouRadio)
            {
                errCode = 9;
                result = accountMonitor.GetErrorCode(errCode, String.Empty);
                return false;
            }

            if (errCode != 0) return false;
            else return true;

        }

      

        private static riskParameter riskPara = new riskParameter();


        public static void Init()
        {
            //初始化加载风控参数
            List<BWNameTable> BWRecords = DBAccessLayer.GetWBNamwList();

            riskPara.changkouRadio = 0.1;
            riskPara.riskLevel = 0.4;
            riskPara.PerStockCostPercentage = 0.05;
            riskPara.stockRadio = 0.05;

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
        /// 获取白名单列表
        /// </summary>
        public static string LoadWhiteList()
        {
            //初始化加载风控参数
            List<BWNameTable> BWRecords = DBAccessLayer.GetWBNamwList();

            riskPara.WhiteNameList.Clear();

            foreach (BWNameTable record in BWRecords)
            {
                riskPara.WhiteNameList.Add(record.Code + "|" + record.Amount + "|" + record.PercentageA + "|" + record.Value + "|" + record.PercentageB);
            }

            return JsonConvert.SerializeObject(BWRecords);
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

                if (WhiteLi.Trim() != string.Empty)
                {
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

                    DBAccessLayer.SetWBNameList(Records);

                }


                riskPara.changkouRadio = para.changkouRadio;
                riskPara.PerStockCostPercentage = para.PerStockCostPercentage;
                riskPara.riskLevel = para.riskLevel;
                riskPara.stockRadio = para.stockRadio;

                return "success";
            }
            catch (Exception ex)
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
        public double changkouRadio = 0.1;

        /// <summary>
        /// 风险度限制
        /// </summary>
        public double riskLevel = 0.65;

        /// <summary>
        /// 股票占总资金比例
        /// 全部股票市值 除以 （证券总资产 加上期货权益）
        /// </summary>
        public double stockRadio = 0.05;

        /// <summary>
        /// 单只股票所占资金比例
        /// </summary>
        public double PerStockCostPercentage = 0.05;

        /// <summary>
        /// 持仓信息
        /// </summary>
        public AccountInfo account = new AccountInfo();

    }


}