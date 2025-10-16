
namespace PubnubApi
{
    public enum PNPushType
    {
        [System.Obsolete("GCM is decommissioned by Google. Please use FCM instead.", false)]
        GCM,
        FCM,
        [System.Obsolete("APNs is deprecated by Apple.", false)]
        APNS,
        APNS2
    }

    /// <summary>
    /// Extension methods for PNPushType enum to get url value
    /// </summary>
    public static class PNPushTypeExtensions
    {
        /// <summary>
        /// Converts PNPushType enum to the appropriate URL string.
        /// GCM is converted to 'fcm' since GCM is decommisioned.
        /// </summary>
        /// <param name="pushType">The push type enum value</param>
        /// <returns>URL string representation of the push type</returns>
        public static string ToUrlString(this PNPushType pushType)
        {
            return pushType switch
            {
                PNPushType.GCM => "fcm",  // GCM is deprecated, use FCM
                PNPushType.FCM => "fcm",
                PNPushType.APNS => "apns",
                PNPushType.APNS2 => "apns2",
                _ => pushType.ToString().ToLowerInvariant()
            };
        }
    }
}
