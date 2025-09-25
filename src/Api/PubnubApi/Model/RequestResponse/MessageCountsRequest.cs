using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubApi
{
    /// <summary>
    /// Request model for message counts operations using the request/response API pattern.
    /// This request retrieves the count of messages for specified channels within given timetoken ranges.
    /// </summary>
    public class MessageCountsRequest
    {
        /// <summary>
        /// The channels to get message counts for. Required field.
        /// At least one channel must be specified.
        /// </summary>
        public string[] Channels { get; set; }

        /// <summary>
        /// The timetokens for each channel to count messages from.
        /// Can be a single timetoken (applied to all channels) or an array matching the number of channels.
        /// If not specified, counts all messages in the channels.
        /// When single timetoken: counts messages from that time to now for all channels.
        /// When multiple timetokens: each timetoken corresponds to its channel at the same index.
        /// </summary>
        public long[] ChannelTimetokens { get; set; }

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
            // Validate required channels
            if (Channels == null || Channels.Length == 0)
            {
                throw new ArgumentException("Channels are required for message counts operation");
            }

            // Validate each channel is not null or empty
            if (Channels.Any(channel => string.IsNullOrWhiteSpace(channel)))
            {
                throw new ArgumentException("Channel names cannot be null or empty");
            }

            // Validate timetoken array if provided
            if (ChannelTimetokens != null && ChannelTimetokens.Length > 0)
            {
                // Timetokens must be either 1 (for all channels) or match channel count
                if (ChannelTimetokens.Length != 1 && ChannelTimetokens.Length != Channels.Length)
                {
                    throw new ArgumentException(
                        $"ChannelTimetokens must have either 1 element (for all channels) or {Channels.Length} elements (one per channel)");
                }

                // Validate timetokens are non-negative
                if (ChannelTimetokens.Any(tt => tt < 0))
                {
                    throw new ArgumentException("Timetokens cannot be negative");
                }
            }
        }

        /// <summary>
        /// Creates a new MessageCountsRequest for a single channel with optional timetoken.
        /// </summary>
        /// <param name="channel">The channel to get message count for</param>
        /// <param name="fromTimetoken">Optional timetoken to count messages from</param>
        /// <returns>A configured MessageCountsRequest</returns>
        public static MessageCountsRequest ForChannel(string channel, long? fromTimetoken = null)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            var request = new MessageCountsRequest
            {
                Channels = new[] { channel }
            };

            if (fromTimetoken.HasValue)
            {
                request.ChannelTimetokens = new[] { fromTimetoken.Value };
            }

            return request;
        }

        /// <summary>
        /// Creates a new MessageCountsRequest for multiple channels with optional shared timetoken.
        /// </summary>
        /// <param name="channels">The channels to get message counts for</param>
        /// <param name="fromTimetoken">Optional shared timetoken to count messages from for all channels</param>
        /// <returns>A configured MessageCountsRequest</returns>
        public static MessageCountsRequest ForChannels(string[] channels, long? fromTimetoken = null)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Channels cannot be null or empty", nameof(channels));
            }

            var request = new MessageCountsRequest
            {
                Channels = channels
            };

            if (fromTimetoken.HasValue)
            {
                request.ChannelTimetokens = new[] { fromTimetoken.Value };
            }

            return request;
        }

        /// <summary>
        /// Creates a new MessageCountsRequest for multiple channels with individual timetokens.
        /// </summary>
        /// <param name="channels">The channels to get message counts for</param>
        /// <param name="channelTimetokens">Individual timetokens for each channel (must match channel count)</param>
        /// <returns>A configured MessageCountsRequest</returns>
        public static MessageCountsRequest ForChannelsWithIndividualTimetokens(string[] channels, long[] channelTimetokens)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Channels cannot be null or empty", nameof(channels));
            }

            if (channelTimetokens == null || channelTimetokens.Length != channels.Length)
            {
                throw new ArgumentException(
                    $"ChannelTimetokens must have {channels.Length} elements to match channel count",
                    nameof(channelTimetokens));
            }

            return new MessageCountsRequest
            {
                Channels = channels,
                ChannelTimetokens = channelTimetokens
            };
        }
    }
}