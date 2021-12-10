using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembershipsResult
    {
        public List<PNMembershipsItemResult> Memberships { get; internal set; }
        public int TotalCount { get; internal set; }
        public PNPageObject Page { get; internal set; }
    }
}
