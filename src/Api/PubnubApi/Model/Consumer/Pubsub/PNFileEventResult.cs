using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFileEventResult
    {
        public string Channel { get; internal set; }
        public string Subscription { get; internal set; }
        public string Publisher { get; internal set; }
        public long Timetoken { get; internal set; }
        public object Message { get; internal set; }
        public PNFile File { get; internal set; }
    }
}
