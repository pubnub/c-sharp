using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
	public class MessageCountsOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private Dictionary<string, object> queryParam;

		private string[] channelNames;
		private long[] timetokens;
		private PNCallback<PNMessageCountResult> savedCallback;

		public MessageCountsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public MessageCountsOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		public MessageCountsOperation ChannelsTimetoken(long[] timetokens)
		{
			this.timetokens = timetokens;
			return this;
		}

		public MessageCountsOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNMessageCountResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNMessageCountResult> callback)
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}
			this.savedCallback = callback;
			MessageCounts(this.channelNames, this.timetokens, this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNMessageCountResult>> ExecuteAsync()
		{
			if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0) {
				throw new MissingMemberException("Invalid Subscribe Key");
			}

			return await MessageCounts(this.channelNames, this.timetokens, this.queryParam);
		}

		internal void Retry()
		{
			MessageCounts(this.channelNames, this.timetokens, this.queryParam, savedCallback);
		}

		internal void MessageCounts(string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam, PNCallback<PNMessageCountResult> callback)
		{
			if (channels == null || channels.Length == 0) {
				throw new ArgumentException("Missing Channel");
			}
			RequestState<PNMessageCountResult> requestState = new RequestState<PNMessageCountResult>();
			requestState.Channels = channels;
			requestState.ResponseType = PNOperationType.PNMessageCountsOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNMessageCountsOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNMessageCountsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNMessageCountResult), status);
				}
			});
		}

		internal async Task<PNResult<PNMessageCountResult>> MessageCounts(string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam)
		{
			if (channels == null || channels.Length == 0) {
				throw new ArgumentException("Missing Channel");
			}
			PNResult<PNMessageCountResult> returnValue = new PNResult<PNMessageCountResult>();
			RequestState<PNMessageCountResult> requestState = new RequestState<PNMessageCountResult>();
			requestState.Channels = channels;
			requestState.ResponseType = PNOperationType.PNMessageCountsOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNMessageCountsOperation);
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
					PNMessageCountResult responseResult = responseBuilder.JsonToObject<PNMessageCountResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNMessageCountsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
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

		private RequestParameter CreateRequestParameter()
		{
			string channelString = (channelNames != null && channelNames.Length > 0) ? string.Join(",", channelNames) : "";

			List<string> pathSegments = new List<string>() {
				"v3",
				"history",
				"sub-key",
				config.SubscribeKey,
				"message-counts"
			};

			if (!string.IsNullOrEmpty(channelString)) {
				pathSegments.Add(UriUtil.EncodeUriComponent(channelString, PNOperationType.PNMessageCountsOperation, true, false, false));
			}

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (timetokens != null && timetokens.Length > 0) {
				string tt = string.Join(",", timetokens.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
				if (timetokens.Length == 1) {
					requestQueryStringParams.Add("timetoken", tt);
				} else {
					requestQueryStringParams.Add("channelsTimetoken", UriUtil.EncodeUriComponent(tt, PNOperationType.PNMessageCountsOperation, false, false, false));
				}
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNMessageCountsOperation, false, false, false));
					}
				}
			}

			var queryString = UriUtil.BuildQueryString(requestQueryStringParams);

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}
}
