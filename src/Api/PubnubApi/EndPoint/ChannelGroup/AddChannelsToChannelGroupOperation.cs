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
	public class AddChannelsToChannelGroupOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelGroupName = "";
		private string[] channelNames;
		private PNCallback<PNChannelGroupsAddChannelResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public AddChannelsToChannelGroupOperation ChannelGroup(string channelGroup)
		{
			this.channelGroupName = channelGroup;
			return this;
		}

		public AddChannelsToChannelGroupOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		public AddChannelsToChannelGroupOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNChannelGroupsAddChannelResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNChannelGroupsAddChannelResult> callback)
		{
			this.savedCallback = callback;
			AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, callback);
		}

		public async Task<PNResult<PNChannelGroupsAddChannelResult>> ExecuteAsync()
		{
			return await AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, savedCallback);
		}

		internal void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelGroupsAddChannelResult> callback)
		{
			if (channels == null || channels.Length == 0) {
				throw new ArgumentException("Missing channel(s)");
			}

			if (nameSpace == null) {
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0) {
				throw new ArgumentException("Missing groupName");
			}
			RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
			requestState.ResponseType = PNOperationType.PNAddChannelsToGroupOperation;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new[] { groupName };
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAddChannelsToGroupOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAddChannelsToGroupOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					requestState.PubnubCallback.OnResponse(default(PNChannelGroupsAddChannelResult), status);
				}
			});
		}

		internal async Task<PNResult<PNChannelGroupsAddChannelResult>> AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
		{
			if (channels == null || channels.Length == 0) {
				throw new ArgumentException("Missing channel(s)");
			}

			if (nameSpace == null) {
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0) {
				throw new ArgumentException("Missing groupName");
			}
			PNResult<PNChannelGroupsAddChannelResult> returnValue = new PNResult<PNChannelGroupsAddChannelResult>();
			RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
			requestState.ResponseType = PNOperationType.PNAddChannelsToGroupOperation;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new[] { groupName };
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAddChannelsToGroupOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNChannelGroupsAddChannelResult responseResult = responseBuilder.JsonToObject<PNChannelGroupsAddChannelResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerRevokeToken, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
			List<string> pathSegments = new List<string>
			{
				"v1",
				"channel-registration",
				"sub-key",
				config.SubscribeKey,
				"channel-group",
				channelGroupName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>
			{
				{ "add", UriUtil.EncodeUriComponent(string.Join(",", channelNames.OrderBy(x => x).ToArray()), PNOperationType.PNAddChannelsToGroupOperation, false, false, false) }
			};

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNAddChannelsToGroupOperation, false, false, false));
					}
				}
			}

			string queryString = UriUtil.BuildQueryString(requestQueryStringParams);

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}
}
