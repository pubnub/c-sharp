using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class GetAllChannelMetadataOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private int limit = -1;
		private bool includeCount;
		private bool includeCustom;
		private string channelsFilter;
		private PNPageObject page = new PNPageObject();
		private List<string> sortField;

		private PNCallback<PNGetAllChannelMetadataResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetAllChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			if (instance != null) {
				if (!ChannelRequest.ContainsKey(instance.InstanceId)) {
					ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
				}
				if (!ChannelInternetStatus.ContainsKey(instance.InstanceId)) {
					ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
				}
				if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId)) {
					ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
				}
			}
		}

		public GetAllChannelMetadataOperation Page(PNPageObject pageObject)
		{
			this.page = pageObject;
			return this;
		}

		public GetAllChannelMetadataOperation Limit(int numberOfChannels)
		{
			this.limit = numberOfChannels;
			return this;
		}

		public GetAllChannelMetadataOperation IncludeCount(bool includeTotalCount)
		{
			this.includeCount = includeTotalCount;
			return this;
		}

		public GetAllChannelMetadataOperation IncludeCustom(bool includeCustomData)
		{
			this.includeCustom = includeCustomData;
			return this;
		}

		public GetAllChannelMetadataOperation Filter(string filterExpression)
		{
			this.channelsFilter = filterExpression;
			return this;
		}

		public GetAllChannelMetadataOperation Sort(List<string> sortByField)
		{
			this.sortField = sortByField;
			return this;
		}

		public GetAllChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNGetAllChannelMetadataResult> callback)
		{
			this.savedCallback = callback;
			GetAllChannelMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.channelsFilter, this.sortField, this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNGetAllChannelMetadataResult>> ExecuteAsync()
		{
			return await GetAllChannelMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.channelsFilter, this.sortField, this.queryParam);
		}

		internal void Retry()
		{
			GetAllChannelMetadataList(this.page, this.limit, this.includeCount, this.includeCustom, this.channelsFilter, this.sortField, this.queryParam, savedCallback);
		}

		private void GetAllChannelMetadataList(PNPageObject page, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNGetAllChannelMetadataResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			RequestState<PNGetAllChannelMetadataResult> requestState = new RequestState<PNGetAllChannelMetadataResult>();
			requestState.ResponseType = PNOperationType.PNGetAllChannelMetadataOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = false;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetAllChannelMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetAllChannelMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNGetAllChannelMetadataResult>> GetAllChannelMetadataList(PNPageObject page, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNGetAllChannelMetadataResult> returnValue = new PNResult<PNGetAllChannelMetadataResult>();
			RequestState<PNGetAllChannelMetadataResult> requestState = new RequestState<PNGetAllChannelMetadataResult>();
			requestState.ResponseType = PNOperationType.PNGetAllChannelMetadataOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = false;
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetAllChannelMetadataOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNGetAllChannelMetadataResult responseResult = responseBuilder.JsonToObject<PNGetAllChannelMetadataResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetAllChannelMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
				"channels"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(page?.Next)) {
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page?.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (includeCustom) {
				requestQueryStringParams.Add("include", "custom");
			}
			if (!string.IsNullOrEmpty(channelsFilter)) {
				requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(channelsFilter, PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}

}
