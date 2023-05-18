using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class RemoveAllPushChannelsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNPushType pubnubPushType;
        private string deviceTokenId = "";
        private PushEnvironment pushEnvironment = PushEnvironment.Development;
        private string deviceTopic = "";
        private PNCallback<PNPushRemoveAllChannelsResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public RemoveAllPushChannelsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public RemoveAllPushChannelsOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public RemoveAllPushChannelsOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        /// <summary>
        /// Applies to APNS2 Only. Default = Development
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public RemoveAllPushChannelsOperation Environment(PushEnvironment environment)
        {
            this.pushEnvironment = environment;
            return this;
        }

        /// <summary>
        /// Applies to APNS2 Only
        /// </summary>
        /// <param name="deviceTopic"></param>
        /// <returns></returns>
        public RemoveAllPushChannelsOperation Topic(string deviceTopic)
        {
            this.deviceTopic = deviceTopic;
            return this;
        }

        public RemoveAllPushChannelsOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNPushRemoveAllChannelsResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNPushRemoveAllChannelsResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNPushRemoveAllChannelsResult>> ExecuteAsync()
        {
            return await RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void RemoveAllChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushRemoveAllChannelsResult> callback)
        {
            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic))
            {
                throw new ArgumentException("Missing Topic");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildUnregisterDevicePushRequest("GET", "", pushType, pushToken, environment, deviceTopic, externalQueryParam);

            RequestState<PNPushRemoveAllChannelsResult> requestState = new RequestState<PNPushRemoveAllChannelsResult>();
            requestState.ResponseType = PNOperationType.PushUnregister;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse<PNPushRemoveAllChannelsResult>(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        internal async Task<PNResult<PNPushRemoveAllChannelsResult>> RemoveAllChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic))
            {
                throw new ArgumentException("Missing Topic");
            }
            PNResult<PNPushRemoveAllChannelsResult> ret = new PNResult<PNPushRemoveAllChannelsResult>();

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildUnregisterDevicePushRequest("GET", "", pushType, pushToken, environment, deviceTopic, externalQueryParam);

            RequestState<PNPushRemoveAllChannelsResult> requestState = new RequestState<PNPushRemoveAllChannelsResult>();
            requestState.ResponseType = PNOperationType.PushUnregister;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNPushRemoveAllChannelsResult responseResult = responseBuilder.JsonToObject<PNPushRemoveAllChannelsResult>(resultList, true);
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
