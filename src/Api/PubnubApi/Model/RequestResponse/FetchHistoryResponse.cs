using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Response model for fetch history operations using the request/response API pattern.
    /// </summary>
    public class FetchHistoryResponse
    {
        /// <summary>
        /// Information about a single history message.
        /// </summary>
        public class HistoryMessage
        {
            /// <summary>
            /// The timetoken when this message was published.
            /// </summary>
            public long Timetoken { get; internal set; }

            /// <summary>
            /// The message content.
            /// </summary>
            public object Entry { get; internal set; }

            /// <summary>
            /// Metadata associated with this message.
            /// </summary>
            public Dictionary<string, object> Meta { get; internal set; }

            /// <summary>
            /// Message actions associated with this message.
            /// </summary>
            public Dictionary<string, List<PNMessageActionItem>> Actions { get; internal set; }

            /// <summary>
            /// The UUID of the publisher.
            /// </summary>
            public string Uuid { get; internal set; }

            /// <summary>
            /// The message type indicator.
            /// </summary>
            public int MessageType { get; internal set; }

            /// <summary>
            /// Custom message type if specified.
            /// </summary>
            public string CustomMessageType { get; internal set; }

            /// <summary>
            /// Creates a HistoryMessage from PNHistoryItemResult.
            /// </summary>
            internal static HistoryMessage FromPNHistoryItem(PNHistoryItemResult item)
            {
                if (item == null) return null;

                return new HistoryMessage
                {
                    Timetoken = item.Timetoken,
                    Entry = item.Entry,
                    Meta = item.Meta,
                    Actions = item.ActionItems,
                    Uuid = item.Uuid,
                    MessageType = item.MessageType,
                    CustomMessageType = item.CustomMessageType
                };
            }
        }

        /// <summary>
        /// Information about pagination for fetching more messages.
        /// </summary>
        public class MoreInfo
        {
            /// <summary>
            /// The start timetoken for fetching more messages.
            /// </summary>
            public long Start { get; internal set; }

            /// <summary>
            /// The end timetoken for fetching more messages.
            /// </summary>
            public long End { get; internal set; }

            /// <summary>
            /// The maximum number of messages that were requested.
            /// </summary>
            public int Max { get; internal set; }

            /// <summary>
            /// Creates a MoreInfo from PNFetchHistoryResult.MoreInfo.
            /// </summary>
            internal static MoreInfo FromPNMoreInfo(PNFetchHistoryResult.MoreInfo info)
            {
                if (info == null) return null;

                return new MoreInfo
                {
                    Start = info.Start,
                    End = info.End,
                    Max = info.Max
                };
            }
        }

        /// <summary>
        /// Messages organized by channel name.
        /// </summary>
        public Dictionary<string, List<HistoryMessage>> Messages { get; internal set; }

        /// <summary>
        /// Pagination information for fetching additional messages.
        /// </summary>
        public MoreInfo More { get; internal set; }

        /// <summary>
        /// Indicates whether the fetch history operation was successful.
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// HTTP status code from the fetch history request.
        /// </summary>
        public int StatusCode { get; internal set; }

        /// <summary>
        /// Any error information if the fetch failed.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Creates a successful FetchHistoryResponse from PNFetchHistoryResult.
        /// </summary>
        /// <param name="result">The PNFetchHistoryResult from the internal operation</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>A successful FetchHistoryResponse</returns>
        internal static FetchHistoryResponse CreateSuccess(PNFetchHistoryResult result, int statusCode = 200)
        {
            var response = new FetchHistoryResponse
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Messages = new Dictionary<string, List<HistoryMessage>>(),
                More = MoreInfo.FromPNMoreInfo(result?.More)
            };

            // Convert messages
            if (result?.Messages != null)
            {
                foreach (var channelMessages in result.Messages)
                {
                    var convertedMessages = new List<HistoryMessage>();
                    if (channelMessages.Value != null)
                    {
                        foreach (var message in channelMessages.Value)
                        {
                            var historyMessage = HistoryMessage.FromPNHistoryItem(message);
                            if (historyMessage != null)
                            {
                                convertedMessages.Add(historyMessage);
                            }
                        }
                    }
                    response.Messages[channelMessages.Key] = convertedMessages;
                }
            }

            return response;
        }

        /// <summary>
        /// Creates an error FetchHistoryResponse.
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error FetchHistoryResponse</returns>
        internal static FetchHistoryResponse CreateError(string errorMessage, int statusCode = 400)
        {
            return new FetchHistoryResponse
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Messages = new Dictionary<string, List<HistoryMessage>>()
            };
        }
    }
}