using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMembersItemResult
    {
        public PNUuidMetadataResult UuidMetadata { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public string Updated { get; set; }
    }
}
