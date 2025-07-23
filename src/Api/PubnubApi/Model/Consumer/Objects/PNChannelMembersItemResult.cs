using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMembersItemResult
    {
        public PNUuidMetadataResult UuidMetadata { get; internal set; }
        public Dictionary<string, object> Custom { get; internal set; }
        
        public string Type { get; internal set; }

        public string Status { get; internal set; }
        public string Updated { get; internal set; }
    }
}
