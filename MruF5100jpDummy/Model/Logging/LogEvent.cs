using Prism.Events;
using System;

namespace MruF5100jpDummy.Model.Logging
{
    public enum LogType
    {
        DataReceive,
        DataSend,
        System
    }

    public class Log
    {
        public string content;
        public DateTime dateTime;
        public LogType logType;
    }

    public class LogEvent : PubSubEvent<Log> { }
}
