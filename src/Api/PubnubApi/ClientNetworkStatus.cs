using System;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PubnubApi
{
	#region "Network Status -- code split required"
	internal class ClientNetworkStatus
	{
        private static IJsonPluggableLibrary jsonLib;
        private static PNConfiguration pubnubConfig;
        private static IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        private static bool networkStatus = true;
		private static bool machineSuspendMode = false;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClient = null;
#endif

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        public ClientNetworkStatus(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, HttpClient refHttpClient)
#else
        public ClientNetworkStatus(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log)
#endif
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            httpClient = refHttpClient;
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromSeconds(pubnubConfig.NonSubscribeRequestTimeout);
            }
#endif
        }

        private static ManualResetEvent mres = new ManualResetEvent(false);
        private static ManualResetEvent mreSocketAsync = new ManualResetEvent(false);

		internal static bool MachineSuspendMode
		{
			get
			{
				return machineSuspendMode;
			}
			set
			{
				machineSuspendMode = value;
			}
		}

		internal bool CheckInternetStatus<T>(bool systemActive, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
            if (unit != null)
            {
                return unit.InternetAvailable;
            }
            else if (machineSuspendMode)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
                Task[] tasks = new Task[1];

                tasks[0] = Task.Factory.StartNew(async() => await CheckClientNetworkAvailability(CallbackClientNetworkStatus, type, callback, channels, channelGroups));

                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException ae) {
                    foreach (var ie in ae.InnerExceptions)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} AggregateException CheckInternetStatus Error: {1} {2} ", DateTime.Now.ToString(), ie.GetType().Name, ie.Message), pubnubConfig.LogVerbosity);
                    }
                }

                return networkStatus;
			}
		}
		//#endif

		private void CallbackClientNetworkStatus(bool status)
		{
			networkStatus = status;
		}

        private static object internetCheckLock = new object();
        private static bool isInternetCheckRunning = false;

        internal bool IsInternetCheckRunning()
        {
            return isInternetCheckRunning;
        }

        private async Task<bool> CheckClientNetworkAvailability<T>(Action<bool> internalCallback, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
            lock (internetCheckLock)
            {
                if (isInternetCheckRunning)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
                    return networkStatus;
                }
            }
            //mres = new ManualResetEvent(false);


			InternetState<T> state = new InternetState<T>();
			state.InternalCallback = internalCallback;
            state.PubnubCallbacck = callback;
            state.ResponseType = type;
            //state.ErrorCallback = errorCallback;
            state.Channels = channels;
            state.ChannelGroups = channelGroups;

            networkStatus = await CheckSocketConnect<T>(state);
            return networkStatus;

            //ThreadPool.QueueUserWorkItem(CheckSocketConnect<T>, state);

            //mres.WaitOne(500);

		}

        private async Task<bool> CheckSocketConnect<T>(object internetState)
		{
            lock (internetCheckLock)
            {
                isInternetCheckRunning = true;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} CheckSocketConnect Entered", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);

            Action<bool> internalCallback = null;
            PNCallback<T> pubnubCallback = null;
            PNOperationType type = PNOperationType.None;
            string[] channels = null;
            string[] channelGroups = null;

            var t = new TaskCompletionSource<bool>();

            InternetState<T> state = internetState as InternetState<T>;
            if (state != null)
            {
                internalCallback = state.InternalCallback;
                type = state.ResponseType;
                pubnubCallback = state.PubnubCallbacck;
                channels = state.Channels;
                channelGroups = state.ChannelGroups;
            }

            PubnubApi.Interface.IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(pubnubConfig, jsonLib, unit, pubnubLog, null);
            Uri requestUri = urlBuilder.BuildTimeRequest();
            try
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                var response = await httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} HttpClient CheckSocketConnect Resp {1}", DateTime.Now.ToString(), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                    networkStatus = true;
                    t.TrySetResult(true);
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} HttpClient CheckSocketConnect Resp {1}", DateTime.Now.ToString(), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                    networkStatus = false;
                    t.TrySetResult(false);
                }
#else
                HttpWebRequest myRequest = null;
                myRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUri);
                myRequest.Method = "GET";
                using (HttpWebResponse response = await Task.Factory.FromAsync<HttpWebResponse>(myRequest.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)myRequest.EndGetResponse(asyncPubnubResult), null))
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} WebRequest CheckSocketConnect Resp {1}", DateTime.Now.ToString(), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                        networkStatus = true;
                        t.TrySetResult(true);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                networkStatus = false;
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} CheckSocketConnect (HttpClient Or Task.Factory) Failed {1}", DateTime.Now.ToString(), ex.Message), pubnubConfig.LogVerbosity);
                if (!networkStatus)
                {
                    t.TrySetResult(false);
                    isInternetCheckRunning = false;
                    ParseCheckSocketConnectException<T>(ex, type, channels, channelGroups, pubnubCallback, internalCallback);
                }
            }
            finally
            {
                isInternetCheckRunning = false;
            }
            return await t.Task;
		}


        private void ParseCheckSocketConnectException<T>(Exception ex, PNOperationType type, string[] channels, string[] channelGroups, PNCallback<T> callback, Action<bool> internalcallback)
		{
            PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
            StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
            PNStatus status = statusBuilder.CreateStatusResponse<T>(type, errorCategory, null, (int)System.Net.HttpStatusCode.NotFound, ex);

            if (callback != null)
            {
                callback.OnResponse(default(T), status);
            }

			LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} ParseCheckSocketConnectException Error. {1}", DateTime.Now.ToString(), ex.Message), pubnubConfig.LogVerbosity);
			internalcallback(false);
		}

		private static void JsonResponseToCallback<T>(object result, Action<T> callback)
		{
			string callbackJson = "";

			if (typeof(T) == typeof(string))
			{
				callbackJson = jsonLib.SerializeToJsonString(result);

				Action<string> castCallback = callback as Action<string>;
				castCallback(callbackJson);
			}
		}

	}
#endregion
}

