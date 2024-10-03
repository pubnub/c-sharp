using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class RemoveChannelMembersOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelId = string.Empty;
		private List<string> delMember;
		private string commandDelimitedIncludeOptions = string.Empty;
		private PNPageObject page;
		private int limit = -1;
		private bool includeCount;
		private List<string> sortField;

		private PNCallback<PNChannelMembersResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemoveChannelMembersOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public RemoveChannelMembersOperation Channel(string channelName)
		{
			this.channelId = channelName;
			return this;
		}

		public RemoveChannelMembersOperation Uuids(List<string> uuidList)
		{
			this.delMember = uuidList;
			return this;
		}

		public RemoveChannelMembersOperation Include(PNChannelMemberField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
				this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public RemoveChannelMembersOperation Page(PNPageObject pageObject)
		{
			this.page = pageObject;
			return this;
		}

		public RemoveChannelMembersOperation Limit(int numberOfObjects)
		{
			this.limit = numberOfObjects;
			return this;
		}

		public RemoveChannelMembersOperation IncludeCount(bool includeTotalCount)
		{
			this.includeCount = includeTotalCount;
			return this;
		}

		public RemoveChannelMembersOperation Sort(List<string> sortByField)
		{
			this.sortField = sortByField;
			return this;
		}

		public RemoveChannelMembersOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNChannelMembersResult> callback)
		{
			if (string.IsNullOrEmpty(this.channelId) || string.IsNullOrEmpty(channelId.Trim())) {
				throw new ArgumentException("Missing Channel");
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			this.savedCallback = callback;
			ProcessRemoveChannelMembersOperationRequest(this.channelId, this.delMember, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
		}

		public async Task<PNResult<PNChannelMembersResult>> ExecuteAsync()
		{
			return await ProcessRemoveChannelMembersOperationRequest(this.channelId, this.delMember, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			ProcessRemoveChannelMembersOperationRequest(this.channelId, this.delMember, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
		}

		private void ProcessRemoveChannelMembersOperationRequest(string spaceId, List<string> removeMemberList, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelMembersResult> callback)
		{
			PNPageObject internalPage;
			if (page == null) { internalPage = new PNPageObject(); } else { internalPage = page; }

			RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>();
			requestState.ResponseType = PNOperationType.PNRemoveChannelMembersOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePatchMethod = true;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveChannelMembersOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveChannelMembersOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNChannelMembersResult>> ProcessRemoveChannelMembersOperationRequest(string channel, List<string> removeMemberList, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNChannelMembersResult> returnValue = new PNResult<PNChannelMembersResult>();

			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim())) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			PNPageObject internalPage;
			if (page == null) { internalPage = new PNPageObject(); } else { internalPage = page; }

			RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>();
			requestState.ResponseType = PNOperationType.PNRemoveChannelMembersOperation;
			requestState.Reconnect = false;
			requestState.UsePatchMethod = true;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveChannelMembersOperation);
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
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveChannelMembersOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

			return returnValue;
		}

		private static string MapEnumValueToEndpoint(string enumValue)
		{
			string ret = string.Empty;
			if (enumValue.ToLowerInvariant() == "custom") {
				ret = "custom";
			} else if (enumValue.ToLowerInvariant() == "uuid") {
				ret = "uuid";
			} else if (enumValue.ToLowerInvariant() == "channel") {
				ret = "channel";
			} else if (enumValue.ToLowerInvariant() == "channel_custom") {
				ret = "channel.custom";
			} else if (enumValue.ToLowerInvariant() == "uuid_custom") {
				ret = "uuid.custom";
			}
			return ret;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (delMember != null) {
				List<Dictionary<string, Dictionary<string, string>>> removeMemberFormatList = new List<Dictionary<string, Dictionary<string, string>>>();
				for (int index = 0; index < delMember.Count; index++) {
					Dictionary<string, Dictionary<string, string>> currentMemberFormat = new Dictionary<string, Dictionary<string, string>>();
					if (!string.IsNullOrEmpty(delMember[index])) {
						currentMemberFormat.Add("uuid", new Dictionary<string, string> { { "id", delMember[index] } });
						removeMemberFormatList.Add(currentMemberFormat);
					}
				}
				if (removeMemberFormatList.Count > 0) {
					messageEnvelope.Add("delete", removeMemberFormatList);
				}
			}
			string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

			List<string> pathSegments = new List<string>
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
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNRemoveChannelMembersOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNRemoveChannelMembersOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNRemoveChannelMembersOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNRemoveChannelMembersOperation, false, false, false));
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNRemoveChannelMembersOperation, false, false, false));
					}
				}
			}

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
