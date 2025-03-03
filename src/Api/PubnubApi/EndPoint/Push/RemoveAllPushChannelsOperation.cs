using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class RemoveAllPushChannelsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNPushType pubnubPushType;
		private string deviceTokenId = string.Empty;
		private PushEnvironment pushEnvironment = PushEnvironment.Development;
		private string deviceTopic = string.Empty;
		private PNCallback<PNPushRemoveAllChannelsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemoveAllPushChannelsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public RemoveAllPushChannelsOperation PushType(PNPushType pushType)
		{
			this.pubnubPushType = pushType;
			return this;
		}

		public RemoveAllPushChannelsOperation DeviceId(string deviceId)
		{
			this.deviceTokenId = deviceId;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only. Default = Development
		/// </summary>
		/// <param name="environment"></param>
		/// <returns></returns>
		public RemoveAllPushChannelsOperation Environment(PushEnvironment environment)
		{
			this.pushEnvironment = environment;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only
		/// </summary>
		/// <param name="deviceTopic"></param>
		/// <returns></returns>
		public RemoveAllPushChannelsOperation Topic(string deviceTopic)
		{
			this.deviceTopic = deviceTopic;
			return this;
		}

		public RemoveAllPushChannelsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPushRemoveAllChannelsResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPushRemoveAllChannelsResult> callback)
		{
			savedCallback = callback;
			logger.Trace($"{GetType().Name} Execute invoked");
			RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
		}

		public async Task<PNResult<PNPushRemoveAllChannelsResult>> ExecuteAsync()
		{
			logger.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			RemoveAllChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
		}

		internal void RemoveAllChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushRemoveAllChannelsResult> callback)
		{
			if (pushToken == null) {
				throw new ArgumentException("Missing deviceId");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNPushRemoveAllChannelsResult> requestState = new RequestState<PNPushRemoveAllChannelsResult>
				{
					ResponseType = PNOperationType.PushUnregister,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushUnregister);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushUnregister, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		internal async Task<PNResult<PNPushRemoveAllChannelsResult>> RemoveAllChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
		{
			if (pushToken == null) {
				throw new ArgumentException("Missing deviceId");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			PNResult<PNPushRemoveAllChannelsResult> returnValue = new PNResult<PNPushRemoveAllChannelsResult>();
			RequestState<PNPushRemoveAllChannelsResult> requestState = new RequestState<PNPushRemoveAllChannelsResult>();
			requestState.ResponseType = PNOperationType.PushUnregister;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushUnregister);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				Tuple<string, PNStatus> JsonAndStatusTuple;
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNPushRemoveAllChannelsResult responseResult = responseBuilder.JsonToObject<PNPushRemoveAllChannelsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushUnregister, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = pubnubPushType == PNPushType.APNS2 ? new List<string>
				{
					"v2",
					"push",
					"sub-key",
					config.SubscribeKey,
					"devices-apns2",
					deviceTokenId,
					"remove"
				} : new List<string>
				{
					"v1",
					"push",
					"sub-key",
					config.SubscribeKey,
					"devices",
					deviceTokenId,
					"remove"
				};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (pubnubPushType == PNPushType.APNS2) {
				requestQueryStringParams.Add("environment", pushEnvironment.ToString().ToLowerInvariant());
				requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, PNOperationType.PushUnregister, false, false, false));
			} else {
				requestQueryStringParams.Add("type", pubnubPushType.ToString().ToLowerInvariant());
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PushUnregister, false, false, false));
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
