using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EndPoint
{
	internal class SubscribeManager2 : IDisposable
	{
		private PNConfiguration config;
		private IJsonPluggableLibrary jsonLibrary;
		private IPubnubUnitTest unit;
		private Pubnub pubnubInstance;
		private CancellationTokenSource cancellationTokenSource;
		private Timer SubscribeHeartbeatCheckTimer;
		private PubnubLogModule logger;
		public SubscribeManager2(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubInstance = instance;
			logger = pubnubConfig.Logger;
		}

		public async Task<Tuple<HandshakeResponse, PNStatus>> HandshakeRequest(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			string presenceState = string.Empty;
			if (config.MaintainPresenceState) presenceState = BuildJsonUserState(channels, channelGroups, true);
			var requestParameter = CreateSubscribeRequestParameter(channels: channels, channelGroups: channelGroups, timetoken: timetoken.GetValueOrDefault(), region: region.GetValueOrDefault(), stateJsonValue: presenceState, initialSubscribeUrlParams: initialSubscribeUrlParams, externalQueryParam: externalQueryParam);
			var transportRequest = pubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSubscribeOperation);
			cancellationTokenSource = transportRequest.CancellationTokenSource;
			RequestState<HandshakeResponse> pubnubRequestState = new RequestState<HandshakeResponse>
			{
				Channels = channels,
				ChannelGroups = channelGroups,
				ResponseType = responseType,
				Timetoken = timetoken.GetValueOrDefault(),
				Region = region.GetValueOrDefault(),
				TimeQueued = DateTime.Now
			};
			var transportResponse = await pubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);

			if (transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode && transportResponse.Error == null && transportResponse.Content != null) {
				var responseJson = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, channels, channelGroups);
				HandshakeResponse handshakeResponse = jsonLibrary.DeserializeToObject<HandshakeResponse>(responseJson);
				return new Tuple<HandshakeResponse, PNStatus>(handshakeResponse, status);
			}

			PNStatus errStatus;
			if (transportResponse.Error != null) {
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(transportResponse.Error);
				errStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(pubnubRequestState.ResponseType, category, pubnubRequestState, Constants.ResourceNotFoundStatusCode, new PNException(transportResponse.Error));
			} else {
				string responseString = string.Empty;
				if (transportResponse.Content != null) responseString = Encoding.UTF8.GetString(transportResponse.Content);
				errStatus = GetStatusIfError(pubnubRequestState, responseString);
			}
			return new Tuple<HandshakeResponse, PNStatus>(null, errStatus);
		}

		internal void HandshakeRequestCancellation()
		{
			try {
				if (cancellationTokenSource != null) {
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				} else
				{
					logger?.Trace($" SubscribeManager  HandshakeRequestCancellation. No request to cancel.");
				}

				logger?.Trace($"SubscribeManager  HandshakeRequestCancellation. Done.");
			} catch (Exception ex)
			{
				logger?.Trace($" SubscribeManager  HandshakeRequestCancellation Exception: {ex}");
			}
		}
		internal async Task<Tuple<ReceivingResponse<object>, PNStatus>> ReceiveRequest<T>(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			Tuple<ReceivingResponse<object>, PNStatus> resp = new Tuple<ReceivingResponse<object>, PNStatus>(null, null);
			try {
				string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
				var requestParameter = CreateSubscribeRequestParameter(channels: channels, channelGroups: channelGroups, timetoken: timetoken.GetValueOrDefault(), region: region.GetValueOrDefault(), stateJsonValue: channelsJsonState, initialSubscribeUrlParams: initialSubscribeUrlParams, externalQueryParam: externalQueryParam);
				var transportRequest = pubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSubscribeOperation);
				cancellationTokenSource = transportRequest.CancellationTokenSource;
				RequestState<ReceivingResponse<object>> pubnubRequestState = new RequestState<ReceivingResponse<object>>
					{
						Channels = channels,
						ChannelGroups = channelGroups,
						ResponseType = responseType,
						Timetoken = timetoken.GetValueOrDefault(),
						Region = region.GetValueOrDefault(),
						TimeQueued = DateTime.Now
					};

				var transportResponse = await pubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
				if (transportResponse.Content != null && transportResponse.Error == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					var responseJson = Encoding.UTF8.GetString(transportResponse.Content);
					PNStatus status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, channels, channelGroups);
					ReceivingResponse<object> receiveResponse = jsonLibrary.DeserializeToObject<ReceivingResponse<object>>(responseJson);
					return new Tuple<ReceivingResponse<object>, PNStatus>(receiveResponse, status);
				}
				PNStatus errStatus;
				if (transportResponse.Error != null) {
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(transportResponse.Error);
					errStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(pubnubRequestState.ResponseType, category, pubnubRequestState, Constants.ResourceNotFoundStatusCode, new PNException(transportResponse.Error));
				} else {
					string responseString = string.Empty;
					if (transportResponse.Content != null) responseString = Encoding.UTF8.GetString(transportResponse.Content);
					errStatus = GetStatusIfError(pubnubRequestState, responseString);
				}
				return new Tuple<ReceivingResponse<object>, PNStatus>(null, errStatus);
			} catch (Exception ex)
			{
				logger?.Error(
					$" SubscribeManager=> MultiChannelSubscribeInit \n channel(s)={string.Join(",", channels.OrderBy(x => x).ToArray())} \n cg(s)={string.Join(",", channelGroups.OrderBy(x => x).ToArray())} \n Exception Details={ex}");
			}
			return resp;
		}

		internal void ReceiveRequestCancellation()
		{
			try {
				if (cancellationTokenSource != null) {
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				} else {
					logger?.Trace($"SubscribeManager  RequestCancellation. No request to cancel.");
				}
				logger?.Trace($"SubscribeManager  ReceiveRequestCancellation. Done.");
			} catch (Exception ex)
			{
				logger?.Trace($"SubscribeManager  ReceiveRequestCancellation Exception: {ex}");
			}
		}

		internal void ReceiveReconnectRequestCancellation()
		{
			try {
				if (cancellationTokenSource != null) {
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				} else {
					logger?.Trace($"SubscribeManager  ReceiveReconnectRequestCancellation. No request to cancel.");
				}
				logger?.Trace($"SubscribeManager  ReceiveReconnectRequestCancellation. Done.");
			} catch (Exception ex) {
				logger?.Trace($"SubscribeManager  ReceiveReconnectRequestCancellation Exception: {ex}");
			}
		}

		protected string BuildJsonUserState(string channel, string channelGroup, bool local)
		{
			Dictionary<string, object> channelUserStateDictionary = null;
			Dictionary<string, object> channelGroupUserStateDictionary = null;

			if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(channelGroup)) {
				throw new ArgumentException("BuildJsonUserState takes either channel or channelGroup at one time. Send one at a time by passing empty value for other.");
			}

			StringBuilder jsonStateBuilder = new StringBuilder();

			if (channelUserStateDictionary != null) {
				string[] channelUserStateKeys = channelUserStateDictionary.Keys.ToArray<string>();

				for (int keyIndex = 0; keyIndex < channelUserStateKeys.Length; keyIndex++) {
					string channelUserStateKey = channelUserStateKeys[keyIndex];
					object channelUserStateValue = channelUserStateDictionary[channelUserStateKey];
					if (channelUserStateValue == null) {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
					} else if (channelUserStateValue.GetType().ToString() == "System.Boolean") {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, channelUserStateValue.ToString().ToLowerInvariant());
					} else {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, (channelUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelUserStateValue) : channelUserStateValue);
					}
					if (keyIndex < channelUserStateKeys.Length - 1) {
						jsonStateBuilder.Append(',');
					}
				}
			}
			if (channelGroupUserStateDictionary != null) {
				string[] channelGroupUserStateKeys = channelGroupUserStateDictionary.Keys.ToArray<string>();

				for (int keyIndex = 0; keyIndex < channelGroupUserStateKeys.Length; keyIndex++) {
					string channelGroupUserStateKey = channelGroupUserStateKeys[keyIndex];
					object channelGroupUserStateValue = channelGroupUserStateDictionary[channelGroupUserStateKey];
					if (channelGroupUserStateValue == null) {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
					} else if (channelGroupUserStateValue.GetType().ToString() == "System.Boolean") {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, channelGroupUserStateValue.ToString().ToLowerInvariant());
					} else {
						jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, (channelGroupUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelGroupUserStateValue) : channelGroupUserStateValue);
					}
					if (keyIndex < channelGroupUserStateKeys.Length - 1) {
						jsonStateBuilder.Append(',');
					}
				}
			}

			return jsonStateBuilder.ToString();
		}

		protected string BuildJsonUserState(string[] channels, string[] channelGroups, bool local)
		{
			string retJsonUserState = "";

			StringBuilder jsonStateBuilder = new StringBuilder();

			if (channels != null && channels.Length > 0) {
				for (int index = 0; index < channels.Length; index++) {
					string currentJsonState = BuildJsonUserState(channels[index], "", local);
					if (!string.IsNullOrEmpty(currentJsonState)) {
						currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channels[index], currentJsonState);
						if (jsonStateBuilder.Length > 0) {
							jsonStateBuilder.Append(',');
						}
						jsonStateBuilder.Append(currentJsonState);
					}
				}
			}

			if (channelGroups != null && channelGroups.Length > 0) {
				for (int index = 0; index < channelGroups.Length; index++) {
					string currentJsonState = BuildJsonUserState("", channelGroups[index], local);
					if (!string.IsNullOrEmpty(currentJsonState)) {
						currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channelGroups[index], currentJsonState);
						if (jsonStateBuilder.Length > 0) {
							jsonStateBuilder.Append(',');
						}
						jsonStateBuilder.Append(currentJsonState);
					}
				}
			}

			if (jsonStateBuilder.Length > 0) {
				retJsonUserState = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", jsonStateBuilder);
			}

			return retJsonUserState;
		}

		private PNStatus GetStatusIfError<T>(RequestState<T> asyncRequestState, string jsonString)
		{
			PNStatus status = null;
			if (string.IsNullOrEmpty(jsonString)) { return status; }

			PNConfiguration currentConfig;
			PNOperationType type = PNOperationType.None;
			if (asyncRequestState != null) {
				type = asyncRequestState.ResponseType;
			}
			if (jsonLibrary.IsDictionaryCompatible(jsonString, type)) {
				Dictionary<string, object> deserializeStatus = jsonLibrary.DeserializeToDictionaryOfObject(jsonString);
				int statusCode = 0; //default. assuming all is ok 
				if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && string.Equals(deserializeStatus["error"].ToString(), "true", StringComparison.OrdinalIgnoreCase)) {
					status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
				} else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && deserializeStatus.ContainsKey("status") && Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode) && statusCode > 0) {
					string errorMessageJson = deserializeStatus["error"].ToString();
					Dictionary<string, object> errorDic = jsonLibrary.DeserializeToDictionaryOfObject(errorMessageJson);
					if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("message")
						&& statusCode != 200) {
						string statusMessage = errorDic["message"].ToString();
						PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
						status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
					} else if (statusCode != 200) {
						PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, errorMessageJson);
						status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
					}
				} else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("status") && string.Equals(deserializeStatus["status"].ToString(), "error", StringComparison.OrdinalIgnoreCase) && deserializeStatus.ContainsKey("error")) {
					string errorMessageJson = deserializeStatus["error"].ToString();
					Dictionary<string, object> errorDic = jsonLibrary.DeserializeToDictionaryOfObject(errorMessageJson);
					if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("code") && errorDic.ContainsKey("message")) {
						statusCode = PNStatusCodeHelper.GetHttpStatusCode(errorDic["code"].ToString());
						string statusMessage = errorDic["message"].ToString();
						if (statusCode != 200) {
							PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
							status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
						}
					}
				} else if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message")) {
					var _ = Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
					string statusMessage = deserializeStatus["message"].ToString();

					if (statusCode != 200) {
						PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
						status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
					}
				} else if (deserializeStatus.ContainsKey("message") && statusCode != 200) {
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, deserializeStatus["message"].ToString());
					status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
				}

			} else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<head", StringComparison.CurrentCultureIgnoreCase) == 0
				  || jsonString.ToLowerInvariant().TrimStart().IndexOf("<html", StringComparison.CurrentCultureIgnoreCase) == 0
				  || jsonString.ToLowerInvariant().TrimStart().IndexOf("<!doctype", StringComparison.CurrentCultureIgnoreCase) == 0)//Html is not expected. Only json format messages are expected.
			  {
				status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
			  } else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<?xml", StringComparison.CurrentCultureIgnoreCase) == 0
			             || jsonString.ToLowerInvariant().TrimStart().IndexOf("<Error", StringComparison.CurrentCultureIgnoreCase) == 0) {
				status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
			} else if (!NewtonsoftJsonDotNet.JsonFastCheck(jsonString)) {
				status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
			}

			return status;
		}

		internal bool Disconnect()
		{
			return true;
		}

		private bool disposedValue;

		protected virtual void DisposeInternal(bool disposing)
		{
			if (!disposedValue) {
				if (SubscribeHeartbeatCheckTimer != null) {
					SubscribeHeartbeatCheckTimer.Dispose();
				}
				disposedValue = true;
			}
		}

		void IDisposable.Dispose()
		{
			DisposeInternal(true);
		}

		private RequestParameter CreateSubscribeRequestParameter(string[] channels, string[] channelGroups, long timetoken, int region, string stateJsonValue, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			string channelsSegment = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"subscribe",
				config.SubscribeKey,
				channelsSegment,
				"0"
			};

			Dictionary<string, string> internalInitialSubscribeUrlParams = new Dictionary<string, string>();
			if (initialSubscribeUrlParams != null) {
				internalInitialSubscribeUrlParams = initialSubscribeUrlParams;
			}

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(internalInitialSubscribeUrlParams);

			if (!requestQueryStringParams.ContainsKey("filter-expr") && !string.IsNullOrEmpty(config.FilterExpression)) {
				requestQueryStringParams.Add("filter-expr", UriUtil.EncodeUriComponent(config.FilterExpression, PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (!requestQueryStringParams.ContainsKey("ee")) {
				requestQueryStringParams.Add("ee", "");
			}

			if (!requestQueryStringParams.ContainsKey("tt")) {
				requestQueryStringParams.Add("tt", timetoken.ToString(CultureInfo.InvariantCulture));
			}

			if (!requestQueryStringParams.ContainsKey("tr") && region > 0) {
				requestQueryStringParams.Add("tr", region.ToString(CultureInfo.InvariantCulture));
			}

			if (config.PresenceTimeout != 0) {
				requestQueryStringParams.Add("heartbeat", config.PresenceTimeout.ToString(CultureInfo.InvariantCulture));
			}

			if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "") {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (stateJsonValue != "{}" && stateJsonValue != "") {
				requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(stateJsonValue, PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (externalQueryParam != null && externalQueryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in externalQueryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSubscribeOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
			};
			return requestParameter;
		}

		private Tuple<T, PNStatus> GetTupleFromException<T>(Exception exception, RequestState<T> pubnubRequestState)
		{
			string errorMessage = exception.Message;
			string exceptionMessage = string.Empty;
			if (exception.InnerException != null) exceptionMessage = exception.InnerException.ToString();
			if (exceptionMessage.IndexOf("The request was aborted: The request was canceled", StringComparison.CurrentCultureIgnoreCase) == -1
			&& exceptionMessage.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.CurrentCultureIgnoreCase) == -1
			&& (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation && exceptionMessage.IndexOf("The operation has timed out", StringComparison.CurrentCultureIgnoreCase) == -1)
			&& exceptionMessage.IndexOf("A task was canceled", StringComparison.CurrentCultureIgnoreCase) == -1
			&& errorMessage.IndexOf("The operation was canceled", StringComparison.CurrentCultureIgnoreCase) == -1) {
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(exception.InnerException);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(pubnubRequestState.ResponseType, category, pubnubRequestState, Constants.ResourceNotFoundStatusCode, new PNException(exception));
				return new Tuple<T, PNStatus>(default, status);
			}
			return new Tuple<T, PNStatus>(default, null);
		}

	}
}
