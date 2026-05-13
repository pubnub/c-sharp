using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for updating an entity with complete resource replacement (PUT) via DataSync.
    /// </summary>
    public class UpdateEntityParameters
    {
        /// <summary>
        /// Entity identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Schema version of the entity class. Required. Must be >= 1.
        /// Note: entityClass is immutable and cannot be changed after creation.
        /// </summary>
        public int EntityClassVersion { get; set; }

        /// <summary>
        /// Entity status (e.g., "active", "inactive"). 1–100 characters.
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
