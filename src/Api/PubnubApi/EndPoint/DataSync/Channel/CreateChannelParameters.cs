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
        /// Schema version of the entity class. Required. Must be >= 1.
        /// </summary>
        public int EntityClassVersion { get; set; }

        /// <summary>
        /// Channel status (e.g., "active", "archived"). 1–100 characters.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User-defined custom properties. Supports arbitrarily nested objects.
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
        /// Required for POST requests.
        /// </summary>
        public string IdempotencyKey { get; set; }
    }
}
