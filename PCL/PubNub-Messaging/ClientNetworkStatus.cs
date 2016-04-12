using System;
using System.Threading;
using System.Net;

namespace PubNubMessaging.Core
{
	#region "Network Status -- code split required"
	internal abstract class ClientNetworkStatus
	{
		private static bool _status = true;
		private static bool _failClientNetworkForTesting = false;
		private static bool _machineSuspendMode = false;

		private static IJsonPluggableLibrary _jsonPluggableLibrary;
		internal static IJsonPluggableLibrary JsonPluggableLibrary
		{
			get
			{
				return _jsonPluggableLibrary;
			}
			set
			{
				_jsonPluggableLibrary = value;
			}
		}

        private static ManualResetEvent mres = new ManualResetEvent(false);
        private static ManualResetEvent mreSocketAsync = new ManualResetEvent(false);

        internal static bool SimulateNetworkFailForTesting
		{
			get
			{
				return _failClientNetworkForTesting;
			}

			set
			{
				_failClientNetworkForTesting = value;
			}

		}

		internal static bool MachineSuspendMode
		{
			get
			{
				return _machineSuspendMode;
			}
			set
			{
				_machineSuspendMode = value;
			}
		}

		internal static bool CheckInternetStatus<T>(bool systemActive, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
			if (_failClientNetworkForTesting || _machineSuspendMode)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
                CheckClientNetworkAvailability<T>(CallbackClientNetworkStatus, errorCallback, channels, channelGroups);
				return _status;
			}
		}
		//#endif

		public static bool GetInternetStatus()
		{
			return _status;
		}

		private static void CallbackClientNetworkStatus(bool status)
		{
			_status = status;
		}

        private static object _internetCheckLock = new object();
        private static bool isInternetCheckRunning = false;

		private static void CheckClientNetworkAvailability<T>(Action<bool> callback, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
            lock (_internetCheckLock)
            {
                if (isInternetCheckRunning)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} InternetCheckRunning Already running", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
                    return;
                }
            }
            mres = new ManualResetEvent(false);


			InternetState<T> state = new InternetState<T>();
			state.Callback = callback;
			state.ErrorCallback = errorCallback;
			state.Channels = channels;
            state.ChannelGroups = channelGroups;

            ThreadPool.QueueUserWorkItem(CheckSocketConnect<T>, state);

            mres.WaitOne(500);

		}

		private static void CheckSocketConnect<T>(object internetState)
		{
            lock (_internetCheckLock)
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
                InternetState<T> state = internetState as InternetState<T>;
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
                                    _status = true;
                                }
                            }
						}
						catch(Exception ex)
						{
                            _status = false;
                            ParseCheckSocketConnectException<T>(ex, channels, channelGroups, errorCallback, callback);
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
				
				_status = false;
				ParseCheckSocketConnectException<T>(ex, channels, channelGroups, errorCallback, callback);
			}
			finally
			{
                isInternetCheckRunning = false;
                mres.Set();
			}
		}


        static void ParseCheckSocketConnectException<T> (Exception ex, string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback, Action<bool> callback)
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

		private static void GoToCallback<T>(object result, Action<T> Callback)
		{
			if (Callback != null)
			{
				if (typeof(T) == typeof(string))
				{
					JsonResponseToCallback(result, Callback);
				}
				else
				{
					Callback((T)(object)result);
				}
			}
		}

		private static void GoToCallback<T>(PubnubClientError result, Action<PubnubClientError> Callback)
		{
			if (Callback != null)
			{
				//TODO:
				//Include custom message related to error/status code for developer
				//error.AdditionalMessage = MyCustomErrorMessageRetrieverBasedOnStatusCode(error.StatusCode);

				Callback(result);
			}
		}


		private static void JsonResponseToCallback<T>(object result, Action<T> callback)
		{
			string callbackJson = "";

			if (typeof(T) == typeof(string))
			{
				callbackJson = _jsonPluggableLibrary.SerializeToJsonString(result);

				Action<string> castCallback = callback as Action<string>;
				castCallback(callbackJson);
			}
		}

	}
	#endregion
}

