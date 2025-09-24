using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Request model for publish operations using the request/response API pattern.
    /// </summary>
    public class PublishRequest
    {
        /// <summary>
        /// The message to publish. Can be any serializable object.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// The channel to publish to. Required field.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Whether to store the message in history. Default is true.
        /// </summary>
        public bool StoreInHistory { get; set; } = true;

        /// <summary>
        /// Time to live for the message in hours. Default is -1 (no TTL).
        /// </summary>
        public int Ttl { get; set; } = -1;

        /// <summary>
        /// Custom metadata to include with the message.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Whether to use HTTP POST instead of GET. Default is false.
        /// </summary>
        public bool UsePost { get; set; } = false;

        /// <summary>
        /// Custom message type identifier.
        /// </summary>
        public string CustomMessageType { get; set; }

        /// <summary>
        /// Additional query parameters to include in the request.
        /// </summary>
        public Dictionary<string, object> QueryParameters { get; set; }

        /// <summary>
        /// Validates that the request has all required fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when required fields are missing or invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Channel?.Trim()))
            {
                throw new ArgumentException("Channel is required and cannot be null or empty.", nameof(Channel));
            }

            if (Message == null)
            {
                throw new ArgumentException("Message is required and cannot be null.", nameof(Message));
            }
        }
    }
}