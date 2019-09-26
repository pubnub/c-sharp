using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetMembershipsResult
    {
        public List<PNGetMembershipsItemResult> Memberships { get; set; }
        public int TotalCount { get; set; }
        public PNPage Page { get; set; }
    }
}
