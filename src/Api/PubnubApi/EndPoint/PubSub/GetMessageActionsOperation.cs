using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
	public class GetMessageActionsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string messageActionChannelName = "";
		private long startTimetoken = -1;
		private long endTimetoken = -1;
		private int limit = -1;
		private PNCallback<PNGetMessageActionsResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetMessageActionsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public GetMessageActionsOperation Channel(string channelName)
		{
			messageActionChannelName = channelName;
			return this;
		}

		public GetMessageActionsOperation Start(long startTimetoken)
		{
			this.startTimetoken = startTimetoken;
			return this;
		}

		public GetMessageActionsOperation End(long endTimetoken)
		{
			this.endTimetoken = endTimetoken;
			return this;
		}

		public GetMessageActionsOperation Limit(int numberOfRecords)
		{
			limit = numberOfRecords;
			return this;
		}

		public GetMessageActionsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNGetMessageActionsResult> callback)
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}

			GetMessageActions(this.messageActionChannelName, this.startTimetoken, this.endTimetoken, this.limit, this.queryParam, callback);
		}

		public async Task<PNResult<PNGetMessageActionsResult>> ExecuteAsync()
		{
			if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0) {
				throw new MissingMemberException("subscribe key is required");
			}

			return await GetMessageActions(this.messageActionChannelName, this.startTimetoken, this.endTimetoken, this.limit, this.queryParam);
		}

		internal void Retry()
		{
			GetMessageActions(this.messageActionChannelName, this.startTimetoken, this.endTimetoken, this.limit, this.queryParam, savedCallback);
		}

		private void GetMessageActions(string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam, PNCallback<PNGetMessageActionsResult> callback)
		{
			if (string.IsNullOrEmpty(messageActionChannelName) || string.IsNullOrEmpty(messageActionChannelName.Trim())) {
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
			RequestState<PNGetMessageActionsResult> requestState = new RequestState<PNGetMessageActionsResult>();
			requestState.Channels = new[] { messageActionChannelName };
			requestState.ResponseType = PNOperationType.PNGetMessageActionsOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			string responseString = string.Empty;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetMessageActionsOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				if (t.Result.Error == null) {
					responseString = Encoding.UTF8.GetString(t.Result.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						ProcessResponseCallbacks(null, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					requestState.PubnubCallback.OnResponse(default(PNGetMessageActionsResult), status);
				}
			});
			CleanUp();
		}

		private async Task<PNResult<PNGetMessageActionsResult>> GetMessageActions(string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNGetMessageActionsResult> returnValue = new PNResult<PNGetMessageActionsResult>();

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
			RequestState<PNGetMessageActionsResult> requestState = new RequestState<PNGetMessageActionsResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNGetMessageActionsOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetMessageActionsOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
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
					PNGetMessageActionsResult responseResult = responseBuilder.JsonToObject<PNGetMessageActionsResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetMessageActionsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
				messageActionChannelName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (startTimetoken >= 0) {
				requestQueryStringParams.Add("start", startTimetoken.ToString(CultureInfo.InvariantCulture));
			}
			if (endTimetoken >= 0) {
				requestQueryStringParams.Add("end", endTimetoken.ToString(CultureInfo.InvariantCulture));
			}
			if (limit >= 0) {
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetMessageActionsOperation, false, false, false));
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
