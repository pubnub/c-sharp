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
	public class RemoveMembershipsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private string uuid = string.Empty;
		private List<string> delMembership;
		private string commandDelimitedIncludeOptions = string.Empty;
		private PNPageObject page = new PNPageObject();
		private int limit = -1;
		private bool includeCount;
		private List<string> sortField;

		private PNCallback<PNMembershipsResult> savedCallback;
		private Dictionary<string, object> queryParam;
		public RemoveMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;


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

		public RemoveMembershipsOperation Uuid(string id)
		{
			this.uuid = id;
			return this;
		}

		public RemoveMembershipsOperation Channels(List<string> channelIdList)
		{
			this.delMembership = channelIdList;
			return this;
		}

		public RemoveMembershipsOperation Include(PNMembershipField[] includeOptions)
		{
			if (includeOptions != null) {
				string[] arrayInclude = includeOptions.Select(x => UrlParameterConverter.MapEnumValueToEndpoint(x.ToString())).ToArray();
				this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
			}
			return this;
		}

		public RemoveMembershipsOperation Page(PNPageObject pageObject)
		{
			this.page = pageObject;
			return this;
		}

		public RemoveMembershipsOperation Limit(int numberOfObjects)
		{
			this.limit = numberOfObjects;
			return this;
		}

		public RemoveMembershipsOperation IncludeCount(bool includeTotalCount)
		{
			this.includeCount = includeTotalCount;
			return this;
		}

		public RemoveMembershipsOperation Sort(List<string> sortByField)
		{
			this.sortField = sortByField;
			return this;
		}

		public RemoveMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
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
			this.savedCallback = callback;
			RemoveUuidMemberships(this.uuid, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
		}

		public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await RemoveUuidMemberships(this.uuid, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			RemoveUuidMemberships(this.uuid, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
		}

		private void RemoveUuidMemberships(string uuid, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>
				{
					ResponseType = PNOperationType.PNRemoveMembershipsOperation,
					PubnubCallback = callback,
					Reconnect = false,
					UsePatchMethod = true,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveMembershipsOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
						logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private async Task<PNResult<PNMembershipsResult>> RemoveUuidMemberships(string uuid, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNMembershipsResult> returnValue = new PNResult<PNMembershipsResult>();

			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>
				{
					ResponseType = PNOperationType.PNRemoveMembershipsOperation,
					Reconnect = false,
					UsePatchMethod = true,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveMembershipsOperation);
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
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
					PNMembershipsResult responseResult = responseBuilder.JsonToObject<PNMembershipsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Trace($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
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
			byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

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
				requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNRemoveMembershipsOperation, false, false, false));
			}
			if (!string.IsNullOrEmpty(page.Prev)) {
				requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNRemoveMembershipsOperation, false, false, false));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}
			if (includeCount) {
				requestQueryStringParams.Add("count", "true");
			}
			if (!string.IsNullOrEmpty(commandDelimitedIncludeOptions)) {
				requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions, PNOperationType.PNRemoveMembershipsOperation, false, false, false));
			}
			if (sortField != null && sortField.Count > 0) {
				requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()), PNOperationType.PNRemoveMembershipsOperation, false, false, false));
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNRemoveMembershipsOperation, false, false, false));
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
