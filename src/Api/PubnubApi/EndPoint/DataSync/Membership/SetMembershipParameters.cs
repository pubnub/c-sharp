using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class UpdateMembershipParameters
    {
        /// <summary>
        /// Membership identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Version of the membership relationship class. Required. Must be >= 1.
        /// </summary>
        public int RelationshipClassVersion { get; set; }

        /// <summary>
        /// Membership status (e.g., "active", "banned"). 1–100 characters.
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
