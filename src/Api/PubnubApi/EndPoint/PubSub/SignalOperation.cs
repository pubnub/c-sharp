using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class SignalOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private object signalMessage;
		private string channelName = string.Empty;
		private string customMessageType;
		private PNCallback<PNPublishResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public SignalOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public SignalOperation Message(object message)
		{
			signalMessage = message;
			return this;
		}

		public SignalOperation Channel(string channelName)
		{
			this.channelName = channelName;
			return this;
		}
		
		public SignalOperation CustomMessageType(string customMessageType)
		{
			this.customMessageType = customMessageType;
			return this;
		}

		public SignalOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNPublishResult> callback)
		{
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(channelName.Trim()) || this.signalMessage == null) {
				throw new ArgumentException("Missing Channel or Message");
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				throw new MissingMemberException("Invalid publish key");
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}
			Signal(this.channelName, this.signalMessage, null, this.queryParam, callback);
		}

		public async Task<PNResult<PNPublishResult>> ExecuteAsync()
		{
			return await Signal(this.channelName, this.signalMessage, null, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			Signal(this.channelName, this.signalMessage, null, this.queryParam, savedCallback);
		}

		private void Signal(string channel, object message, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
				callback.OnResponse(null, status);
				return;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
				callback.OnResponse(null, status);
				return;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid subscribe key", new ArgumentException("Invalid subscribe key")) };
				callback.OnResponse(null, status);
				return;
			}
			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNSignalOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameters = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameters, operationType: PNOperationType.PNSignalOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest).ContinueWith(t => {
				if (t.Result.Error == null) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);

						if (result != null && result.Count >= 3)
						{
							_ = Int32.TryParse(result[0].ToString(), out var signalStatus);
							if (signalStatus == 1) {
								ProcessResponseCallbacks(result, requestState);
							} else {
								PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
								PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNSignalOperation, category, requestState, 400, new PNException(responseString));
								requestState.PubnubCallback.OnResponse(default, status);
							}
						} else {
							ProcessResponseCallbacks(result, requestState);
						}
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSignalOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNPublishResult>> Signal(string channel, object message, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNPublishResult> returnValue = new PNResult<PNPublishResult>();

			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid subscribe key", new ArgumentException("Invalid subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNSignalOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNPublishOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError<PNPublishResult>(requestState, responseString);
				Tuple<string, PNStatus> jsonAndStatusTuple;
				if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, Constants.HttpRequestSuccessStatusCode, null);
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					requestState.GotJsonResponse = true;
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString??"", errorStatus);
				}
				returnValue.Status = jsonAndStatusTuple.Item2;
				string json = jsonAndStatusTuple.Item1;

				if (!string.IsNullOrEmpty(json)) {
					List<object> result = ProcessJsonResponse(requestState, json);

					if (result is { Count: >= 3 }) {
						_ = int.TryParse(result[0].ToString(), out var publishStatus);
						if (publishStatus == 1) {
							List<object> resultList = ProcessJsonResponse(requestState, json);
							if (resultList is { Count: > 0 }) {
								ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
								PNPublishResult responseResult = responseBuilder.JsonToObject<PNPublishResult>(resultList, true);
								if (responseResult != null) {
									returnValue.Result = responseResult;
								}
							}
						}
						else
						{
							PNStatusCategory category =
								PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
							PNStatus status =
								new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(
									PNOperationType.PNPublishOperation, category, requestState, 400,
									new PNException(responseString));
							returnValue.Status = status;
						}
					}
				}
			} else {
				var statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> urlSegments =
			[
				"signal",
				config.PublishKey,
				config.SubscribeKey,
				"0",
				channelName,
				"0",
				jsonLibrary.SerializeToJsonString(this.signalMessage)
			];
			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			
			if (!string.IsNullOrEmpty(customMessageType)) {
				requestQueryStringParams.Add("custom_message_type", customMessageType);
			}
			
			if (queryParam is { Count: > 0 }) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNPublishOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = urlSegments,
				Query = requestQueryStringParams
			};

			return requestParameter;
		}
	}
}
