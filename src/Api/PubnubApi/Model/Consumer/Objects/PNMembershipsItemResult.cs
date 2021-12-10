using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembershipsItemResult
    {
        public PNChannelMetadataResult ChannelMetadata { get; internal set; }
        public Dictionary<string, object> Custom { get; internal set; }
        public string Updated { get; internal set; }
    }
}
