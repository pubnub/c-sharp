using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for listing relationships via DataSync.
    /// </summary>
    public class GetRelationshipsParameters
    {
        /// <summary>
        /// Relationship class name to filter by. Required.
        /// </summary>
        public string RelationshipClass { get; set; }

        /// <summary>
        /// Filter relationships by the first entity (A-side) ID. Optional.
        /// At least one of EntityAId or EntityBId should be provided for meaningful results.
        /// </summary>
        public string EntityAId { get; set; }

        /// <summary>
        /// Filter relationships by the second entity (B-side) ID. Optional.
        /// At least one of EntityAId or EntityBId should be provided for meaningful results.
        /// </summary>
        public string EntityBId { get; set; }

        /// <summary>
        /// Pagination cursor returned from a previous request.
        /// </summary>
        public string Cursor { get; set; }

        /// <summary>
        /// Maximum number of items to return per page.
        /// Min 1, max 100, default 20.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Filter expression using AppContext Query Language (e.g., "status == 'active'").
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Advanced filter expression supporting logical operators and nested conditions.
        /// </summary>
        public string FilterAdvanced { get; set; }

        /// <summary>
        /// Comma-separated list of fields to sort by. Prefix with + for ascending
        /// or - for descending (default). Example: "-createdAt,+id".
        /// </summary>
        public string Sort { get; set; }
    }

    /// <summary>
    /// Result returned by GetRelationships (list) containing an array of relationships
    /// plus cursor-based pagination metadata and HATEOAS links.
    /// </summary>
    public class PNDataSyncRelationshipsListResult
    {
        public List<PNDataSyncRelationshipResult> Data { get; internal set; } = new();
        public PaginationMeta Meta { get; internal set; }
        public PaginationLinks Links { get; internal set; }
    }
}
