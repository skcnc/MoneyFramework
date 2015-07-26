using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stork_Future_TaoLi.Test;
using Newtonsoft.Json;
using System.IO;

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
            TestClass.isRun = false;
            
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

        public ActionResult MonitorConsole()
        {
            return View();
        }

        public string ImportHarbor(String InputJson)
        {
            try
            {
                object obj = JsonConvert.DeserializeObject(InputJson);
                StrategyMonitorClass.QCommands.Enqueue(obj);
                return "SUCCESS";
            }
            catch(Exception ex)
            {
                return ex.ToString();
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
    }
}
