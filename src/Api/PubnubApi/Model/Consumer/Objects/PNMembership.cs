#if NET60
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembership
    {
#if NET60
        [JsonPropertyName("channel")]
#else
        [JsonProperty(PropertyName = "channel")]
#endif   
        public string Channel { get; set; }

#if NET60
        [JsonPropertyName("custom"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#else
        [JsonProperty(PropertyName = "custom", DefaultValueHandling = DefaultValueHandling.Ignore)]
#endif

        public Dictionary<string, object> Custom { get; set; }
    }
}
