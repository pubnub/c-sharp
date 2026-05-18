using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class CreateMembershipParameters
    {
        /// <summary>
        /// Membership identifier. Optional — if not provided, the server generates one.
        /// Must be 1–255 characters if provided.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Channel ID. Required. Immutable after creation.
        /// Maps to entityAId on the wire.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// User ID. Required. Immutable after creation.
        /// Maps to entityBId on the wire.
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
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
        /// Required for POST requests.
        /// </summary>
        public string IdempotencyKey { get; set; }
    }
}
