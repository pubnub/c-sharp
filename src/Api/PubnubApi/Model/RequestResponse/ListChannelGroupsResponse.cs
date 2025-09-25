using System;
using System.Collections.Generic;

namespace PubnubApi
{
    /// <summary>
    /// Response object for listing all channel groups
    /// </summary>
    public class ListChannelGroupsResponse
    {
        /// <summary>
        /// The list of channel groups
        /// </summary>
        public List<string> Groups { get; }

        /// <summary>
        /// The exception if the operation failed
        /// </summary>
        public Exception Exception { get; }

        private ListChannelGroupsResponse(List<string> groups, Exception exception = null)
        {
            Groups = groups;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        internal static ListChannelGroupsResponse CreateSuccess(PNChannelGroupsListAllResult result)
        {
            return new ListChannelGroupsResponse(result?.Groups ?? new List<string>(), null);
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        internal static ListChannelGroupsResponse CreateFailure(Exception exception)
        {
            return new ListChannelGroupsResponse(new List<string>(), exception);
        }
    }
}