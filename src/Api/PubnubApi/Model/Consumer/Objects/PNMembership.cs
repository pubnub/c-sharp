using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembership
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("custom")]
        public Dictionary<string, object> Custom { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
