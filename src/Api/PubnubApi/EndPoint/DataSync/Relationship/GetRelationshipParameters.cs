namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for retrieving a relationship by ID via DataSync.
    /// </summary>
    public class GetRelationshipParameters
    {
        /// <summary>
        /// Relationship identifier. Required.
        /// </summary>
        public string Id { get; set; }
    }
}
