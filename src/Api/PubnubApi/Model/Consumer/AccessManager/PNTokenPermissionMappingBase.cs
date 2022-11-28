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
        [Obsolete("Channels is deprecated, please use Spaces instead.")]
        public Dictionary<string, PNTokenAuthValues> Channels { get; set; }

        [Obsolete("ChannelGroups is deprecated.")]
        public Dictionary<string, PNTokenAuthValues> ChannelGroups { get; set; }

        [Obsolete("Uuids is deprecated, please use Users instead.")]
        public Dictionary<string, PNTokenAuthValues> Uuids { get; set; }

        public Dictionary<string, PNTokenAuthValues> Users { get; set; }
        public Dictionary<string, PNTokenAuthValues> Spaces { get; set; }

    }

}
