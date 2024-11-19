using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNHistoryItemResult
    {
        public long Timetoken { get; internal set; }
        public object Entry { get; internal set; }
        public object Meta { get; internal set; }
        public object Actions { get; internal set; }
        public string Uuid { get; internal set; }
        public int MessageType { get; internal set; }
        
        public string CustomMessageType { get; internal set; }
    }
}
