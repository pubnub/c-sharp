using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Parameters for setting a user with complete resource replacement (PUT) via DataSync.
    /// If the user doesn't exist and <see cref="IfMatch"/> isn't set, the user is created.
    /// </summary>
    public class SetUserParameters
    {
        /// <summary>
        /// User identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Entity class name. Optional — defaults to "User" on the server when the user is created.
        /// Must be "User" or a subclass of "User". Immutable after creation, so when the user
        /// already exists it must be omitted or match the existing value.
        /// </summary>
        public string EntityClass { get; set; }

        /// <summary>
        /// Schema version of the entity class. Required. Must be >= 1.
        /// </summary>
        public int EntityClassVersion { get; set; }

        /// <summary>
        /// Class hierarchy level ("Global" or "SubKey") used to disambiguate classes
        /// with the same name defined at different levels. Optional; only used when the user is created.
        /// </summary>
        public string EntityClassLevel { get; set; }

        /// <summary>
        /// User status (e.g., "active", "inactive"). 1–100 characters.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User-defined custom properties. Supports arbitrarily nested objects.
        /// Replaces the entire payload — omitted fields are removed.
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the request only updates an existing
        /// user and the server rejects it when the current resource version does not match (HTTP 412).
        /// </summary>
        /// <remarks>
        /// Required Access Manager permissions: with <c>IfMatch</c>, Get and Update. Without it,
        /// Get and Create when the user doesn't exist, or Get and Update when it does; grant Get,
        /// Create and Update when it isn't known whether the user exists.
        /// </remarks>
        public string IfMatch { get; set; }
    }
}
