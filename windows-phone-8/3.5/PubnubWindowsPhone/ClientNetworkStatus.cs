using System;
using System.Threading;
using System.Net.Sockets;
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
		private static ManualResetEventSlim mres = new ManualResetEventSlim(false);
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
		#if(__MonoCS__ && !UNITY_IOS && !UNITY_ANDROID)
		static UdpClient udp;
		#endif

		#if(UNITY_IOS || UNITY_ANDROID || __MonoCS__)
		static HttpWebRequest request;
		static WebResponse response;
		internal static int HeartbeatInterval {
			get;
			set;
		}
		internal static bool CheckInternetStatusUnity<T>(bool systemActive, Action<PubnubClientError> errorCallback, string[] channels, int heartBeatInterval)
		{
			HeartbeatInterval = heartBeatInterval;
			if (_failClientNetworkForTesting)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
				CheckClientNetworkAvailability<T>(CallbackClientNetworkStatus, errorCallback, channels);
				return _status;
			}
		}
		#endif
		internal static bool CheckInternetStatus<T>(bool systemActive, Action<PubnubClientError> errorCallback, string[] channels)
		{
			if (_failClientNetworkForTesting || _machineSuspendMode)
			{
				//Only to simulate network fail
				return false;
			}
			else
			{
				CheckClientNetworkAvailability<T>(CallbackClientNetworkStatus, errorCallback, channels);
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

		private static void CheckClientNetworkAvailability<T>(Action<bool> callback, Action<PubnubClientError> errorCallback, string[] channels)
		{
			InternetState<T> state = new InternetState<T>();
			state.Callback = callback;
			state.ErrorCallback = errorCallback;
			state.Channels = channels;
			#if (UNITY_ANDROID || UNITY_IOS)
			CheckSocketConnect<T>(state);
			#elif(__MonoCS__)
			CheckSocketConnect<T>(state);
			#else
			ThreadPool.QueueUserWorkItem(CheckSocketConnect<T>, state);
			#endif

			#if (SILVERLIGHT || WINDOWS_PHONE)
			mres.WaitOne();
			#elif(!UNITY_ANDROID && !UNITY_IOS)
			mres.Wait();
			#endif
		}

		private static void CheckSocketConnect<T>(object internetState)
		{
			InternetState<T> state = internetState as InternetState<T>;
			Action<bool> callback = state.Callback;
			Action<PubnubClientError> errorCallback = state.ErrorCallback;
			string[] channels = state.Channels;
			try
			{
				#if (SILVERLIGHT || WINDOWS_PHONE)
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					SocketAsyncEventArgs sae = new SocketAsyncEventArgs();
					sae.UserToken = state;
					sae.RemoteEndPoint = new DnsEndPoint("pubsub.pubnub.com", 80);
					sae.Completed += new EventHandler<SocketAsyncEventArgs>(socketAsync_Completed<T>);
					bool test = socket.ConnectAsync(sae);

					mreSocketAsync.WaitOne(1000);
					sae.Completed -= new EventHandler<SocketAsyncEventArgs>(socketAsync_Completed<T>);
					socket.Close();
				}
				#elif (UNITY_IOS || UNITY_ANDROID)
				request = (HttpWebRequest)WebRequest.Create("http://pubsub.pubnub.com");
				if(request!= null){
					request.Timeout = HeartbeatInterval * 1000;
					request.ContentType = "application/json";
					response = request.GetResponse ();
					if(response != null){
						if(((HttpWebResponse)response).ContentLength <= 0){
							_status = false;
							throw new Exception("Failed to connect");
						} else {
							using(Stream dataStream = response.GetResponseStream ()){
								using(StreamReader reader = new StreamReader (dataStream)){
									string responseFromServer = reader.ReadToEnd ();
									LoggingMethod.WriteToLog(string.Format("DateTime {0}, Response:{1}", DateTime.Now.ToString(), responseFromServer), LoggingMethod.LevelInfo);
									_status = true;
									callback(true);
									reader.Close();
								}
								dataStream.Close();
							}
						}
					} 
				}
				#elif(__MonoCS__)
				udp = new UdpClient("pubsub.pubnub.com", 80);
				IPAddress localAddress = ((IPEndPoint)udp.Client.LocalEndPoint).Address;
				if(udp != null && udp.Client != null){
					EndPoint remotepoint = udp.Client.RemoteEndPoint;
					string remoteAddress = (remotepoint != null) ? remotepoint.ToString() : "";
					LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus LocalIP: {1}, RemoteEndPoint:{2}", DateTime.Now.ToString(), localAddress.ToString(), remoteAddress), LoggingMethod.LevelVerbose);
					_status =true;
					callback(true);
				}
				#else
				using (UdpClient udp = new UdpClient("pubsub.pubnub.com", 80))
				{
					IPAddress localAddress = ((IPEndPoint)udp.Client.LocalEndPoint).Address;
					EndPoint remotepoint = udp.Client.RemoteEndPoint;
					string remoteAddress = (remotepoint != null) ? remotepoint.ToString() : "";
					udp.Close();

					LoggingMethod.WriteToLog(string.Format("DateTime {0} checkInternetStatus LocalIP: {1}, RemoteEndPoint:{2}", DateTime.Now.ToString(), localAddress.ToString(), remoteAddress), LoggingMethod.LevelVerbose);
					callback(true);
				}
				#endif
			}
			#if (UNITY_IOS || UNITY_ANDROID)
			catch (WebException webEx){

				if(webEx.Message.Contains("404")){
					_status =true;
					callback(true);
				} else {
					_status =false;
					ParseCheckSocketConnectException<T>(webEx, channels, errorCallback, callback);
				}
			}
			#endif
			catch (Exception ex)
			{
				#if(__MonoCS__)
				_status = false;
				#endif
				ParseCheckSocketConnectException<T>(ex, channels, errorCallback, callback);
			}
			finally
			{
				#if (UNITY_IOS || UNITY_ANDROID)
				if(response!=null){
					response.Close();

					response = null;
				}

				if(request!=null){
					request = null;
				}
				#elif(__MonoCS__)
				if(udp!=null){
					udp.Close();
				}
				#endif
				#if(UNITY_IOS)
				GC.Collect();
				#endif
			}
			#if (!UNITY_ANDROID && !UNITY_IOS)
			mres.Set();
			#endif
		}

		static void ParseCheckSocketConnectException<T> (Exception ex, string[] channels, Action<PubnubClientError> errorCallback, Action<bool> callback)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType(ex);
			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);
			PubnubClientError error = new PubnubClientError(statusCode, PubnubErrorSeverity.Warn, true, ex.Message, ex, PubnubMessageSource.Client, null, null, errorDescription, string.Join(",", channels));
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

