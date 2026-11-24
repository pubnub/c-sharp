using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Parameters for setting a relationship with complete resource replacement (PUT) via DataSync.
    /// If the relationship doesn't exist and <see cref="IfMatch"/> isn't set, the relationship is created.
    /// </summary>
    public class SetRelationshipParameters
    {
        /// <summary>
        /// Relationship identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// First entity ID (the "A" side of the relationship). Required when the relationship is created.
        /// Immutable after creation, so when the relationship already exists it must be omitted or match
        /// the existing value.
        /// </summary>
        public string EntityAId { get; set; }

        /// <summary>
        /// Second entity ID (the "B" side of the relationship). Required when the relationship is created.
        /// Immutable after creation, so when the relationship already exists it must be omitted or match
        /// the existing value.
        /// </summary>
        public string EntityBId { get; set; }

        /// <summary>
        /// Relationship class identifier (e.g., "ProductOwner", "FriendOf"). Required when the relationship
        /// is created. Immutable after creation, so when the relationship already exists it must be omitted
        /// or match the existing value.
        /// </summary>
        public string RelationshipClass { get; set; }

        /// <summary>
        /// Version of the relationship class. Required. Must be >= 1.
        /// </summary>
        public int RelationshipClassVersion { get; set; }

        /// <summary>
        /// Relationship status (e.g., "active", "inactive"). 1–100 characters.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User-defined custom properties. Supports arbitrarily nested objects.
        /// Replaces the entire payload — omitted fields are removed.
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the request only updates an existing
        /// relationship and the server rejects it when the current resource version does not match (HTTP 412).
        /// </summary>
        /// <remarks>
        /// Required Access Manager permissions: with <c>IfMatch</c>, Get and Update. Without it,
        /// Get and Create when the relationship doesn't exist, or Get and Update when it does; grant Get,
        /// Create and Update when it isn't known whether the relationship exists.
        /// </remarks>
        public string IfMatch { get; set; }
    }
}
