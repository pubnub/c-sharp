using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembership
    {
        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }

        [JsonProperty(PropertyName = "custom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, object> Custom { get; set; }
    }
}
