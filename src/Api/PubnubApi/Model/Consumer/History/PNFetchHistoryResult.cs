
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNFetchHistoryResult
    {
        public class MoreInfo
        {
            public long Start { get; internal set; }
            public long End { get; internal set; }
            public int Max { get; internal set; }
        }

        public Dictionary<string,List<PNHistoryItemResult>> Messages { get; internal set; }
        public MoreInfo More { get; internal set; }
    }
}
