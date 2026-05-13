using System;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    /// <summary>
    /// Parameters for creating a new entity via DataSync.
    /// </summary>
    public class CreateEntityParameters
    {
        /// <summary>
        /// Entity identifier. Optional — if not provided, the server generates one.
        /// Must be 1–255 characters if provided.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Entity class identifier (e.g., "vehicle", "order", "sensor").
        /// Required. Immutable after creation.
        /// </summary>
        public string EntityClass { get; set; }

        /// <summary>
        /// Schema version of the entity class. Required. Must be >= 1.
        /// </summary>
        public int EntityClassVersion { get; set; }

        /// <summary>
        /// Entity status (e.g., "active", "inactive"). 1–100 characters.
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
