using System;
using System.Threading;
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

		public static Pubnub PubnubInstance {
			set {
				pubnubInstance = value;
			}
			get {
				return pubnubInstance;
			}
		}

		public static PNConfiguration PubnubConfiguation {
			set {
				pubnubConfig = value;
			}
			get {
				return pubnubConfig;
			}
		}

		public static IJsonPluggableLibrary JsonLibrary {
			set {
				jsonLib = value;
			}
			get {
				return jsonLib;
			}
		}

		public static IPubnubUnitTest PubnubUnitTest {
			set {
				unit = value;
			}
			get {
				return unit;
			}
		}

		public static IPubnubLog PubnubLog {
			set {
				pubnubLog = value;
			}
			get {
				return pubnubLog;
			}
		}

		internal static bool CheckInternetStatus<T>(bool systemActive, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
			if (unit != null) {
				return unit.InternetAvailable;
			} else {
				try {
					Task[] tasks = new Task[1];
					tasks[0] = Task.Factory.StartNew(async () => {
						await CheckClientNetworkAvailability(CallbackClientNetworkStatus, type, callback, channels, channelGroups).ConfigureAwait(false);
					}, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
					tasks[0].ConfigureAwait(false);
					Task.WaitAll(tasks);
				} catch (AggregateException ae) {
					foreach (var ie in ae.InnerExceptions) {
						LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} AggregateException CheckInternetStatus Error: {1} {2} ", DateTime.Now.ToString(CultureInfo.InvariantCulture), ie.GetType().Name, ie.Message), pubnubConfig.LogVerbosity);
					}
				} catch (Exception ex) {
					LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} Exception CheckInternetStatus Error: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
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

		private static async Task<bool> CheckClientNetworkAvailability<T>(Action<bool> internalCallback, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
			lock (internetCheckLock) {
				if (isInternetCheckRunning) {
					LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
					return networkStatus;
				}
			}

			InternetState<T> state = new InternetState<T>();
			state.InternalCallback = internalCallback;
			state.PubnubCallbacck = callback;
			state.ResponseType = type;
			state.Channels = channels;
			state.ChannelGroups = channelGroups;

			networkStatus = await CheckSocketConnect<T>(state).ConfigureAwait(false);
			return networkStatus;
		}

		private static async Task<bool> CheckSocketConnect<T>(object internetState)
		{
			lock (internetCheckLock) {
				isInternetCheckRunning = true;
			}
			LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} CheckSocketConnect Entered", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);

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
				bool gotTimeResp = false;
				gotTimeResp = await GetTimeWithTaskFactoryAsync().ConfigureAwait(false);

			} catch (Exception ex) {
				networkStatus = false;
				LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} CheckSocketConnect (HttpClient Or Task.Factory) Failed {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
				if (!networkStatus) {
					t.TrySetResult(false);
					isInternetCheckRunning = false;
					ParseCheckSocketConnectException<T>(ex, type, pubnubCallback, internalCallback);
				}
			} finally {
				isInternetCheckRunning = false;
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

			LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} ParseCheckSocketConnectException Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
			internalcallback(false);
		}

		private static async Task<bool> GetTimeWithTaskFactoryAsync()
		{
			bool successFlag = false;
			try {
				var transportResponse = await TimeRequest();
				if (transportResponse.Error != null) throw transportResponse.Error;
				successFlag = transportResponse.StatusCode == 200;
			} finally {
				networkStatus = successFlag;
			}
			return networkStatus;
		}

		private static async Task<TransportResponse> TimeRequest()
		{
			List<string> pathSegments = new List<string> {
				"time",
				"0"
			};
			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments
			};
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNTimeOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			return transportResponse;
		}
	}

}

