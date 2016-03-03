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

        private static int FutureTradeHandLimitation = 10;


        /// <summary>
        /// 本地缓存风控信息
        /// </summary>
        public static Dictionary<String, TMRiskInfo> LocalRiskInfo = new Dictionary<string, TMRiskInfo>();

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

        /// <summary>
        /// 风控检测
        /// 仅针对股票买入和期货开仓进行判断
        /// 新交易影响持仓参数：
        ///     股票：
        ///         可用资金减少
        ///         股票成本增加
        ///         风控冻结资金量增加
        ///     期货：
        ///         期货冻结资金增加
        ///         可用资金减少
        /// </summary>
        /// <param name="alias">用户名</param>
        /// <param name="orderlist">交易列表</param>
        /// <param name="result">结果</param>
        /// <returns>是否通过风控</returns>
        public static bool RiskDetection(string alias, List<TradeOrderStruct> orderlist, out string result)
        {
            //第一步： 计算新交易引入后对于股票，期货持仓参数的预见性影响
            alias = alias.Trim();

            //获取真实实时账户信息
            UserInfo user = DBAccessLayer.GetOneUser(alias);

            if (user == null) 
            {
                result = "未查到用户：" + alias;
                return false; 
            }

            //刷新内存中的资金信息
            AccountInfo current_account = accountMonitor.UpdateAccountList(user);

            StockAccountTable stock_account = accountMonitor.GetStockAccount(alias);

            if (stock_account == null) {
                result = "stockAccountDictionary中不存在用户：" + alias;
                return false;
            }

            FutureAccountTable future_account = accountMonitor.GetFutureAccount(alias);

            if (future_account == null)
            {

                result = "futureAccountDictionary中不存在用户： " + alias;
            }

            //计划买入股票交易列表
            var Stock_to_buy_var = (from item
                                in orderlist
                                where item.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionBuy &&
                                (item.cSecurityType == "S" || item.cSecurityType == "s")
                                select item);

           
            List<TradeOrderStruct> Stock_to_buy = new List<TradeOrderStruct>();

            if (Stock_to_buy_var.Count() != 0) Stock_to_buy = Stock_to_buy_var.ToList();

            //计划卖出股票交易列表
            var Stock_to_sell_var = (from item
                                in orderlist
                                where item.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionSell &&
                                (item.cSecurityType == "S" || item.cSecurityType == "s")
                                select item);

             List<TradeOrderStruct> Stock_to_sell = new List<TradeOrderStruct>();

            if (Stock_to_sell_var.Count() != 0) Stock_to_sell = Stock_to_sell_var.ToList();

            //计划开仓期货交易列表
            var Future_to_open_var = (from item
                                     in orderlist
                                 where item.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetOpen &&
                                 (item.cSecurityType == "F" || item.cSecurityType == "f")
                                 select item);

            List<TradeOrderStruct> Future_to_open = new List<TradeOrderStruct>();

            if (Future_to_open_var.Count() != 0) Future_to_open = Future_to_open_var.ToList();

            //计划平仓期货交易列表
            var Future_to_close_var = (from item
                                     in orderlist
                                 where item.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetClose &&
                                 (item.cSecurityType == "F" || item.cSecurityType == "f")
                                 select item);

            List<TradeOrderStruct> Future_to_close = new List<TradeOrderStruct>();

            if (Future_to_close_var.Count() != 0) Future_to_close = Future_to_close_var.ToList();
            
            //计算预期购买股票成本
            double stock_etimate_add_cost = 0;

            foreach (TradeOrderStruct order in Stock_to_buy)
            {
                stock_etimate_add_cost += order.dOrderPrice * order.nSecurityAmount;
            }

            //计算预期期货追加保证金
            double future_estimate_add_deposit = 0;

            foreach(TradeOrderStruct order in Future_to_open)
            {
                future_estimate_add_deposit += (order.nSecurityAmount * order.dOrderPrice * AccountPARA.MarginValue);
            }


            //第二步： 计算修改的资金账户参数是否满足风控指标要求

            //判断卖出股票是否小于持仓
            foreach(TradeOrderStruct order in Stock_to_sell)
            {
                //计算当前该股票持仓减去
                int current_position_entrust_amount = 
            }

            //第三部： 实际减少股票资金
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