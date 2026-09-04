using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class CreateChannelParameters
    {
        /// <summary>
        /// Channel identifier. Optional — if not provided, the server generates one.
        /// Must be 1–255 characters if provided.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Entity class name. Optional — defaults to "Channel" on the server if not
        /// provided. Must be "Channel" or a subclass of "Channel".
        /// </summary>
        public string EntityClass { get; set; }

        /// <summary>
        /// Schema version of the entity class. Required. Must be >= 1.
        /// </summary>
        public int EntityClassVersion { get; set; } = 1;

        /// <summary>
        /// Class hierarchy level ("Global" or "SubKey") used to disambiguate classes
        /// with the same name defined at different levels. Optional.
        /// </summary>
        public string EntityClassLevel { get; set; }

        /// <summary>
        /// Channel status (e.g., "active", "archived"). 1–100 characters.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User-defined custom properties. Supports arbitrarily nested objects.
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }
    }
}
