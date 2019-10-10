using System;
using Newtonsoft.Json;

namespace PubnubApi
{
public class PNMessageAction
{
    public PNMessageAction()
    {
        this.Type = "";
        this.Value = "";
    }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; }
}
}
