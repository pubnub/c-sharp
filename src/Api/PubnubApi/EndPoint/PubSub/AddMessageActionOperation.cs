using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

namespace PubnubApi.EndPoint
{
	public class AddMessageActionOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelName = "";
		private long messageTimetoken;
		private PNMessageAction messageAction;
		private PNCallback<PNAddMessageActionResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AddMessageActionOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			PubnubInstance = instance;
		}


		public AddMessageActionOperation Channel(string channel)
		{
			channelName = channel;
			return this;
		}

		/// <summary>
		/// The publish timetoken of a parent message
		/// </summary>
		/// <param name="timetoken"></param>
		/// <returns></returns>
		public AddMessageActionOperation MessageTimetoken(long timetoken)
		{
			messageTimetoken = timetoken;
			return this;
		}

		public AddMessageActionOperation Action(PNMessageAction messageAction)
		{
			this.messageAction = messageAction;
			return this;
		}

		public AddMessageActionOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNAddMessageActionResult> callback)
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}

			savedCallback = callback;
			Publish(channelName, messageTimetoken, messageAction, queryParam, callback);
		}

		public async Task<PNResult<PNAddMessageActionResult>> ExecuteAsync()
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			return await Publish(this.channelName, this.messageTimetoken, this.messageAction, this.queryParam).ConfigureAwait(false);
		}


		internal void Retry()
		{
			Publish(this.channelName, this.messageTimetoken, this.messageAction, this.queryParam, savedCallback);
		}

		private void Publish(string channel, long messageTimetoken, PNMessageAction messageAction, Dictionary<string, object> externalQueryParam, PNCallback<PNAddMessageActionResult> callback)
		{
			if (string.IsNullOrEmpty(channelName) || string.IsNullOrEmpty(channelName.Trim()) || messageAction == null) {
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

			RequestState<PNAddMessageActionResult> requestState = new RequestState<PNAddMessageActionResult>();
			requestState.Channels = new[] { channelName };
			requestState.ResponseType = PNOperationType.PNAddMessageActionOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = true;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameters = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameters, PNOperationType.PNPublishOperation);

			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
			if (t.Result.Error == null) {
				var responseString = Encoding.UTF8.GetString(t.Result.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json))
				{
					List<object> result = ProcessJsonResponse(requestState, json);
					ProcessResponseCallbacks(result, requestState);
				} else {
					requestState.PubnubCallback.OnResponse(default, errorStatus);
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
				requestState.PubnubCallback.OnResponse(default(PNAddMessageActionResult), status);
			}
			CleanUp();
			});
		}

		private async Task<PNResult<PNAddMessageActionResult>> Publish(string channel, long messageTimetoken, PNMessageAction messageAction, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNAddMessageActionResult> returnValue = new PNResult<PNAddMessageActionResult>();
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || messageAction == null) {
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
			RequestState<PNAddMessageActionResult> requestState = new RequestState<PNAddMessageActionResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNAddMessageActionOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = true;

			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParams = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParams, operationType: PNOperationType.PNAddMessageActionOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
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
					PNAddMessageActionResult responseResult = responseBuilder.JsonToObject<PNAddMessageActionResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				} else {
					returnValue.Status = errorStatus;
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAddMessageActionOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> urlSegments = new List<string>() {
			"v1",
			"message-actions",
			config.SubscribeKey,
			"channel",
			this.channelName,
			"message",
			messageTimetoken.ToString(CultureInfo.InvariantCulture)
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNAddMessageActionOperation, false, false, false));
					}
				}
			}

			string postMessage = jsonLibrary.SerializeToJsonString(messageAction);
			var requestParam = new RequestParameter() {
				RequestType = Constants.POST,
				PathSegment = urlSegments,
				BodyContentString = postMessage,
				Query = requestQueryStringParams
			};
			return requestParam;
		}

		private void CleanUp()
		{
			this.savedCallback = null;
		}
	}
}
