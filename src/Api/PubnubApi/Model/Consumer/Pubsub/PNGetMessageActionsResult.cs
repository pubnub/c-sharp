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
            public long Start { get; set; }
            public long End { get; set; }
            public int Limit { get; set; }
        }

        public List<PNMessageActionItem> MessageActions { get; set; }
        public MoreInfo More { get; set; }
    }
}
