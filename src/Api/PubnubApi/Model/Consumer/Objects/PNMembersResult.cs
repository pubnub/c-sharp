using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembersResult
    {
        public List<PNMembersItemResult> Members { get; set; }
        public int TotalCount { get; set; }
        public PNPage Page { get; set; }
    }
}
