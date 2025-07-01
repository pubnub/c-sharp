using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;

namespace PubnubApi
{
	internal static class ClientNetworkStatus
	{
		private static IJsonPluggableLibrary jsonLib;
		private static PNConfiguration pubnubConfig;
		private static IPubnubUnitTest unit;
		private static IPubnubLog pubnubLog;
		private static Pubnub pubnubInstance;

		private static bool networkStatus = true;

		public static Pubnub PubnubInstance { get; set; }

		public static PNConfiguration PubnubConfiguation { get; set; }

		public static IJsonPluggableLibrary JsonLibrary { get; set; }

		public static IPubnubUnitTest PubnubUnitTest { get; set; }

		public static IPubnubLog PubnubLog { get; set; }

		internal static  bool CheckInternetStatus<T>(bool systemActive, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
			if (unit != null) {
				return unit.InternetAvailable;
			} else {
				try {
					return CheckClientNetworkAvailability(CallbackClientNetworkStatus, type, callback, channels, channelGroups);
				} catch (AggregateException ae) {
					foreach (var ie in ae.InnerExceptions)
					{
						PubnubConfiguation?.Logger?.Warn($"AggregateException CheckInternetStatus : {ie.GetType().Name} {ie.Message}");
					}
				} catch (Exception ex)
				{
					PubnubConfiguation?.Logger?.Warn($"CheckInternetStatus Exception: {ex.Message}");
				}

				return networkStatus;
			}
		}

		private static void CallbackClientNetworkStatus(bool status)
		{
			networkStatus = status;
		}

		private static object internetCheckLock = new object();
		private static bool isInternetCheckRunning;

		internal static bool IsInternetCheckRunning()
		{
			return isInternetCheckRunning;
		}

		private static bool CheckClientNetworkAvailability<T>(Action<bool> internalCallback, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
			lock (internetCheckLock) {
				if (isInternetCheckRunning)
				{
					PubnubConfiguation?.Logger?.Trace($"InternetCheckRunning Already running");
					return networkStatus;
				}
			}

			InternetState<T> state = new InternetState<T>
			{
				InternalCallback = internalCallback,
				PubnubCallbacck = callback,
				ResponseType = type,
				Channels = channels,
				ChannelGroups = channelGroups
			};

			CheckSocketConnect<T>(state).ConfigureAwait(false);
			return networkStatus;
		}

		private static async Task<bool> CheckSocketConnect<T>(object internetState)
		{
			lock (internetCheckLock) {
				isInternetCheckRunning = true;
			}

			Action<bool> internalCallback = null;
			PNCallback<T> pubnubCallback = null;
			PNOperationType type = PNOperationType.None;

			var t = new TaskCompletionSource<bool>();

			InternetState<T> state = internetState as InternetState<T>;
			if (state != null) {
				internalCallback = state.InternalCallback;
				type = state.ResponseType;
				pubnubCallback = state.PubnubCallbacck;
			}
			try {
				var gotTimeResp = await GetTimeWithTaskFactoryAsync().ConfigureAwait(false);
				isInternetCheckRunning = gotTimeResp;
				networkStatus = gotTimeResp;
			} catch (Exception ex) {
				networkStatus = false;
				PubnubConfiguation?.Logger?.Error(
					$"CheckSocketConnect (HttpClient Or Task.Factory) Failed {ex.Message}");
				if (!networkStatus) {
					t.TrySetResult(false);
					isInternetCheckRunning = false;
					ParseCheckSocketConnectException<T>(ex, type, pubnubCallback, internalCallback);
				}
			}
			return isInternetCheckRunning;
		}


		private static void ParseCheckSocketConnectException<T>(Exception ex, PNOperationType type, PNCallback<T> callback, Action<bool> internalcallback)
		{
			PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
			StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
			PNStatus status = statusBuilder.CreateStatusResponse<T>(type, errorCategory, null, 404, new PNException(ex));

			if (callback != null) {
				callback.OnResponse(default(T), status);
			}

			PubnubConfiguation?.Logger?.Trace($"ParseCheckSocketConnectException Error. {ex.Message}");
			internalcallback(false);
		}

		private static async Task<bool> GetTimeWithTaskFactoryAsync()
		{
			bool successFlag = false;
			try
			{
				var transportResponse = await TimeRequest().ConfigureAwait(false);
				if (transportResponse.Error == null)
				{
					successFlag = transportResponse.StatusCode == 200;
				}
			}
			finally {
				networkStatus = successFlag;
			}
			return successFlag;
		}

		private static async Task<TransportResponse> TimeRequest()
		{
			List<string> pathSegments =
			[
				"time",
				"0"
			];
			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments
			};
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNTimeOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			return transportResponse;
		}
	}

}

