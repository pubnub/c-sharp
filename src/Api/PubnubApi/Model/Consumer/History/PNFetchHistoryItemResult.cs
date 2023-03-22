using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFetchHistoryItemResult
    {
        public long Timetoken { get; internal set; }
        public object Entry { get; internal set; }
        public object Meta { get; internal set; }
        public object Actions { get; internal set; }
        public string Uuid { get; internal set; }
        public MessageType MessageType { get; internal set; }
        public string SpaceId { get; internal set; }
    }
}
