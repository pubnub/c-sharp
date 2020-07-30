using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMetadataResult
    {
        public string Channel { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public Dictionary<string, object> Custom { get; internal set; }
        public string Updated { get; internal set; }
    }
}
