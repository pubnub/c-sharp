using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class GetChannelMembersOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string channelMetadataId = "";
        private int limit = -1;
        private bool includeCount;
        private string commandDelimitedIncludeOptions = "";
        private string membersFilter;
        private PNPageObject page;
        private List<string> sortField;

        private PNCallback<PNChannelMembersResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetChannelMembersOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetChannelMembersOperation Channel(string channelName)
        {
            this.channelMetadataId = channelName;
            return this;
        }

        public GetChannelMembersOperation Page(PNPageObject pageObject)
        {
            this.page = pageObject;
            return this;
        }

        public GetChannelMembersOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public GetChannelMembersOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public GetChannelMembersOperation Include(PNChannelMemberField[] includeOptions)
        {
            if (includeOptions != null)
            {
                string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
                this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
            }
            return this;
        }

        public GetChannelMembersOperation Filter(string filterExpression)
        {
            this.membersFilter = filterExpression;
            return this;
        }

        public GetChannelMembersOperation Sort(List<string> sortByField)
        {
            this.sortField = sortByField;
            return this;
        }

        public GetChannelMembersOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNChannelMembersResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetMembersList(this.channelMetadataId, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membersFilter, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GetMembersList(this.channelMetadataId, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membersFilter, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNChannelMembersResult>> ExecuteAsync()
        {
            return await GetMembersList(this.channelMetadataId, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membersFilter, this.sortField, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GetMembersList(this.channelMetadataId, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membersFilter, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GetMembersList(this.channelMetadataId, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membersFilter, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void GetMembersList(string spaceId, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelMembersResult> callback)
        {
            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildGetAllMembersRequest("GET", "", spaceId, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, filter, sort, externalQueryParam);

            RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>();
            requestState.ResponseType = PNOperationType.PNGetChannelMembersOperation;
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
                else
                {
                    if (r.Result.Item2 != null)
                    {
                        callback.OnResponse(null, r.Result.Item2);
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNChannelMembersResult>> GetMembersList(string spaceId, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNChannelMembersResult> ret = new PNResult<PNChannelMembersResult>();

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildGetAllMembersRequest("GET", "", spaceId, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, filter, sort, externalQueryParam);

            RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>();
            requestState.ResponseType = PNOperationType.PNGetChannelMembersOperation;
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
                PNChannelMembersResult responseResult = responseBuilder.JsonToObject<PNChannelMembersResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

        private static string MapEnumValueToEndpoint(string enumValue)
        {
            string ret = "";
            if (enumValue.ToLowerInvariant() == "custom")
            {
                ret = "custom";
            }
            else if (enumValue.ToLowerInvariant() == "uuid")
            {
                ret = "uuid";
            }
            else if (enumValue.ToLowerInvariant() == "uuid_custom")
            {
                ret = "uuid.custom";
            }
            return ret;
        }
    }

}
