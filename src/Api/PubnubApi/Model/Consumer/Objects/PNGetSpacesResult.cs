using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetSpacesResult
    {
        public List<PNSpaceResult> Spaces { get; set; }
        public int TotalCount { get; set; }
        public PNPage Page { get; set; }
    }

}
