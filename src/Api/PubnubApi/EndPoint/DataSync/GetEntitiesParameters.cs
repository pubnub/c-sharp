using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for listing entities via DataSync.
    /// </summary>
    public class GetEntitiesParameters
    {
        /// <summary>
        /// Entity class name to filter by. Required.
        /// </summary>
        public string EntityClass { get; set; }

        /// <summary>
        /// Schema version of the entity class. Optional — if not provided the server
        /// returns entities matching the latest version.
        /// </summary>
        public int? EntityClassVersion { get; set; }

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
    /// Result returned by GetEntities (list) containing an array of entities
    /// plus cursor-based pagination metadata and HATEOAS links.
    /// </summary>
    public class PNDataSyncEntitiesListResult
    {
        public List<PNDataSyncEntityResult> Data { get; internal set; } = new();
        public PaginationMeta Meta { get; internal set; }
        public PaginationLinks Links { get; internal set; }
    }

    /// <summary>
    /// Cursor-based pagination metadata returned in list responses.
    /// </summary>
    public class PaginationMeta
    {
        public string NextCursor { get; internal set; }
        public string PrevCursor { get; internal set; }
        public bool HasNext { get; internal set; }
        public bool HasPrev { get; internal set; }
        public int? Limit { get; internal set; }
    }

    /// <summary>
    /// HATEOAS navigation links returned in list responses.
    /// </summary>
    public class PaginationLinks
    {
        public string Self { get; internal set; }
        public string Next { get; internal set; }
        public string Prev { get; internal set; }
    }
}
