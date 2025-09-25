using System;

namespace PubnubApi
{
    /// <summary>
    /// Request object for removing channels from a channel group
    /// </summary>
    public class RemoveChannelsFromChannelGroupRequest
    {
        /// <summary>
        /// The channels to remove from the channel group
        /// </summary>
        public string[] Channels { get; set; }

        /// <summary>
        /// The channel group to remove channels from
        /// </summary>
        public string ChannelGroup { get; set; }

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        internal void Validate()
        {
            if (Channels == null || Channels.Length == 0)
            {
                throw new ArgumentException("Channels cannot be null or empty");
            }

            if (string.IsNullOrEmpty(ChannelGroup) || ChannelGroup.Trim().Length == 0)
            {
                throw new ArgumentException("ChannelGroup cannot be null or empty");
            }
        }
    }
}