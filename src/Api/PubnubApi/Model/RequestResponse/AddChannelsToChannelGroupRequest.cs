using System;

namespace PubnubApi
{
    /// <summary>
    /// Request object for adding channels to a channel group
    /// </summary>
    public class AddChannelsToChannelGroupRequest
    {
        /// <summary>
        /// The channels to add to the channel group
        /// </summary>
        public string[] Channels { get; set; }

        /// <summary>
        /// The channel group to add channels to
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