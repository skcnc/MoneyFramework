using Newtonsoft.Json;
using Stork_Future_TaoLi.Account;
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

        public static riskParameter riskPara = new riskParameter();

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
            if (InputJson != null && InputJson != String.Empty)
            {
                riskPara.account = accountMonitor.GetAccountInfo(InputJson, out err);
            }
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
                            Amount = s.Split('|')[1],
                            PercentageA = Convert.ToDouble(s.Split('|')[2]),
                            Value = s.Split('|')[3],
                            PercentageB = Convert.ToDouble(s.Split('|')[4]),
                            flag = true

                        });
                    }

                    DBAccessLayer.SetWBNameList(Records);

                }


                riskPara.changkouRatio = para.changkouRatio;
                riskPara.PerStockCostPercentage = para.PerStockCostPercentage;
                riskPara.riskLevel = para.riskLevel;
                riskPara.stockRatio = para.stockRatio;

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
            result = string.Empty;
            if (alias == null) return false;
            //第一步： 计算新交易引入后对于股票，期货持仓参数的预见性影响
            alias = alias.Trim();

            //风控错误码
            int errCode = 0;


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

            //计算预期期货对应股票市值变化值
            double future_estimate_market_value = 0;

            foreach(TradeOrderStruct order in Future_to_open)
            {
                future_estimate_add_deposit += (order.nSecurityAmount * order.dOrderPrice * AccountPARA.MarginValue * AccountPARA.Factor(order.cSecurityCode));
                if (order.cTradeDirection == TradeOrientationAndFlag.FutureTradeDirectionBuy)
                {
                    future_estimate_market_value += (order.nSecurityAmount * order.dOrderPrice * AccountPARA.Factor(order.cSecurityCode));
                }
                else if(order.cTradeDirection == TradeOrientationAndFlag.FutureTradeDirectionSell)
                {
                    future_estimate_market_value -= (order.nSecurityAmount * order.dOrderPrice * AccountPARA.Factor(order.cSecurityCode));
                }
            }

            


            //第二步： 计算修改的资金账户参数是否满足风控指标要求

            //判断卖出/平仓证券是否小于持仓

            #region 黑白名单判断及总股本限制

            //获得白名单
            List<BWNameTable> BWRecords = DBAccessLayer.GetWBNamwList();

            foreach(TradeOrderStruct tos in orderlist)
            {
                BWNameTable BW_record=  BWRecords.Find(delegate(BWNameTable item) { return item.Code.Trim() == tos.cSecurityCode.Trim(); });

                if (BW_record == null) { errCode = 4; result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode); return false; }

                if(tos.cSecurityType.ToUpper() == "S" && tos.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionBuy)
                {
                    //判断总股本数量和流通股数量限制
                    double buylimit = Convert.ToDouble(BW_record.PercentageA) * Convert.ToDouble(BW_record.Amount);
                    double buylimit2 = Convert.ToDouble(BW_record.PercentageB) * Convert.ToDouble(BW_record.Value);
                    
                    int entrust_amount = 0;

                    AccountEntrust entrust_record = current_account.entrusts.Find(
                            delegate(AccountEntrust item)
                            {
                                return item.code == tos.cSecurityCode;
                            }
                        );

                    if (entrust_record != null) entrust_amount = Convert.ToInt32(entrust_record.dealAmount);

                    int position_amount = 0;

                    AccountPosition position_record = current_account.positions.Find(
                            delegate(AccountPosition item)
                            {
                                return item.code == tos.cSecurityCode;
                            }
                        );

                    if (position_record != null) { position_amount = Convert.ToInt32(position_record.amount); }

                    //总股本限制
                   if(tos.nSecurityAmount + entrust_amount + position_amount > buylimit)
                   {
                       errCode = 5;
                       result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                       return false;
                   }

                    //流通股限制
                    if(tos.nSecurityAmount + entrust_amount + position_amount > buylimit2)
                    {
                        errCode = 5;
                        result = accountMonitor.GetErrorCode(errCode, tos.cSecurityCode);
                        return false;
                    }
                }
            }

            #endregion

            #region 判断卖出/平仓证券是否小于持仓
            foreach (TradeOrderStruct order in orderlist)
            {
                if (order.cSecurityType == "F" || order.cSecurityType == "f")
                {
                    if (order.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetClose)
                    {
                        int position_amount = 0;

                        //期货平仓交易，需要判断对应开仓数量是否满足
                        if (order.cTradeDirection == TradeOrientationAndFlag.FutureTradeDirectionBuy)
                        {
                            //买入平仓，需要判断卖出开仓数量

                            AccountPosition position_record = current_account.positions.Find(
                                    delegate(AccountPosition item)
                                    {
                                        return item.direction == TradeOrientationAndFlag.FutureTradeDirectionSell && order.cSecurityCode == item.code;
                                    }
                                );

                            if (position_record != null) { position_amount = Convert.ToInt32(position_record.amount); }

                            if(order.nSecurityAmount > position_amount)
                            {
                                //买入平仓数量小于卖出仓位，交易被拒绝。
                                errCode = 12;
                                result = accountMonitor.GetErrorCode(errCode, order.cSecurityCode + "|" + position_amount + "|" + order.cTradeDirection);
                                return false;
                            }

                        }
                        else if (order.cTradeDirection == TradeOrientationAndFlag.FutureTradeDirectionSell)
                        {
                            //卖出平仓，需要判断买入开仓的数量

                            AccountPosition position_record = current_account.positions.Find(
                                    delegate(AccountPosition item)
                                    {
                                        return item.direction == TradeOrientationAndFlag.FutureTradeDirectionBuy && order.cSecurityCode == item.code;
                                    }
                                );

                            if (position_record != null) { position_amount = Convert.ToInt32(position_record.amount); }

                            if (order.nSecurityAmount > position_amount)
                            {
                                //卖出平仓数量小于买入仓位，交易被拒绝。
                                errCode = 12;
                                result = accountMonitor.GetErrorCode(errCode, order.cSecurityCode + "|" + position_amount + "|" + order.cTradeDirection);
                                return false;
                            }
                        }
                    }
                }
                else if (order.cSecurityType == "S" || order.cSecurityType == "s")
                {
                    if (order.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionSell)
                    {
                        //股票卖出交易，需要判断当前持仓数量是否满足
                        int entrust_amount = 0;

                        AccountEntrust entrust_record = current_account.entrusts.Find(
                                delegate(AccountEntrust item)
                                {
                                    return item.code == order.cSecurityCode && item.direction == TradeOrientationAndFlag.StockTradeDirectionSell;
                                }
                            );

                        if (entrust_record != null) entrust_amount = Convert.ToInt32(entrust_record.dealAmount);

                        int position_amount = 0;

                        AccountPosition position_record = current_account.positions.Find(
                                delegate(AccountPosition item)
                                {
                                    return item.code == order.cSecurityCode;
                                }
                            );

                        if (position_record != null) { position_amount = Convert.ToInt32(position_record.amount); }

                        //判断当前持仓和委托+下单卖出总和比较
                        if (order.nSecurityAmount + entrust_amount > position_amount)
                        {
                            errCode = 11;
                            result = accountMonitor.GetErrorCode(errCode, order.cSecurityCode + "|" + (entrust_amount + position_amount).ToString());
                            return false;
                        }
                    }
                }





            }
            #endregion

            #region 判断买入股票和期货需要资金是否高于当前可用资金

            //预期股票剩余资金
            double stock_balance = Convert.ToDouble(current_account.account) - stock_etimate_add_cost;

            if (stock_balance <= 0)
            {
                errCode = 1;
                result = accountMonitor.GetErrorCode(errCode, string.Empty);
                return false;
            }

            //预期期货可用资金
            double future_balance = Convert.ToDouble(current_account.faccount) - future_estimate_add_deposit;

            if(future_balance <= 0)
            {
                errCode = 2;
                result = accountMonitor.GetErrorCode(errCode, string.Empty);
                return false;
            }

            #endregion

            #region 单一股票占总资产不超过5%

            //总资产
            double totalAccount = Convert.ToDouble(current_account.account) + Convert.ToDouble(current_account.fstockvalue) + Convert.ToDouble(current_account.fvalue);

            foreach(TradeOrderStruct tos in orderlist)
            {
                if(tos.cSecurityType.ToUpper() == "S")
                {
                    if (CheckStockException(tos.cSecurityCode))
                        continue;

                    if(tos.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionBuy)
                    {
                        double entrust_value  = 0;

                        AccountEntrust entrust_record = current_account.entrusts.Find(
                                delegate(AccountEntrust item)
                                {
                                    return item.code == tos.cSecurityCode;
                                }
                            );

                        if (entrust_record != null) entrust_value = Convert.ToDouble(entrust_record.dealMoney) * Convert.ToDouble(entrust_record.dealAmount);

                        double position_value = 0;

                        AccountPosition position_record = current_account.positions.Find(
                                delegate(AccountPosition item)
                                {
                                    return item.code == tos.cSecurityCode;
                                }
                            );

                        if (position_record != null) { position_value = Convert.ToDouble(position_record.price) * Convert.ToDouble(position_record.amount); }

                        if ((tos.nSecurityAmount * tos.dOrderPrice + entrust_value + position_value) / totalAccount > riskPara.PerStockCostPercentage)
                        {
                            errCode  = 6;
                            result = accountMonitor.GetErrorCode(errCode,tos.cSecurityCode);
                            return false;
                        }
                    }
                }
            }

            #endregion

            #region 股票所占投资总额不超过80%
            //总市值，totalAccount
            //当前股票成本 ， current_account.cost + 委托中的买入股票成本
            double estimate_stock_value = Convert.ToDouble(current_account.value);

            foreach(AccountEntrust record in current_account.entrusts)
            {
                if(record.direction == TradeOrientationAndFlag.StockTradeDirectionBuy)
                {
                    estimate_stock_value += Convert.ToDouble(record.dealMoney) * Convert.ToDouble(record.dealAmount);
                }
            }

            foreach(RiskFrozenInfo record in current_account.riskFrozenInfo)
            {
                if(record.Type.ToUpper() == "S" && record.TradeDirection == TradeOrientationAndFlag.StockTradeDirectionBuy)
                {
                    estimate_stock_value += Convert.ToDouble(record.FrozenCost);
                }
            }

            estimate_stock_value += stock_etimate_add_cost;

            if (estimate_stock_value / totalAccount > riskPara.stockRatio)
            {
                errCode = 13;
                result = accountMonitor.GetErrorCode(errCode, (estimate_stock_value / totalAccount * 100).ToString());
                return false;
            }
            #endregion

            #region 单日期货交易次数不超过10手

            if(FutureTradeTimes.Keys.Contains(alias))
            {
                if(DateTime.Now.Hour > 16)
                {
                    //下午四点后，默认计数清零
                    FutureTradeTimes[alias] = 0;
                }

                if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    //周末直接清零
                    FutureTradeTimes[alias] = 0;
                }
            }
            else
            {
                FutureTradeTimes.Add(alias,0);
            }


            int future_trade_hand_count = 0;

            foreach(TradeOrderStruct tos in orderlist)
            {
                if(tos.cSecurityType.ToUpper() == "F" && tos.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetOpen)
                {
                    //只有期货开仓才会计入数量限制，平仓不限
                    future_trade_hand_count += Convert.ToInt32(tos.nSecurityAmount);
                }
            }


            if(FutureTradeTimes[alias] + future_trade_hand_count > 10)
            {
                errCode = 10;
                result = accountMonitor.GetErrorCode(errCode,string.Empty);
                return false;
            }

            #endregion

            #region 期货风险度和敞口比例

            //期货风险度
            double risk_radio = (Convert.ToDouble(current_account.fbond) + future_estimate_add_deposit) / (Convert.ToDouble(current_account.fvalue));

            if(risk_radio >= riskPara.riskLevel)
            {
                errCode = 8;
                result = accountMonitor.GetErrorCode(errCode,string.Empty);
                return false;
            }


            #endregion

            #region 监控用户登录判断

            if (!userOper.CheckMonitorUser(CONFIG.GlobalMonitor))
            {
                errCode = 14;
                result = accountMonitor.GetErrorCode(errCode, string.Empty);
                return false;
            }

            #endregion


            //敞口
            foreach (TradeOrderStruct tos in orderlist)
            {
                //股票卖出和期货平仓交易不计算敞口
                if (tos.cSecurityType.ToUpper() == "S" && tos.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionSell)
                    continue;
                if (tos.cSecurityType.ToUpper() == "F" && tos.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetClose)
                    continue;

                //股票买入和期货开仓需要通过敞口验证
                double changkouRatio = 0;

                if (!((Convert.ToDouble(current_account.value) + stock_etimate_add_cost) == 0))
                {
                    changkouRatio = (Convert.ToDouble(current_account.fstockvalue) + future_estimate_market_value + Convert.ToDouble(current_account.value) + stock_etimate_add_cost) / (Convert.ToDouble(current_account.value) + stock_etimate_add_cost);
                    if (Math.Abs(changkouRatio) > riskPara.changkouRatio)
                    {
                        errCode = 9;
                        result = accountMonitor.GetErrorCode(errCode, changkouRatio.ToString());
                        return false;
                    }
                }
                else
                {
                    errCode = 9;
                    result = accountMonitor.GetErrorCode(errCode, "无穷大");
                    return false;
                }
            }

            //第三部： 实际减少股票资金

            foreach(TradeOrderStruct tos in orderlist)
            {
                if(tos.cSecurityType.ToUpper() == "S"){
                    if(tos.cTradeDirection == TradeOrientationAndFlag.StockTradeDirectionBuy)
                    {
                        //股票交易买入需要计入风控列表
                        accountMonitor.UpdateRiskFrozonAccount(user.alias,tos.cSecurityCode,Convert.ToInt32(tos.nSecurityAmount),tos.nSecurityAmount
                             * tos.dOrderPrice,"S",tos.cTradeDirection);
                    }
                }
                else if(tos.cSecurityType.ToUpper() == "F")
                {
                    if(tos.cOffsetFlag == TradeOrientationAndFlag.FutureTradeOffsetOpen)
                    {
                        //期货交易开仓需要计入风控列表
                        accountMonitor.UpdateRiskFrozonAccount(user.alias, tos.cSecurityCode, Convert.ToInt32(tos.nSecurityAmount), tos.nSecurityAmount * tos.dOrderPrice, "F", tos.cTradeDirection);
                    }
                }
            }

            errCode = 0;

            result = accountMonitor.GetErrorCode(errCode, string.Empty);

            return true;
        }

        /// <summary>
        /// 判断比例意外情况
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool CheckStockException(string code)
        {
            if (code.Length > 3 && code.Substring(0, 3) == "510")
                return true;

            if (code.Length >= 4 && code.Substring(0, 4) == "2040")
                return true;

            if (code.Length >= 4 && code.Substring(0, 4) == "1318")
                return true;

            if (code == "519888" || code == "511990" || code == "511880" || code == "159001")
                return true;

            return false;
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
        public double changkouRatio = 0.1;

        /// <summary>
        /// 风险度限制
        /// </summary>
        public double riskLevel = 0.65;

        /// <summary>
        /// 股票占总资金比例
        /// 全部股票市值 除以 （证券总资产 加上期货权益）
        /// </summary>
        public double stockRatio = 0.8;

        /// <summary>
        /// 单只股票所占资金比例
        /// </summary>
        public double PerStockCostPercentage = 0.05;

        ///// <summary>
        ///// 股票占总投资比例
        ///// </summary>
        //public double StockCostRatio = 0.8;

        /// <summary>
        /// 持仓信息
        /// </summary>
        public AccountInfo account = new AccountInfo();

    }


}