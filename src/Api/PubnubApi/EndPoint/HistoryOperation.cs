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
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private bool reverseOption;
        private bool includeTimetokenOption;
        private long startTimetoken = -1;
        private long endTimetoken = -1;
        private int historyCount = -1;
        private Dictionary<string, object> queryParam;

        private string channelName = "";
        private PNCallback<PNHistoryResult> savedCallback;

        public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
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

        public HistoryOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Async(PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                History(this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void History(string channel, long start, long end, int count, bool reverse, bool includeToken, Dictionary<string, object> externalQueryParam, PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildHistoryRequest(channel, start, end, count, reverse, includeToken, externalQueryParam);

            RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
            requestState.Channels = new [] { channel };
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
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }
    }
}
