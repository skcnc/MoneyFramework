using Stork_Future_TaoLi.StrategyModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Stork_Future_TaoLi
{
    public class StrategyMonitorClass
    {
        /// <summary>
        /// 用户输入指令队列，内容参照UserBehave 中的预定义
        /// </summary>
        public static Queue QCommands = new Queue();

        private static LogWirter log = new LogWirter();

        /// <summary>
        /// 策略实例监管线程
        /// </summary>
        public Dictionary<Guid, StrategyWorker> Workers = new Dictionary<Guid, StrategyWorker>();

        /// <summary>
        /// 行情订阅关系表
        /// 行情对象 1 To N 策略实例 
        /// </summary>
        private Dictionary<string, List<Guid>> MarketSubscribeList = new Dictionary<string, List<Guid>>();

        /// <summary>
        /// 策略管理线程启动
        /// </summary>
        public void Run()
        {
            Thread excutedThread = new Thread(new ThreadStart(ThreadProc));
            excutedThread.Start();



            //测试参数开始
            // Add Three Workers
            //int j = 0;

            //while (j < 3)
            //{
            //    InitParameters para = new InitParameters();
            //    Dictionary<string, int> order = new Dictionary<string, int>();

            //    RecruitNewWorker(para, order);
            //    j++;
            //}
           
            
            //int i = 0;
            //foreach(StrategyWorker _worker in Workers.Values)
            //{
            //    switch(i)
            //    {
            //        case 0:
            //            _worker.SetSubscribeList(new List<string>{
            //                "600005"
            //            });
            //            break;
            //        case 1:
            //            _worker.SetSubscribeList(new List<string>{
            //               "600005",
            //               "600651"
            //            });
            //            break;
            //        case 2:
            //            _worker.SetSubscribeList(new List<string>{
            //                "600104",
            //                "600005",
            //                "600651"
            //            });
            //            break;
            //    }
            //    i++;
            //}
            //测试参数结束


            Thread.Sleep(1000);
        }

        /// <summary>a
        /// 创建新的策略实例
        /// </summary>
        /// <param name="para"></param>
        /// <param name="orderList"></param>
        private void RecruitNewWorker(InitParameters para,Dictionary<string,int> orderList)
        {
            //创建新的策略实例
            StrategyWorker newWorker = new StrategyWorker();
            newWorker.BaseNumber = para;
            newWorker.LiStockOrder = orderList;


            newWorker.StrategyInstanceID = Guid.NewGuid();
            newWorker.StrategyInstanceName = DateTime.Now.ToString("yyyyMMddHHmmss") + para.CT + para.Index;

            Workers.Add(newWorker.StrategyInstanceID, newWorker);
            newWorker.RUN();

            //向行情模块添加消息列表映射
            MarketInfo.SetStrategyQueue(new KeyValuePair<Guid, Queue>(newWorker.StrategyInstanceID, newWorker.GetRefQueue()));

        }

        /// <summary>
        /// 更新策略实例参数
        /// </summary>
        /// <param name="para">开仓基本参数</param>
        /// <param name="orderList">交易列表</param>
        /// <param name="id">需要修改的策略实例ID</param>
        public void UpdateWorker(InitParameters para,Dictionary<string,int> orderList,Guid id)
        {
            StrategyWorker worker = Workers[id];
            worker.UpdateBaseParas(para, orderList);
        }

        public void DeleteWorker(Guid id)
        {
            //结束策略实例
            Workers[id].breaklabel = true;

            //策略管理中删除该实例信息
            Workers.Remove(id);

            //行情模块中删除该策略实例的订阅，和消息队列
            DeleteStrategySubscribe(id);
            MarketInfo.SetStrategyQueue(new KeyValuePair<Guid, Queue>(id, new Queue()));

        }


        public StrategyMonitorClass()
        {
            log.EventSourceName = "策略管理日志";
            log.EventLogType = System.Diagnostics.EventLogEntryType.Information;
            log.EventLogID = 64001;
        }
    

        /// <summary>
        /// 检索工作线程，更新订阅列表
        /// </summary>
        private void CheckSubscribeUpdate()
        {
            //获取发生改变的工作实例列表
            List<Guid> ToChangeList = (from item in Workers where item.Value.bSubscribeChange == true select item.Key).ToList();

            foreach(Guid g in ToChangeList)
            {
                //遍历其中订阅的股票期货信息，剔除其中不匹配内容
                List<String> _newSubscribeMarketList = Workers[g].GetSubscribeList();
                List<String> _oldSubscribeMarketList = (from item in MarketSubscribeList where item.Value.Contains(g) select item.Key).ToList();

                List<String> _ExistAndDoNotChange = (from item in _newSubscribeMarketList where _oldSubscribeMarketList.Contains(item) select item).ToList();
                List<String> _ToAdd = (from item in _newSubscribeMarketList where !(_ExistAndDoNotChange.Contains(item)) select item).ToList();
                List<String> _ToDelete = (from item in _oldSubscribeMarketList where !(_ExistAndDoNotChange.Contains(item)) select item).ToList();

                for(int i = 0;i < _ToAdd.Count ;i++)
                {
                    if (MarketSubscribeList.Keys.Contains(_ToAdd[i]))
                    {
                        MarketSubscribeList[_ToAdd[i]].Add(g);
                    }
                    else
                    {
                        MarketSubscribeList.Add(_ToAdd[i], new List<Guid>(){
                            g
                        });
                    }
                }

                 for(int i = 0;i < _ToDelete.Count ;i++)
                 {
                     MarketSubscribeList[_ToDelete[i]].Remove(g);
                 }

            }

            if(ToChangeList.Count != 0)
            {
                MapMarketStratgy.SetMapSS(MarketSubscribeList);
            }
        }

        /// <summary>
        /// 删除策略后，清除订阅列表中对应策略订阅
        /// </summary>
        /// <param name="Sid"></param>
        private void DeleteStrategySubscribe(Guid Sid)
        {
            foreach (var item in MarketSubscribeList)
            {
                item.Value.Remove(Sid);
            }
        }

        /// <summary>
        /// 策略管理线程工作函数
        /// 任务： 
        /// 1. 获取用户请求队列中的指令
        /// 2. 巡检工作组中订阅列表修改
        /// </summary>
        private void ThreadProc()
        {
            while(true)
            {
                if ((DateTime.Now - GlobalHeartBeat.GetGlobalTime()).TotalMinutes > 15)
                {
                    log.LogEvent("系统供血模块无响应，策略管理线程即将停止！");
                    
                    //管理策略线程退出前，对正在运行的工作策略执行“绞杀”，并维护数据库记录，这个过程称为 grace broken 
                    break;
                }

                while(QCommands.Count > 0)
                {
                    object obj = QCommands.Dequeue();

                    #region 指令类型判断
                    if (obj is OPENCREATE){
                        OPENCREATE value = (OPENCREATE)obj;

                    }
                    else if (obj is OPENMODIFY)
                    {

                    }
                    else if (obj is OPENALLOW) { }
                    else if (obj is OPENRUN) { }
                    else if (obj is OPENDELETE) { }
                    else if (obj is CLOSECREATE) { }
                    else if (obj is CLOSEMODIFY) { }
                    else if (obj is CLOSERUN) { }
                    else if (obj is CLOSEALLOW) { }
                    else if (obj is CLOSEDELETE) { }
                    else
                    { }

                    #endregion
                }

                //巡检工作组订阅内容修改
                CheckSubscribeUpdate();

                Thread.Sleep(10);
            }
        }


    }


   
}