using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetAllChannelMetadataResult
    {
        public List<PNChannelMetadataResult> Channels { get; internal set; }
        public int TotalCount { get; internal set; }
        public PNPageObject Page { get; internal set; }
    }

}
