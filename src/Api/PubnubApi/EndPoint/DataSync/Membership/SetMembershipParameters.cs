using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Parameters for setting a membership with complete resource replacement (PUT) via DataSync.
    /// If the membership doesn't exist and <see cref="IfMatch"/> isn't set, the membership is created.
    /// </summary>
    public class SetMembershipParameters
    {
        /// <summary>
        /// Membership identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Channel ID. Required when the membership is created. Immutable after creation, so when
        /// the membership already exists it must be omitted or match the existing value.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// User ID. Required when the membership is created. Immutable after creation, so when
        /// the membership already exists it must be omitted or match the existing value.
        /// </summary>
        public string UserId { get; set; }

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
        /// ETag for optimistic concurrency control. If provided, the request only updates an existing
        /// membership and the server rejects it when the current resource version does not match (HTTP 412).
        /// </summary>
        /// <remarks>
        /// Required Access Manager permissions: with <c>IfMatch</c>, Get and Update. Without it,
        /// Get and Create when the membership doesn't exist, or Get and Update when it does; grant Get,
        /// Create and Update when it isn't known whether the membership exists.
        /// </remarks>
        public string IfMatch { get; set; }
    }
}
