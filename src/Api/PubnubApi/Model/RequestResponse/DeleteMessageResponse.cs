using System;

namespace PubnubApi
{
    /// <summary>
    /// Response model for delete message operations using the request/response API pattern.
    /// Indicates successful deletion of messages from a channel.
    /// </summary>
    public class DeleteMessageResponse
    {
        /// <summary>
        /// Gets a value indicating whether the delete operation was successful.
        /// Always true for a successfully returned response (errors throw exceptions).
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Gets the HTTP status code from the delete operation.
        /// Typically 200 for successful operations.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the channel from which messages were deleted.
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the start timetoken used in the delete operation, if specified.
        /// </summary>
        public long? StartTimetoken { get; private set; }

        /// <summary>
        /// Gets the end timetoken used in the delete operation, if specified.
        /// </summary>
        public long? EndTimetoken { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage.
        /// </summary>
        private DeleteMessageResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Creates a successful delete message response.
        /// </summary>
        /// <param name="channel">The channel from which messages were deleted</param>
        /// <param name="statusCode">The HTTP status code from the operation</param>
        /// <param name="startTimetoken">The start timetoken used, if any</param>
        /// <param name="endTimetoken">The end timetoken used, if any</param>
        /// <returns>A DeleteMessageResponse indicating success</returns>
        internal static DeleteMessageResponse CreateSuccess(string channel, int statusCode, long? startTimetoken = null, long? endTimetoken = null)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            return new DeleteMessageResponse
            {
                Success = true,
                StatusCode = statusCode,
                Channel = channel,
                StartTimetoken = startTimetoken,
                EndTimetoken = endTimetoken
            };
        }

        /// <summary>
        /// Creates a successful delete message response from PubNub result.
        /// Since PNDeleteMessageResult is empty, we only use status information.
        /// </summary>
        /// <param name="result">The PubNub delete message result (currently unused as it's empty)</param>
        /// <param name="channel">The channel from which messages were deleted</param>
        /// <param name="statusCode">The HTTP status code from the operation</param>
        /// <param name="startTimetoken">The start timetoken used, if any</param>
        /// <param name="endTimetoken">The end timetoken used, if any</param>
        /// <returns>A DeleteMessageResponse indicating success</returns>
        internal static DeleteMessageResponse CreateSuccess(PNDeleteMessageResult result, string channel, int statusCode, long? startTimetoken = null, long? endTimetoken = null)
        {
            // Note: PNDeleteMessageResult is currently an empty class, so we don't use it
            // This signature is provided for consistency with other response creators
            return CreateSuccess(channel, statusCode, startTimetoken, endTimetoken);
        }

        /// <summary>
        /// Returns a string representation of the delete message response.
        /// </summary>
        /// <returns>A formatted string describing the deletion</returns>
        public override string ToString()
        {
            var rangeInfo = "";
            if (StartTimetoken.HasValue && EndTimetoken.HasValue)
            {
                rangeInfo = $" (Range: {StartTimetoken.Value} to {EndTimetoken.Value})";
            }
            else if (StartTimetoken.HasValue)
            {
                rangeInfo = $" (From: {StartTimetoken.Value})";
            }
            else if (EndTimetoken.HasValue)
            {
                rangeInfo = $" (Until: {EndTimetoken.Value})";
            }

            return $"DeleteMessageResponse: Success={Success}, Channel={Channel}, StatusCode={StatusCode}{rangeInfo}";
        }
    }
}