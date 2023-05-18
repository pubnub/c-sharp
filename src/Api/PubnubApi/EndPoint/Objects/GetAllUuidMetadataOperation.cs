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
    public class GetAllUuidMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private int limit = -1;
        private bool includeCount;
        private bool includeCustom;
        private string usersFilter;
        private PNPageObject page;
        private List<string> sortField;

        private PNCallback<PNGetAllUuidMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetAllUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetAllUuidMetadataOperation Page(PNPageObject pageObject)
        {
            this.page = pageObject;
            return this;
        }

        public GetAllUuidMetadataOperation Limit(int numberOfUuids)
        {
            this.limit = numberOfUuids;
            return this;
        }
        public GetAllUuidMetadataOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public GetAllUuidMetadataOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }
        public GetAllUuidMetadataOperation Filter(string filterExpression)
        {
            this.usersFilter = filterExpression;
            return this;
        }

        public GetAllUuidMetadataOperation Sort(List<string> sortByField)
        {
            this.sortField = sortByField;
            return this;
        }

        public GetAllUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGetAllUuidMetadataResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNGetAllUuidMetadataResult>> ExecuteAsync()
        {
            return await GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam).ConfigureAwait(false);
        }


        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void GetUuidMetadataList(PNPageObject page, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNGetAllUuidMetadataResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }
            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGetAllUuidMetadataRequest("GET", "", internalPage.Next, internalPage.Prev, limit, includeCount, includeCustom, filter, sort, externalQueryParam);

            RequestState<PNGetAllUuidMetadataResult> requestState = new RequestState<PNGetAllUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNGetAllUuidMetadataOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = false;
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

        private async Task<PNResult<PNGetAllUuidMetadataResult>> GetUuidMetadataList(PNPageObject page, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGetAllUuidMetadataResult> ret = new PNResult<PNGetAllUuidMetadataResult>();

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGetAllUuidMetadataRequest("GET", "", internalPage.Next, internalPage.Prev, limit, includeCount, includeCustom, filter, sort, externalQueryParam);

            RequestState<PNGetAllUuidMetadataResult> requestState = new RequestState<PNGetAllUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNGetAllUuidMetadataOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = false;
            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNGetAllUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNGetAllUuidMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }
    }
}
