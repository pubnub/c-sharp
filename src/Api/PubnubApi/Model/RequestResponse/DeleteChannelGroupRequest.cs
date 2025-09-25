using System;

namespace PubnubApi
{
    /// <summary>
    /// Request object for deleting a channel group
    /// </summary>
    public class DeleteChannelGroupRequest
    {
        /// <summary>
        /// The channel group to delete
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