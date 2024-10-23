using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class AuditPushChannelOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNPushType pubnubPushType;
		private string deviceTokenId = string.Empty;
		private PushEnvironment pushEnvironment = PushEnvironment.Development;
		private string deviceTopic = string.Empty;
		private PNCallback<PNPushListProvisionsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public AuditPushChannelOperation PushType(PNPushType pushType)
		{
			this.pubnubPushType = pushType;
			return this;
		}

		public AuditPushChannelOperation DeviceId(string deviceId)
		{
			this.deviceTokenId = deviceId;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only. Default = Development
		/// </summary>
		/// <param name="environment"></param>
		/// <returns></returns>
		public AuditPushChannelOperation Environment(PushEnvironment environment)
		{
			this.pushEnvironment = environment;
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
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPushListProvisionsResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPushListProvisionsResult> callback)
		{
			this.savedCallback = callback;
			GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
		}

		public async Task<PNResult<PNPushListProvisionsResult>> ExecuteAsync()
		{
			return await GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
		}

		internal void GetChannelsForDevice(PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushListProvisionsResult> callback)
		{
			if (pushToken == null) {
				throw new ArgumentException("Missing Uri");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>();
			requestState.ResponseType = PNOperationType.PushGet;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

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
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushGet, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
			RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>();
			requestState.ResponseType = PNOperationType.PushGet;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;
			PNResult<PNPushListProvisionsResult> returnValue = new PNResult<PNPushListProvisionsResult>();

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushGet);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
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
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
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
				requestQueryStringParams.Add("type", pubnubPushType.ToString().ToLowerInvariant());
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PushGet, false, false, false));
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
