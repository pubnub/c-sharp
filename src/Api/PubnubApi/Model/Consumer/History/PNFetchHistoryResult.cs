
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNFetchHistoryResult
    {
        public class MoreInfo
        {
            public long Start { get; set; }
            public long End { get; set; }
            public int Max { get; set; }
        }

        public Dictionary<string,List<PNHistoryItemResult>> Messages { get; set; }
        public MoreInfo More { get; set; }
    }
}
