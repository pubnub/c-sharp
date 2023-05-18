using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class AddPushChannelOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNPushType pubnubPushType;
        private string[] channelNames;
        private string deviceTokenId = "";
        private PushEnvironment pushEnvironment = PushEnvironment.Development;
        private string deviceTopic = "";
        private PNCallback<PNPushAddChannelResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public AddPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public AddPushChannelOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public AddPushChannelOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        public AddPushChannelOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        /// <summary>
        /// Applies to APNS2 Only. Default = Development
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public AddPushChannelOperation Environment(PushEnvironment environment)
        {
            this.pushEnvironment = environment;
            return this;
        }

        /// <summary>
        /// Applies to APNS2 Only
        /// </summary>
        /// <param name="deviceTopic"></param>
        /// <returns></returns>
        public AddPushChannelOperation Topic(string deviceTopic)
        {
            this.deviceTopic = deviceTopic;
            return this;
        }

        public AddPushChannelOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNPushAddChannelResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNPushAddChannelResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNPushAddChannelResult>> ExecuteAsync()
        {
            return await RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void RegisterDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushAddChannelResult> callback)
        {
            if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic))
            {
                throw new ArgumentException("Missing Topic");
            }

            string channel = string.Join(",", channels.OrderBy(x => x).ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildRegisterDevicePushRequest("GET", "", channel, pushType, pushToken, environment, deviceTopic, externalQueryParam);

            RequestState<PNPushAddChannelResult> requestState = new RequestState<PNPushAddChannelResult>();
            requestState.Channels = new [] { channel };
            requestState.ResponseType = PNOperationType.PushRegister;
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

        internal async Task<PNResult<PNPushAddChannelResult>> RegisterDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic))
            {
                throw new ArgumentException("Missing Topic");
            }
            PNResult<PNPushAddChannelResult> ret = new PNResult<PNPushAddChannelResult>();

            string channel = string.Join(",", channels.OrderBy(x => x).ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildRegisterDevicePushRequest("GET", "", channel, pushType, pushToken, environment, deviceTopic, externalQueryParam);

            RequestState<PNPushAddChannelResult> requestState = new RequestState<PNPushAddChannelResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PushRegister;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNPushAddChannelResult responseResult = responseBuilder.JsonToObject<PNPushAddChannelResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }
}
