using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class HistoryOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private bool reverseOption = false;
        private bool includeTimetokenOption = false;
        private long startTimetoken = -1;
        private long endTimetoken = -1;
        private int historyCount = -1;

        private string channelName = "";

        public HistoryOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public HistoryOperation channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public HistoryOperation reverse(bool reverse)
        {
            this.reverseOption = reverse;
            return this;
        }

        public HistoryOperation includeTimetoken(bool includeTimetoken)
        {
            this.includeTimetokenOption = includeTimetoken;
            return this;
        }

        public HistoryOperation start(long start)
        {
            this.startTimetoken = start;
            return this;
        }

        public HistoryOperation end(long end)
        {
            this.endTimetoken = end;
            return this;
        }

        public HistoryOperation count(int count)
        {
            this.historyCount = count;
            return this;
        }

        public void async(PNCallback<DetailedHistoryAck> callback)
        {
            History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, callback.result, callback.error);
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

            string json = UrlProcessRequest< DetailedHistoryAck>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<DetailedHistoryAck>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
            else
            {
                HistoryExceptionHandler(channel, errorCallback);
            }
        }

        private void HistoryExceptionHandler(string channelName, Action<PubnubClientError> errorCallback)
        {
            string message = "Operation Timeout or Network connnect error";

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, HistoryExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

            new PNCallbackService(config, jsonLibrary).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                channelName, "", errorCallback, message,
                PubnubErrorCode.DetailedHistoryOperationTimeout, null, null);
        }

    }
}
