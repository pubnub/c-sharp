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
	public class HistoryOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private bool reverseOption;
		private bool includeTimetokenOption;
		private bool withMetaOption;
		private long startTimetoken = -1;
		private long endTimetoken = -1;
		private int historyCount = -1;
		private Dictionary<string, object> queryParam;

		private string channelName = "";
		private PNCallback<PNHistoryResult> savedCallback;

		public HistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;


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

		public HistoryOperation Channel(string channel)
		{
			channelName = channel;
			return this;
		}

		public HistoryOperation Reverse(bool reverse)
		{
			reverseOption = reverse;
			return this;
		}

		public HistoryOperation IncludeTimetoken(bool includeTimetoken)
		{
			includeTimetokenOption = includeTimetoken;
			return this;
		}

		public HistoryOperation IncludeMeta(bool withMeta)
		{
			withMetaOption = withMeta;
			return this;
		}

		public HistoryOperation Start(long start)
		{
			startTimetoken = start;
			return this;
		}

		public HistoryOperation End(long end)
		{
			endTimetoken = end;
			return this;
		}

		public HistoryOperation Count(int count)
		{
			historyCount = count;
			return this;
		}

		public HistoryOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNHistoryResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNHistoryResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}
			this.savedCallback = callback;
			History(callback);
		}

		public async Task<PNResult<PNHistoryResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}

			return await History().ConfigureAwait(false);
		}

		internal void Retry()
		{
			History(savedCallback);
		}

		internal void History(PNCallback<PNHistoryResult> callback)
		{
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim())) {
				throw new ArgumentException("Missing Channel");
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>();
			requestState.Channels = new[] { this.channelName };
			requestState.ResponseType = PNOperationType.PNHistoryOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParemeter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNHistoryOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHistoryOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Trace($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNHistoryResult), status);
				}
			});
		}

		internal async Task<PNResult<PNHistoryResult>> History()
		{
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim())) {
				throw new ArgumentException("Missing Channel");
			}
			logger?.Trace($"{GetType().Name} parameter validated.");
			RequestState<PNHistoryResult> requestState = new RequestState<PNHistoryResult>
			{
				Channels = new[] { this.channelName },
				ResponseType = PNOperationType.PNHistoryOperation,
				Reconnect = false,
				EndPointOperation = this
			};

			PNResult<PNHistoryResult> returnValue = new PNResult<PNHistoryResult>();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParemeter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNHistoryOperation);
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
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
					PNHistoryResult responseResult = responseBuilder.JsonToObject<PNHistoryResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHistoryOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Trace($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParemeter()
		{
			List<string> pathSegments = new List<string>()
			{
				"v2",
				"history",
				"sub-key",
				config.SubscribeKey,
				"channel",
				channelName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>
			{
				{ "count", (historyCount <= -1) ? "100" : historyCount.ToString(CultureInfo.InvariantCulture) }
			};

			if (reverseOption) {
				requestQueryStringParams.Add("reverse", "true");
			}
			if (startTimetoken != -1) {
				requestQueryStringParams.Add("start", startTimetoken.ToString(CultureInfo.InvariantCulture));
			}
			if (endTimetoken != -1) {
				requestQueryStringParams.Add("end", endTimetoken.ToString(CultureInfo.InvariantCulture));
			}

			if (includeTimetokenOption) {
				requestQueryStringParams.Add("include_token", "true");
			}

			if (withMetaOption) {
				requestQueryStringParams.Add("include_meta", "true");
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNHistoryOperation, false, false, false));
					}
				}
			}

			var requestParemeter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParemeter;
		}
	}
}
