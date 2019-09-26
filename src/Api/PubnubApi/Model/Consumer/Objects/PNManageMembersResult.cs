using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNManageMembersResult
    {
        public List<PNManageMembersItemResult> Members { get; set; }
        public int TotalCount { get; set; }
        public PNPage Page { get; set; }
    }
}
