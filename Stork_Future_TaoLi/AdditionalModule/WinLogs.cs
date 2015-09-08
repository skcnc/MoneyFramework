using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Stork_Future_TaoLi
{
    public class LogWirter
    {
        /// <summary>
        /// 事件源名称
        /// </summary>
        private string eventSourceName;
        EventLogEntryType eventLogType;
        private int eventLogId;
        public LogWirter()
        {
            eventSourceName = "test";
            eventLogId = 1000;
            eventLogType = EventLogEntryType.Warning;
        }

        /// <summary>
        /// 消息事件源名称
        /// </summary>
        public string EventSourceName
        {
            set { eventSourceName = value; }
        }

        /// <summary>
        /// 消息事件源ID
        /// </summary>
        public int EventLogID
        {
            set { eventLogId = value; }
        }

        /// <summary>
        /// 消息事件类型
        /// </summary>
        public EventLogEntryType EventLogType
        {
            set { eventLogType = value; }
        }

        /// <summary>
        /// 写入系统日志
        /// </summary>
        /// <param name="message">事件内容</param>
        public void LogEvent(string message)
        {
            try
            {
                if (!EventLog.SourceExists(eventSourceName))
                {
                    EventLog.CreateEventSource(eventSourceName, "Application");
                }
                //EventLog.WriteEntry(eventSourceName, message, EventLogEntryType.Error);
                EventLog.WriteEntry(eventSourceName, message, eventLogType, eventLogId);
            }
            catch { }
        }
    }

    class GlobalErrorLog
    {
        //private static LogWirter errLog = new LogWirter();

        GlobalErrorLog()
        {
            errLog.EventSourceName = "全局报警";
            errLog.EventLogType = System.Diagnostics.EventLogEntryType.Error;
            errLog.EventLogID = 60001;
        }

        private static LogWirter errLog = new LogWirter();

        public static LogWirter LogInstance
        {
            get
            {
                
                if (errLog == null)
                    errLog = new LogWirter();

                return errLog;
            }
        }

    }

    class GlobalHeartBeat
    {
        static DateTime lastHeartBeat = DateTime.Now;
        static object rootsync = new object();

        /// <summary>
        /// 修改全局心跳时间
        /// </summary>
        public static void SetGlobalTime()
        {
            lock (rootsync)
            {
                lastHeartBeat = DateTime.Now;
            }
        }

        /// <summary>
        /// 获取最近心跳
        /// </summary>
        /// <returns></returns>
        public static DateTime GetGlobalTime()
        {
            return lastHeartBeat;
        }



    }

   
}