using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Response object for listing channels in a channel group
    /// </summary>
    public class ListChannelsForChannelGroupResponse
    {
        /// <summary>
        /// The list of channels in the channel group
        /// </summary>
        public List<string> Channels { get; }

        /// <summary>
        /// The channel group name
        /// </summary>
        public string ChannelGroup { get; }

        /// <summary>
        /// The exception if the operation failed
        /// </summary>
        public Exception Exception { get; }

        private ListChannelsForChannelGroupResponse(List<string> channels, string channelGroup, Exception exception = null)
        {
            Channels = channels;
            ChannelGroup = channelGroup;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        internal static ListChannelsForChannelGroupResponse CreateSuccess(PNChannelGroupsAllChannelsResult result)
        {
            return new ListChannelsForChannelGroupResponse(
                result?.Channels ?? new List<string>(),
                result?.ChannelGroup,
                null);
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        internal static ListChannelsForChannelGroupResponse CreateFailure(Exception exception)
        {
            return new ListChannelsForChannelGroupResponse(new List<string>(), null, exception);
        }
    }
}