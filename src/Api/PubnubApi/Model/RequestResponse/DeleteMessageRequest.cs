using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Request model for delete message operations using the request/response API pattern.
    /// This request deletes messages from a specific channel within an optional timetoken range.
    /// </summary>
    public class DeleteMessageRequest
    {
        /// <summary>
        /// The channel from which to delete messages. Required field.
        /// Only a single channel is supported for delete operations.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// The starting timetoken for the deletion range (inclusive).
        /// If not specified, deletes from the beginning of the channel history.
        /// </summary>
        public long? Start { get; set; }

        /// <summary>
        /// The ending timetoken for the deletion range (exclusive).
        /// If not specified, deletes to the end of the channel history.
        /// </summary>
        public long? End { get; set; }

        /// <summary>
        /// Additional query parameters for the request.
        /// Allows for future extensibility and custom parameters.
        /// </summary>
        public Dictionary<string, object> QueryParameters { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        public void Validate()
        {
            // Validate required channel
            if (string.IsNullOrWhiteSpace(Channel))
            {
                throw new ArgumentException("Channel is required for delete message operation");
            }

            // Validate timetoken values if provided
            if (Start.HasValue && Start.Value < 0)
            {
                throw new ArgumentException("Start timetoken cannot be negative");
            }

            if (End.HasValue && End.Value < 0)
            {
                throw new ArgumentException("End timetoken cannot be negative");
            }

            // Validate logical timetoken range
            if (Start.HasValue && End.HasValue && Start.Value > End.Value)
            {
                throw new ArgumentException("Start timetoken must be less than or equal to end timetoken");
            }
        }

        /// <summary>
        /// Creates a new DeleteMessageRequest for deleting all messages in a channel.
        /// </summary>
        /// <param name="channel">The channel to delete messages from</param>
        /// <returns>A configured DeleteMessageRequest</returns>
        public static DeleteMessageRequest ForChannel(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            return new DeleteMessageRequest
            {
                Channel = channel
            };
        }

        /// <summary>
        /// Creates a new DeleteMessageRequest for deleting messages within a specific timetoken range.
        /// </summary>
        /// <param name="channel">The channel to delete messages from</param>
        /// <param name="start">The starting timetoken (inclusive)</param>
        /// <param name="end">The ending timetoken (exclusive)</param>
        /// <returns>A configured DeleteMessageRequest</returns>
        public static DeleteMessageRequest ForChannelWithRange(string channel, long start, long end)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            if (start < 0)
            {
                throw new ArgumentException("Start timetoken cannot be negative", nameof(start));
            }

            if (end < 0)
            {
                throw new ArgumentException("End timetoken cannot be negative", nameof(end));
            }

            if (start > end)
            {
                throw new ArgumentException("Start timetoken must be less than or equal to end timetoken");
            }

            return new DeleteMessageRequest
            {
                Channel = channel,
                Start = start,
                End = end
            };
        }

        /// <summary>
        /// Creates a new DeleteMessageRequest for deleting messages from a specific start time.
        /// </summary>
        /// <param name="channel">The channel to delete messages from</param>
        /// <param name="start">The starting timetoken (inclusive)</param>
        /// <returns>A configured DeleteMessageRequest</returns>
        public static DeleteMessageRequest ForChannelFromTime(string channel, long start)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            if (start < 0)
            {
                throw new ArgumentException("Start timetoken cannot be negative", nameof(start));
            }

            return new DeleteMessageRequest
            {
                Channel = channel,
                Start = start
            };
        }

        /// <summary>
        /// Creates a new DeleteMessageRequest for deleting messages up to a specific end time.
        /// </summary>
        /// <param name="channel">The channel to delete messages from</param>
        /// <param name="end">The ending timetoken (exclusive)</param>
        /// <returns>A configured DeleteMessageRequest</returns>
        public static DeleteMessageRequest ForChannelUntilTime(string channel, long end)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            if (end < 0)
            {
                throw new ArgumentException("End timetoken cannot be negative", nameof(end));
            }

            return new DeleteMessageRequest
            {
                Channel = channel,
                End = end
            };
        }
    }
}