using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class UpdateChannelParameters
    {
        /// <summary>
        /// Channel identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Schema version of the entity class. Required. Must be >= 1.
        /// </summary>
        public int EntityClassVersion { get; set; }

        /// <summary>
        /// Channel status (e.g., "active", "archived"). 1–100 characters.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User-defined custom properties. Supports arbitrarily nested objects.
        /// Replaces the entire payload — omitted fields are removed.
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the server rejects
        /// the update when the current resource version does not match (HTTP 412).
        /// </summary>
        public string IfMatch { get; set; }
    }
}
