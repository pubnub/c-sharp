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
    public class GetMessageActionsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string messageActionChannelName = "";
        private long startTT = -1;
        private long endTT = -1;
        private int limitRecords = -1;
        private PNCallback<PNGetMessageActionsResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetMessageActionsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetMessageActionsOperation Channel(string channelName)
        {
            messageActionChannelName = channelName;
            return this;
        }

        public GetMessageActionsOperation Start(long startTimetoken)
        {
            startTT = startTimetoken;
            return this;
        }

        public GetMessageActionsOperation End(long endTimetoken)
        {
            endTT = endTimetoken;
            return this;
        }

        public GetMessageActionsOperation Limit(int numberOfRecords)
        {
            limitRecords = numberOfRecords;
            return this;
        }

        public GetMessageActionsOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGetMessageActionsResult> callback)
        {
            if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0)
            {
                throw new MissingMemberException("subscribe key is required");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetMessageActions(this.messageActionChannelName, this.startTT, this.endTT, this.limitRecords, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GetMessageActions(this.messageActionChannelName, this.startTT, this.endTT, this.limitRecords, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNGetMessageActionsResult>> ExecuteAsync()
        {
            if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0)
            {
                throw new MissingMemberException("subscribe key is required");
            }

            return await GetMessageActions(this.messageActionChannelName, this.startTT, this.endTT, this.limitRecords, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GetMessageActions(this.messageActionChannelName, this.startTT, this.endTT, this.limitRecords, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GetMessageActions(this.messageActionChannelName, this.startTT, this.endTT, this.limitRecords, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void GetMessageActions(string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam, PNCallback<PNGetMessageActionsResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
                callback.OnResponse(null, status);
                return;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
                callback.OnResponse(null, status);
                return;
            }

            if (callback == null)
            {
                return;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildGetMessageActionsRequest("GET", "", channel, start, end, limit, externalQueryParam);

            RequestState<PNGetMessageActionsResult> requestState = new RequestState<PNGetMessageActionsResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNGetMessageActionsOperation;
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

                CleanUp();
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNGetMessageActionsResult>> GetMessageActions(string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGetMessageActionsResult> ret = new PNResult<PNGetMessageActionsResult>();

            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
                ret.Status = status;
                return ret;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
                ret.Status = status;
                return ret;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildGetMessageActionsRequest("GET", "", channel, start, end, limit, externalQueryParam);

            RequestState<PNGetMessageActionsResult> requestState = new RequestState<PNGetMessageActionsResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNGetMessageActionsOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNGetMessageActionsResult responseResult = responseBuilder.JsonToObject<PNGetMessageActionsResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

        private void CleanUp()
        {
            this.savedCallback = null;
        }
    }
}
