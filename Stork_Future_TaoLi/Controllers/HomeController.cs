using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stork_Future_TaoLi;
using Newtonsoft.Json;
using System.IO;
using Stork_Future_TaoLi.Modulars;
using Stork_Future_TaoLi.Account;
using Stork_Future_TaoLi.Database;
using Stork_Future_TaoLi.TradeModule;
using Stork_Future_TaoLi.Queues;
using Stork_Future_TaoLi;

namespace Stork_Future_TaoLi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult SysLogin()
        {
            return View();
        }

        public ActionResult MonitorConsole()
        {
            return View();
        }

        public ActionResult SysMonitor()
        {
            return View();
        }

        public ActionResult MainPage()
        {
            return View();
        }

        public ActionResult RiskControl()
        {
            return View();
        }

        public ActionResult UserManger()
        {
            return View();
        }

        public ActionResult AccountInfo()
        {
            return View();
        }

        public string ImportHarbor(String InputJson)
        {
            try
            {
                string mark = InputJson.Substring(0, 2);
                string jsonString = InputJson.Substring(2);
                object obj = new object();

                if (mark == "A1") {obj =  (object)(JsonConvert.DeserializeObject<OPENCREATE>(jsonString)); }
                else if (mark == "A2") { obj = (object)(JsonConvert.DeserializeObject<OPENMODIFY>(jsonString)); }
                else if (mark == "A3") { obj = (object)(JsonConvert.DeserializeObject<OPENRUN>(jsonString)); }
                else if (mark == "A4") { obj = (object)(JsonConvert.DeserializeObject<OPENALLOW>(jsonString)); }
                else if (mark == "A5") { obj = (object)(JsonConvert.DeserializeObject<OPENDELETE>(jsonString)); }
                else if (mark == "B1") { obj = (object)(JsonConvert.DeserializeObject<CLOSECREATE>(jsonString)); }
                else if (mark == "B2") { obj = (object)(JsonConvert.DeserializeObject<CLOSEMODIFY>(jsonString)); }
                else if (mark == "B3") { obj = (object)(JsonConvert.DeserializeObject<CLOSERUN>(jsonString)); }
                else if (mark == "B4") { obj = (object)(JsonConvert.DeserializeObject<CLOSEALLOW>(jsonString)); }
                else if (mark == "B5") { obj = (object)(JsonConvert.DeserializeObject<CLOSEDELETE>(jsonString)); }

                StrategyMonitorClass.QCommands.Enqueue(obj);
                return "SUCCESS";
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// 获取风控参数
        /// </summary>
        /// <param name="InputJson">用户名</param>
        /// <returns>Json格式的风控参数</returns>
        public string GetRiskParameter(String InputJson)
        {
            return riskmonitor.GetRiskParaJson(InputJson);
        }

        /// <summary>
        /// 获取白名单
        /// </summary>
        /// <returns></returns>
        public string GetWhiteList()
        {
            return riskmonitor.LoadWhiteList();
        }

        public string SetRiskParameter(String InputJson,String WhiteLi)
        {
            return riskmonitor.SetRiskParaJson(InputJson, WhiteLi);
        }

        public string register(String InputJson)
        {
            return userOper.register(InputJson);
        }

        public string userlogin(String InputJson)
        {

            return userOper.login(InputJson).ToString();
            
        }

        public string userlogout(String username)
        {
            if (username == null) { return "1"; }

            return userOper.logout(username);
                
        }

        public string changePW(String InputJson)
        {
            return userOper.ChangePassword(InputJson).ToString();   
        }

        public string ImportTrade(String InputJson)
        {
            try
            {
                string mark = InputJson.Substring(0, 2);
                string jsonString = InputJson.Substring(2);
                object obj = new object();

                if (mark == "C1") { obj = (object)(JsonConvert.DeserializeObject<MakeOrder>(jsonString)); }

                queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue(obj);
                return "SUCCESS";

            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("生成交易失败： " + InputJson);
                return "FALSE";
            }
        }

        public string ImportBatchTrades()
        {
            try
            {
                List<string> strs = pythonOper.GetInstance().GetBatchTradeList();

                foreach(string s in strs)
                {
                    try
                    {
                        string[] vars = s.Split('\t');

                        MakeOrder order = new MakeOrder()
                        {
                            belongStrategy = "00",
                            User = vars[0].Trim(),
                            exchangeId = vars[1].Trim().ToUpper(),
                            cSecurityCode = vars[2].Trim(),
                            nSecurityAmount = Convert.ToInt64(vars[3].Trim()),
                            dOrderPrice = Convert.ToDouble(vars[4].Trim()),
                            cTradeDirection = vars[5].Trim(),
                            offsetflag = vars[6].Trim(),
                            cSecurityType = vars[7].Trim(),
                            OrderRef = 0
                        };


                        queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue((object)order);
                       
                    }
                    catch
                    {
                        GlobalErrorLog.LogInstance.LogEvent("批量交易生成部分失败：" + s);
                        return "FALSE";
                    }
                }

                return "SUCCESS";
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }

        public string MatchOpenPara(String strategyId)
        {
            try
            {


                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string AjaxTest(FileStream file)
        {
            string s = file.Name;

            return "SUCCESS";
        }

        public string LoadAccountInfo(String user)
        {
            string result = string.Empty;
            AccountInfo info = accountMonitor.GetAccountInfo(user, out result);

            return JsonConvert.SerializeObject(info);
        }

        public string RefundTrade(String orderRef)
        {
            if(orderRef == null || orderRef == string.Empty)
            {
                return "非法委托编号！";
            }

            int reference = int.Parse(orderRef.Trim());

            ERecord srecord = EntrustRecord.GetEntrustRecord(reference);

            if(srecord != null)
            {
                //找到股票委托交易信息
                RefundStruct refund = new RefundStruct()
                {
                    Direction = srecord.Direction.Trim(),
                    ExchangeId = srecord.ExchangeId.Trim(),
                    OffSetFlag = "0",
                    SecurityCode = srecord.Code,
                    SecurityType = "S",
                    OrderRef = srecord.OrderRef.ToString(),
                    OrderSysId = srecord.SysOrderRef
                };

                queue_refund_thread.GetQueue().Enqueue(refund);

                return "success";
            }

            RecordItem frecord = TradeRecord.GetInstance().getOrderInfo(reference);

            if(frecord != null)
            {
                //找到期货委托交易信息

                RefundStruct refund = new RefundStruct()
                {
                    Direction = frecord.Orientation,
                    ExchangeId = String.Empty,
                    OffSetFlag = frecord.CombOffsetFlag.ToString(),
                    SecurityCode = frecord.Code,
                    SecurityType = "F",
                    OrderRef = frecord.OrderRef.ToString(),
                    OrderSysId = frecord.OrderSysID
                };

                queue_refund_thread.GetQueue().Enqueue(refund);
                return "success";
            }

            return "未找到委托对应交易！";
        }

        public string GetRiskInfo()
        {
            List<AccountInfo> infos = accountMonitor.GetAccountInfoAll();

            return JsonConvert.SerializeObject(infos);
        }

        public ActionResult OPEN_EDIT()
        {
            ViewBag.ID = Request.QueryString["StrategyID"];
            ViewBag.USER = Request.QueryString["USER"];
            return View();
        }

        public ActionResult CLOSE_EDIT()
        {
            ViewBag.ID = Request.QueryString["StrategyID"];
            ViewBag.USER = Request.QueryString["USER"];
            return View();
        }

        public ActionResult TradeMonitor()
        {
            ViewBag.ID = Request.QueryString["USER"];
            return View();
        }

        public ActionResult MarketView()
        {
            return View();
        }

        public ActionResult SysRegister()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
    }
}
