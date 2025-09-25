using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubApi
{
    /// <summary>
    /// Request model for fetch history operations using the request/response API pattern.
    /// </summary>
    public class FetchHistoryRequest
    {
        /// <summary>
        /// The channels to fetch history from. Required field.
        /// </summary>
        public string[] Channels { get; set; }

        /// <summary>
        /// The start timetoken to fetch messages from.
        /// If not specified, fetches from the beginning of history.
        /// </summary>
        public long? Start { get; set; }

        /// <summary>
        /// The end timetoken to fetch messages until.
        /// If not specified, fetches until the most recent message.
        /// </summary>
        public long? End { get; set; }

        /// <summary>
        /// Maximum number of messages to return per channel.
        /// Default is 100 for single channel, 25 for multiple channels or when including message actions.
        /// </summary>
        public int? MaximumPerChannel { get; set; }

        /// <summary>
        /// Whether to return messages in reverse order (oldest first).
        /// Default is false (newest first).
        /// </summary>
        public bool Reverse { get; set; } = false;

        /// <summary>
        /// Whether to include metadata with each message.
        /// Default is false.
        /// </summary>
        public bool IncludeMeta { get; set; } = false;

        /// <summary>
        /// Whether to include message actions with each message.
        /// Only supported when fetching history for a single channel.
        /// Default is false.
        /// </summary>
        public bool IncludeMessageActions { get; set; } = false;

        /// <summary>
        /// Whether to include the publisher UUID with each message.
        /// Default is true.
        /// </summary>
        public bool IncludeUuid { get; set; } = true;

        /// <summary>
        /// Whether to include the message type indicator.
        /// Default is true.
        /// </summary>
        public bool IncludeMessageType { get; set; } = true;

        /// <summary>
        /// Whether to include custom message type information.
        /// Default is false.
        /// </summary>
        public bool IncludeCustomMessageType { get; set; } = false;

        /// <summary>
        /// Additional query parameters to include in the request.
        /// </summary>
        public Dictionary<string, object> QueryParameters { get; set; }

        /// <summary>
        /// Validates that the request has all required fields and valid combinations.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when required fields are missing or invalid.</exception>
        public void Validate()
        {
            if (Channels == null || Channels.Length == 0 || Channels.Any(c => string.IsNullOrEmpty(c?.Trim())))
            {
                throw new ArgumentException("Channels is required and cannot be null, empty, or contain empty channel names.", nameof(Channels));
            }

            if (IncludeMessageActions && Channels.Length > 1)
            {
                throw new ArgumentException("IncludeMessageActions is only supported when fetching history for a single channel.", nameof(IncludeMessageActions));
            }

            if (Start.HasValue && Start.Value < 0)
            {
                throw new ArgumentException("Start timetoken must be non-negative.", nameof(Start));
            }

            if (End.HasValue && End.Value < 0)
            {
                throw new ArgumentException("End timetoken must be non-negative.", nameof(End));
            }

            if (MaximumPerChannel.HasValue && MaximumPerChannel.Value <= 0)
            {
                throw new ArgumentException("MaximumPerChannel must be greater than 0.", nameof(MaximumPerChannel));
            }
        }

        /// <summary>
        /// Gets the effective maximum per channel value based on the request configuration.
        /// </summary>
        /// <returns>The maximum number of messages per channel to fetch.</returns>
        internal int GetEffectiveMaximumPerChannel()
        {
            if (MaximumPerChannel.HasValue && MaximumPerChannel.Value > 0)
            {
                return MaximumPerChannel.Value;
            }

            // Default based on configuration
            if (IncludeMessageActions || (Channels != null && Channels.Length > 1))
            {
                return 25;
            }

            return 100;
        }
    }
}