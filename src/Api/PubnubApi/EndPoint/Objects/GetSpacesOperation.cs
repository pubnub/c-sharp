using System;
using System.Collections.Generic;
using PubnubApi.Interface;
using System.Threading;
using System.Net;
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
using System.Threading.Tasks;
#endif

namespace PubnubApi.EndPoint
{
    public class GetSpacesOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private int limit = -1;
        private bool includeCount;
        private bool includeCustom;
        private string spacesFilter;
        private PNPage page;

        private PNCallback<PNGetSpacesResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetSpacesOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetSpacesOperation Page(PNPage bookmarkPage)
        {
            this.page = bookmarkPage;
            return this;
        }

        public GetSpacesOperation Limit(int numberOfSpaces)
        {
            this.limit = numberOfSpaces;
            return this;
        }

        public GetSpacesOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public GetSpacesOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }

        public GetSpacesOperation Filter(string filterExpression)
        {
            this.spacesFilter = filterExpression;
            return this;
        }

        public GetSpacesOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGetSpacesResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetSpaceList(this.page, this.limit, this.includeCount, this.includeCustom, this.spacesFilter, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GetSpaceList(this.page, this.limit, this.includeCount, this.includeCustom, this.spacesFilter, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GetSpaceList(this.page, this.limit, this.includeCount, this.includeCustom, this.spacesFilter, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GetSpaceList(this.page, this.limit, this.includeCount, this.includeCustom, this.spacesFilter, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void GetSpaceList(PNPage page, int limit, bool includeCount, bool includeCustom, string filter, Dictionary<string, object> externalQueryParam, PNCallback<PNGetSpacesResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }
            PNPage internalPage;
            if (page == null) { internalPage = new PNPage(); }
            else { internalPage = page; }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildGetAllSpacesRequest("GET", "", internalPage.Next, internalPage.Prev, limit, includeCount, includeCustom, filter, externalQueryParam);

            RequestState<PNGetSpacesResult> requestState = new RequestState<PNGetSpacesResult>();
            requestState.ResponseType = PNOperationType.PNGetSpacesOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            requestState.UsePostMethod = false;
            json = UrlProcessRequest<PNGetSpacesResult>(request, requestState, false);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNGetSpacesResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }

}
