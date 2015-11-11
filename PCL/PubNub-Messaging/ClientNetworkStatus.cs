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

		#if (SILVERLIGHT  || WINDOWS_PHONE)
		private static ManualResetEvent mres = new ManualResetEvent(false);
		private static ManualResetEvent mreSocketAsync = new ManualResetEvent(false);
		#elif(!UNITY_IOS && !UNITY_ANDROID)
		private static ManualResetEvent mres = new ManualResetEvent(false);
		#endif
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

		private static void CheckClientNetworkAvailability<T>(Action<bool> callback, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
			InternetState<T> state = new InternetState<T>();
			state.Callback = callback;
			state.ErrorCallback = errorCallback;
			state.Channels = channels;
            state.ChannelGroups = channelGroups;

			CheckSocketConnect<T>(state);

		}

		private static void CheckSocketConnect<T>(object internetState)
		{
			InternetState<T> state = internetState as InternetState<T>;
			Action<bool> callback = state.Callback;
			Action<PubnubClientError> errorCallback = state.ErrorCallback;
			string[] channels = state.Channels;
            string[] channelGroups = state.ChannelGroups;
			try
			{
				//_status = true;
				//callback(true);
				//System.Net.WebRequest test = System.Net.WebRequest.Create("http://pubsub.pubnub.com");

				HttpWebRequest myRequest = (HttpWebRequest)System.Net.WebRequest.Create("http://pubsub.pubnub.com");
				myRequest.Method = "HEAD";
				myRequest.BeginGetResponse(cb => 
					{
						try
						{
							//Just want to check whether we can hit server to check internet connection.
							//Expecting webexception with code 404. No response is expected.
							myRequest.EndGetResponse(cb);
							_status = true;
						}
						catch(WebException webEx)
						{
							if (webEx.Response != null)
							{
								HttpStatusCode currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
								if ((int)currentHttpStatusCode == 404)
								{
									//The remote server returned an error: (404) Nothing
									_status = true;
								}
							}
							else
							{
								_status = false;
							}
						}
						mres.Set();
					}, null);
				
				mres.WaitOne(100);
//				using (UdpSocketClient socket = new UdpSocketClient())
//				{
//					await socket.ConnectAsync("pubsub.pubnub.com", 80);
//				}
			}
			catch (Exception ex)
			{
				
				_status = false;
				ParseCheckSocketConnectException<T>(ex, channels, channelGroups, errorCallback, callback);
			}
			finally
			{
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

