using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class DeleteMessageOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private long startTimetoken = -1;
		private long endTimetoken = -1;

		private string channelName = "";
		private PNCallback<PNDeleteMessageResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public DeleteMessageOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			PubnubInstance = instance;

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

		public DeleteMessageOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public DeleteMessageOperation Start(long start)
		{
			this.startTimetoken = start;
			return this;
		}

		public DeleteMessageOperation End(long end)
		{
			this.endTimetoken = end;
			return this;
		}

		public DeleteMessageOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNDeleteMessageResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNDeleteMessageResult> callback)
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}
			this.savedCallback = callback;
			DeleteMessage(this.channelName, this.startTimetoken, this.endTimetoken, this.queryParam, callback);
		}

		public async Task<PNResult<PNDeleteMessageResult>> ExecuteAsync()
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}

			return await DeleteMessage(this.channelName, this.startTimetoken, this.endTimetoken, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			DeleteMessage(this.channelName, this.startTimetoken, this.endTimetoken, this.queryParam, savedCallback);
		}

		internal void DeleteMessage(string channel, long start, long end, Dictionary<string, object> externalQueryParam, PNCallback<PNDeleteMessageResult> callback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim())) {
				throw new ArgumentException("Missing Channel");
			}
			RequestState<PNDeleteMessageResult> requestState = new RequestState<PNDeleteMessageResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNDeleteMessageOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDeleteMessageOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDeleteMessageOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNDeleteMessageResult), status);
				}
			});
		}

		internal async Task<PNResult<PNDeleteMessageResult>> DeleteMessage(string channel, long start, long end, Dictionary<string, object> externalQueryParam)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim())) {
				throw new ArgumentException("Missing Channel");
			}
			PNResult<PNDeleteMessageResult> returnValue = new PNResult<PNDeleteMessageResult>();
			RequestState<PNDeleteMessageResult> requestState = new RequestState<PNDeleteMessageResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNDeleteMessageOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDeleteMessageOperation);
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
					PNDeleteMessageResult responseResult = responseBuilder.JsonToObject<PNDeleteMessageResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDeleteMessageOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>() {
				"v3",
				"history",
				"sub-key",
				config.SubscribeKey,
				"channel",
				channelName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (startTimetoken != -1) {
				requestQueryStringParams.Add("start", startTimetoken.ToString(CultureInfo.InvariantCulture));
			}
			if (endTimetoken != -1) {
				requestQueryStringParams.Add("end", endTimetoken.ToString(CultureInfo.InvariantCulture));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDeleteMessageOperation, false, false, false));
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
