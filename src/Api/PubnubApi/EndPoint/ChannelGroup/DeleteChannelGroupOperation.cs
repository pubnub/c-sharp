using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class DeleteChannelGroupOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelGroupName = "";
		private PNCallback<PNChannelGroupsDeleteGroupResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public DeleteChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public DeleteChannelGroupOperation ChannelGroup(string channelGroup)
		{
			this.channelGroupName = channelGroup;
			return this;
		}

		public DeleteChannelGroupOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNChannelGroupsDeleteGroupResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNChannelGroupsDeleteGroupResult> callback)
		{
			this.savedCallback = callback;
			DeleteChannelGroup(this.channelGroupName, this.queryParam, callback);
		}

		public async Task<PNResult<PNChannelGroupsDeleteGroupResult>> ExecuteAsync()
		{
			return await DeleteChannelGroup(this.channelGroupName, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			DeleteChannelGroup(this.channelGroupName, this.queryParam, savedCallback);
		}

		internal void DeleteChannelGroup(string groupName, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelGroupsDeleteGroupResult> callback)
		{
			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0) {
				throw new ArgumentException("Missing groupName");
			}

			RequestState<PNChannelGroupsDeleteGroupResult> requestState = new RequestState<PNChannelGroupsDeleteGroupResult>();
			requestState.ResponseType = PNOperationType.PNRemoveGroupOperation;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new[] { groupName };
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveGroupOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveGroupOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					requestState.PubnubCallback.OnResponse(default(PNChannelGroupsDeleteGroupResult), status);
				}
			});
		}

		internal async Task<PNResult<PNChannelGroupsDeleteGroupResult>> DeleteChannelGroup(string groupName, Dictionary<string, object> externalQueryParam)
		{
			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0) {
				throw new ArgumentException("Missing groupName");
			}
			PNResult<PNChannelGroupsDeleteGroupResult> returnValue = new PNResult<PNChannelGroupsDeleteGroupResult>();
			RequestState<PNChannelGroupsDeleteGroupResult> requestState = new RequestState<PNChannelGroupsDeleteGroupResult>();
			requestState.ResponseType = PNOperationType.PNRemoveGroupOperation;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new[] { groupName };
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveGroupOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNChannelGroupsDeleteGroupResult responseResult = responseBuilder.JsonToObject<PNChannelGroupsDeleteGroupResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveGroupOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;
		}

		private RequestParameter CreateRequestParameter()
		{

			List<string> pathSegments = new List<string>
			{
					"v1",
					"channel-registration",
					"sub-key",
					config.SubscribeKey,
					"channel-group",
					channelGroupName,
					"remove"
				};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNRemoveGroupOperation, false, false, false));
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
