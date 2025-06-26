using System;
using System.Text.Json.Serialization;

namespace PubnubApi
{
public class PNMessageAction
{
    public PNMessageAction()
    {
        this.Type = "";
        this.Value = "";
    }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
}
