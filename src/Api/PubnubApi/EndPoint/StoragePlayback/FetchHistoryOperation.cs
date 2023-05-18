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
    public class FetchHistoryOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private bool reverseOption;
        private bool withMetaOption;
        private bool withMessageActionsOption;
        private bool includeMessageType = true; //default to  true
        private bool withUuidOption = true; //default to  true
        private long startTimetoken = -1;
        private long endTimetoken = -1;
        private int perChannelCount = -1;
        private Dictionary<string, object> queryParam;

        private string[] channelNames;
        private PNCallback<PNFetchHistoryResult> savedCallback;

        public FetchHistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public FetchHistoryOperation Channels(string[] channelNames)
        {
            this.channelNames = channelNames;
            return this;
        }

        public FetchHistoryOperation Reverse(bool reverse)
        {
            this.reverseOption = reverse;
            return this;
        }

        public FetchHistoryOperation IncludeMeta(bool withMeta)
        {
            this.withMetaOption = withMeta;
            return this;
        }

        public FetchHistoryOperation IncludeMessageType(bool withMessageType)
        {
            includeMessageType = withMessageType;
            return this;
        }

        public FetchHistoryOperation IncludeUuid(bool withUuid)
        {
            withUuidOption = withUuid;
            return this;
        }

        public FetchHistoryOperation IncludeMessageActions(bool withMessageActions)
        {
            this.withMessageActionsOption = withMessageActions;
            return this;
        }

        public FetchHistoryOperation Start(long start)
        {
            this.startTimetoken = start;
            return this;
        }

        public FetchHistoryOperation End(long end)
        {
            this.endTimetoken = end;
            return this;
        }

        public FetchHistoryOperation MaximumPerChannel(int count)
        {
            this.perChannelCount = count;
            return this;
        }

        public FetchHistoryOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNFetchHistoryResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

            if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]))
            {
                throw new MissingMemberException("Missing channel name(s)");
            }

            if (this.withMessageActionsOption && this.channelNames != null && this.channelNames.Length > 1)
            {
                throw new NotSupportedException("Only one channel can be used along with MessageActions");
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

        public async Task<PNResult<PNFetchHistoryResult>> ExecuteAsync()
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

            if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]))
            {
                throw new MissingMemberException("Missing channel name(s)");
            }

            if (this.withMessageActionsOption && this.channelNames != null && this.channelNames.Length > 1)
            {
                throw new NotSupportedException("Only one channel can be used along with MessageActions");
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

        internal void History(PNCallback<PNFetchHistoryResult> callback)
        {
            if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]) || string.IsNullOrEmpty(this.channelNames[0].Trim()))
            {
                throw new ArgumentException("Missing Channel(s)");
            }
            string channel = string.Join(",", this.channelNames);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildFetchRequest("GET", "", this.channelNames, this.startTimetoken, this.endTimetoken, this.perChannelCount, this.reverseOption, this.withMetaOption, this.withMessageActionsOption, this.withUuidOption, this.includeMessageType, this.queryParam);

            RequestState<PNFetchHistoryResult> requestState = new RequestState<PNFetchHistoryResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNFetchHistoryOperation;
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

        internal async Task<PNResult<PNFetchHistoryResult>> History()
        {
            if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]) || string.IsNullOrEmpty(this.channelNames[0].Trim()))
            {
                throw new ArgumentException("Missing Channel(s)");
            }
            PNResult<PNFetchHistoryResult> ret = new PNResult<PNFetchHistoryResult>();
            string channel = string.Join(",", this.channelNames);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildFetchRequest("GET", "", this.channelNames, this.startTimetoken, this.endTimetoken, this.perChannelCount, this.reverseOption, this.withMetaOption, this.withMessageActionsOption, this.withUuidOption, this.includeMessageType, this.queryParam);

            RequestState<PNFetchHistoryResult> requestState = new RequestState<PNFetchHistoryResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNFetchHistoryOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNFetchHistoryResult responseResult = responseBuilder.JsonToObject<PNFetchHistoryResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }

}
