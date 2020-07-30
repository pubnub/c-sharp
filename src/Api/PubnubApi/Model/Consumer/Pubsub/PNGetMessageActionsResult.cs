using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetMessageActionsResult
    {
        public class MoreInfo
        {
            public long Start { get; internal set; }
            public long End { get; internal set; }
            public int Limit { get; internal set; }
        }

        public List<PNMessageActionItem> MessageActions { get; internal set; }
        public MoreInfo More { get; internal set; }
    }
}
