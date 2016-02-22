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

        public string SetRiskParameter(String InputJson,String WhiteLi,String BlackLi)
        {
            return riskmonitor.SetRiskParaJson(InputJson, WhiteLi, BlackLi);
        }

        public string register(String InputJson)
        {
            return userOper.register(InputJson);
        }

        public string userlogin(String InputJson)
        {
            return userOper.login(InputJson).ToString();
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
