using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace PubnubApi.EndPoint
{
    public class HistoryOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr;

        private bool reverseOption = false;
        private bool includeTimetokenOption = false;
        private long startTimetoken = -1;
        private long endTimetoken = -1;
        private int historyCount = -1;

        private string channelName = "";
        private PNCallback<PNHistoryResult> savedCallback = null;

        public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
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
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void History(string channel, long start, long end, int count, bool reverse, bool includeToken, PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildHistoryRequest(channel, start, end, count, reverse, includeToken);

            RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PNHistoryOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest< PNHistoryResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNHistoryResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.Add(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }
    }
}
