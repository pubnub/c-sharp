using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class AuditPushChannelOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private PNPushType pubnubPushType;
		private string deviceTokenId = string.Empty;
		private PushEnvironment pushEnvironment = PushEnvironment.Development;
		private string deviceTopic = string.Empty;
		private PNCallback<PNPushListProvisionsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

		}

		public AuditPushChannelOperation PushType(PNPushType pushType)
		{
			pubnubPushType = pushType;
			return this;
		}

		public AuditPushChannelOperation DeviceId(string deviceId)
		{
			deviceTokenId = deviceId;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only. Default = Development
		/// </summary>
		/// <param name="environment"></param>
		/// <returns></returns>
		public AuditPushChannelOperation Environment(PushEnvironment environment)
		{
			pushEnvironment = environment;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only
		/// </summary>
		/// <param name="deviceTopic"></param>
		/// <returns></returns>
		public AuditPushChannelOperation Topic(string deviceTopic)
		{
			this.deviceTopic = deviceTopic;
			return this;
		}

		public AuditPushChannelOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPushListProvisionsResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPushListProvisionsResult> callback)
		{
			savedCallback = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			GetChannelsForDevice(pubnubPushType, deviceTokenId, pushEnvironment, deviceTopic, queryParam, callback);
		}

		public async Task<PNResult<PNPushListProvisionsResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await GetChannelsForDevice(pubnubPushType, deviceTokenId, pushEnvironment, deviceTopic, queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetChannelsForDevice(pubnubPushType, deviceTokenId, pushEnvironment, deviceTopic, queryParam, savedCallback);
		}

		internal void GetChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushListProvisionsResult> callback)
		{
			if (pushToken == null) {
				throw new ArgumentException("Missing Uri");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>
				{
					ResponseType = PNOperationType.PushGet,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushGet);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushGet, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNPushListProvisionsResult), status);
				}
			});
		}

		internal async Task<PNResult<PNPushListProvisionsResult>> GetChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
		{
			if (pushToken == null) {
				throw new ArgumentException("Missing Uri");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>
				{
					ResponseType = PNOperationType.PushGet,
					Reconnect = false,
					EndPointOperation = this
				};
			PNResult<PNPushListProvisionsResult> returnValue = new PNResult<PNPushListProvisionsResult>();
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushGet);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				Tuple<string, PNStatus> JsonAndStatusTuple;
				if (errorStatus == null) {
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
					PNPushListProvisionsResult responseResult = responseBuilder.JsonToObject<PNPushListProvisionsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushGet, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Trace($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
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
					deviceTokenId
				} : new List<string>
				{
					"v1",
					"push",
					"sub-key",
					config.SubscribeKey,
					"devices",
					deviceTokenId
				};



			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (pubnubPushType == PNPushType.APNS2) {
				requestQueryStringParams.Add("environment", pushEnvironment.ToString().ToLowerInvariant());
				requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, PNOperationType.PushGet, false, false, false));
			} else {
				requestQueryStringParams.Add("type", pubnubPushType.ToUrlString());
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PushGet, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}
}
