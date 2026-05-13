namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for deleting an entity by ID via DataSync.
    /// </summary>
    public class DeleteEntityParameters
    {
        /// <summary>
        /// Entity identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the server rejects
        /// the delete when the current resource version does not match (HTTP 412).
        /// </summary>
        public string IfMatch { get; set; }
    }
}
