
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNFetchHistoryResult
    {
        public Dictionary<string,List<PNHistoryItemResult>> Messages { get; set; }
    }
}
