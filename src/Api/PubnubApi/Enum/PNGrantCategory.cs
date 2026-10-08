namespace PubnubApi
{
    /// <summary>
    /// App Context resource types that a PAMv2 category grant applies to as a whole.
    /// </summary>
    public enum PNGrantCategory
    {
        /// <summary>
        /// Listing all channel metadata on the subscribe key.
        /// </summary>
        Channels,

        /// <summary>
        /// Listing all uuid metadata on the subscribe key.
        /// </summary>
        Uuids
    }
}
