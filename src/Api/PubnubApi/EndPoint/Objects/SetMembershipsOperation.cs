using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

namespace PubnubApi.EndPoint
{
	public class SetMembershipsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string uuid = "";
		private List<PNMembership> addMembership;
		private string commandDelimitedIncludeOptions = "";
		private PNPageObject page = new PNPageObject();
		private int limit = -1;
		private bool includeCount;
		private List<string> sortField;

		private PNCallback<PNMembershipsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public SetMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public SetMembershipsOperation Uuid(string id)
		{
			this.uuid = id;
			return this;
		}

		public SetMembershipsOperation Channels(List<PNMembership> memberships)
		{
			this.addMembership = memberships;
			return this;
		}

		public SetMembershipsOperation Include(PNMembershipField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => UrlParameterConverter.MapEnumValueToEndpoint(x.ToString())).ToArray();
				this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public SetMembershipsOperation Page(PNPageObject pageObject)
		{
			this.page = pageObject;
			return this;
		}

		public SetMembershipsOperation Limit(int numberOfObjects)
		{
			this.limit = numberOfObjects;
			return this;
		}

		public SetMembershipsOperation IncludeCount(bool includeTotalCount)
		{
			this.includeCount = includeTotalCount;
			return this;
		}

		public SetMembershipsOperation Sort(List<string> sortByField)
		{
			this.sortField = sortByField;
			return this;
		}

		public SetMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNMembershipsResult> callback)
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			this.savedCallback = callback;
			SetChannelMembershipWithUuid(this.uuid, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
		}

		public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
		{
			return await SetChannelMembershipWithUuid(this.uuid, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			SetChannelMembershipWithUuid(this.uuid, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
		}

		private void SetChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
			requestState.ResponseType = PNOperationType.PNSetMembershipsOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePatchMethod = true;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetMembershipsOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNMembershipsResult>> SetChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNMembershipsResult> returnValue = new PNResult<PNMembershipsResult>();

			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
			requestState.ResponseType = PNOperationType.PNSetMembershipsOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePatchMethod = true;

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetMembershipsOperation);
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
					PNMembershipsResult responseResult = responseBuilder.JsonToObject<PNMembershipsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (addMembership != null) {
				List<Dictionary<string, object>> setMembershipFormatList = new List<Dictionary<string, object>>();
				for (int index = 0; index < addMembership.Count; index++) {
					Dictionary<string, object> currentMembershipFormat = new Dictionary<string, object>
					{
						{ "channel", new Dictionary<string, string> { { "id", addMembership[index].Channel } } }
					};
					if (addMembership[index].Custom != null) {
						currentMembershipFormat.Add("custom", addMembership[index].Custom);
					}
					setMembershipFormatList.Add(currentMembershipFormat);
				}
				if (setMembershipFormatList.Count > 0) {
					messageEnvelope.Add("set", setMembershipFormatList);
				}
			}
			string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
			byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"uuids",
				string.IsNullOrEmpty(this.uuid) ? string.Empty : uuid,
				"channels"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(page.Next)) {
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNSetMembershipsOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNSetMembershipsOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNSetMembershipsOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNSetMembershipsOperation, false, false, false));
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSetMembershipsOperation, false, false, false));
					}
				}
			}
			string queryString = UriUtil.BuildQueryString(requestQueryStringParams);

			var requestParameter = new RequestParameter() {
				RequestType = Constants.PATCH,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
				BodyContentString = patchMessage
			};
			return requestParameter;
		}
	}
}
