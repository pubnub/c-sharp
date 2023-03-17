using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNTokenResources : PNTokenPermissionMappingBase
    {

    }
    public class PNTokenPatterns : PNTokenPermissionMappingBase
    {

    }
    public class PNTokenPermissionMappingBase
    {
        public Dictionary<string, PNTokenAuthValues> Channels { get; set; }
        public Dictionary<string, PNTokenAuthValues> ChannelGroups { get; set; }
        public Dictionary<string, PNTokenAuthValues> Uuids { get; set; }
        public Dictionary<string, PNTokenAuthValues> Users { get; set; }
        public Dictionary<string, PNTokenAuthValues> Spaces { get; set; }

    }

}
