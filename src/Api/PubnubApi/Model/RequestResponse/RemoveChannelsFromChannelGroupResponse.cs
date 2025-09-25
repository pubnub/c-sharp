using System;

namespace PubnubApi
{
    /// <summary>
    /// Response object for removing channels from a channel group
    /// </summary>
    public class RemoveChannelsFromChannelGroupResponse
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

        private RemoveChannelsFromChannelGroupResponse(bool success, string message, Exception exception = null)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        internal static RemoveChannelsFromChannelGroupResponse CreateSuccess(PNChannelGroupsRemoveChannelResult result)
        {
            return new RemoveChannelsFromChannelGroupResponse(true, result?.Message ?? "Channel(s) removed successfully", null);
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        internal static RemoveChannelsFromChannelGroupResponse CreateFailure(Exception exception)
        {
            return new RemoveChannelsFromChannelGroupResponse(false, exception?.Message ?? "Operation failed", exception);
        }
    }
}