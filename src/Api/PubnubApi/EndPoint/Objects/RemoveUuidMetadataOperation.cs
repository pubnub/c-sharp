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
    public class RemoveUuidMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string usrUuid = "";

        private PNCallback<PNRemoveUuidMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public RemoveUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public RemoveUuidMetadataOperation Uuid(string uuid)
        {
            this.usrUuid = uuid;
            return this;
        }

        public RemoveUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNRemoveUuidMetadataResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid Subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                RemoveUuidMetadata(this.usrUuid, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                RemoveUuidMetadata(this.usrUuid, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }


        public async Task<PNResult<PNRemoveUuidMetadataResult>> ExecuteAsync()
        {
            return await RemoveUuidMetadata(this.usrUuid, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                RemoveUuidMetadata(this.usrUuid, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                RemoveUuidMetadata(this.usrUuid, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void RemoveUuidMetadata(string uuid, Dictionary<string, object> externalQueryParam, PNCallback<PNRemoveUuidMetadataResult> callback)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildDeleteUuidMetadataRequest("DELETE", "", uuid, externalQueryParam);

            RequestState<PNRemoveUuidMetadataResult> requestState = new RequestState<PNRemoveUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNDeleteUuidMetadataOperation;
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


        private async Task<PNResult<PNRemoveUuidMetadataResult>> RemoveUuidMetadata(string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNRemoveUuidMetadataResult> ret = new PNResult<PNRemoveUuidMetadataResult>();

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
                ret.Status = errStatus;
                return ret;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildDeleteUuidMetadataRequest("DELETE", "", uuid, externalQueryParam);

            RequestState<PNRemoveUuidMetadataResult> requestState = new RequestState<PNRemoveUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNDeleteUuidMetadataOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNRemoveUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNRemoveUuidMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;

        }
    }
}
