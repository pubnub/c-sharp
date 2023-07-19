using Newtonsoft.Json;

namespace PubnubApi.EventEngine.Subscribe.Common
{
    public class SubscriptionCursor
    {
        public long? Timetoken { get; set; }
        public int? Region { get; set; }
    }
    
    public class HandshakeResponse
    {
        [JsonProperty("t")]
        public Timetoken Timetoken { get; set; }

        [JsonProperty("m")]
        public object[] Messages { get; set; }
    }
    public class HandshakeError
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }

    public class Timetoken
    {
        [JsonProperty("t")]
        public long? Timestamp { get; set; }

        [JsonProperty("r")]
        public int? Region { get; set; }

    }
}