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

        public HistoryOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public HistoryOperation Reverse(bool reverse)
        {
            this.reverseOption = reverse;
            return this;
        }

        public HistoryOperation IncludeTimetoken(bool includeTimetoken)
        {
            this.includeTimetokenOption = includeTimetoken;
            return this;
        }

        public HistoryOperation Start(long start)
        {
            this.startTimetoken = start;
            return this;
        }

        public HistoryOperation End(long end)
        {
            this.endTimetoken = end;
            return this;
        }

        public HistoryOperation Count(int count)
        {
            this.historyCount = count;
            return this;
        }

        public void Async(PNCallback<PNHistoryResult> callback)
        {
            History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, callback);
        }


        internal void History(string channel, long start, long end, int count, bool reverse, bool includeToken, PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildHistoryRequest(channel, start, end, count, reverse, includeToken);

            RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PNHistoryOperation;
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest< PNHistoryResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNHistoryResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
