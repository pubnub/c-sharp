using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class RevokeTokenOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string pnToken = string.Empty;
        private PNCallback<PNAccessManagerRevokeTokenResult> savedCallbackRevokeToken;
        private Dictionary<string, object> queryParam;

        public RevokeTokenOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            PubnubInstance = instance;
            
            InitializeDefaultVariableObjectStates();
        }

        public RevokeTokenOperation Token(string tokenToBeRevoked)
        {
            this.pnToken = tokenToBeRevoked;
            return this;
        }

        public RevokeTokenOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNAccessManagerRevokeTokenResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallbackRevokeToken = callback;
                RevokeAccess(callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallbackRevokeToken = callback;
                RevokeAccess(callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNAccessManagerRevokeTokenResult>> ExecuteAsync()
        {
            return await RevokeAccess().ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                RevokeAccess(savedCallbackRevokeToken);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                RevokeAccess(savedCallbackRevokeToken);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void RevokeAccess(PNCallback<PNAccessManagerRevokeTokenResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            RequestState<PNAccessManagerRevokeTokenResult> requestState = new RequestState<PNAccessManagerRevokeTokenResult>();
            requestState.ResponseType = PNOperationType.PNAccessManagerRevokeToken;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string requestMethodName = "DELETE";
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildRevokeV3AccessRequest(requestMethodName, null, pnToken, this.queryParam);

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

        internal async Task<PNResult<PNAccessManagerRevokeTokenResult>> RevokeAccess()
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            PNResult<PNAccessManagerRevokeTokenResult> ret = new PNResult<PNAccessManagerRevokeTokenResult>();

            string requestMethodName = "DELETE";
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildRevokeV3AccessRequest(requestMethodName, null, pnToken, queryParam);

            RequestState<PNAccessManagerRevokeTokenResult> requestState = new RequestState<PNAccessManagerRevokeTokenResult>();
            requestState.ResponseType = PNOperationType.PNAccessManagerRevokeToken;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                if (resultList != null && resultList.Count > 0)
                {
                    ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                    PNAccessManagerRevokeTokenResult responseResult = responseBuilder.JsonToObject<PNAccessManagerRevokeTokenResult>(resultList, true);
                    if (responseResult != null)
                    {
                        ret.Result = responseResult;
                    }
                }
            }

            return ret;
        }

    }
}
