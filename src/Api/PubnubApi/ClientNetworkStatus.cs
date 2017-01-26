using System;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace PubnubApi
{
	#region "Network Status -- code split required"
	internal class ClientNetworkStatus
	{
        private static IJsonPluggableLibrary jsonLib;
        private static PNConfiguration pubnubConfig;
        private static IPubnubUnitTest unit = null;

        private static bool networkStatus = true;
		private static bool machineSuspendMode = false;

        public ClientNetworkStatus(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
        }

        public ClientNetworkStatus(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            unit = pubnubUnit;
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
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckInternetStatus Error: {1} {2} ", DateTime.Now.ToString(), ie.GetType().Name, ie.Message), pubnubConfig.LogVerbosity);
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
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
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

            HttpWebRequest myRequest = null;
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

            PubnubApi.Interface.IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(pubnubConfig, jsonLib, unit);
            Uri requestUri = urlBuilder.BuildTimeRequest();

            myRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUri);
            try
            {
                myRequest.Method = "GET";
                using (HttpWebResponse response = await Task.Factory.FromAsync<HttpWebResponse>(myRequest.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)myRequest.EndGetResponse(asyncPubnubResult), null))
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Resp {1}", DateTime.Now.ToString(), response.StatusCode.ToString()), pubnubConfig.LogVerbosity);
                        networkStatus = true;
                        t.TrySetResult(true);
                    }
                }
            }
            catch (Exception ex)
            {
                networkStatus = false;
                LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Failed {1}", DateTime.Now.ToString(), ex.Message), pubnubConfig.LogVerbosity);
//#if !PORTABLE111 && !NETSTANDARD10
//                try
//                {
//                    using (System.Net.Sockets.UdpClient udp = new System.Net.Sockets.UdpClient(80))
//                    {
//                        IPAddress localAddress = ((IPEndPoint)udp.Client.LocalEndPoint).Address;
//                        if (udp != null && udp.Client != null && udp.Client.RemoteEndPoint != null)
//                        {
//                            udp.Client.SendTimeout = pubnubConfig.NonSubscribeRequestTimeout * 1000;
//                            System.Net.EndPoint remotepoint = udp.Client.RemoteEndPoint;
//                            string remoteAddress = (remotepoint != null) ? remotepoint.ToString() : "";
//                            LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus LocalIP: {1}, RemoteEndPoint:{2}", DateTime.Now.ToString(), localAddress.ToString(), remoteAddress), pubnubConfig.LogVerbosity);
//                            networkStatus = true;
//                            t.TrySetResult(true);
//                        }
//                    }
//                }
//                catch {
//                    networkStatus = false;
//                }
//#endif
                if (!networkStatus)
                {
                    isInternetCheckRunning = false;
                    t.TrySetResult(false);
                    ParseCheckSocketConnectException<T>(ex, type, channels, channelGroups, pubnubCallback, internalCallback);
                }
            }
            finally
            {
                isInternetCheckRunning = false;
            }
            return await t.Task;
		}


        private static void ParseCheckSocketConnectException<T>(Exception ex, PNOperationType type, string[] channels, string[] channelGroups, PNCallback<T> callback, Action<bool> internalcallback)
		{
            PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
            StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
            PNStatus status = statusBuilder.CreateStatusResponse<T>(type, errorCategory, null, (int)System.Net.HttpStatusCode.NotFound, ex);

            if (callback != null)
            {
                callback.OnResponse(default(T), status);
            }

			LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus Error. {1}", DateTime.Now.ToString(), ex.Message), pubnubConfig.LogVerbosity);
			internalcallback(false);
		}

		//private static void GoToCallback<T>(object result, Action<T> callback)
		//{
		//	if (callback != null)
		//	{
		//		if (typeof(T) == typeof(string))
		//		{
		//			JsonResponseToCallback(result, callback);
		//		}
		//		else
		//		{
		//			callback((T)(object)result);
		//		}
		//	}
		//}

		//private static void GoToCallback<T>(PubnubClientError result, Action<PubnubClientError> callback)
		//{
		//	if (callback != null)
		//	{
		//		//TODO:
		//		//Include custom message related to error/status code for developer
		//		//error.AdditionalMessage = MyCustomErrorMessageRetrieverBasedOnStatusCode(error.StatusCode);

		//		callback(result);
		//	}
		//}


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

