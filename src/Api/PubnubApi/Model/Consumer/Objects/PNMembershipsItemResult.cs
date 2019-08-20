using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembershipsItemResult
    {
        public string SpaceId { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public PNSpaceResult Space { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
    }
}
