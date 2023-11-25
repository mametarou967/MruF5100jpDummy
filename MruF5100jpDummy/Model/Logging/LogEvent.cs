using Prism.Events;
using System;
using System.Windows.Media;

namespace MruF5100jpDummy.Model.Logging
{

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug
    }

    public class Log
    {
        public string content;
        public DateTime dateTime;
        // Viewのプロパティをここに書くのは行儀が悪いが、Logは例外とする
        public LogLevel logLevel;
    }

    public class LogEvent : PubSubEvent<Log> { }
}
