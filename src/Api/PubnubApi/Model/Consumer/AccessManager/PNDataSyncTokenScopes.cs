using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Optional DataSync permission scopes. Serialized to the REST keys
    /// datasync:entities / datasync:relationships / datasync:memberships
    /// inside the grant token resources / patterns sections.
    /// </summary>
    public class PNDataSyncTokenScopes
    {
        /// <summary>
        /// Concrete or pattern entity ids, e.g. "order-456" (resources) or "order-*" (patterns).
        /// </summary>
        public Dictionary<string, PNTokenAuthValues> Entities { get; set; }

        /// <summary>
        /// Composite relationship ids, e.g. "user.A:channel.X".
        /// </summary>
        public Dictionary<string, PNTokenAuthValues> Relationships { get; set; }

        /// <summary>
        /// Composite membership ids, e.g. "user-123:channel-X".
        /// </summary>
        public Dictionary<string, PNTokenAuthValues> Memberships { get; set; }
    }
}
