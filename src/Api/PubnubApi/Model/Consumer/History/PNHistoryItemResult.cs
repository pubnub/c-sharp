using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHistoryItemResult
    {
        public long Timetoken { get; internal set; }
        public object Entry { get; internal set; }
        public Dictionary<string, object> Meta { get; internal set; }
        public Dictionary<string, List<PNMessageActionItem>> Actions { get; internal set; }
        public string Uuid { get; internal set; }
        public int MessageType { get; internal set; }
        
        public string CustomMessageType { get; internal set; }
    }
}
