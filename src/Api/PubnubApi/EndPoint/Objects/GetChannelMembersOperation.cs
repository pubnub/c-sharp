using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class GetChannelMembersOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelId = "";
		private int limit = -1;
		private bool includeCount;
		private string commandDelimitedIncludeOptions = "";
		private string membersFilter;
		private PNPageObject page = new PNPageObject();
		private List<string> sortField;

		private PNCallback<PNChannelMembersResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetChannelMembersOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public GetChannelMembersOperation Channel(string channelName)
		{
			channelId = channelName;
			return this;
		}

		public GetChannelMembersOperation Page(PNPageObject pageObject)
		{
			page = pageObject;
			return this;
		}

		public GetChannelMembersOperation Limit(int numberOfObjects)
		{
			limit = numberOfObjects;
			return this;
		}

		public GetChannelMembersOperation IncludeCount(bool includeTotalCount)
		{
			includeCount = includeTotalCount;
			return this;
		}

		public GetChannelMembersOperation Include(PNChannelMemberField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
				commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public GetChannelMembersOperation Filter(string filterExpression)
		{
			membersFilter = filterExpression;
			return this;
		}

		public GetChannelMembersOperation Sort(List<string> sortByField)
		{
			sortField = sortByField;
			return this;
		}

		public GetChannelMembersOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNChannelMembersResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			savedCallback = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			GetMembersList(channelId, page, limit, includeCount, commandDelimitedIncludeOptions, membersFilter, sortField, queryParam, savedCallback);
		}

		public async Task<PNResult<PNChannelMembersResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await GetMembersList(channelId, page, limit, includeCount, commandDelimitedIncludeOptions, membersFilter, sortField, queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetMembersList(channelId, page, limit, includeCount, commandDelimitedIncludeOptions, membersFilter, sortField, queryParam, savedCallback);
		}

		private void GetMembersList(string spaceId, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelMembersResult> callback)
		{
			RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>
				{
					ResponseType = PNOperationType.PNGetChannelMembersOperation,
					PubnubCallback = callback,
					Reconnect = false,
					UsePostMethod = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetChannelMembersOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(null, errorStatus);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetChannelMembersOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private async Task<PNResult<PNChannelMembersResult>> GetMembersList(string spaceId, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNChannelMembersResult> returnValue = new PNResult<PNChannelMembersResult>();
			RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>
				{
					ResponseType = PNOperationType.PNGetChannelMembersOperation,
					Reconnect = false,
					UsePostMethod = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetChannelMembersOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null&& transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
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
					PNChannelMembersResult responseResult = responseBuilder.JsonToObject<PNChannelMembersResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetChannelMembersOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private static string MapEnumValueToEndpoint(string enumValue)
		{
			string ret = "";
			if (enumValue.ToLowerInvariant() == "custom") {
				ret = "custom";
			} else if (enumValue.ToLowerInvariant() == "uuid") {
				ret = "uuid";
			} else if (enumValue.ToLowerInvariant() == "uuid_custom") {
				ret = "uuid.custom";
			}
			return ret;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegment = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"channels",
				string.IsNullOrEmpty(channelId) ? string.Empty : channelId,
				"uuids"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(page.Next)) {
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNGetChannelMembersOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNGetChannelMembersOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNGetChannelMembersOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(membersFilter)) {
				requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(membersFilter, PNOperationType.PNGetChannelMembersOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNGetChannelMembersOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetChannelMembersOperation, false, false, false));
					}
				}
			}
			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegment,
				Query = requestQueryStringParams
			};

			return requestParameter;
		}
	}

}
