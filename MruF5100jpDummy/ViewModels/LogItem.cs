using System;
using System.Windows.Media;

namespace MruF5100jpDummy.ViewModels
{
    public class LogItem
    {
        public string Timestamp { get; set; }
        public string Content { get; set; }
        public bool IsReceived { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
    }
}