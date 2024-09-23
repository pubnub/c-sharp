using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
	public class RemovePushChannelOperation : PubnubCoreBase
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
		private PNCallback<PNPushRemoveChannelResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemovePushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			PubnubInstance = instance;

			if (!ChannelRequest.ContainsKey(instance.InstanceId)) {
				ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
			}
			if (!ChannelInternetStatus.ContainsKey(instance.InstanceId)) {
				ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
			}
			if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId)) {
				ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
			}
		}

		public RemovePushChannelOperation PushType(PNPushType pushType)
		{
			this.pubnubPushType = pushType;
			return this;
		}

		public RemovePushChannelOperation DeviceId(string deviceId)
		{
			this.deviceTokenId = deviceId;
			return this;
		}

		public RemovePushChannelOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only. Default = Development
		/// </summary>
		/// <param name="environment"></param>
		/// <returns></returns>
		public RemovePushChannelOperation Environment(PushEnvironment environment)
		{
			this.pushEnvironment = environment;
			return this;
		}

		/// <summary>
		/// Applies to APNS2 Only
		/// </summary>
		/// <param name="deviceTopic"></param>
		/// <returns></returns>
		public RemovePushChannelOperation Topic(string deviceTopic)
		{
			this.deviceTopic = deviceTopic;
			return this;
		}

		public RemovePushChannelOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPushRemoveChannelResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPushRemoveChannelResult> callback)
		{
			this.savedCallback = callback;
			RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, callback);
		}

		public async Task<PNResult<PNPushRemoveChannelResult>> ExecuteAsync()
		{
			return await RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam);
		}

		internal void Retry()
		{
			RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, this.pushEnvironment, this.deviceTopic, this.queryParam, savedCallback);
		}

		internal void RemoveChannelForDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam, PNCallback<PNPushRemoveChannelResult> callback)
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
			RequestState<PNPushRemoveChannelResult> requestState = new RequestState<PNPushRemoveChannelResult>();
			requestState.Channels = channels.Select(c => c).ToArray();
			requestState.ResponseType = PNOperationType.PushRemove;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushRemove);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushRemove, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNPushRemoveChannelResult), status);
				}
			});
		}

		internal async Task<PNResult<PNPushRemoveChannelResult>> RemoveChannelForDevice(string[] channels, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
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
			PNResult<PNPushRemoveChannelResult> returnValue = new PNResult<PNPushRemoveChannelResult>();
			RequestState<PNPushRemoveChannelResult> requestState = new RequestState<PNPushRemoveChannelResult>();
			requestState.Channels = channels.Select(c => c).ToArray();
			requestState.ResponseType = PNOperationType.PushRemove;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PushRemove);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
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
					PNPushRemoveChannelResult responseResult = responseBuilder.JsonToObject<PNPushRemoveChannelResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PushRemove, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
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
				requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, PNOperationType.PushRemove, false, false, false));
			} else {
				requestQueryStringParams.Add("type", pubnubPushType.ToString().ToLowerInvariant());
			}

			requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(string.Join(",", channelNames.OrderBy(x => x).ToArray()), PNOperationType.PushRemove, false, false, false));

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PushRemove, false, false, false));
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
