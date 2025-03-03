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
	public class ManageMembershipsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string uuid = string.Empty;
		private List<PNMembership> addMembership;
		private List<string> delMembership;
		private string commandDelimitedIncludeOptions = string.Empty;
		private PNPageObject page = new PNPageObject();
		private int limit = -1;
		private bool includeCount;
		private List<string> sortField;

		private PNCallback<PNMembershipsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public ManageMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public ManageMembershipsOperation Uuid(string id)
		{
			uuid = id;
			return this;
		}

		public ManageMembershipsOperation Set(List<PNMembership> membership)
		{
			addMembership = membership;
			return this;
		}

		public ManageMembershipsOperation Remove(List<string> channelIdList)
		{
			delMembership = channelIdList;
			return this;
		}

		public ManageMembershipsOperation Include(PNMembershipField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => UrlParameterConverter.MapEnumValueToEndpoint(x.ToString())).ToArray();
				commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public ManageMembershipsOperation Page(PNPageObject pageObject)
		{
			page = pageObject;
			return this;
		}

		public ManageMembershipsOperation Limit(int numberOfObjects)
		{
			limit = numberOfObjects;
			return this;
		}

		public ManageMembershipsOperation IncludeCount(bool includeTotalCount)
		{
			includeCount = includeTotalCount;
			return this;
		}

		public ManageMembershipsOperation Sort(List<string> sortByField)
		{
			sortField = sortByField;
			return this;
		}

		public ManageMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNMembershipsResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			savedCallback = callback;
			ManageChannelMembershipWithUuid(uuid, addMembership, delMembership, page, limit, includeCount, commandDelimitedIncludeOptions, sortField, queryParam, callback);
		}

		public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await ManageChannelMembershipWithUuid(uuid, addMembership, delMembership, page, limit, includeCount, commandDelimitedIncludeOptions, sortField, queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			ManageChannelMembershipWithUuid(uuid, addMembership, delMembership, page, limit, includeCount, commandDelimitedIncludeOptions, sortField, queryParam, savedCallback);
		}

		private void ManageChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>
				{
					ResponseType = PNOperationType.PNManageMembershipsOperation,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this,
					UsePatchMethod = true
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNManageMembershipsOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
                        logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						callback.OnResponse(null, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNManageMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNMembershipsResult>> ManageChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
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
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
			requestState.ResponseType = PNOperationType.PNManageMembershipsOperation;
			requestState.Reconnect = false;
			requestState.UsePatchMethod = true;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNManageMembershipsOperation);
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
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNManageMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (addMembership != null) {
				List<Dictionary<string, object>> setMembershipFormatList = new List<Dictionary<string, object>>();
				for (int index = 0; index < addMembership.Count; index++) {
					Dictionary<string, object> currentMembershipFormat = new Dictionary<string, object>();
					currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", addMembership[index].Channel } });
					if (addMembership[index].Custom != null) {
						currentMembershipFormat.Add("custom", addMembership[index].Custom);
					}
					setMembershipFormatList.Add(currentMembershipFormat);
				}
				if (setMembershipFormatList.Count > 0) {
					messageEnvelope.Add("set", setMembershipFormatList);
				}
			}
			if (delMembership != null) {
				List<Dictionary<string, Dictionary<string, string>>> removeMembershipFormatList = new List<Dictionary<string, Dictionary<string, string>>>();
				for (int index = 0; index < delMembership.Count; index++) {
					Dictionary<string, Dictionary<string, string>> currentMembershipFormat = new Dictionary<string, Dictionary<string, string>>();
					if (!string.IsNullOrEmpty(delMembership[index])) {
						currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", delMembership[index] } });
						removeMembershipFormatList.Add(currentMembershipFormat);
					}
				}
				if (removeMembershipFormatList.Count > 0) {
					messageEnvelope.Add("delete", removeMembershipFormatList);
				}
			}
			string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

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
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNManageMembershipsOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNManageMembershipsOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNManageMembershipsOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNManageMembershipsOperation, false, false, false));
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNManageMembershipsOperation, false, false, false));
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
