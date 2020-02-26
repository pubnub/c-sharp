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
    public class UpdateSpaceOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private string spcId = "";
        private string spcName = "";
        private string spcDesc = "";
        private Dictionary<string, object> spcCustom;

        private PNCallback<PNUpdateSpaceResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public UpdateSpaceOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;

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

        public UpdateSpaceOperation Id(string spaceId)
        {
            this.spcId = spaceId;
            return this;
        }

        public UpdateSpaceOperation Name(string spaceName)
        {
            this.spcName = spaceName;
            return this;
        }

        public UpdateSpaceOperation Description(string spaceDescription)
        {
            this.spcDesc = spaceDescription;
            return this;
        }

        public UpdateSpaceOperation CustomObject(Dictionary<string, object> spaceCustomObject)
        {
            this.spcCustom = spaceCustomObject;
            return this;
        }

        public UpdateSpaceOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNUpdateSpaceResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNUpdateSpaceResult>> ExecuteAsync()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            return await UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam);
#else
            return await UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam);
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                UpdateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void UpdateSpace(string spaceId, string spaceName, string spaceDescription, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNUpdateSpaceResult> callback)
        {
            if (string.IsNullOrEmpty(spaceId) || string.IsNullOrEmpty(spaceId.Trim()) || spaceName == null)
            {
                throw new ArgumentException("Missing Id or Name");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }


            RequestState<PNUpdateSpaceResult> requestState = new RequestState<PNUpdateSpaceResult>();
            requestState.ResponseType = PNOperationType.PNUpdateSpaceOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("id", spaceId);
            messageEnvelope.Add("name", spaceName);
            messageEnvelope.Add("description", spaceDescription);
            if (spaceCustom != null)
            {
                messageEnvelope.Add("custom", spaceCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildUpdateSpaceRequest("PATCH", patchMessage, spaceId, spaceCustom, externalQueryParam);

            UrlProcessRequest(request, requestState, false, patchMessage).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNUpdateSpaceResult>> UpdateSpace(string spaceId, string spaceName, string spaceDescription, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam)
        {
            if (string.IsNullOrEmpty(spaceId) || string.IsNullOrEmpty(spaceId.Trim()) || spaceName == null)
            {
                throw new ArgumentException("Missing Id or Name");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }
            PNResult<PNUpdateSpaceResult> ret = new PNResult<PNUpdateSpaceResult>();

            RequestState<PNUpdateSpaceResult> requestState = new RequestState<PNUpdateSpaceResult>();
            requestState.ResponseType = PNOperationType.PNUpdateSpaceOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("id", spaceId);
            messageEnvelope.Add("name", spaceName);
            messageEnvelope.Add("description", spaceDescription);
            if (spaceCustom != null)
            {
                messageEnvelope.Add("custom", spaceCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildUpdateSpaceRequest("PATCH", patchMessage, spaceId, spaceCustom, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, patchMessage);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNUpdateSpaceResult responseResult = responseBuilder.JsonToObject<PNUpdateSpaceResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }
}
