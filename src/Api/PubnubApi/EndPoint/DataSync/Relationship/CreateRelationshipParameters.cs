using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for creating a new relationship via DataSync.
    /// </summary>
    public class CreateRelationshipParameters
    {
        /// <summary>
        /// Relationship identifier. Optional — if not provided, the server generates one.
        /// Must be 1–255 characters if provided.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// First entity ID (the "A" side of the relationship). Required. Immutable after creation.
        /// Must be 1–255 characters.
        /// </summary>
        public string EntityAId { get; set; }

        /// <summary>
        /// Second entity ID (the "B" side of the relationship). Required. Immutable after creation.
        /// Must be 1–255 characters.
        /// </summary>
        public string EntityBId { get; set; }

        /// <summary>
        /// Relationship class identifier (e.g., "ProductOwner", "FriendOf").
        /// Required. Immutable after creation.
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
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
        /// Required for POST requests.
        /// </summary>
        public string IdempotencyKey { get; set; }
    }
}
