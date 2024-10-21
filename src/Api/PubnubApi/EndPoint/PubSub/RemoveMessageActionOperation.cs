using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

namespace PubnubApi.EndPoint
{
	public class RemoveMessageActionOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelName = "";
		private long messageTimetoken;
		private long actionTimetoken;
		private string actionUuid;
		private PNCallback<PNRemoveMessageActionResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemoveMessageActionOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			PubnubInstance = instance;
		}

		public RemoveMessageActionOperation Channel(string channelName)
		{
			this.channelName = channelName;
			return this;
		}

		/// <summary>
		/// The publish timetoken of a parent message
		/// </summary>
		/// <param name="timetoken"></param>
		/// <returns></returns>
		public RemoveMessageActionOperation MessageTimetoken(long timetoken)
		{
			messageTimetoken = timetoken;
			return this;
		}

		/// <summary>
		/// The publish timetoken of the action
		/// </summary>
		/// <param name="timetoken"></param>
		/// <returns></returns>
		public RemoveMessageActionOperation ActionTimetoken(long timetoken)
		{
			actionTimetoken = timetoken;
			return this;
		}

		public RemoveMessageActionOperation Uuid(string messageActionUuid)
		{
			actionUuid = messageActionUuid;
			return this;
		}

		public RemoveMessageActionOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNRemoveMessageActionResult> callback)
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}
			RemoveMessageAction(this.channelName, this.messageTimetoken, this.actionTimetoken, this.actionUuid, this.queryParam, callback);
		}

		public async Task<PNResult<PNRemoveMessageActionResult>> ExecuteAsync()
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			return await RemoveMessageAction(this.channelName, this.messageTimetoken, this.actionTimetoken, this.actionUuid, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			RemoveMessageAction(this.channelName, this.messageTimetoken, this.actionTimetoken, this.actionUuid, this.queryParam, savedCallback);
		}

		private void RemoveMessageAction(string channel, long messageTimetoken, long actionTimetoken, string messageActionUuid, Dictionary<string, object> externalQueryParam, PNCallback<PNRemoveMessageActionResult> callback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim())) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
				callback.OnResponse(null, status);
				return;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
				callback.OnResponse(null, status);
				return;
			}

			if (callback == null) {
				return;
			}
			RequestState<PNRemoveMessageActionResult> requestState = new RequestState<PNRemoveMessageActionResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNRemoveMessageActionOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveMessageActionOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				if (t.Result.Error == null) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveMessageActionOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					requestState.PubnubCallback.OnResponse(default(PNRemoveMessageActionResult), status);
				}
				CleanUp();
			});
		}

		private async Task<PNResult<PNRemoveMessageActionResult>> RemoveMessageAction(string channel, long messageTimetoken, long actionTimetoken, string messageActionUuid, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNRemoveMessageActionResult> returnValue = new PNResult<PNRemoveMessageActionResult>();

			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim())) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
				returnValue.Status = status;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
				returnValue.Status = status;
				return returnValue;
			}

			RequestState<PNRemoveMessageActionResult> requestState = new RequestState<PNRemoveMessageActionResult>();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNRemoveMessageActionOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveMessageActionOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);

			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
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
					PNRemoveMessageActionResult responseResult = responseBuilder.JsonToObject<PNRemoveMessageActionResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveMessageActionOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private void CleanUp()
		{
			this.savedCallback = null;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>
			{
				"v1",
				"message-actions",
				config.SubscribeKey,
				"channel",
				channelName,
				"message",
				messageTimetoken.ToString(CultureInfo.InvariantCulture),
				"action",
				actionTimetoken.ToString(CultureInfo.InvariantCulture)
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (actionUuid != null) {
				requestQueryStringParams.Add("uuid", UriUtil.EncodeUriComponent(actionUuid, PNOperationType.PNRemoveMessageActionOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNRemoveMessageActionOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.DELETE,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};

			return requestParameter;
		}
	}
}
