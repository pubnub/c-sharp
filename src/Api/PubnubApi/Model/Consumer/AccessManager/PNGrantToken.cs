using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGrantToken
    {
        public int Version { get; set; }

        public long Timestamp { get; set; }

        public int TTL { get; set; }

        public Dictionary<string, PNResourcePermission> Channels { get; set; }
        public Dictionary<string, PNResourcePermission> ChannelGroups { get; set; }
        public Dictionary<string, PNResourcePermission> Users { get; set; }
        public Dictionary<string, PNResourcePermission> Spaces { get; set; }

        public Dictionary<string, PNResourcePermission> ChannelPatterns { get; set; }
        public Dictionary<string, PNResourcePermission> GroupPatterns { get; set; }
        public Dictionary<string, PNResourcePermission> UserPatterns { get; set; }
        public Dictionary<string, PNResourcePermission> SpacePatterns { get; set; }

        public Dictionary<string, object> Meta { get; set; }

        public string Signature { get; set; }
    }
}
