using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
    public class GetAllUuidMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;

        private int limit = -1;
        private bool includeCount;
        private bool includeCustom;
        private string usersFilter;
        private PNPageObject page;
        private List<string> sortField;

        private PNCallback<PNGetAllUuidMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetAllUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;

            if (instance != null)
            {
                if (!ChannelRequest.ContainsKey(instance.InstanceId))
                {
                    ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
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
            this.savedCallback = callback;
            GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
        }

        public async Task<PNResult<PNGetAllUuidMetadataResult>> ExecuteAsync()
        {
            return await GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam);
        }


        internal void Retry()
        {
            GetUuidMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.usersFilter, this.sortField, this.queryParam, savedCallback);
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
            RequestState<PNGetAllUuidMetadataResult> requestState = new RequestState<PNGetAllUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNGetAllUuidMetadataOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetAllUuidMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetAllUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNGetAllUuidMetadataResult), status);
				}
			});
        }

        private async Task<PNResult<PNGetAllUuidMetadataResult>> GetUuidMetadataList(PNPageObject page, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGetAllUuidMetadataResult> returnValue = new PNResult<PNGetAllUuidMetadataResult>();

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            RequestState<PNGetAllUuidMetadataResult> requestState = new RequestState<PNGetAllUuidMetadataResult>();
            requestState.ResponseType = PNOperationType.PNGetAllUuidMetadataOperation;
            requestState.Reconnect = false;
            requestState.UsePostMethod = false;
            requestState.EndPointOperation = this;
            Tuple<string, PNStatus> JsonAndStatusTuple;

            var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetAllUuidMetadataOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNGetAllUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNGetAllUuidMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    returnValue.Result = responseResult;
                }
            }
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetAllUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"uuids"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(page.Next))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNGetAllUuidMetadataOperation, false, false, false));
            }
            if (!string.IsNullOrEmpty(page.Prev))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNGetAllUuidMetadataOperation, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (!string.IsNullOrEmpty(usersFilter))
            {
                requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(usersFilter, PNOperationType.PNGetAllUuidMetadataOperation, false, false, false));
            }
            if (sortField != null && sortField.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",",sortField.ToArray()), PNOperationType.PNGetAllUuidMetadataOperation, false, false, false));
            }

            if (queryParam != null && queryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetAllUuidMetadataOperation, false, false, false));
                    }
                }
            }
            string queryString = UriUtil.BuildQueryString(requestQueryStringParams);
            var requestParameter = new RequestParameter() {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = requestQueryStringParams
            };

            return requestParameter;
        }
    }
}
