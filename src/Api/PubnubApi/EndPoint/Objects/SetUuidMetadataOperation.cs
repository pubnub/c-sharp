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
    public class SetUuidMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string uuidId;
        private string uuidName;
        private string uuidEmail;
        private string uuidExternalId;
        private string uuidProfileUrl;
        private Dictionary<string, object> uuidCustom;
        private bool includeCustom;


        private PNCallback<PNSetUuidMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SetUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public SetUuidMetadataOperation Uuid(string uuid)
        {
            this.uuidId = uuid;
            return this;
        }

        public SetUuidMetadataOperation Name(string name)
        {
            this.uuidName = name;
            return this;
        }

        public SetUuidMetadataOperation Email(string email)
        {
            this.uuidEmail = email;
            return this;
        }


        public SetUuidMetadataOperation ExternalId(string externalId)
        {
            this.uuidExternalId = externalId;
            return this;
        }


        public SetUuidMetadataOperation ProfileUrl(string profileUrl)
        {
            this.uuidProfileUrl = profileUrl;
            return this;
        }

        public SetUuidMetadataOperation Custom(Dictionary<string, object> customObject)
        {
            this.uuidCustom = customObject;
            return this;
        }

        public SetUuidMetadataOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }

        public SetUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNSetUuidMetadataResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNSetUuidMetadataResult>> ExecuteAsync()
        {
            return await SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void SetUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNSetUuidMetadataResult> callback)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            RequestState<PNSetUuidMetadataResult> requestState = new RequestState<PNSetUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNSetUuidMetadataOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (uuidName != null)
            {
                messageEnvelope.Add("name", uuidName);
            }
            if (uuidExternalId != null)
            {
                messageEnvelope.Add("externalId", uuidExternalId);
            }
            if (uuidProfileUrl != null)
            {
                messageEnvelope.Add("profileUrl", uuidProfileUrl);
            }
            if (uuidEmail != null)
            {
                messageEnvelope.Add("email", uuidEmail);
            }
            if (uuidCustom != null)
            {
                messageEnvelope.Add("custom", uuidCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildSetUuidMetadataRequest("PATCH", patchMessage, uuid, includeCustom, externalQueryParam);

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

        private async Task<PNResult<PNSetUuidMetadataResult>> SetUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }
            PNResult<PNSetUuidMetadataResult> ret = new PNResult<PNSetUuidMetadataResult>();

            RequestState<PNSetUuidMetadataResult> requestState = new RequestState<PNSetUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNSetUuidMetadataOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("name", uuidName);
            if (uuidExternalId != null)
            {
                messageEnvelope.Add("externalId", uuidExternalId);
            }
            if (uuidProfileUrl != null)
            {
                messageEnvelope.Add("profileUrl", uuidProfileUrl);
            }
            if (uuidEmail != null)
            {
                messageEnvelope.Add("email", uuidEmail);
            }
            if (uuidCustom != null)
            {
                messageEnvelope.Add("custom", uuidCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildSetUuidMetadataRequest("PATCH", patchMessage, uuid, includeCustom, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, patchData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNSetUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNSetUuidMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }
}
