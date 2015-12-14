using System;
using System.Threading;
#if NETFX_CORE
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Foundation;
#else
using System.Net.Sockets;
#endif
using System.Net;
using System.Globalization;

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

		private static void CheckClientNetworkAvailability<T>(Action<bool> callback, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
			InternetState<T> state = new InternetState<T>();
			state.Callback = callback;
			state.ErrorCallback = errorCallback;
			state.Channels = channels;
            state.ChannelGroups = channelGroups;
			ThreadPool.QueueUserWorkItem(CheckSocketConnect<T>, state);

            mres.WaitOne(500, false);
        }

		private static void CheckSocketConnect<T>(object internetState)
		{
			InternetState<T> state = internetState as InternetState<T>;
			Action<bool> callback = state.Callback;
			Action<PubnubClientError> errorCallback = state.ErrorCallback;
			string[] channels = state.Channels;
            string[] channelGroups = state.ChannelGroups;
            HttpWebRequest myRequest = null;
			try
			{
                myRequest = (HttpWebRequest)System.Net.WebRequest.Create("http://pubsub.pubnub.com/time/0");
                LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Req {1}", DateTime.Now.ToString(), myRequest.RequestUri.ToString()), LoggingMethod.LevelInfo);
                myRequest.BeginGetResponse(cb =>
                {
                    try
                    {
                        using (HttpWebResponse resp = myRequest.EndGetResponse(cb) as HttpWebResponse)
                        {
                            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                            {
                                System.IO.Stream stream = resp.GetResponseStream();
                                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(stream))
                                {
                                    stream.Flush();
                                    string jsonString = streamReader.ReadToEnd();
                                    LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Resp {1}", DateTime.Now.ToString(), jsonString), LoggingMethod.LevelInfo);
                                    _status = true;
                                }
                            }
                            resp.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnect Failed {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelInfo);
                    }
                    finally
                    {
                        mreSocketAsync.Set();
                    }
                }, null);

                mreSocketAsync.WaitOne(330, false);
            }
			catch (Exception ex)
			{
                _status = false;
                LoggingMethod.WriteToLog(string.Format("DateTime {0} CheckSocketConnectTime FAILED {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelInfo);
				ParseCheckSocketConnectException<T>(ex, channels, channelGroups, errorCallback, callback);
                if (myRequest != null)
                {
                    myRequest.Abort();
                    myRequest = null;
                }
            }
            finally
            {
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

		#if (SILVERLIGHT || WINDOWS_PHONE)
		static void socketAsync_Completed<T>(object sender, SocketAsyncEventArgs e)
		{
			if (e.LastOperation == SocketAsyncOperation.Connect)
			{
				Socket skt = sender as Socket;
				InternetState<T> state = e.UserToken as InternetState<T>;
				if (state != null)
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0} socketAsync_Completed.", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
					state.Callback(true);
				}
				mreSocketAsync.Set();
			}
		}
		#endif

	}
	#endregion
}

