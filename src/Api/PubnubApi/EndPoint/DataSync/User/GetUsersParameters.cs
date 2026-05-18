using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class GetUsersParameters
    {
        /// <summary>
        /// Schema version of the entity class. Optional — if not provided the server
        /// returns users matching the latest version.
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

    public class PNDataSyncUsersListResult
    {
        public List<PNDataSyncUserResult> Data { get; internal set; } = new();
        public PaginationMeta Meta { get; internal set; }
        public PaginationLinks Links { get; internal set; }
    }
}
