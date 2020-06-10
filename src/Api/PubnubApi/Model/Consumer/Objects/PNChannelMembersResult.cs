using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMembersResult
    {
        public List<PNChannelMembersItemResult> ChannelMembers { get; set; }
        public int TotalCount { get; set; }
        public PNPageObject Page { get; set; }
    }
}
