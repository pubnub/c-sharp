using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Response model for publish operations using the request/response API pattern.
    /// </summary>
    public class PublishResponse
    {
        /// <summary>
        /// The timetoken when the message was published.
        /// </summary>
        public long Timetoken { get; internal set; }

        /// <summary>
        /// The channel the message was published to.
        /// </summary>
        public string Channel { get; internal set; }

        /// <summary>
        /// Indicates whether the publish operation was successful.
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// HTTP status code from the publish request.
        /// </summary>
        public int StatusCode { get; internal set; }

        /// <summary>
        /// Additional response headers if available.
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }

        /// <summary>
        /// Any error information if the publish failed.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Creates a successful PublishResponse.
        /// </summary>
        /// <param name="timetoken">The publish timetoken</param>
        /// <param name="channel">The channel published to</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>A successful PublishResponse</returns>
        public static PublishResponse CreateSuccess(long timetoken, string channel, int statusCode = 200)
        {
            return new PublishResponse
            {
                Timetoken = timetoken,
                Channel = channel,
                IsSuccess = true,
                StatusCode = statusCode,
                Headers = new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Creates an error PublishResponse.
        /// </summary>
        /// <param name="channel">The channel that was attempted to publish to</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error PublishResponse</returns>
        public static PublishResponse CreateError(string channel, string errorMessage, int statusCode = 400)
        {
            return new PublishResponse
            {
                Channel = channel,
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Headers = new Dictionary<string, string>()
            };
        }
    }
}