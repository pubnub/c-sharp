using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class MessageCountsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string, object> queryParam;

        private string[] channelNames;
        private long[] msgCountArrayTimetoken;
        private PNCallback<PNMessageCountResult> savedCallback;

        public MessageCountsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public MessageCountsOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public MessageCountsOperation ChannelsTimetoken(long[] timetokens)
        {
            this.msgCountArrayTimetoken = timetokens;
            return this;
        }

        public MessageCountsOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNMessageCountResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNMessageCountResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                MessageCounts(this.channelNames, this.msgCountArrayTimetoken, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                MessageCounts(this.channelNames, this.msgCountArrayTimetoken, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNMessageCountResult>> ExecuteAsync()
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }

            return await MessageCounts(this.channelNames, this.msgCountArrayTimetoken, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                MessageCounts(this.channelNames, this.msgCountArrayTimetoken, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                MessageCounts(this.channelNames, this.msgCountArrayTimetoken, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void MessageCounts(string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam, PNCallback<PNMessageCountResult> callback)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildMessageCountsRequest("GET", "", channels, timetokens, externalQueryParam);

            RequestState<PNMessageCountResult> requestState = new RequestState<PNMessageCountResult>();
            requestState.Channels = channels;
            requestState.ResponseType = PNOperationType.PNMessageCountsOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse<PNMessageCountResult>(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        internal async Task<PNResult<PNMessageCountResult>> MessageCounts(string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }
            PNResult<PNMessageCountResult> ret = new PNResult<PNMessageCountResult>();

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildMessageCountsRequest("GET", "", channels, timetokens, externalQueryParam);

            RequestState<PNMessageCountResult> requestState = new RequestState<PNMessageCountResult>();
            requestState.Channels = channels;
            requestState.ResponseType = PNOperationType.PNMessageCountsOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNMessageCountResult responseResult = responseBuilder.JsonToObject<PNMessageCountResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
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
