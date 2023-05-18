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
    public class SetChannelMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string chMetaId = "";
        private string chMetaName = "";
        private string chMetaDesc;
        private Dictionary<string, object> chMetaCustom;
        private bool includeCustom;

        private PNCallback<PNSetChannelMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SetChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            if (instance != null)
            {
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

        public SetChannelMetadataOperation Channel(string channelName)
        {
            this.chMetaId = channelName;
            return this;
        }

        public SetChannelMetadataOperation Name(string channelMetadataName)
        {
            this.chMetaName = channelMetadataName;
            return this;
        }

        public SetChannelMetadataOperation Description(string channelMetadataDescription)
        {
            this.chMetaDesc = channelMetadataDescription;
            return this;
        }

        public SetChannelMetadataOperation Custom(Dictionary<string, object> channelMetadataCustomObject)
        {
            this.chMetaCustom = channelMetadataCustomObject;
            return this;
        }

        public SetChannelMetadataOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }


        public SetChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNSetChannelMetadataResult> callback)
        {
            if (string.IsNullOrEmpty(chMetaId) || string.IsNullOrEmpty(chMetaId.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }


#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                SetChannelMetadata(this.chMetaId, this.includeCustom, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                SetChannelMetadata(this.chMetaId, this.includeCustom, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNSetChannelMetadataResult>> ExecuteAsync()
        {
            return await SetChannelMetadata(this.chMetaId, this.includeCustom, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                SetChannelMetadata(this.chMetaId, this.includeCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                SetChannelMetadata(this.chMetaId, this.includeCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void SetChannelMetadata(string channelMetaId, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNSetChannelMetadataResult> callback)
        {
            RequestState<PNSetChannelMetadataResult> requestState = new RequestState<PNSetChannelMetadataResult>();
            requestState.ResponseType = PNOperationType.PNSetChannelMetadataOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (this.chMetaName != null)
            {
                messageEnvelope.Add("name", chMetaName);
            }
            if (this.chMetaDesc != null)
            {
                messageEnvelope.Add("description", chMetaDesc);
            }
            if (this.chMetaCustom != null)
            {
                messageEnvelope.Add("custom", chMetaCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSetChannelMetadataRequest("PATCH", patchMessage, channelMetaId, includeCustom, externalQueryParam);

            UrlProcessRequest(request, requestState, false, patchData).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNSetChannelMetadataResult>> SetChannelMetadata(string channelMetaId, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNSetChannelMetadataResult> ret = new PNResult<PNSetChannelMetadataResult>();

            if (string.IsNullOrEmpty(channelMetaId) || string.IsNullOrEmpty(channelMetaId.Trim()))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel")) };
                ret.Status = errStatus;
                return ret;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
                ret.Status = errStatus;
                return ret;
            }

            RequestState<PNSetChannelMetadataResult> requestState = new RequestState<PNSetChannelMetadataResult>();
            requestState.ResponseType = PNOperationType.PNSetChannelMetadataOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (this.chMetaName != null)
            {
                messageEnvelope.Add("name", chMetaName);
            }
            if (this.chMetaDesc != null)
            {
                messageEnvelope.Add("description", chMetaDesc);
            }
            if (this.chMetaCustom != null)
            {
                messageEnvelope.Add("custom", chMetaCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSetChannelMetadataRequest("PATCH", patchMessage, channelMetaId, includeCustom, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, patchData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNSetChannelMetadataResult responseResult = responseBuilder.JsonToObject<PNSetChannelMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

    }
}
