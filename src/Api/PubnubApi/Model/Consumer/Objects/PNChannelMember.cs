#if NET60
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNChannelMember
    {

#if NET60
        [JsonPropertyName("uuid")]
#else
        [JsonProperty(PropertyName = "uuid")]
#endif        
        public string Uuid { get; set; }

#if NET60
        [JsonPropertyName("custom"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#else
        [JsonProperty(PropertyName = "custom", DefaultValueHandling = DefaultValueHandling.Ignore)]
#endif
        public Dictionary<string, object> Custom { get; set; }
    }
}
