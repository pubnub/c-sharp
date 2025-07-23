using System.Collections.Generic;

namespace PubnubApi
{
    public class PNUuidMetadataResult
    {
        public string Uuid { get; internal set; }
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public string Status { get; internal set; }
        public string Type { get; internal set; }
        public string ExternalId { get; internal set; }
        public string ProfileUrl { get; internal set; }
        public Dictionary<string, object> Custom { get; internal set; }
        public string Updated { get; internal set; }
    }
}
