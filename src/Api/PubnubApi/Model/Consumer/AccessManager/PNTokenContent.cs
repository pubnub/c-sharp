using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNTokenContent
    {
        public int Version { get; set; }

        public long Timestamp { get; set; }

        public int TTL { get; set; }

        public PNTokenResources Resources { get; set; }

        public PNTokenPatterns Patterns { get; set; }

        public Dictionary<string, object> Meta { get; set; }

        public string AuthorizedUuid { get; set; }

        public string Signature { get; set; }
    }
}
