using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class PatchEntityParameters
    {
        /// <summary>
        /// Entity identifier. Required.
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

    public enum JsonPatchOperationType
    {
        Add,
        Remove,
        Replace,
        Move,
        Copy,
        Test
    }

    /// <summary>
    /// A single JSON Patch operation as defined by RFC 6902.
    /// </summary>
    public class JsonPatchOperation
    {
        /// <summary>
        /// The operation to perform: "add", "remove", "replace", "move", "copy", or "test".
        /// </summary>
        public JsonPatchOperationType Op { get; set; }

        /// <summary>
        /// JSON Pointer (RFC 6901) to the target location.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The value to apply. Required for "add", "replace", and "test" operations.
        /// Can be any JSON-serializable value including null.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Source location (JSON Pointer). Required for "move" and "copy" operations.
        /// </summary>
        public string From { get; set; }
    }
}
