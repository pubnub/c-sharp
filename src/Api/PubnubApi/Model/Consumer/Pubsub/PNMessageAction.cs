using System;
#if NET60
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif

namespace PubnubApi
{
public class PNMessageAction
{
    public PNMessageAction()
    {
        this.Type = "";
        this.Value = "";
    }

#if NET60
        [JsonPropertyName("type")]
#else
        [JsonProperty(PropertyName = "type")]
#endif
    public string Type { get; set; }

#if NET60
        [JsonPropertyName("value")]
#else
        [JsonProperty(PropertyName = "value")]
#endif
    public string Value { get; set; }
}
}
