using System;
using System.Threading;
using System.Net;

namespace PubnubApi
{
	#region "Network Status -- code split required"
	internal abstract class ClientNetworkStatus
	{
		private static bool networkStatus = true;
		private static bool failClientNetworkForTesting = false;
		private static bool machineSuspendMode = false;

		private static IJsonPluggableLibrary jsonPluggableLibrary;
		internal static IJsonPluggableLibrary JsonPluggableLibrary
		{
			get
			{
				return jsonPluggableLibrary;
			}
			set
			{
				jsonPluggableLibrary = value;
			}
		}

        private static ManualResetEvent mres = new ManualResetEvent(false);
        private static ManualResetEvent mreSocketAsync = new ManualResetEvent(false);

        internal static bool SimulateNetworkFailForTesting
		{
			get
			{
				return failClientNetworkForTesting;
			}

			set
			{
				failClientNetworkForTesting = value;
			}

		}

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

		internal static bool CheckInternetStatus(bool systemActive, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
			if (failClientNetworkForTesting || machineSuspendMode)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
                CheckClientNetworkAvailability(CallbackClientNetworkStatus, errorCallback, channels, channelGroups);
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

		private static void CheckClientNetworkAvailability(Action<bool> callback, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
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


			InternetState state = new InternetState();
			state.Callback = callback;
			state.ErrorCallback = errorCallback;
			state.Channels = channels;
            state.ChannelGroups = channelGroups;

            ThreadPool.QueueUserWorkItem(CheckSocketConnect, state);

            mres.WaitOne(500);

		}

		private static void CheckSocketConnect(object internetState)
		{
            lock (internetCheckLock)
            {
                isInternetCheckRunning = true;
            }

            HttpWebRequest myRequest = null;
            Action<bool> callback = null;
            Action<PubnubClientError> errorCallback = null;
            string[] channels = null;
            string[] channelGroups = null;

			try
			{
                InternetState state = internetState as InternetState;
                if (state != null)
                {
                    callback = state.Callback;
                    errorCallback = state.ErrorCallback;
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
                            ParseCheckSocketConnectException(ex, channels, channelGroups, errorCallback, callback);
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
				ParseCheckSocketConnectException(ex, channels, channelGroups, errorCallback, callback);
			}
			finally
			{
                isInternetCheckRunning = false;
                mres.Set();
			}
		}


        private static void ParseCheckSocketConnectException(Exception ex, string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback, Action<bool> callback)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType(ex);
			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);
            string channel = (channels == null) ? "" : string.Join(",", channels);
            string channelGroup = (channelGroups == null) ? "" : string.Join(",", channelGroups);
            PubnubClientError error = new PubnubClientError(statusCode, PubnubErrorSeverity.Warn, true, ex.ToString(), ex, PubnubMessageSource.Client, null, null, errorDescription, channel, channelGroup);
			GoToCallback(error, errorCallback);


			LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
			callback(false);
		}

		private static void GoToCallback<T>(object result, Action<T> callback)
		{
			if (callback != null)
			{
				if (typeof(T) == typeof(string))
				{
					JsonResponseToCallback(result, callback);
				}
				else
				{
					callback((T)(object)result);
				}
			}
		}

		private static void GoToCallback<T>(PubnubClientError result, Action<PubnubClientError> callback)
		{
			if (callback != null)
			{
				//TODO:
				//Include custom message related to error/status code for developer
				//error.AdditionalMessage = MyCustomErrorMessageRetrieverBasedOnStatusCode(error.StatusCode);

				callback(result);
			}
		}


		private static void JsonResponseToCallback<T>(object result, Action<T> callback)
		{
			string callbackJson = "";

			if (typeof(T) == typeof(string))
			{
				callbackJson = jsonPluggableLibrary.SerializeToJsonString(result);

				Action<string> castCallback = callback as Action<string>;
				castCallback(callbackJson);
			}
		}

	}
	#endregion
}

