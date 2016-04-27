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
using System.Text;
using System.Net.Http;
using Stork_Future_TaoLi.Variables_Type;
using System.Net;
using System.Net.Http.Headers;
using Stork_Future_TaoLi.StrategyModule;

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
                DBAccessLayer.LogSysInfo("HomeController", ex.ToString());
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
                MakeOrder order = new MakeOrder();

                if (mark == "C1") { order = (JsonConvert.DeserializeObject<MakeOrder>(jsonString)); }

                List<MakeOrder> orders = new List<MakeOrder>();
                orders.Add(order);

                queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue((object)orders);

                return "SUCCESS";

            }
            catch (Exception ex)
            {
                GlobalErrorLog.LogInstance.LogEvent("生成交易失败： " + InputJson);
                DBAccessLayer.LogSysInfo("HomeController", ex.ToString());
                return "FALSE";
            }
        }

        public string ImportBatchTrades(String JsonString)
        {
            try
            {
                List<MakeOrder> orders = new List<MakeOrder>();
                orders = JsonConvert.DeserializeObject<List<MakeOrder>>(JsonString);
                queue_prd_trade_from_tradeMonitor.GetQueue().Enqueue((object)orders);
               

                return "SUCCESS";
            }
            catch(Exception ex)
            {
                DBAccessLayer.LogSysInfo("HomeController-ImportBatchTrades", ex.ToString());
                return ex.ToString();
            }
        }

        public string GetBatchTrade(string user)
        {
            try
            {
                List<string> strs = pythonOper.GetInstance().GetBatchTradeList();
                List<MakeOrder> orders = new List<MakeOrder>();

                foreach (string s in strs)
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


                        orders.Add(order);

                    }
                    catch
                    {
                        GlobalErrorLog.LogInstance.LogEvent("批量交易生成部分失败：" + s);
                        return "FALSE";
                    }
                }

                return JsonConvert.SerializeObject(orders);
            }
            catch (Exception ex)
            {
                DBAccessLayer.LogSysInfo("HomeController-ImportBatchTrades", ex.ToString());
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

        [HttpPost]
        public ActionResult authorizedtrade(HttpPostedFileBase file, String USER)
        {
            if(file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    byte[] array = binaryReader.ReadBytes(file.ContentLength);
                    var str = Encoding.Default.GetString(array);

                    if (!str.Contains('\n'))
                    {
                        return View(); 
                    }
                    List<AuthorizedOrder> AuthorizedOrders = new List<AuthorizedOrder>();
                    string[] orders = str.Split('\n');

                    for (int i = 0; i < orders.Length; i++)
                    {

                        string order = orders[i];

                        if (order.Trim() == string.Empty)
                            break;

                        if (order.Contains('\r'))
                        {
                            order = order.Substring(0, order.Length - 1);
                        }

                        if (!order.Contains('|')) { return View(); }

                        string[] values = order.Split('|');

                        if (values.Count() < 8) return View();

                        string exchange = values[1];
                        string code = values[2];
                        string num = values[3];
                        string price = values[4];
                        string direction = values[5];
                        string offsetflag = values[6];
                        string type = values[7];
                        string limitedflag = values[8];
                        string lossPrice = values[9];
                        string superPrice = values[10];
                        string cost = values[11];

                        int status = 0; //未启动

                        try
                        {
                            AuthorizedOrder a_order = new AuthorizedOrder()
                            {
                                belongStrategy = "00",
                                cSecurityCode = code.Trim(),
                                cSecurityType = type.Trim(),
                                cTradeDirection = direction.Trim(),
                                dOrderPrice = Convert.ToDouble(price.Trim()),
                                exchangeId = exchange.Trim(),
                                nSecurityAmount = Convert.ToInt32(num.Trim()),
                                offsetflag = offsetflag.Trim(),
                                OrderRef = 0,
                                User = USER,
                                LimitedPrice = limitedflag,
                                LossValue = Convert.ToSingle(lossPrice.Trim()),
                                SurplusValue = Convert.ToSingle(superPrice.Trim()),
                                cost = Convert.ToSingle(cost.Trim()),
                                Status = status
                            };

                            AuthorizedOrders.Add(a_order);
                        }
                        catch(Exception ex)
                        {
                            DBAccessLayer.LogSysInfo("HomeController", ex.ToString());
                        }
                    }


                    queue_authorized_trade.EnQueue((object)AuthorizedOrders);

                    return RedirectToAction("AuthorizedTradeB");
                }
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult authorizedtradeB(HttpPostedFileBase file, String USER)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    byte[] array = binaryReader.ReadBytes(file.ContentLength);
                    var str = Encoding.Default.GetString(array);

                    if (!str.Contains('\n'))
                    {
                        return View();
                    }
                    List<AuthorizedOrder> AuthorizedOrders = new List<AuthorizedOrder>();
                    string[] orders = str.Split('\n');

                    for (int i = 0; i < orders.Length; i++)
                    {

                        string order = orders[i];

                        if (order.Trim() == string.Empty)
                            break;

                        if (order.Contains('\r'))
                        {
                            order = order.Substring(0, order.Length - 1);
                        }

                        if (!order.Contains('|')) { return View(); }

                        string[] values = order.Split('|');

                        if (values.Count() < 8) return View();

                        string exchange = values[1];
                        string code = values[2];
                        string num = values[3];
                        string price = values[4];
                        string direction = values[5];
                        string offsetflag = values[6];
                        string type = values[7];
                        string limitedflag = values[8];
                        string lossPrice = values[9];
                        string superPrice = values[10];
                        string cost = values[11];
                        int status = 0; //未启动

                        try
                        {
                            AuthorizedOrder a_order = new AuthorizedOrder()
                            {
                                belongStrategy = "00",
                                cSecurityCode = code.Trim(),
                                cSecurityType = type.Trim(),
                                cTradeDirection = direction.Trim(),
                                dOrderPrice = Convert.ToDouble(price.Trim()),
                                exchangeId = exchange.Trim(),
                                nSecurityAmount = Convert.ToInt32(num.Trim()),
                                offsetflag = offsetflag.Trim(),
                                OrderRef = 0,
                                User = USER,
                                LimitedPrice = limitedflag,
                                LossValue = Convert.ToSingle(lossPrice.Trim()),
                                SurplusValue = Convert.ToSingle(superPrice.Trim()),
                                cost = Convert.ToSingle(cost.Trim()),
                                Status = status
                            };

                            AuthorizedOrders.Add(a_order);
                        }
                        catch (Exception ex)
                        {
                            DBAccessLayer.LogSysInfo("HomeController", ex.ToString());
                        }
                    }


                    queue_authorized_trade.EnQueue((object)AuthorizedOrders);

                    return RedirectToAction("AuthorizedTrade");
                }
            }
            else
            {
                return View();
            }
        }

        public void downloadAuthorizedFile(String USER)
        {
            try
            {
                String strategy = AuthorizedTradesList.GetUserViewStrategy(USER);

                if (strategy.Trim() == String.Empty) return;
                System.String filename = strategy.Split('|')[0];

                //filename = "sc20160427025316";

                // set the http content type to "APPLICATION/OCTET-STREAM
                Response.ContentType = "APPLICATION/OCTET-STREAM";

                // initialize the http content-disposition header to
                // indicate a file attachment with the default filename
                // "myFile.txt"
                System.String disHeader = "Attachment; Filename=\"" + filename +
                   "\"";
                Response.AppendHeader("Content-Disposition", disHeader);

                // transfer the file byte-by-byte to the response object
                System.IO.FileInfo fileToDownload = new
                   System.IO.FileInfo(CONFIG.AUTHORIZED_BASE_URL + filename);
                Response.Flush();
                Response.WriteFile(fileToDownload.FullName);
            }
            catch (System.Exception e)
            // file IO errors
            {
                GlobalErrorLog.LogInstance.LogEvent(e.ToString());
            }

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

        public ActionResult BatchTrade()
        {
            return View();
        }

        public ActionResult AuthorizedTrade()
        {
            return View();
        }

        public ActionResult AuthorizedTradeB()
        {
            return View();
        }
    }
}
