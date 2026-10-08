namespace PubnubApi
{
    /// <summary>
    /// Category-level App Context permissions of a token. A category applies to a resource type
    /// as a whole: resource-level or pattern Get permissions do not allow listing all metadata of that type.
    /// </summary>
    public class PNTokenCategories
    {
        /// <summary>
        /// Get allows listing all channel metadata on the subscribe key.
        /// </summary>
        public PNTokenCategoryPermissions Channels { get; set; } = new PNTokenCategoryPermissions();

        /// <summary>
        /// Get allows listing all uuid metadata on the subscribe key.
        /// </summary>
        public PNTokenCategoryPermissions Uuids { get; set; } = new PNTokenCategoryPermissions();
    }

    public class PNTokenCategoryPermissions
    {
        /// <summary>
        /// Get. The only permission accepted for categories.
        /// </summary>
        public bool Get { get; set; }
    }
}
