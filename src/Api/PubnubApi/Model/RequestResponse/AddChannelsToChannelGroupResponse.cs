using System;

namespace PubnubApi
{
    /// <summary>
    /// Response object for adding channels to a channel group
    /// </summary>
    public class AddChannelsToChannelGroupResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The response message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The exception if the operation failed
        /// </summary>
        public Exception Exception { get; }

        private AddChannelsToChannelGroupResponse(bool success, string message, Exception exception = null)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        internal static AddChannelsToChannelGroupResponse CreateSuccess(PNChannelGroupsAddChannelResult result)
        {
            return new AddChannelsToChannelGroupResponse(true, "Channel(s) added successfully", null);
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        internal static AddChannelsToChannelGroupResponse CreateFailure(Exception exception)
        {
            return new AddChannelsToChannelGroupResponse(false, exception?.Message ?? "Operation failed", exception);
        }
    }
}