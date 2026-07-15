using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class PatchChannelParameters
    {
        /// <summary>
        /// Channel identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// List of JSON Patch operations (RFC 6902) to apply.
        /// </summary>
        public List<JsonPatchOperation> Operations { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the server rejects
        /// the patch when the current resource version does not match (HTTP 412).
        /// </summary>
        public string IfMatch { get; set; }
    }
}
