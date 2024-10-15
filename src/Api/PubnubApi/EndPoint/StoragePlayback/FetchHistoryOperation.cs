using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class FetchHistoryOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private bool reverseOption;
		private bool withMetaOption;
		private bool withMessageActionsOption;
		private bool includeMessageType = true; //default to  true
		private bool withUuidOption = true; //default to  true
		private long startTimetoken = -1;
		private long endTimetoken = -1;
		private int perChannelCount = -1;
		private Dictionary<string, object> queryParam;

		private string[] channelNames;
		private PNCallback<PNFetchHistoryResult> savedCallback;

		public FetchHistoryOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public FetchHistoryOperation Channels(string[] channelNames)
		{
			this.channelNames = channelNames;
			return this;
		}

		public FetchHistoryOperation Reverse(bool reverse)
		{
			this.reverseOption = reverse;
			return this;
		}

		public FetchHistoryOperation IncludeMeta(bool withMeta)
		{
			this.withMetaOption = withMeta;
			return this;
		}

		public FetchHistoryOperation IncludeMessageType(bool withMessageType)
		{
			includeMessageType = withMessageType;
			return this;
		}

		public FetchHistoryOperation IncludeUuid(bool withUuid)
		{
			withUuidOption = withUuid;
			return this;
		}

		public FetchHistoryOperation IncludeMessageActions(bool withMessageActions)
		{
			this.withMessageActionsOption = withMessageActions;
			return this;
		}

		public FetchHistoryOperation Start(long start)
		{
			this.startTimetoken = start;
			return this;
		}

		public FetchHistoryOperation End(long end)
		{
			this.endTimetoken = end;
			return this;
		}

		public FetchHistoryOperation MaximumPerChannel(int count)
		{
			this.perChannelCount = count;
			return this;
		}

		public FetchHistoryOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNFetchHistoryResult> callback)
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}

			if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0])) {
				throw new MissingMemberException("Missing channel name(s)");
			}

			if (this.withMessageActionsOption && this.channelNames != null && this.channelNames.Length > 1) {
				throw new NotSupportedException("Only one channel can be used along with MessageActions");
			}
			this.savedCallback = callback;
			History(callback);
		}

		public async Task<PNResult<PNFetchHistoryResult>> ExecuteAsync()
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}

			if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0])) {
				throw new MissingMemberException("Missing channel name(s)");
			}

			if (this.withMessageActionsOption && this.channelNames != null && this.channelNames.Length > 1) {
				throw new NotSupportedException("Only one channel can be used along with MessageActions");
			}

			return await History();
		}

		internal void Retry()
		{
			History(savedCallback);
		}

		internal void History(PNCallback<PNFetchHistoryResult> callback)
		{
			if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]) || string.IsNullOrEmpty(this.channelNames[0].Trim())) {
				throw new ArgumentException("Missing Channel(s)");
			}
			string channel = string.Join(",", this.channelNames);

			RequestState<PNFetchHistoryResult> requestState = new RequestState<PNFetchHistoryResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNFetchHistoryOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFetchHistoryOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFetchHistoryOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		internal async Task<PNResult<PNFetchHistoryResult>> History()
		{
			if (this.channelNames == null || this.channelNames.Length == 0 || string.IsNullOrEmpty(this.channelNames[0]) || string.IsNullOrEmpty(this.channelNames[0].Trim())) {
				throw new ArgumentException("Missing Channel(s)");
			}
			PNResult<PNFetchHistoryResult> returnValue = new PNResult<PNFetchHistoryResult>();
			string channel = string.Join(",", this.channelNames);
			RequestState<PNFetchHistoryResult> requestState = new RequestState<PNFetchHistoryResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNFetchHistoryOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFetchHistoryOperation);
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
					PNFetchHistoryResult responseResult = responseBuilder.JsonToObject<PNFetchHistoryResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFetchHistoryOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			string channelsString = (channelNames != null && channelNames.Length > 0) ? string.Join(",", channelNames.OrderBy(x => x).ToArray()) : "";

			List<string> pathSegments = new List<string>() {
			"v3",
			withMessageActionsOption ? "history-with-actions" : "history",
			"sub-key",
			config.SubscribeKey,
			"channel",
			channelsString
		};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>
			{
				{ "max", (this.perChannelCount <= -1) ? (withMessageActionsOption || (channelNames != null && channelNames.Length > 1) ? "25" : "100") : perChannelCount.ToString(CultureInfo.InvariantCulture) }
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

			if (withMetaOption) {
				requestQueryStringParams.Add("include_meta", "true");
			}

			if (withUuidOption) {
				requestQueryStringParams.Add("include_uuid", "true");
			}

			if (includeMessageType) {
				requestQueryStringParams.Add("include_message_type", "true");
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNFetchHistoryOperation, false, false, false));
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
