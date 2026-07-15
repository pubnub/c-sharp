namespace PubnubApi.EndPoint
{
    public class DeleteMembershipParameters
    {
        /// <summary>
        /// Membership identifier. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ETag for optimistic concurrency control. If provided, the server rejects
        /// the delete when the current resource version does not match (HTTP 412).
        /// </summary>
        public string IfMatch { get; set; }
    }
}
