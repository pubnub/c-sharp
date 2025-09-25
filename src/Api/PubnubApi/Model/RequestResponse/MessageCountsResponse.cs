using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubApi
{
    /// <summary>
    /// Response model for message counts operations using the request/response API pattern.
    /// Contains the message counts for requested channels.
    /// </summary>
    public class MessageCountsResponse
    {
        /// <summary>
        /// Dictionary mapping channel names to their respective message counts.
        /// Key: Channel name
        /// Value: Count of messages in that channel
        /// </summary>
        public Dictionary<string, long> Channels { get; private set; }

        /// <summary>
        /// Gets the total message count across all channels.
        /// </summary>
        public long TotalMessageCount => Channels?.Sum(kvp => kvp.Value) ?? 0;

        /// <summary>
        /// Gets the number of channels in the response.
        /// </summary>
        public int ChannelCount => Channels?.Count ?? 0;

        /// <summary>
        /// Private constructor to enforce factory method usage.
        /// </summary>
        private MessageCountsResponse()
        {
            Channels = new Dictionary<string, long>();
        }

        /// <summary>
        /// Creates a successful message counts response from PubNub result.
        /// </summary>
        /// <param name="result">The PubNub message count result</param>
        /// <returns>A MessageCountsResponse containing the channel counts</returns>
        internal static MessageCountsResponse CreateSuccess(PNMessageCountResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new MessageCountsResponse
            {
                Channels = result.Channels ?? new Dictionary<string, long>()
            };
        }

        /// <summary>
        /// Creates an empty message counts response.
        /// Used when no messages are found or for error scenarios.
        /// </summary>
        /// <returns>An empty MessageCountsResponse</returns>
        internal static MessageCountsResponse CreateEmpty()
        {
            return new MessageCountsResponse
            {
                Channels = new Dictionary<string, long>()
            };
        }

        /// <summary>
        /// Gets the message count for a specific channel.
        /// </summary>
        /// <param name="channel">The channel name</param>
        /// <returns>The message count for the channel, or 0 if not found</returns>
        public long GetCountForChannel(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            return Channels != null && Channels.TryGetValue(channel, out var count) ? count : 0;
        }

        /// <summary>
        /// Checks if the response contains data for a specific channel.
        /// </summary>
        /// <param name="channel">The channel name to check</param>
        /// <returns>True if the channel is present in the response, false otherwise</returns>
        public bool HasChannel(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return false;
            }

            return Channels != null && Channels.ContainsKey(channel);
        }

        /// <summary>
        /// Gets all channel names from the response.
        /// </summary>
        /// <returns>An array of channel names</returns>
        public string[] GetChannelNames()
        {
            return Channels?.Keys.ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Returns a string representation of the message counts response.
        /// </summary>
        /// <returns>A formatted string showing channel counts</returns>
        public override string ToString()
        {
            if (Channels == null || Channels.Count == 0)
            {
                return "MessageCountsResponse: No channels";
            }

            var channelInfo = string.Join(", ", Channels.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            return $"MessageCountsResponse: {ChannelCount} channel(s), Total: {TotalMessageCount} [{channelInfo}]";
        }
    }
}