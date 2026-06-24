using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Per-resource projection assignment for Data Sync.
    /// The value is the single projection name the principal is "looking through"
    /// for that resource (use "__default__" for the base projection).
    /// </summary>
    public class PNDataSyncProjectionScope
    {
        /// <summary>
        /// Entity id -&gt; projection name. Key e.g. "user.A" (resources) or "user.*" (patterns).
        /// </summary>
        public Dictionary<string, string> Entities { get; set; }

        /// <summary>
        /// Relationship id -&gt; projection name. Key e.g. "user.A:channel.X".
        /// </summary>
        public Dictionary<string, string> Relationships { get; set; }

        /// <summary>
        /// Membership id -&gt; projection name.
        /// </summary>
        public Dictionary<string, string> Memberships { get; set; }
    }

    /// <summary>
    /// Data Sync projection permissions for grant token requests.
    /// Encoded into the "pn-projections" key within the token's meta section.
    /// Exact (res) match takes priority over pattern (pat) match.
    /// </summary>
    public class PNDataSyncProjections
    {
        /// <summary>
        /// Projection assignments for specific Data Sync resources (by exact id).
        /// </summary>
        public PNDataSyncProjectionScope Resources { get; set; }

        /// <summary>
        /// Projection assignments matching Data Sync resources by regex pattern.
        /// </summary>
        public PNDataSyncProjectionScope Patterns { get; set; }
    }
}
