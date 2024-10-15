using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class AddPushChannelOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNPushType pubnubPushType;
		private string[] channelNames;
		private string deviceTokenId = "";
		private PushEnvironment pushEnvironment = PushEnvironment.Development;
		private string deviceTopic = "";
		private PNCallback<PNPushAddChannelResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AddPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			PubnubInstance = instance;
		}

		public AddPushChannelOperation PushType(PNPushType pushType)
		{
			this.pubnubPushType = pushType;
			return this;
		}

		public AddPushChannelOperation DeviceId(string deviceId)
		{
			this.deviceTokenId = deviceId;
			return this;
		}

		public AddPushChannelOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only. Default = Development
		/// </summary>
		/// <param name="environment"></param>
		/// <returns></returns>
		public AddPushChannelOperation Environment(PushEnvironment environment)
		{
			this.pushEnvironment = environment;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only
		/// </summary>
		/// <param name="deviceTopic"></param>
		/// <returns></returns>
		public AddPushChannelOperation Topic(string deviceTopic)
		{
			this.deviceTopic = deviceTopic;
			return this;
		}

		public AddPushChannelOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPushAddChannelResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPushAddChannelResult> callback)
		{
			this.savedCallback = callback;
			RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
		}

		public async Task<PNResult<PNPushAddChannelResult>> ExecuteAsync()
		{
			return await RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam);
		}

		internal void Retry()
		{
			RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
		}

		internal void RegisterDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushAddChannelResult> callback)
		{
			if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0) {
				throw new ArgumentException("Missing Channel");
			}

			if (pushToken == null) {
				throw new ArgumentException("Missing deviceId");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			string channel = string.Join(",", channels.OrderBy(x => x).ToArray());
			RequestState<PNPushAddChannelResult> requestState = new RequestState<PNPushAddChannelResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PushRegister;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushRegister);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushRegister, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNPushAddChannelResult), status);
				}
			});
		}

		internal async Task<PNResult<PNPushAddChannelResult>> RegisterDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
		{
			if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0) {
				throw new ArgumentException("Missing Channel");
			}

			if (pushToken == null) {
				throw new ArgumentException("Missing deviceId");
			}

			if (pushType == PNPushType.APNS2 && string.IsNullOrEmpty(deviceTopic)) {
				throw new ArgumentException("Missing Topic");
			}
			string channel = string.Join(",", channels.OrderBy(x => x).ToArray());
			PNResult<PNPushAddChannelResult> returnValue = new PNResult<PNPushAddChannelResult>();
			RequestState<PNPushAddChannelResult> requestState = new RequestState<PNPushAddChannelResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PushRegister;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushRegister);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
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
					PNPushAddChannelResult responseResult = responseBuilder.JsonToObject<PNPushAddChannelResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushRegister, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = pubnubPushType == PNPushType.APNS2 ? new List<string>() {
					"v2",
					"push",
					"sub-key",
					config.SubscribeKey,
					"devices-apns2",
					deviceTokenId
				} : new List<string>() {
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
				requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, PNOperationType.PushRegister, false, false, false));
			} else {
				requestQueryStringParams.Add("type", pubnubPushType.ToString().ToLowerInvariant());
			}
			requestQueryStringParams.Add("add", UriUtil.EncodeUriComponent(string.Join(",", channelNames.OrderBy(x => x).ToArray()), PNOperationType.PushRegister, false, false, false));

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PushRegister, false, false, false));
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
