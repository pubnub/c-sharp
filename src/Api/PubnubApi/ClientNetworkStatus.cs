using System;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PubnubApi
{
	#region "Network Status -- code split required"
	internal class ClientNetworkStatus
	{
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly PNConfiguration pubnubConfig;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;

        private bool networkStatus = true;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClient;
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

		internal bool CheckInternetStatus<T>(bool systemActive, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
            if (unit != null)
            {
                return unit.InternetAvailable;
            }
			else
			{
                Task[] tasks = new Task[1];

                tasks[0] = Task.Factory.StartNew(async() => await CheckClientNetworkAvailability(CallbackClientNetworkStatus, type, callback, channels, channelGroups).ConfigureAwait(false));
                tasks[0].ConfigureAwait(false);
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException ae) {
                    foreach (var ie in ae.InnerExceptions)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} AggregateException CheckInternetStatus Error: {1} {2} ", DateTime.Now.ToString(CultureInfo.InvariantCulture), ie.GetType().Name, ie.Message), pubnubConfig.LogVerbosity);
                    }
                }

                return networkStatus;
			}
		}

		private void CallbackClientNetworkStatus(bool status)
		{
			networkStatus = status;
		}

        private static object internetCheckLock = new object();
        private bool isInternetCheckRunning;

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
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
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

        private async Task<bool> CheckSocketConnect<T>(object internetState)
		{
            lock (internetCheckLock)
            {
                isInternetCheckRunning = true;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} CheckSocketConnect Entered", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);

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
            Uri requestUri = urlBuilder.BuildTimeRequest(null);
            try
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                var response = await httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} HttpClient CheckSocketConnect Resp {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                    networkStatus = true;
                    t.TrySetResult(true);
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} HttpClient CheckSocketConnect Resp {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                    networkStatus = false;
                    t.TrySetResult(false);
                }
#else
                HttpWebRequest myRequest = null;
                myRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUri);
                myRequest.Method = "GET";
                using (HttpWebResponse response = await Task.Factory.FromAsync<HttpWebResponse>(myRequest.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)myRequest.EndGetResponse(asyncPubnubResult), null))
                {
                    if (response != null)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} WebRequest CheckSocketConnect Resp {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            networkStatus = true;
                            t.TrySetResult(true);
                        }
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} WebRequest CheckSocketConnect FAILED.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                networkStatus = false;
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} CheckSocketConnect (HttpClient Or Task.Factory) Failed {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
                if (!networkStatus)
                {
                    t.TrySetResult(false);
                    isInternetCheckRunning = false;
                    ParseCheckSocketConnectException<T>(ex, type, pubnubCallback, internalCallback);
                }
            }
            finally
            {
                isInternetCheckRunning = false;
            }
            return await t.Task.ConfigureAwait(false);
		}


        private void ParseCheckSocketConnectException<T>(Exception ex, PNOperationType type, PNCallback<T> callback, Action<bool> internalcallback)
		{
            PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
            StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
            PNStatus status = statusBuilder.CreateStatusResponse<T>(type, errorCategory, null, (int)System.Net.HttpStatusCode.NotFound, ex);

            if (callback != null)
            {
                callback.OnResponse(default(T), status);
            }

			LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} ParseCheckSocketConnectException Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
			internalcallback(false);
		}

	}
#endregion
}

