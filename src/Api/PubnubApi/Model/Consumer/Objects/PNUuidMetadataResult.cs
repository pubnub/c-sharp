using System.Collections.Generic;

namespace PubnubApi
{
    public class PNUuidMetadataResult
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string ProfileUrl { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public string Updated { get; set; }
    }
}
