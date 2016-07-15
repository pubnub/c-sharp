using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class HistoryOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        public HistoryOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            config = pnConfig;
        }

        public HistoryOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            config = pnConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        internal void History(string channel, long start, long end, int count, bool reverse, bool includeToken, Action<DetailedHistoryAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildHistoryRequest(channel, start, end, count, reverse, includeToken);

            RequestState<DetailedHistoryAck> requestState = new RequestState<DetailedHistoryAck>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.DetailedHistory;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<DetailedHistoryAck>(request, requestState, false);
        }

    }
}
