using System;

namespace PubnubApi
{
    /// <summary>
    /// Request object for listing channels in a channel group
    /// </summary>
    public class ListChannelsForChannelGroupRequest
    {
        /// <summary>
        /// The channel group to list channels for
        /// </summary>
        public string ChannelGroup { get; set; }

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        internal void Validate()
        {
            if (string.IsNullOrEmpty(ChannelGroup) || ChannelGroup.Trim().Length == 0)
            {
                throw new ArgumentException("ChannelGroup cannot be null or empty");
            }
        }
    }
}