using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class GetMembershipsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string uuid = string.Empty;
		private int limit = -1;
		private bool includeCount;
		private string commandDelimitedIncludeOptions = "";
		private string membershipsFilter;
		private PNPageObject page = new PNPageObject();
		private List<string> sortField;

		private PNCallback<PNMembershipsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public GetMembershipsOperation Uuid(string id)
		{
			this.uuid = id;
			return this;
		}

		public GetMembershipsOperation Page(PNPageObject pageObject)
		{
			this.page = pageObject;
			return this;
		}

		public GetMembershipsOperation Limit(int numberOfObjects)
		{
			this.limit = numberOfObjects;
			return this;
		}

		public GetMembershipsOperation IncludeCount(bool includeTotalCount)
		{
			this.includeCount = includeTotalCount;
			return this;
		}

		public GetMembershipsOperation Include(PNMembershipField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => UrlParameterConverter.MapEnumValueToEndpoint(x.ToString())).ToArray();
				this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public GetMembershipsOperation Filter(string filterExpression)
		{
			this.membershipsFilter = filterExpression;
			return this;
		}

		public GetMembershipsOperation Sort(List<string> sortByField)
		{
			this.sortField = sortByField;
			return this;
		}

		public GetMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNMembershipsResult> callback)
		{
			logger.Trace($"{GetType().Name} Execute invoked");
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}
			this.savedCallback = callback;
			GetMembershipsList(this.uuid, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membershipsFilter, this.sortField, this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
		{
			logger.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await GetMembershipsList(this.uuid, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membershipsFilter, this.sortField, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetMembershipsList(this.uuid, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.membershipsFilter, this.sortField, this.queryParam, savedCallback);
		}

		private void GetMembershipsList(string uuid, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>
				{
					ResponseType = PNOperationType.PNGetMembershipsOperation,
					PubnubCallback = callback,
					UsePostMethod = false,
					Reconnect = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetMembershipsOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						requestState.GotJsonResponse = true;
						logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						callback.OnResponse(null, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNMembershipsResult), status);
				}
			});
		}

		private async Task<PNResult<PNMembershipsResult>> GetMembershipsList(string uuid, PNPageObject page, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNMembershipsResult> returnValue = new PNResult<PNMembershipsResult>();

			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>
				{
					ResponseType = PNOperationType.PNGetMembershipsOperation,
					UsePostMethod = false,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetMembershipsOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
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
					PNMembershipsResult responseResult = responseBuilder.JsonToObject<PNMembershipsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"uuids",
				string.IsNullOrEmpty(uuid) ? string.Empty : uuid,
				"channels"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(page.Next)) {
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNGetMembershipsOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNGetMembershipsOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNGetMembershipsOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(membershipsFilter)) {
				requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(membershipsFilter, PNOperationType.PNGetMembershipsOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNGetMembershipsOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetMembershipsOperation, false, false, false));
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
