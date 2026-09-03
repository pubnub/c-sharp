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

    public class PNDataSyncUsersListResult
    {
        public List<PNDataSyncUserResult> Data { get; internal set; } = new();
        public PaginationMeta Meta { get; internal set; }
    }
}
