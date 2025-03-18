using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Globalization;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class HereNowOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private string[] channelNames;
		private string[] channelGroupNames;
		private bool includeUserState;
		private bool includeChannelUUIDs = true;
		private PNCallback<PNHereNowResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public HereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

		}

		public HereNowOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		public HereNowOperation ChannelGroups(string[] channelGroups)
		{
			this.channelGroupNames = channelGroups;
			return this;
		}

		public HereNowOperation IncludeState(bool includeState)
		{
			this.includeUserState = includeState;
			return this;
		}

		public HereNowOperation IncludeUUIDs(bool includeUUIDs)
		{
			this.includeChannelUUIDs = includeUUIDs;
			return this;
		}

		public HereNowOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNHereNowResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNHereNowResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, this.queryParam, callback);
		}

		public async Task<PNResult<PNHereNowResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, this.queryParam).ConfigureAwait(false);
		}


		internal void Retry()
		{
			HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, this.queryParam, savedCallback);
		}

		internal void HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam, PNCallback<PNHereNowResult> callback)
		{
			RequestState<PNHereNowResult> requestState = new RequestState<PNHereNowResult> {
				Channels = channels,
				ChannelGroups = channelGroups,
				ResponseType = PNOperationType.PNHereNowOperation,
				Reconnect = false,
				PubnubCallback = callback,
				EndPointOperation = this
			};
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNHereNowOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHereNowOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		internal async Task<PNResult<PNHereNowResult>> HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNHereNowResult> returnValue = new PNResult<PNHereNowResult>();

			RequestState<PNHereNowResult> requestState = new RequestState<PNHereNowResult> {
				Channels = channels,
				ChannelGroups = channelGroups,
				ResponseType = PNOperationType.PNHereNowOperation,
				Reconnect = false,
				EndPointOperation = this
			};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNHereNowOperation);
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
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
					PNHereNowResult responseResult = responseBuilder.JsonToObject<PNHereNowResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHereNowOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			string channel = (channelNames != null && channelNames.Length > 0) ? string.Join(",", channelNames.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>() {
				"v2",
				"presence",
				"sub_key",
				config.SubscribeKey,
				"channel",
				channel
			};

			int disableUUID = includeChannelUUIDs ? 0 : 1;
			int userState = includeUserState ? 1 : 0;

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			string commaDelimitedchannelGroup = (channelGroupNames != null) ? string.Join(",", channelGroupNames.OrderBy(x => x).ToArray()) : "";
			if (!string.IsNullOrEmpty(commaDelimitedchannelGroup) && commaDelimitedchannelGroup.Trim().Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(commaDelimitedchannelGroup, PNOperationType.PNHereNowOperation, false, false, false));
			}

			requestQueryStringParams.Add("disable_uuids", disableUUID.ToString(CultureInfo.InvariantCulture));
			requestQueryStringParams.Add("state", userState.ToString(CultureInfo.InvariantCulture));

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNHereNowOperation, false, false, false));
					}
				}
			}
			var requestParameter = new RequestParameter {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;
		}

	}
}
