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
        /// Version of the relationship class. Optional — if not provided the server
        /// uses the latest version. Must be >= 1 when provided.
        /// </summary>
        public int? RelationshipClassVersion { get; set; }

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
        /// Filter expression using AppContext Query Language, evaluated against strongly
        /// consistent storage. Supports a limited number of conditions.
        /// Example: "status == 'active'".
        /// </summary>
        public string FilterFast { get; set; }

        /// <summary>
        /// Filter expression using AppContext Query Language, evaluated against eventually
        /// consistent storage. Supports logical operators and nested conditions.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Comma-separated list of fields to sort by, each optionally suffixed with
        /// :desc (ascending by default). Example: "createdAt:desc,id".
        /// </summary>
        public string Sort { get; set; }
    }

    /// <summary>
    /// Result returned by GetRelationships (list) containing an array of relationships
    /// plus cursor-based pagination metadata.
    /// </summary>
    public class PNDataSyncRelationshipsListResult
    {
        public List<PNDataSyncRelationshipResult> Data { get; internal set; } = new();
        public PaginationMeta Meta { get; internal set; }
    }
}
