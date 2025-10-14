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
		private int limit = 1000;
		private int offset;

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

		/// <summary>
		/// limit number of users details to be returned.
		/// maximum value of limit parameter is 1000.
		/// default value is 1000.
		/// Note: value can not be greater than 1000.
		/// If provided value more than 1000, It will be capped to 1000.
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		public HereNowOperation Limit(int limit)
		{
			if (limit < 1000)
			{
				this.limit = limit;
			}
			return this;
		}
		/// <summary>
		/// use this parameter to provide starting position of results
		/// for pagination purpose
		/// default value is 0.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public HereNowOperation Offset(int offset)
		{
			this.offset = offset;
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
			HereNow( callback);
		}

		public async Task<PNResult<PNHereNowResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await HereNow().ConfigureAwait(false);
		}


		internal void Retry()
		{
			HereNow( savedCallback);
		}

		private void HereNow(PNCallback<PNHereNowResult> callback)
		{
			RequestState<PNHereNowResult> requestState = new RequestState<PNHereNowResult> {
				Channels = channelNames,
				ChannelGroups = channelGroupNames,
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
						result.Add(new Dictionary<string, int>()
						{
							{"offset", offset}
						});
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
					}
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHereNowOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private async Task<PNResult<PNHereNowResult>> HereNow()
		{
			PNResult<PNHereNowResult> returnValue = new PNResult<PNHereNowResult>();

			RequestState<PNHereNowResult> requestState = new RequestState<PNHereNowResult> {
				Channels = channelNames,
				ChannelGroups = channelGroupNames,
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
				Tuple<string, PNStatus> jsonAndStatusTuple;
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = jsonAndStatusTuple.Item2;
				string json = jsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
					PNHereNowResult responseResult = responseBuilder.JsonToObject<PNHereNowResult>(resultList, true);
					if (responseResult != null) {
						if (responseResult.Channels.Count == 1)
						{
							var channelData = responseResult.Channels.FirstOrDefault().Value;
							var currentFetchedOccupancies = channelData.Occupants.Count;
							var totalChannelOccupancies = channelData.Occupancy;
							if (currentFetchedOccupancies + offset < totalChannelOccupancies)
							{
								responseResult.NextOffset = currentFetchedOccupancies + offset;
							}
						} else if (responseResult.Channels.Count > 1)
						{
							int maxOccupancy=0;
							PNHereNowChannelData maxOccupancyChannel =  new PNHereNowChannelData();

							// Find the channel data with maximum occupancy count
							// NOTE: LINQ MaxBy() not available in netstandard 2.0. Due to that this loop approach is implemented.
							// to find the channel data with maximum occupancy among the returned channels data.
							foreach (var channel in responseResult.Channels)
							{
								var hereNowChannelData = channel.Value;
								if (hereNowChannelData.Occupancy > maxOccupancy)
								{
									maxOccupancy = hereNowChannelData.Occupancy;
									maxOccupancyChannel = hereNowChannelData;
								}
							}
							var currentFetchedMaxOccupantCount = maxOccupancyChannel.Occupants.Count;
							if (currentFetchedMaxOccupantCount + offset < maxOccupancy)
							{
								responseResult.NextOffset = currentFetchedMaxOccupantCount + offset;
							}
						}
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
			if (channelNames is { Length: > 0 })
			{
				requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
				if (offset > 0)
				{
					requestQueryStringParams.Add("offset", offset.ToString(CultureInfo.InvariantCulture));
				}
			}

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
