using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

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
        private bool withMetaOption;
        private long startTimetoken = -1;
        private long endTimetoken = -1;
        private int historyCount = -1;
        private Dictionary<string, object> queryParam;

        private string channelName = "";
        private PNCallback<PNHistoryResult> savedCallback;

        public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

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

        public HistoryOperation Channel(string channel)
        {
            channelName = channel;
            return this;
        }

        public HistoryOperation Reverse(bool reverse)
        {
            reverseOption = reverse;
            return this;
        }

        public HistoryOperation IncludeTimetoken(bool includeTimetoken)
        {
            includeTimetokenOption = includeTimetoken;
            return this;
        }

        public HistoryOperation IncludeMeta(bool withMeta)
        {
            withMetaOption = withMeta;
            return this;
        }

        public HistoryOperation Start(long start)
        {
            startTimetoken = start;
            return this;
        }

        public HistoryOperation End(long end)
        {
            endTimetoken = end;
            return this;
        }

        public HistoryOperation Count(int count)
        {
            historyCount = count;
            return this;
        }

        public HistoryOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNHistoryResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                History(callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                History(callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNHistoryResult>> ExecuteAsync()
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

            return await History().ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                History(savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                History(savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void History(PNCallback<PNHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildHistoryRequest("GET", "", this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.withMetaOption, this.queryParam);

            RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
            requestState.Channels = new [] { this.channelName };
            requestState.ResponseType = PNOperationType.PNHistoryOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        internal async Task<PNResult<PNHistoryResult>> History()
        {
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            PNResult<PNHistoryResult> ret = new PNResult<PNHistoryResult>();

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildHistoryRequest("GET", "", this.channelName, this.startTimetoken, this.endTimetoken, this.historyCount, this.reverseOption, this.includeTimetokenOption, this.withMetaOption, this.queryParam);

            RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
            requestState.Channels = new[] { this.channelName };
            requestState.ResponseType = PNOperationType.PNHistoryOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNHistoryResult responseResult = responseBuilder.JsonToObject<PNHistoryResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }
}
