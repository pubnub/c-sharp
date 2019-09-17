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
    public class GetUserOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private bool includeCustom;
        private string usrId = "";

        private PNCallback<PNGetUserResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetUserOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetUserOperation UserId(string id)
        {
            this.usrId = id;
            return this;
        }

        public GetUserOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }

        public GetUserOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGetUserResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetSingleUser(this.usrId, this.includeCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GetSingleUser(this.usrId, this.includeCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GetSingleUser(this.usrId, this.includeCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GetSingleUser(this.usrId, this.includeCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void GetSingleUser(string userId, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNGetUserResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("Missing userId");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildGetSingleUserRequest("GET", "", userId, includeCustom, externalQueryParam);

            RequestState<PNGetUserResult> requestState = new RequestState<PNGetUserResult>();
            requestState.ResponseType = PNOperationType.PNGetUserOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            requestState.UsePostMethod = false;
            json = UrlProcessRequest<PNGetUserResult>(request, requestState, false);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNGetUserResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }

}
