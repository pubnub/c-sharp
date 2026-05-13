using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for retrieving an entity by ID via DataSync.
    /// </summary>
    public class GetEntityParameters
    {
        /// <summary>
        /// Entity identifier. Required.
        /// </summary>
        public string Id { get; set; }
    }
}
