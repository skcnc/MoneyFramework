using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Stork_Future_TaoLi.Hubs
{
    public class StrategyInfoHub : Hub
    {
        static object SyncRoot = new object();

        public void getStrategyID(String InputJson)
        {
            lock (SyncRoot)
            {
                string jsonString = InputJson.Substring(2);

                try
                {
                    SEARCHSTRATEGY obj = (JsonConvert.DeserializeObject<SEARCHSTRATEGY>(jsonString));

                    if (obj == null) return;

                    if (obj == null || obj.CONTRACT == string.Empty || obj.INDEX == string.Empty || obj.BASIS == string.Empty) return;
                    int hd = 0;
                    String id = DBAccessLayer.SearchStrategy(obj,out hd);
      
                    decimal stockcost = 0;
                    List<string> li = DBAccessLayer.GetDealList(id, out stockcost);

                    var connectId = Context.ConnectionId;

                    Clients.Client(connectId).GetStrategyId(id,hd);

                    

                    string orderli = string.Empty;

                    foreach (string deal in li)
                    {
                        orderli += (deal + "\r\n");
                    }

                    Clients.Client(connectId).GetDealList(orderli, stockcost);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return;
                }
            }
        }
    }
}