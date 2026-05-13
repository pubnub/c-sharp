using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for partially updating a relationship via JSON Patch (RFC 6902).
    /// </summary>
    public class PatchRelationshipParameters
    {
        /// <summary>
        /// Relationship identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// List of JSON Patch operations (RFC 6902) to apply. Required; must contain at least one operation.
        /// </summary>
        public List<JsonPatchOperation> Operations { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the server rejects
        /// the patch when the current resource version does not match (HTTP 412).
        /// </summary>
        public string IfMatch { get; set; }

        /// <summary>
        /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
        /// Required for PATCH requests.
        /// </summary>
        public string IdempotencyKey { get; set; }
    }
}
