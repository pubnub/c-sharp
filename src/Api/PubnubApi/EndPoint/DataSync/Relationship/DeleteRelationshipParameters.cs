namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for deleting a relationship by ID via DataSync.
    /// </summary>
    public class DeleteRelationshipParameters
    {
        /// <summary>
        /// Relationship identifier. Required.
        /// </summary>
        public string Id { get; set; }
    }
}
