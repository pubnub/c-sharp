using System;
using System.Threading;
using System.Net;

namespace PubnubApi
{
	#region "Network Status -- code split required"
	internal class ClientNetworkStatus
	{
        private static IJsonPluggableLibrary jsonLib;
        private static PNConfiguration pubnubConfig;

        private static bool networkStatus = true;
		private static bool machineSuspendMode = false;

        public ClientNetworkStatus(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
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
			if (machineSuspendMode)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
                CheckClientNetworkAvailability(CallbackClientNetworkStatus, type, callback, channels, channelGroups);
				return networkStatus;
			}
		}
		//#endif

		public static bool GetInternetStatus()
		{
			return networkStatus;
		}

		private static void CallbackClientNetworkStatus(bool status)
		{
			networkStatus = status;
		}

        private static object internetCheckLock = new object();
        private static bool isInternetCheckRunning = false;

		private static void CheckClientNetworkAvailability<T>(Action<bool> internalCallback, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
		{
            lock (internetCheckLock)
            {
                if (isInternetCheckRunning)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
                    return;
                }
            }
            mres = new ManualResetEvent(false);


			InternetState<T> state = new InternetState<T>();
			state.InternalCallback = internalCallback;
            state.PubnubCallbacck = callback;
            state.ResponseType = type;
            //state.ErrorCallback = errorCallback;
            state.Channels = channels;
            state.ChannelGroups = channelGroups;

            ThreadPool.QueueUserWorkItem(CheckSocketConnect<T>, state);

            mres.WaitOne(500);

		}

		private static void CheckSocketConnect<T>(object internetState)
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

			try
			{
                InternetState<T> state = internetState as InternetState<T>;
                if (state != null)
                {
                    internalCallback = state.InternalCallback;
                    type = state.ResponseType;
                    pubnubCallback = state.PubnubCallbacck;
                    channels = state.Channels;
                    channelGroups = state.ChannelGroups;
                }

                mreSocketAsync = new ManualResetEvent(false);

                myRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://pubsub.pubnub.com/time/0");
				myRequest.BeginGetResponse(cb => 
					{
						try
						{
                            using (HttpWebResponse resp = myRequest.EndGetResponse(cb) as HttpWebResponse)
                            {
                                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                                {
                                    LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Resp {1}", DateTime.Now.ToString(), HttpStatusCode.OK.ToString()), LoggingMethod.LevelInfo);
                                    networkStatus = true;
                                }
                            }
						}
						catch(Exception ex)
						{
                            networkStatus = false;
                            ParseCheckSocketConnectException<T>(ex, type, channels, channelGroups, pubnubCallback, internalCallback);
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Failed {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelInfo);
                        }
                        finally
                        {
                            mreSocketAsync.Set();
                        }

					}, null);

                mreSocketAsync.WaitOne(330);

			}
			catch (Exception ex)
			{
				
				networkStatus = false;
				ParseCheckSocketConnectException<T>(ex, type, channels, channelGroups, pubnubCallback, internalCallback);
			}
			finally
			{
                isInternetCheckRunning = false;
                mres.Set();
			}
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

			LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
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

