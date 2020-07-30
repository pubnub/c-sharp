
using System;
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHistoryResult
    {
        public List<PNHistoryItemResult> Messages { get; internal set; }
        public long StartTimeToken { get; internal set; }
        public long EndTimeToken { get; internal set; }
    }
}
