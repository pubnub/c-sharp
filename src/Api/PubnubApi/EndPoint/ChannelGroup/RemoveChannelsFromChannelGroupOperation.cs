using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class RemoveChannelsFromChannelGroupOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelGroupName = "";
		private string[] channelNames;
		private PNCallback<PNChannelGroupsRemoveChannelResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemoveChannelsFromChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public RemoveChannelsFromChannelGroupOperation ChannelGroup(string channelGroup)
		{
			this.channelGroupName = channelGroup;
			return this;
		}

		public RemoveChannelsFromChannelGroupOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		public RemoveChannelsFromChannelGroupOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNChannelGroupsRemoveChannelResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNChannelGroupsRemoveChannelResult> callback)
		{
			this.savedCallback = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			RemoveChannelsFromChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, callback);
		}

		public async Task<PNResult<PNChannelGroupsRemoveChannelResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await RemoveChannelsFromChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			RemoveChannelsFromChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, savedCallback);
		}

		internal void RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelGroupsRemoveChannelResult> callback)
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
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNChannelGroupsRemoveChannelResult> requestState = new RequestState<PNChannelGroupsRemoveChannelResult>
				{
					ResponseType = PNOperationType.PNRemoveChannelsFromGroupOperation,
					Channels = new string[] { },
					ChannelGroups = new[] { groupName },
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveChannelsFromGroupOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveChannelsFromGroupOperation, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNChannelGroupsRemoveChannelResult), status);
				}
			});
		}

		internal async Task<PNResult<PNChannelGroupsRemoveChannelResult>> RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
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
			
			logger?.Debug($"{GetType().Name} parameter validated.");
			PNResult<PNChannelGroupsRemoveChannelResult> returnValue = new PNResult<PNChannelGroupsRemoveChannelResult>();
			RequestState<PNChannelGroupsRemoveChannelResult> requestState = new RequestState<PNChannelGroupsRemoveChannelResult>
				{
					ResponseType = PNOperationType.PNRemoveChannelsFromGroupOperation,
					Channels = new string[] { },
					ChannelGroups = new[] { groupName },
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveChannelsFromGroupOperation);
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
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNChannelGroupsRemoveChannelResult responseResult = responseBuilder.JsonToObject<PNChannelGroupsRemoveChannelResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveChannelsFromGroupOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
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

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (channelNames.Length > 0) {
				string channelsCommaDelimited = string.Join(",", channelNames.OrderBy(x => x).ToArray());
				requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(channelsCommaDelimited, PNOperationType.PNRemoveChannelsFromGroupOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNRemoveChannelsFromGroupOperation, false, false, false));
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
