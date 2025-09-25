using System;

namespace PubnubApi
{
    /// <summary>
    /// Response object for deleting a channel group
    /// </summary>
    public class DeleteChannelGroupResponse
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

        private DeleteChannelGroupResponse(bool success, string message, Exception exception = null)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        internal static DeleteChannelGroupResponse CreateSuccess(PNChannelGroupsDeleteGroupResult result)
        {
            return new DeleteChannelGroupResponse(true, result?.Message ?? "Channel group deleted successfully", null);
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        internal static DeleteChannelGroupResponse CreateFailure(Exception exception)
        {
            return new DeleteChannelGroupResponse(false, exception?.Message ?? "Operation failed", exception);
        }
    }
}