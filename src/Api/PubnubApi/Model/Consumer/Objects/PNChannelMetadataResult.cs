using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMetadataResult
    {
        public string Channel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public string Updated { get; set; }
    }
}
