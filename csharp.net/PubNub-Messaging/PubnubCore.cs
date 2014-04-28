//Build Date: April 08, 2014
#region "Header"
#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID)
#define USE_JSONFX_UNITY
#elif (UNITY_IOS)
#define USE_JSONFX_UNITY_IOS
#endif
#if (__MonoCS__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER)
#define TRACE
#endif
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
#if WINDOWS_PHONE && WP7
using System.Collections.Concurrent;
#elif WINDOWS_PHONE
using TvdP.Collections;
#else
using System.Collections.Concurrent;
#endif
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

#if USE_JSONFX || USE_JSONFX_UNITY
using JsonFx.Json;
#elif (USE_DOTNET_SERIALIZATION)
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
#elif (USE_MiniJSON)
using MiniJSON;
#elif (USE_JSONFX_UNITY_IOS)
using Pathfinding.Serialization.JsonFx;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
#endregion
namespace PubNubMessaging.Core
{
	// INotifyPropertyChanged provides a standard event for objects to notify clients that one of its properties has changed
	internal abstract class PubnubCore : INotifyPropertyChanged
	{

		#region "Events"

		// Common property changed event
		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) 
            {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region "Class variables"

		int _pubnubWebRequestCallbackIntervalInSeconds = 310;
		int _pubnubOperationTimeoutIntervalInSeconds = 15;
		int _pubnubNetworkTcpCheckIntervalInSeconds = 15;
		int _pubnubNetworkCheckRetries = 50;
		int _pubnubWebRequestRetryIntervalInSeconds = 10;
		int _pubnubPresenceHeartbeatInSeconds = 63;
		int _presenceHeartbeatIntervalInSeconds = 30;
		bool _enableResumeOnReconnect = true;
		bool _uuidChanged = false;
		protected bool overrideTcpKeepAlive = true;
		bool _enableJsonEncodingForPublish = true;
		LoggingMethod.Level _pubnubLogLevel = LoggingMethod.Level.Off;
		PubnubErrorFilter.Level _errorLevel = PubnubErrorFilter.Level.Info;
		protected ConcurrentDictionary<string, long> multiChannelSubscribe = new ConcurrentDictionary<string, long>();
		ConcurrentDictionary<string, PubnubWebRequest> _channelRequest = new ConcurrentDictionary<string, PubnubWebRequest>();
		protected ConcurrentDictionary<string, bool> channelInternetStatus = new ConcurrentDictionary<string, bool>();
		protected ConcurrentDictionary<string, int> channelInternetRetry = new ConcurrentDictionary<string, int>();
		ConcurrentDictionary<string, Timer> _channelReconnectTimer = new ConcurrentDictionary<string, Timer>();
		protected ConcurrentDictionary<Uri, Timer> channelLocalClientHeartbeatTimer = new ConcurrentDictionary<Uri, Timer>();
		protected ConcurrentDictionary<PubnubChannelCallbackKey, object> channelCallbacks = new ConcurrentDictionary<PubnubChannelCallbackKey, object>();
		ConcurrentDictionary<string, Dictionary<string, object>> _channelUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
		protected System.Threading.Timer localClientHeartBeatTimer;
		protected System.Threading.Timer presenceHeartbeatTimer = null;
		protected static bool pubnetSystemActive = true;
		// History of Messages (Obsolete)
		private List<object> _history = new List<object>();

        public List<object> History { get { return _history; } set { _history = value; RaisePropertyChanged("History"); } }

		private static long lastSubscribeTimetoken = 0;
		// Pubnub Core API implementation
		private string _origin = "pubsub.pubnub.com";
		//private string _origin = "presence-beta.pubnub.com"; //"pres-beta.pubnub.com";//"50.112.215.116";//"pam-beta.pubnub.com"; //;"uls-test.pubnub.co"; //"pam-beta.pubnub.com";
		protected string publishKey = "";
		protected string subscribeKey = "";
		protected string secretKey = "";
		protected string cipherKey = "";
		protected bool ssl = false;
		protected string parameters = "";
		private string subscribeParameters = "";
		private string presenceHeartbeatParameters = "";
		private string hereNowParameters = "";
		private string setUserStateparameters = "";
		private string globalHereNowParameters = "";

		#endregion

		#region "Properties"

		internal int SubscribeTimeout 
        {
			get 
            {
				return _pubnubWebRequestCallbackIntervalInSeconds;
			}

			set 
            {
				_pubnubWebRequestCallbackIntervalInSeconds = value;
			}
		}

		internal int NonSubscribeTimeout 
        {
			get 
            {
				return _pubnubOperationTimeoutIntervalInSeconds;
			}

			set 
            {
				_pubnubOperationTimeoutIntervalInSeconds = value;
			}
		}

		internal int NetworkCheckMaxRetries 
        {
			get 
            {
				return _pubnubNetworkCheckRetries;
			}

			set 
            {
				_pubnubNetworkCheckRetries = value;
			}
		}

		internal int NetworkCheckRetryInterval 
        {
			get 
            {
				return _pubnubWebRequestRetryIntervalInSeconds;
			}

			set 
            {
				_pubnubWebRequestRetryIntervalInSeconds = value;
			}
		}

		internal int LocalClientHeartbeatInterval 
        {
			get 
            {
				return _pubnubNetworkTcpCheckIntervalInSeconds;
			}

			set 
            {
				_pubnubNetworkTcpCheckIntervalInSeconds = value;
			}
		}

		internal bool EnableResumeOnReconnect 
        {
			get 
            {
				return _enableResumeOnReconnect;
			}
			set 
            {
				_enableResumeOnReconnect = value;
			}
		}

		public bool EnableJsonEncodingForPublish 
        {
			get 
            {
				return _enableJsonEncodingForPublish;
			}
			set 
            {
				_enableJsonEncodingForPublish = value;
			}
		}

		private string _authenticationKey = "";

		public string AuthenticationKey 
        {
			get 
            {
				return _authenticationKey;
			}

			set 
            {
				_authenticationKey = value;
			}
		}

		private IPubnubUnitTest _pubnubUnitTest;

		public virtual IPubnubUnitTest PubnubUnitTest 
        {
			get 
            {
				return _pubnubUnitTest;
			}
			set 
            {
				_pubnubUnitTest = value;
			}
		}

		private IJsonPluggableLibrary _jsonPluggableLibrary = null;

		public IJsonPluggableLibrary JsonPluggableLibrary 
        {
			get 
            {
				return _jsonPluggableLibrary;
			}
			
            set 
            {
				_jsonPluggableLibrary = value;
				if (_jsonPluggableLibrary is IJsonPluggableLibrary) 
                {
					ClientNetworkStatus.JsonPluggableLibrary = _jsonPluggableLibrary;
				} 
                else 
                {
					_jsonPluggableLibrary = null;
					throw new ArgumentException("Missing or Incorrect JsonPluggableLibrary value");
				}
			}
		}

		public string Origin 
        {
			get 
            {
				return _origin;
			}
			
            set 
            {
				_origin = value;
			}
		}

		private string sessionUUID = "";

		public string SessionUUID 
        {
			get 
            {
				return sessionUUID;
			}
			set 
            {
				sessionUUID = value;
			}
		}

		/// <summary>
		/// This property sets presence expiry timeout.
		/// Presence expiry value in seconds.
		/// </summary>
		internal int PresenceHeartbeat 
        {
			get 
            {
				return _pubnubPresenceHeartbeatInSeconds;
			}
			
            set 
            {
				if (value <= 0 || value > 320) 
                {
					_pubnubPresenceHeartbeatInSeconds = 0;
				} 
                else 
                {
					_pubnubPresenceHeartbeatInSeconds = value;
				}
                if (_pubnubPresenceHeartbeatInSeconds != 0)
                {
                    _presenceHeartbeatIntervalInSeconds = (_pubnubPresenceHeartbeatInSeconds / 2) - 1;
                }
			}
		}

		internal int PresenceHeartbeatInterval 
        {
			get 
            {
				return _presenceHeartbeatIntervalInSeconds;
			}
			
            set 
            {
				_presenceHeartbeatIntervalInSeconds = value;
                if (_presenceHeartbeatIntervalInSeconds > _pubnubPresenceHeartbeatInSeconds - 3 && _pubnubPresenceHeartbeatInSeconds > 0) 
                {
					_presenceHeartbeatIntervalInSeconds = (_pubnubPresenceHeartbeatInSeconds / 2) - 1;
				}
			}
		}

		protected LoggingMethod.Level PubnubLogLevel 
        {
			get 
            {
				return _pubnubLogLevel;
			}
			
            set 
            {
				_pubnubLogLevel = value;
			}
		}

		protected PubnubErrorFilter.Level PubnubErrorLevel 
        {
			get 
            {
				return _errorLevel;
			}
			
            set 
            {
				_errorLevel = value;
			}
		}

		#endregion

		#region "Init"

		/**
         * Pubnub instance initialization function
         * 
         * @param string publishKey.
         * @param string subscribeKey.
         * @param string secretKey.
         * @param bool sslOn
         */
		protected virtual void Init(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
		{
			#if (USE_JSONFX) || (USE_JSONFX_UNITY)
			LoggingMethod.WriteToLog ("USE_JSONFX", LoggingMethod.LevelInfo);
			this.JsonPluggableLibrary = new JsonFXDotNet();
			#elif (USE_DOTNET_SERIALIZATION)
						LoggingMethod.WriteToLog("USE_DOTNET_SERIALIZATION", LoggingMethod.LevelInfo);
						this.JsonPluggableLibrary = new JscriptSerializer();
			#elif (USE_MiniJSON)
						LoggingMethod.WriteToLog("USE_MiniJSON", LoggingMethod.LevelInfo);
						this.JsonPluggableLibrary = new MiniJSONObjectSerializer();
			#elif (USE_JSONFX_UNITY_IOS)
						LoggingMethod.WriteToLog("USE_JSONFX_UNITY_IOS", LoggingMethod.LevelInfo);
						this.JsonPluggableLibrary = new JsonFxUnitySerializer();
            #else
            LoggingMethod.WriteToLog("NewtonsoftJsonDotNet", LoggingMethod.LevelInfo);
						this.JsonPluggableLibrary = new NewtonsoftJsonDotNet();
			#endif

			LoggingMethod.LogLevel = _pubnubLogLevel;
			PubnubErrorFilter.ErrorLevel = _errorLevel;

			this.publishKey = publishKey;
			this.subscribeKey = subscribeKey;
			this.secretKey = secretKey;
			this.cipherKey = cipherKey;
			this.ssl = sslOn;

			VerifyOrSetSessionUUID();
		}

		#endregion

		#region "Internet connection and Reconnect Network"

		protected virtual void ReconnectNetwork<T>(ReconnectState<T> netState)
		{
			System.Threading.Timer timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
				                                  (-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
			if (netState != null && netState.Channels != null) 
            {
				_channelReconnectTimer.AddOrUpdate(string.Join(",", netState.Channels), timer, (key, oldState) => timer);
			}
		}

		protected virtual void ReconnectNetworkCallback<T>(System.Object reconnectState)
		{
			string channel = "";

			ReconnectState<T> netState = reconnectState as ReconnectState<T>;
			try 
            {
				if (netState != null && netState.Channels != null) 
                {
					channel = string.Join(",", netState.Channels);

					if (channelInternetStatus.ContainsKey(channel)
					         && (netState.Type == ResponseType.Subscribe || netState.Type == ResponseType.Presence)) 
                    {
						if (channelInternetStatus[channel]) 
                        {
							//Reset Retry if previous state is true
							channelInternetRetry.AddOrUpdate(channel, 0, (key, oldValue) => 0);
						} 
                        else 
                        {
							channelInternetRetry.AddOrUpdate(channel, 1, (key, oldValue) => oldValue + 1);
							LoggingMethod.WriteToLog(string.Format("DateTime {0}, {1} {2} reconnectNetworkCallback. Retry {3} of {4}", DateTime.Now.ToString(), channel, netState.Type, channelInternetRetry[channel], _pubnubNetworkCheckRetries), LoggingMethod.LevelInfo);

							if (netState.Channels != null) 
                            {
								for (int index = 0; index < netState.Channels.Length; index++) 
                                {
									string activeChannel = netState.Channels[index].ToString();

									string message = string.Format("Detected internet connection problem. Retrying connection attempt {0} of {1}", channelInternetRetry[channel], _pubnubNetworkCheckRetries);

									PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
									callbackKey.Channel = activeChannel;
									callbackKey.Type = netState.Type;

									if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey)) 
                                    {
										PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubChannelCallback<T>;
										if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) 
                                        {
											CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
												activeChannel, currentPubnubCallback.ErrorCallback, message, PubnubErrorCode.NoInternet, 
												null, null);
										}
									}
								}
							}
						}
					}

					if (channelInternetStatus[channel]) 
                    {
						if (_channelReconnectTimer.ContainsKey(channel)) 
                        {
							_channelReconnectTimer[channel].Change(Timeout.Infinite, Timeout.Infinite);
							_channelReconnectTimer[channel].Dispose();
						}
						string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";
						string message = "Internet connection available";

						CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
							multiChannel, netState.ErrorCallback, message, PubnubErrorCode.YesInternet, null, null);

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, {1} {2} reconnectNetworkCallback. Internet Available : {3}", DateTime.Now.ToString(), channel, netState.Type, channelInternetStatus[channel]), LoggingMethod.LevelInfo);
						switch (netState.Type) 
                        {
						case ResponseType.Subscribe:
						case ResponseType.Presence:
							MultiChannelSubscribeRequest<T>(netState.Type, netState.Channels, netState.Timetoken, netState.Callback, netState.ConnectCallback, netState.ErrorCallback, true);
							break;
						default:
							break;
						}
					} 
                    else if (channelInternetRetry[channel] >= _pubnubNetworkCheckRetries) 
                    {
						if (_channelReconnectTimer.ContainsKey(channel)) 
                        {
							_channelReconnectTimer[channel].Change(Timeout.Infinite, Timeout.Infinite);
							_channelReconnectTimer[channel].Dispose();
						}
						switch (netState.Type) 
                        {
						case ResponseType.Subscribe:
						case ResponseType.Presence:
							MultiplexExceptionHandler(netState.Type, netState.Channels, netState.Callback, netState.ConnectCallback, netState.ErrorCallback, true, false);
							break;
						default:
							break;
						}
					}
				} 
                else 
                {
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unknown request state in reconnectNetworkCallback", DateTime.Now.ToString()), LoggingMethod.LevelError);
				}
			} 
            catch (Exception ex) 
            {
				if (netState != null) 
                {
					string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";


					CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
						multiChannel, netState.ErrorCallback, ex, null, null);
				}

				LoggingMethod.WriteToLog(string.Format("DateTime {0} method:reconnectNetworkCallback \n Exception Details={1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
			}
		}

		private bool InternetConnectionStatusWithUnitTestCheck<T>(string channel, Action<PubnubClientError> errorCallback, string[] rawChannels)
		{
			bool networkConnection;
			if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) 
            {
				networkConnection = true;
			} 
            else 
            {
				networkConnection = InternetConnectionStatus<T>(channel, errorCallback, rawChannels);
				if (!networkConnection) 
                {
					string message = "Network connnect error - Internet connection is not available.";
					CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
						channel, errorCallback, message,
						PubnubErrorCode.NoInternet, null, null);
				}
			}

			return networkConnection;
		}

		protected virtual bool InternetConnectionStatus<T>(string channel, Action<PubnubClientError> errorCallback, string[] rawChannels)
		{
			bool networkConnection;
			networkConnection = ClientNetworkStatus.CheckInternetStatus<T>(pubnetSystemActive, errorCallback, rawChannels);
			return networkConnection;
		}

		private void ResetInternetCheckSettings(string[] channels)
		{
			if (channels == null)
				return;

			string multiChannel = string.Join(",", channels);
			if (channelInternetStatus.ContainsKey(multiChannel)) 
            {
				channelInternetStatus.AddOrUpdate(multiChannel, true, (key, oldValue) => true);
			} 
            else 
            {
				channelInternetStatus.GetOrAdd(multiChannel, true); //Set to true for internet connection
			}

			if (channelInternetRetry.ContainsKey(multiChannel)) 
            {
				channelInternetRetry.AddOrUpdate(multiChannel, 0, (key, oldValue) => 0);
			} 
            else 
            {
				channelInternetRetry.GetOrAdd(multiChannel, 0); //Initialize the internet retry count
			}
		}

		protected virtual bool ReconnectNetworkIfOverrideTcpKeepAlive<T>(ResponseType type, string[] channels, object timetoken, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback, string multiChannel)
		{
			if (overrideTcpKeepAlive) 
            {
				LoggingMethod.WriteToLog(string.Format ("DateTime {0}, Subscribe - No internet connection for {1}", DateTime.Now.ToString(), multiChannel), LoggingMethod.LevelInfo);
				ReconnectState<T> netState = new ReconnectState<T>();
				netState.Channels = channels;
				netState.Type = type;
				netState.Callback = userCallback;
				netState.ErrorCallback = errorCallback;
				netState.ConnectCallback = connectCallback;
				netState.Timetoken = timetoken;
				ReconnectNetwork<T>(netState);
				return true;
			} 
            else 
            {
				return false;
			}
		}

		#endregion

		#region "Error Callbacks"

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, Action<PubnubClientError> errorCallback, 
		                                                 string message, PubnubErrorCode errorType, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			int statusCode = (int)errorType;

			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, Action<PubnubClientError> errorCallback, 
		                                                 string message, int currentHttpStatusCode, string statusMessage,
		                                                 PubnubWebRequest req, PubnubWebResponse res)
		{
			PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);

			int statusCode = (int)pubnubErrorType;

			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, Action<PubnubClientError> errorCallback, 
		                                                 Exception ex, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);

			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, true, ex.Message, ex, msgSource, req, res, errorDescription, channel);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, Action<PubnubClientError> errorCallback, 
		                                                 WebException webex, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (webex.Status, webex.Message);
			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, true, webex.Message, webex, msgSource, req, res, errorDescription, channel);
			GoToCallback (error, errorCallback);
			return error;
		}

		#endregion

		#region "Terminate requests and Timers"

		protected void TerminatePendingWebRequest ()
		{
			TerminatePendingWebRequest<object> (null);
		}

		protected void TerminatePendingWebRequest<T> (RequestState<T> state)
		{
			if (state != null && state.Request != null) {
				if (state.Channels != null && state.Channels.Length > 0) {
					string activeChannel = state.Channels [0].ToString (); //Assuming one channel exist, else will refactor later
					PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
					callbackKey.Channel = (state.Type == ResponseType.Subscribe) ? activeChannel.Replace ("-pnpres", "") : activeChannel;
					callbackKey.Type = state.Type;

					if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
						object callbackObject;
						bool channelAvailable = channelCallbacks.TryGetValue (callbackKey, out callbackObject);
						PubnubChannelCallback<T> currentPubnubCallback = null;
						if (channelAvailable) {
							currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
						}
						if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
							state.Request.Abort (currentPubnubCallback.ErrorCallback, _errorLevel);
						}
					}
				}
			} else {
				ICollection<string> keyCollection = _channelRequest.Keys;
				foreach (string key in keyCollection) {
					PubnubWebRequest currentRequest = _channelRequest [key];
					if (currentRequest != null) {
						TerminatePendingWebRequest(currentRequest, null);
					}
				}
			}
		}

		private void TerminatePendingWebRequest(PubnubWebRequest request, Action<PubnubClientError> errorCallback)
		{
			if (request != null) {
				request.Abort(errorCallback, _errorLevel);
			}
		}

		private void RemoveChannelDictionary()
		{
			RemoveChannelDictionary<object>(null);
		}

		private void RemoveChannelDictionary<T>(RequestState<T> state)
		{
			if (state != null && state.Request != null) {
				string channel = (state.Channels != null) ? string.Join (",", state.Channels) : "";

				if (_channelRequest.ContainsKey (channel)) {
					PubnubWebRequest removedRequest;
					bool removeKey = _channelRequest.TryRemove (channel, out removedRequest);
					if (removeKey) {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);
					} else {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelError);
					}
				}
			} else {
				ICollection<string> keyCollection = _channelRequest.Keys;
				foreach (string key in keyCollection) {
					PubnubWebRequest currentRequest = _channelRequest [key];
					if (currentRequest != null) {
						bool removeKey = _channelRequest.TryRemove (key, out currentRequest);
						if (removeKey) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString (), key), LoggingMethod.LevelInfo);
						} else {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString (), key), LoggingMethod.LevelError);
						}
					}
				}
			}
		}

		private void RemoveChannelCallback()
		{
			ICollection<PubnubChannelCallbackKey> channelCollection = channelCallbacks.Keys;
			foreach (PubnubChannelCallbackKey keyChannel in channelCollection) {
				if (channelCallbacks.ContainsKey (keyChannel)) {
					object tempChannelCallback;
					bool removeKey = channelCallbacks.TryRemove (keyChannel, out tempChannelCallback);
					if (removeKey) {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0} RemoveChannelCallback from dictionary in RemoveChannelCallback for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelInfo);
					} else {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to RemoveChannelCallback from dictionary in RemoveChannelCallback for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelError);
					}
				}
			}
		}

        protected virtual void TerminatePresenceHeartbeatTimer()
        {
            if (presenceHeartbeatTimer != null)
            {
                presenceHeartbeatTimer.Dispose();
                presenceHeartbeatTimer = null;
            }

        }
		protected virtual void TerminateLocalClientHeartbeatTimer()
		{
			TerminateLocalClientHeartbeatTimer(null);
		}

		protected virtual void TerminateLocalClientHeartbeatTimer(Uri requestUri)
		{
			if (requestUri != null) {
				if (channelLocalClientHeartbeatTimer.ContainsKey (requestUri)) {
					Timer requestHeatbeatTimer = channelLocalClientHeartbeatTimer [requestUri];
					if (requestHeatbeatTimer != null) {
						try {
							requestHeatbeatTimer.Change (
								(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000,
								(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
							requestHeatbeatTimer.Dispose ();
						} catch (ObjectDisposedException ex) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} Error while accessing requestHeatbeatTimer object in TerminateLocalClientHeartbeatTimer {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelInfo);
						}

						Timer removedTimer = null;
						bool removed = channelLocalClientHeartbeatTimer.TryRemove (requestUri, out removedTimer);
						if (removed) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} Remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString (), requestUri.ToString ()), LoggingMethod.LevelInfo);
						} else {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString (), requestUri.ToString ()), LoggingMethod.LevelInfo);
						}
					}
				}
			} else {
				ConcurrentDictionary<Uri, Timer> timerCollection = channelLocalClientHeartbeatTimer;
				ICollection<Uri> keyCollection = timerCollection.Keys;
				foreach (Uri key in keyCollection) {
					if (channelLocalClientHeartbeatTimer.ContainsKey (key)) {
						Timer currentTimer = channelLocalClientHeartbeatTimer [key];
						currentTimer.Dispose ();
						Timer removedTimer = null;
						bool removed = channelLocalClientHeartbeatTimer.TryRemove (key, out removedTimer);
						if (!removed) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} TerminateLocalClientHeartbeatTimer(null) - Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString (), key.ToString ()), LoggingMethod.LevelInfo);
						}
					}
				}
			}
		}

		private void TerminateReconnectTimer()
		{
			TerminateReconnectTimer(null);
		}

		private void TerminateReconnectTimer(string channelName)
		{
			if (channelName != null) {
				if (_channelReconnectTimer.ContainsKey (channelName)) {
					Timer channelReconnectTimer = _channelReconnectTimer [channelName];
					channelReconnectTimer.Change (
						(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000,
						(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
					channelReconnectTimer.Dispose ();
					Timer removedTimer = null;
					bool removed = _channelReconnectTimer.TryRemove (channelName, out removedTimer);
					if (!removed) {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0} TerminateReconnectTimer - Unable to remove reconnect timer reference from collection for {1}", DateTime.Now.ToString (), channelName), LoggingMethod.LevelInfo);
					}
				}
			} else {
				ConcurrentDictionary<string, Timer> reconnectCollection = _channelReconnectTimer;
				ICollection<string> keyCollection = reconnectCollection.Keys;
				foreach (string key in keyCollection) {
					if (_channelReconnectTimer.ContainsKey (key)) {
						Timer currentTimer = _channelReconnectTimer [key];
						currentTimer.Dispose ();
						Timer removedTimer = null;
						bool removed = _channelReconnectTimer.TryRemove (key, out removedTimer);
						if (!removed) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0} TerminateReconnectTimer(null) - Unable to remove reconnect timer reference from collection for {1}", DateTime.Now.ToString (), key.ToString ()), LoggingMethod.LevelInfo);
						}
					}
				}
			}
		}

		public void EndPendingRequests()
		{
			RemoveChannelDictionary();
			TerminatePendingWebRequest();
			TerminateLocalClientHeartbeatTimer();
			TerminateReconnectTimer();
			RemoveChannelCallback();
            TerminatePresenceHeartbeatTimer();
		}

		public void TerminateCurrentSubscriberRequest()
		{
			string[] channels = GetCurrentSubscriberChannels ();
			if (channels != null) {
				string multiChannel = string.Join (",", channels);
				PubnubWebRequest request = (_channelRequest.ContainsKey (multiChannel)) ? _channelRequest [multiChannel] : null;
				if (request != null) {
					request.Abort (null, _errorLevel);

					LoggingMethod.WriteToLog (string.Format ("DateTime {0} TerminateCurrentSubsciberRequest {1}", DateTime.Now.ToString (), request.RequestUri.ToString ()), LoggingMethod.LevelInfo);
				}
			}
		}

		#endregion

		#region "Change UUID"

		public void ChangeUUID(string newUUID)
		{
			if (string.IsNullOrEmpty (newUUID) || sessionUUID == newUUID) {
				return;
			}
            
			_uuidChanged = true;

			string oldUUID = sessionUUID;
            
			sessionUUID = newUUID;
            
			string[] channels = GetCurrentSubscriberChannels();

            if (channels != null && channels.Length > 0)
            {
                Uri request = BuildMultiChannelLeaveRequest(channels, oldUUID);

                RequestState<string> requestState = new RequestState<string>();
                requestState.Channels = channels;
                requestState.Type = ResponseType.Leave;
                requestState.UserCallback = null;
                requestState.ErrorCallback = null;
                requestState.ConnectCallback = null;
                requestState.Reconnect = false;

                UrlProcessRequest<string>(request, requestState); // connectCallback = null
            }

			TerminateCurrentSubscriberRequest();

		}

		#endregion

		#region "Constructors"

		/**
         * PubNub 3.0 API
         * 
         * Prepare Pubnub messaging class initial state
         * 
         * @param string publishKey.
         * @param string subscribeKey.
         * @param string secretKey.
         * @param bool sslOn
         */
		public PubnubCore (string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
		{
			this.Init (publishKey, subscribeKey, secretKey, cipherKey, sslOn);
		}

		/**
         * PubNub 2.0 Compatibility
         * 
         * Prepare Pubnub messaging class initial state
         * 
         * @param string publishKey.
         * @param string subscribeKey.
         */
		public PubnubCore (string publishKey, string subscribeKey)
		{
			this.Init (publishKey, subscribeKey, "", "", false);
		}

		/// <summary>
		/// PubNub without SSL
		/// Prepare Pubnub messaging class initial state
		/// </summary>
		/// <param name="publishKey"></param>
		/// <param name="subscribeKey"></param>
		/// <param name="secretKey"></param>
		public PubnubCore (string publishKey, string subscribeKey, string secretKey)
		{
			this.Init (publishKey, subscribeKey, secretKey, "", false);
		}

		#endregion

		#region History (obsolete)

		/// <summary>
		/// History (Obsolete)
		/// Load history from a channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		[Obsolete ("This method should no longer be used, please use DetailedHistory() instead.")]
		public bool history (string channel, int limit)
		{
			List<string> url = new List<string> ();

			url.Add ("history");
			url.Add (this.subscribeKey);
			url.Add (channel);
			url.Add ("0");
			url.Add (limit.ToString ());

			return ProcessRequest (url, ResponseType.History);
		}

		/// <summary>
		/// Http Get Request process
		/// </summary>
		/// <param name="urlComponents"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool ProcessRequest (List<string> urlComponents, ResponseType type)
		{
			string channelName = GetChannelName (urlComponents, type);
			StringBuilder url = new StringBuilder ();

			// Add Origin To The Request
			url.Append (this._origin);

			// Generate URL with UTF-8 Encoding
			foreach (string url_bit in urlComponents) {
				url.Append ("/");
				url.Append (EncodeUricomponent (url_bit, type, true));
			}

			VerifyOrSetSessionUUID ();
			if (type == ResponseType.Presence || type == ResponseType.Subscribe) {
				url.Append ("?uuid=");
				url.Append (this.sessionUUID);
			}

			if (type == ResponseType.DetailedHistory)
				url.Append (parameters);

			Uri requestUri = new Uri (url.ToString ());

			ForceCanonicalPathAndQuery (requestUri);

			// Create Request
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUri);

			try {
				// Make request with the following inline Asynchronous callback
				request.BeginGetResponse (new AsyncCallback ((asynchronousResult) => {
					HttpWebRequest asyncWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
					HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse (asynchronousResult);
					using (StreamReader streamReader = new StreamReader (asyncWebResponse.GetResponseStream ())) {
						// Deserialize the result
						string jsonString = streamReader.ReadToEnd ();
						Action<PubnubClientError> dummyCallback = obj => {
						};
						WrapResultBasedOnResponseType<string> (type, jsonString, new string[] { channelName }, false, 0, dummyCallback);
					}
				}), request

				);

				return true;
			} catch (System.Exception) {
				return false;
			}
		}

		#endregion

		#region "Detailed History"

		/**
         * Detailed History
         */
		public bool DetailedHistory (string channel, long start, long end, int count, bool reverse, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return DetailedHistory<object> (channel, start, end, count, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory<T> (string channel, long start, long end, int count, bool reverse, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}


			Uri request = BuildDetailedHistoryRequest (channel, start, end, count, reverse);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			requestState.Type = ResponseType.DetailedHistory;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState);
		}

		public bool DetailedHistory (string channel, long start, Action<object> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
		{
			return DetailedHistory<object> (channel, start, -1, -1, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory<T> (string channel, long start, Action<T> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
		{
			return DetailedHistory<T> (channel, start, -1, -1, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory (string channel, int count, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return DetailedHistory<object> (channel, -1, -1, count, false, userCallback, errorCallback);
		}

		public bool DetailedHistory<T> (string channel, int count, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return DetailedHistory<T> (channel, -1, -1, count, false, userCallback, errorCallback);
		}

		private Uri BuildDetailedHistoryRequest (string channel, long start, long end, int count, bool reverse)
		{
			parameters = "";
			if (count <= -1)
				count = 100;
			parameters = "?count=" + count;
			if (reverse)
				parameters = parameters + "&" + "reverse=" + reverse.ToString ().ToLower ();
			if (start != -1)
				parameters = parameters + "&" + "start=" + start.ToString ().ToLower ();
			if (end != -1)
				parameters = parameters + "&" + "end=" + end.ToString ().ToLower ();
			if (!string.IsNullOrEmpty (_authenticationKey)) {
				parameters = parameters + "&" + "auth=" + _authenticationKey;
			}

			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("history");
			url.Add ("sub-key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (channel);

			return BuildRestApiRequest<Uri> (url, ResponseType.DetailedHistory);
		}

		#endregion

		#region "Publish"

		/// <summary>
		/// Publish
		/// Send a message to a channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="userCallback"></param>
		/// <returns></returns>
		public bool Publish (string channel, object message, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return Publish<object> (channel, message, userCallback, errorCallback);
		}

		public bool Publish<T> (string channel, object message, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ()) || message == null) {
				throw new ArgumentException ("Missing Channel or Message");
			}

			if (string.IsNullOrEmpty (this.publishKey) || string.IsNullOrEmpty (this.publishKey.Trim ()) || this.publishKey.Length <= 0) {
				throw new MissingFieldException ("Invalid publish key");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			Uri request = BuildPublishRequest (channel, message);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			requestState.Type = ResponseType.Publish;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState); 
		}

		private Uri BuildPublishRequest (string channel, object originalMessage)
		{
			string message = (_enableJsonEncodingForPublish) ? JsonEncodePublishMsg (originalMessage) : originalMessage.ToString ();

			// Generate String to Sign
			string signature = "0";
			if (this.secretKey.Length > 0) {
				StringBuilder string_to_sign = new StringBuilder ();
				string_to_sign
					.Append (this.publishKey)
						.Append ('/')
						.Append (this.subscribeKey)
						.Append ('/')
						.Append (this.secretKey)
						.Append ('/')
						.Append (channel)
						.Append ('/')
						.Append (message); // 1

				// Sign Message
				signature = Md5 (string_to_sign.ToString ());
			}

			// Build URL
			List<string> url = new List<string> ();
			url.Add ("publish");
			url.Add (this.publishKey);
			url.Add (this.subscribeKey);
			url.Add (signature);
			url.Add (channel);
			url.Add ("0");
			url.Add (message);

			return BuildRestApiRequest<Uri> (url, ResponseType.Publish);
		}

		#endregion

		#region "Encoding and Crypto"

		private string JsonEncodePublishMsg (object originalMessage)
		{
			string message = _jsonPluggableLibrary.SerializeToJsonString (originalMessage);


			if (this.cipherKey.Length > 0) {
				PubnubCrypto aes = new PubnubCrypto (this.cipherKey);
				string encryptMessage = aes.Encrypt (message);
				message = _jsonPluggableLibrary.SerializeToJsonString (encryptMessage);
			}

			return message;
		}
		//TODO: Identify refactoring
		private List<object> DecodeDecryptLoop (List<object> message, string[] channels, Action<PubnubClientError> errorCallback)
		{
			List<object> returnMessage = new List<object> ();
			if (this.cipherKey.Length > 0) {
				PubnubCrypto aes = new PubnubCrypto (this.cipherKey);
				var myObjectArray = (from item in message
				                         select item as object).ToArray ();
				IEnumerable enumerable = myObjectArray [0] as IEnumerable;
				if (enumerable != null) {
					List<object> receivedMsg = new List<object> ();
					foreach (object element in enumerable) {
						string decryptMessage = "";
						try {
							decryptMessage = aes.Decrypt (element.ToString ());
						} catch (Exception ex) {
							decryptMessage = "**DECRYPT ERROR**";

							string multiChannel = string.Join (",", channels);

							CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
								multiChannel, errorCallback, ex, null, null);
						}
						object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : _jsonPluggableLibrary.DeserializeToObject (decryptMessage);
						receivedMsg.Add (decodeMessage);
					}
					returnMessage.Add (receivedMsg);
				}

				for (int index = 1; index < myObjectArray.Length; index++) {
					returnMessage.Add (myObjectArray [index]);
				}
				return returnMessage;
			} else {
				var myObjectArray = (from item in message
				                         select item as object).ToArray ();
				IEnumerable enumerable = myObjectArray [0] as IEnumerable;
				if (enumerable != null) {
					List<object> receivedMessage = new List<object> ();
					foreach (object element in enumerable) {
						receivedMessage.Add (element);
					}
					returnMessage.Add (receivedMessage);
				}
				for (int index = 1; index < myObjectArray.Length; index++) {
					returnMessage.Add (myObjectArray [index]);
				}
				return returnMessage;
			}
		}

		private static string Md5 (string text)
		{
			MD5 md5 = new MD5CryptoServiceProvider ();
			byte[] data = Encoding.Unicode.GetBytes (text);
			byte[] hash = md5.ComputeHash (data);
			string hexaHash = "";
			foreach (byte b in hash)
				hexaHash += String.Format ("{0:x2}", b);
			return hexaHash;
		}

		protected virtual string EncodeUricomponent (string s, ResponseType type, bool ignoreComma)
		{
			string encodedUri = "";
			StringBuilder o = new StringBuilder ();
			foreach (char ch in s) {
				if (IsUnsafe (ch, ignoreComma)) {
					o.Append ('%');
					o.Append (ToHex (ch / 16));
					o.Append (ToHex (ch % 16));
				} else {
					if (ch == ',' && ignoreComma) {
						o.Append (ch.ToString ());
					} else if (Char.IsSurrogate (ch)) {
						o.Append (ch);
					} else {
						string escapeChar = System.Uri.EscapeDataString (ch.ToString ());
						o.Append (escapeChar);
					}
				}
			}
			encodedUri = o.ToString ();
			if (type == ResponseType.Here_Now || type == ResponseType.DetailedHistory || type == ResponseType.Leave || type == ResponseType.PresenceHeartbeat) {
				encodedUri = encodedUri.Replace ("%2F", "%252F");
			}

			return encodedUri;
		}

		protected char ToHex (int ch)
		{
			return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
		}

		#endregion

		#region "Presence And Subscribe"

		/// <summary>
		/// Subscribe
		/// Listen for a message on a channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		public void Subscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
		{
			Subscribe<object> (channel, userCallback, connectCallback, errorCallback);
		}

		public void Subscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (connectCallback == null) {
				throw new ArgumentException ("Missing connectCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requested subscribe for channel={1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);

			MultiChannelSubscribeInit<T> (ResponseType.Subscribe, channel, userCallback, connectCallback, errorCallback);
		}

		/// <summary>
		/// Presence
		/// Listen for a presence message on a channel or comma delimited channels
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="errorCallback"></param>
		public void Presence (string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
		{
			Presence<object> (channel, userCallback, connectCallback, errorCallback);
		}

		public void Presence<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requested presence for channel={1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);

			MultiChannelSubscribeInit<T> (ResponseType.Presence, channel, userCallback, connectCallback, errorCallback);
		}

		private void MultiChannelSubscribeInit<T> (ResponseType type, string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			string[] rawChannels = channel.Split (',');
			List<string> validChannels = new List<string> ();

			bool networkConnection = InternetConnectionStatusWithUnitTestCheck<T> (channel, errorCallback, rawChannels);

			if (rawChannels.Length > 0 && networkConnection) {
				if (rawChannels.Length != rawChannels.Distinct ().Count ()) {
					rawChannels = rawChannels.Distinct ().ToArray ();
					string message = "Detected and removed duplicate channels";

					CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
						channel, errorCallback, message, PubnubErrorCode.DuplicateChannel, null, null);
				}

				for (int index = 0; index < rawChannels.Length; index++) {
					if (rawChannels [index].Trim ().Length > 0) {
						string channelName = rawChannels [index].Trim ();
						if (type == ResponseType.Presence) {
							channelName = string.Format ("{0}-pnpres", channelName);
						}
						if (multiChannelSubscribe.ContainsKey (channelName)) {
							string message = string.Format ("{0}Already subscribed", (IsPresenceChannel (channelName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel (channelName)) ? PubnubErrorCode.AlreadyPresenceSubscribed : PubnubErrorCode.AlreadySubscribed;

							CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								channelName.Replace ("-pnpres", ""), errorCallback, message, errorType, null, null);
						} else {
							validChannels.Add (channelName);
						}
					}
				}
			}

			if (validChannels.Count > 0) {
				//Retrieve the current channels already subscribed previously and terminate them
				string[] currentChannels = multiChannelSubscribe.Keys.ToArray<string> ();
				if (currentChannels != null && currentChannels.Length > 0) {
					string multiChannelName = string.Join (",", currentChannels);
					if (_channelRequest.ContainsKey (multiChannelName)) {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						PubnubWebRequest webRequest = _channelRequest [multiChannelName];
						_channelRequest [multiChannelName] = null;

						if (webRequest != null)
							TerminateLocalClientHeartbeatTimer (webRequest.RequestUri);

						PubnubWebRequest removedRequest;
						_channelRequest.TryRemove (multiChannelName, out removedRequest);
						bool removedChannel = _channelRequest.TryRemove (multiChannelName, out removedRequest);
						if (removedChannel) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Success to remove channel(s)={1} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						} else {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to remove channel(s)={1} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						}
						if (webRequest != null)
							TerminatePendingWebRequest (webRequest, errorCallback);
					} else {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to capture channel(s)={1} from _channelRequest to abort request.", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
					}
				}


				//Add the valid channels to the channels subscribe list for tracking
				for (int index = 0; index < validChannels.Count; index++) {
					string currentLoopChannel = validChannels [index].ToString ();
					multiChannelSubscribe.GetOrAdd (currentLoopChannel, 0);

					PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
					callbackKey.Channel = currentLoopChannel;
					callbackKey.Type = type;

					PubnubChannelCallback<T> pubnubChannelCallbacks = new PubnubChannelCallback<T> ();
					pubnubChannelCallbacks.Callback = userCallback;
					pubnubChannelCallbacks.ConnectCallback = connectCallback;
					pubnubChannelCallbacks.ErrorCallback = errorCallback;

					channelCallbacks.AddOrUpdate (callbackKey, pubnubChannelCallbacks, (key, oldValue) => pubnubChannelCallbacks);
				}

				//Get all the channels
				string[] channels = multiChannelSubscribe.Keys.ToArray<string> ();

				RequestState<T> state = new RequestState<T> ();
				_channelRequest.AddOrUpdate (string.Join (",", channels), state.Request, (key, oldValue) => state.Request);

				ResetInternetCheckSettings (channels);
				MultiChannelSubscribeRequest<T> (type, channels, 0, userCallback, connectCallback, errorCallback, false);
			}
		}

		/// <summary>
		/// Multi-Channel Subscribe Request - private method for Subscribe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="channels"></param>
		/// <param name="timetoken"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="errorCallback"></param>
		/// <param name="reconnect"></param>
		private void MultiChannelSubscribeRequest<T> (ResponseType type, string[] channels, object timetoken, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback, bool reconnect)
		{
			//Exit if the channel is unsubscribed
			if (multiChannelSubscribe != null && multiChannelSubscribe.Count <= 0) {
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
				return;
			}

			string multiChannel = string.Join (",", channels);
			if (!_channelRequest.ContainsKey (multiChannel)) {
				return;
			}

			if (channelInternetStatus.ContainsKey (multiChannel) && (!channelInternetStatus [multiChannel]) && pubnetSystemActive) {
				if (channelInternetRetry.ContainsKey (multiChannel) && (channelInternetRetry [multiChannel] >= _pubnubNetworkCheckRetries)) {
					LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Subscribe channel={1} - No internet connection. MAXed retries for internet ", DateTime.Now.ToString (), multiChannel), LoggingMethod.LevelInfo);
					MultiplexExceptionHandler<T> (type, channels, userCallback, connectCallback, errorCallback, true, false);
					return;
				}

				if (ReconnectNetworkIfOverrideTcpKeepAlive <T> (type, channels, timetoken, userCallback, connectCallback, errorCallback, multiChannel)) {
					return;
				}

			}

			// Begin recursive subscribe
			try {
				long lastTimetoken = 0;
				long minimumTimetoken = multiChannelSubscribe.Min (token => token.Value);
				long maximumTimetoken = multiChannelSubscribe.Max (token => token.Value);

				if (minimumTimetoken == 0 || reconnect || _uuidChanged) {
					lastTimetoken = 0;
					_uuidChanged = false;
				} else {
					if (lastSubscribeTimetoken == maximumTimetoken) {
						lastTimetoken = maximumTimetoken;
					} else {
						lastTimetoken = lastSubscribeTimetoken;
					}
				}
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Building request for channel(s)={1} with timetoken={2}", DateTime.Now.ToString (), string.Join (",", channels), lastTimetoken), LoggingMethod.LevelInfo);
				// Build URL
				Uri requestUrl = BuildMultiChannelSubscribeRequest (channels, (Convert.ToInt64 (timetoken.ToString ()) == 0) ? Convert.ToInt64 (timetoken.ToString ()) : lastTimetoken);

				RequestState<T> pubnubRequestState = new RequestState<T> ();
				pubnubRequestState.Channels = channels;
				pubnubRequestState.Type = type;
				pubnubRequestState.ConnectCallback = connectCallback;
				pubnubRequestState.UserCallback = userCallback;
				pubnubRequestState.ErrorCallback = errorCallback;
				pubnubRequestState.Reconnect = reconnect;
				pubnubRequestState.Timetoken = Convert.ToInt64 (timetoken.ToString ());

				// Wait for message
				UrlProcessRequest<T> (requestUrl, pubnubRequestState);
			} catch (Exception ex) {
				LoggingMethod.WriteToLog (string.Format ("DateTime {0} method:_subscribe \n channel={1} \n timetoken={2} \n Exception Details={3}", DateTime.Now.ToString (), string.Join (",", channels), timetoken.ToString (), ex.ToString ()), LoggingMethod.LevelError);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					string.Join (",", channels), errorCallback, ex, null, null);

				this.MultiChannelSubscribeRequest<T> (type, channels, timetoken, userCallback, connectCallback, errorCallback, false);
			}
		}

		private Uri BuildMultiChannelSubscribeRequest (string[] channels, object timetoken)
		{
            subscribeParameters = "";
			string channelsJsonState = BuildJsonUserState (channels);
			if (channelsJsonState != "{}" && channelsJsonState != "") {
				subscribeParameters = string.Format ("&state={0}", EncodeUricomponent (channelsJsonState, ResponseType.Subscribe, false));
			}

			List<string> url = new List<string> ();
			url.Add ("subscribe");
			url.Add (this.subscribeKey);
			url.Add (string.Join (",", channels));
			url.Add ("0");
			url.Add (timetoken.ToString ());

			return BuildRestApiRequest<Uri> (url, ResponseType.Subscribe);
		}

		#endregion

		#region "Unsubscribe Presence And Subscribe"

		public void PresenceUnsubscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			PresenceUnsubscribe<object> (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public void PresenceUnsubscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (connectCallback == null) {
				throw new ArgumentException ("Missing connectCallback");
			}
			if (disconnectCallback == null) {
				throw new ArgumentException ("Missing disconnectCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requested presence-unsubscribe for channel(s)={1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);
			MultiChannelUnSubscribeInit<T> (ResponseType.PresenceUnsubscribe, channel, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		private void MultiChannelUnSubscribeInit<T> (ResponseType type, string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			string[] rawChannels = channel.Split (',');
			List<string> validChannels = new List<string> ();

			if (rawChannels.Length > 0) {
				for (int index = 0; index < rawChannels.Length; index++) {
					if (rawChannels [index].Trim ().Length > 0) {
						string channelName = rawChannels [index].Trim ();
						if (type == ResponseType.PresenceUnsubscribe) {
							channelName = string.Format ("{0}-pnpres", channelName);
						}
						if (!multiChannelSubscribe.ContainsKey (channelName)) {
							string message = string.Format ("{0}Channel Not Subscribed", (IsPresenceChannel (channelName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel (channelName)) ? PubnubErrorCode.NotPresenceSubscribed : PubnubErrorCode.NotSubscribed;

							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, channel={1} unsubscribe response={2}", DateTime.Now.ToString (), channelName, message), LoggingMethod.LevelInfo);

							CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								channelName, errorCallback, message, errorType, null, null);
						} else {
							validChannels.Add (channelName);
						}
					} else {
						string message = "Invalid Channel Name For Unsubscribe";

						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, channel={1} unsubscribe response={2}", DateTime.Now.ToString (), rawChannels [index], message), LoggingMethod.LevelInfo);

						CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
							rawChannels [index], errorCallback, message, PubnubErrorCode.InvalidChannel,
							null, null);
					}
				}
			}

			if (validChannels.Count > 0) {
				//Retrieve the current channels already subscribed previously and terminate them
				string[] currentChannels = multiChannelSubscribe.Keys.ToArray<string> ();
				if (currentChannels != null && currentChannels.Length > 0) {
					string multiChannelName = string.Join (",", currentChannels);
					if (_channelRequest.ContainsKey (multiChannelName)) {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						PubnubWebRequest webRequest = _channelRequest [multiChannelName];
						_channelRequest [multiChannelName] = null;

						if (webRequest != null) {
							TerminateLocalClientHeartbeatTimer (webRequest.RequestUri);
						}

						PubnubWebRequest removedRequest;
						bool removedChannel = _channelRequest.TryRemove (multiChannelName, out removedRequest);
						if (removedChannel) {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Success to remove channel(s)={1} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						} else {
							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to remove channel(s)={1} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
						}
						if (webRequest != null)
							TerminatePendingWebRequest (webRequest, errorCallback);
					} else {
						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to capture channel(s)={1} from _channelRequest to abort request.", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
					}

					if (type == ResponseType.Unsubscribe) {
						//just fire leave() event to REST API for safeguard
						Uri request = BuildMultiChannelLeaveRequest (validChannels.ToArray ());

						RequestState<T> requestState = new RequestState<T> ();
						requestState.Channels = new string[] { channel };
						requestState.Type = ResponseType.Leave;
						requestState.UserCallback = null;
						requestState.ErrorCallback = null;
						requestState.ConnectCallback = null;
						requestState.Reconnect = false;

						UrlProcessRequest<T> (request, requestState); // connectCallback = null
					}
				}


				//Remove the valid channels from subscribe list for unsubscribe 
				for (int index = 0; index < validChannels.Count; index++) {
					long timetokenValue;
					string channelToBeRemoved = validChannels [index].ToString ();
					bool unsubscribeStatus = multiChannelSubscribe.TryRemove (channelToBeRemoved, out timetokenValue);
					if (unsubscribeStatus) {
						List<object> result = new List<object> ();
						string jsonString = string.Format ("[1, \"{0}Unsubscribed from {1}\"]", (IsPresenceChannel (channelToBeRemoved)) ? "Presence " : "", channelToBeRemoved.Replace ("-pnpres", ""));
						result = _jsonPluggableLibrary.DeserializeToListOfObject (jsonString);
						result.Add (channelToBeRemoved.Replace ("-pnpres", ""));
						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, JSON response={1}", DateTime.Now.ToString (), jsonString), LoggingMethod.LevelInfo);
						GoToCallback<T> (result, disconnectCallback);

						DeleteLocalUserState (channelToBeRemoved);
					} else {
						string message = "Unsubscribe Error. Please retry the unsubscribe operation.";

						PubnubErrorCode errorType = (IsPresenceChannel (channelToBeRemoved)) ? PubnubErrorCode.PresenceUnsubscribeFailed : PubnubErrorCode.UnsubscribeFailed;

						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, channel={1} unsubscribe error", DateTime.Now.ToString (), channelToBeRemoved), LoggingMethod.LevelInfo);

						CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
							channelToBeRemoved, errorCallback, message, errorType, null, null);

					}
				}

				//Get all the channels
				string[] channels = multiChannelSubscribe.Keys.ToArray<string> ();

				if (channels != null && channels.Length > 0) {
					RequestState<T> state = new RequestState<T> ();
					_channelRequest.AddOrUpdate (string.Join (",", channels), state.Request, (key, oldValue) => state.Request);

					ResetInternetCheckSettings (channels);

					//Modify the value for type ResponseType. Presence or Subscrie is ok, but sending the close value would make sense
					if (string.Join (",", channels).IndexOf ("-pnpres") > 0) {
						type = ResponseType.Presence;
					} else {
						type = ResponseType.Subscribe;
					}

					//Continue with any remaining channels for subscribe/presence
					MultiChannelSubscribeRequest<T> (type, channels, 0, userCallback, connectCallback, errorCallback, false);
				} else {
					if (presenceHeartbeatTimer != null) {
						// Stop the presence heartbeat timer if there are no channels subscribed
						presenceHeartbeatTimer.Dispose ();
						presenceHeartbeatTimer = null;
					}
					LoggingMethod.WriteToLog (string.Format ("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
				}
			}

		}

		/// <summary>
		/// To unsubscribe a channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="disconnectCallback"></param>
		/// <param name="errorCallback"></param>
		public void Unsubscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			Unsubscribe<object> (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		/// <summary>
		/// To unsubscribe a channel
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="disconnectCallback"></param>
		/// <param name="errorCallback"></param>
		public void Unsubscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (connectCallback == null) {
				throw new ArgumentException ("Missing connectCallback");
			}
			if (disconnectCallback == null) {
				throw new ArgumentException ("Missing disconnectCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requested unsubscribe for channel(s)={1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);
			MultiChannelUnSubscribeInit<T> (ResponseType.Unsubscribe, channel, userCallback, connectCallback, disconnectCallback, errorCallback);

		}

		private Uri BuildMultiChannelLeaveRequest (string[] channels)
		{
			return BuildMultiChannelLeaveRequest (channels, "");
		}

		private Uri BuildMultiChannelLeaveRequest (string[] channels, string uuid)
		{
			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (string.Join (",", channels));
			url.Add ("leave");

			return BuildRestApiRequest<Uri> (url, ResponseType.Leave, uuid);
		}

		#endregion

		#region "HereNow"

		internal bool HereNow (string channel, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return HereNow<object> (channel, true, false, userCallback, errorCallback);
		}

		internal bool HereNow<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return HereNow<T> (channel, true, false, userCallback, errorCallback);
		}

		internal bool HereNow<T> (string channel, bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			Uri request = BuildHereNowRequest (channel, showUUIDList, includeUserState);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			requestState.Type = ResponseType.Here_Now;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState);
		}

		private Uri BuildHereNowRequest (string channel, bool showUUIDList, bool includeUserState)
		{
			int disableUUID = (showUUIDList) ? 0 : 1;
			int userState = (includeUserState) ? 1 : 0;
			hereNowParameters = string.Format ("?disable_uuids={0}&state={1}", disableUUID, userState);

			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (channel);

			return BuildRestApiRequest<Uri> (url, ResponseType.Here_Now);
		}

		private Uri BuildPresenceHeartbeatRequest (string[] channels)
		{
            presenceHeartbeatParameters = "";
			string channelsJsonState = BuildJsonUserState (channels);
			if (channelsJsonState != "{}" && channelsJsonState != "") {
				presenceHeartbeatParameters = string.Format ("&state={0}", EncodeUricomponent (channelsJsonState, ResponseType.PresenceHeartbeat, false));
			}

			List<string> url = new List<string> ();
            
			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (string.Join (",", channels));
			url.Add ("heartbeat");

			return BuildRestApiRequest<Uri> (url, ResponseType.PresenceHeartbeat);
		}

		#endregion

		#region "Global Here Now"

		internal void GlobalHereNow (Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			GlobalHereNow<object> (true, false, userCallback, errorCallback);
		}

		internal bool GlobalHereNow<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return GlobalHereNow<T> (true, false, userCallback, errorCallback);
		}

		internal bool GlobalHereNow<T> (bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			Uri request = BuildGlobalHereNowRequest (showUUIDList, includeUserState);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = null;
			requestState.Type = ResponseType.GlobalHere_Now;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState);
		}

		private Uri BuildGlobalHereNowRequest (bool showUUIDList, bool includeUserState)
		{
			int disableUUID = (showUUIDList) ? 0 : 1;
			int userState = (includeUserState) ? 1 : 0;
			globalHereNowParameters = string.Format ("?disable_uuids={0}&state={1}", disableUUID, userState);

			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);

			return BuildRestApiRequest<Uri> (url, ResponseType.GlobalHere_Now);
		}

		#endregion

		#region "WhereNow"

		internal void WhereNow (string uuid, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			WhereNow<object> (uuid, userCallback, errorCallback);
		}

		internal void WhereNow<T> (string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			if (string.IsNullOrEmpty (uuid)) {
				VerifyOrSetSessionUUID ();
				uuid = sessionUUID;
			}
			Uri request = BuildWhereNowRequest (uuid);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { uuid };
			requestState.Type = ResponseType.Where_Now;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T> (request, requestState);
		}

		private Uri BuildWhereNowRequest (string uuid)
		{
			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("uuid");
			url.Add (uuid);

			return BuildRestApiRequest<Uri> (url, ResponseType.Where_Now);
		}

		#endregion

		#region "Time"

		/// <summary>
		/// Time
		/// Timestamp from PubNub Cloud
		/// </summary>
		/// <param name="userCallback"></param>
		/// <param name="errorCallback"></param>
		/// <returns></returns>
		public bool Time (Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return Time<object> (userCallback, errorCallback);
		}

		public bool Time<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			Uri request = BuildTimeRequest ();

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = null;
			requestState.Type = ResponseType.Time;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState); 
		}

		public Uri BuildTimeRequest ()
		{
			List<string> url = new List<string> ();

			url.Add ("time");
			url.Add ("0");

			return BuildRestApiRequest<Uri> (url, ResponseType.Time);
		}

		#endregion

		#region "User State"

		private string AddOrUpdateOrDeleteLocalUserState (string channel, string userStateKey, object userStateValue)
		{
            string retJsonUserState = "";

			Dictionary<string, object> userStateDictionary = null;

			if (_channelUserState.ContainsKey (channel)) {
				userStateDictionary = _channelUserState [channel];
				if (userStateDictionary != null) {
					if (userStateDictionary.ContainsKey (userStateKey)) {
						if (userStateValue != null) {
							userStateDictionary [userStateKey] = userStateValue;
						} else {
							userStateDictionary.Remove (userStateKey);
						}
					} else {
						if (!string.IsNullOrEmpty (userStateKey) && userStateKey.Trim ().Length > 0 && userStateValue != null) {
							userStateDictionary.Add (userStateKey, userStateValue);
						}
					}
				} else {
					userStateDictionary = new Dictionary<string, object> ();
					userStateDictionary.Add (userStateKey, userStateValue);
				}

				_channelUserState.AddOrUpdate (channel, userStateDictionary, (oldData, newData) => userStateDictionary);
			} else {
				if (!string.IsNullOrEmpty (userStateKey) && userStateKey.Trim ().Length > 0 && userStateValue != null) {
					userStateDictionary = new Dictionary<string, object> ();
					userStateDictionary.Add (userStateKey, userStateValue);

					_channelUserState.AddOrUpdate (channel, userStateDictionary, (oldData, newData) => userStateDictionary);
				}
			}

            string jsonUserState = BuildJsonUserState(channel);
            if (jsonUserState != "")
            {
                retJsonUserState = string.Format("{{{0}}}", jsonUserState);
            }
            return retJsonUserState;
		}

		private bool DeleteLocalUserState (string channel)
		{
			bool userStateDeleted = false;

			if (_channelUserState.ContainsKey (channel)) {
				Dictionary<string, object> returnedUserState = null;
				userStateDeleted = _channelUserState.TryRemove (channel, out returnedUserState);
			}

			return userStateDeleted;
		}
		//private bool DeleteLocalMetadata(string[] channels)
		//{
		//    bool metadataDeleted = false;
		//    foreach (string channel in channels)
		//    {
		//        if (_channelMetadata.ContainsKey(channel))
		//        {
		//            Dictionary<string, object> returnedMetadata = null;
		//            metadataDeleted = _channelMetadata.TryRemove(channel, out returnedMetadata);
		//            if (!metadataDeleted) break;
		//        }
		//    }
		//    return metadataDeleted;
		//}
		private string BuildJsonUserState (string channel)
		{
			string retJsonUserState = "";

			Dictionary<string, object> userStateDictionary = null;

			if (_channelUserState.ContainsKey (channel)) {
				userStateDictionary = _channelUserState [channel];
			}

			StringBuilder jsonStateBuilder = new StringBuilder ();

			if (userStateDictionary != null) 
            {
				string[] userStateKeys = userStateDictionary.Keys.ToArray<string> ();

				for (int keyIndex = 0; keyIndex < userStateKeys.Length; keyIndex++) 
                {
					string useStateKey = userStateKeys [keyIndex];
					object userStateValue = userStateDictionary [useStateKey];
					jsonStateBuilder.AppendFormat ("\"{0}\":{1}", useStateKey, (userStateValue.GetType ().ToString () == "System.String") ? string.Format ("\"{0}\"", userStateValue) : userStateValue);
					if (keyIndex < userStateKeys.Length - 1) 
                    {
						jsonStateBuilder.Append (",");
					}
				}
			}

            return jsonStateBuilder.ToString();
		}

		private string BuildJsonUserState (string[] channels)
		{
            string retJsonUserState = "";

			StringBuilder jsonStateBuilder = new StringBuilder ();

			if (channels != null) 
            {
                for (int index = 0; index < channels.Length; index++)
                {
                    string currentJsonState = BuildJsonUserState(channels[index].ToString());
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format("\"{0}\":{{{1}}}", channels[index].ToString(), currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(",");
                        }
                        jsonStateBuilder.Append(currentJsonState);
                    }
                }

                if (jsonStateBuilder.Length > 0)
                {
                    retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
                }
            }

            return retJsonUserState;
		}

		internal string SetLocalUserState (string channel, string userStateKey, int userStateValue)
		{
			return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
		}

		internal string SetLocalUserState (string channel, string userStateKey, double userStateValue)
		{
			return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
		}

		internal string SetLocalUserState (string channel, string userStateKey, string userStateValue)
		{
			return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
		}

		internal string GetLocalUserState (string channel)
		{
            string retJsonUserState = "";
            StringBuilder jsonStateBuilder = new StringBuilder();

            jsonStateBuilder.Append(BuildJsonUserState(channel));
            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
            }
            
            return retJsonUserState;
		}

		internal string GetLocalUserState (string[] channels)
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append (BuildJsonUserState (channels));
			return builder.ToString ();
		}

        internal void SetUserState<T>(string channel, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }
            if (string.IsNullOrEmpty(jsonUserState) || string.IsNullOrEmpty(jsonUserState.Trim()))
            {
                throw new ArgumentException("Missing User State");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }
            if (_jsonPluggableLibrary == null)
            {
                throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
            }

            if (!_jsonPluggableLibrary.IsDictionaryCompatible(jsonUserState))
            {
                throw new MissingFieldException("Missing json format for user state");
            }
            else
            {
                Dictionary<string, object> deserializeUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonUserState);
                if (deserializeUserState == null)
                {
                    throw new MissingFieldException("Missing json format user state");
                }
                else
                {
                    _channelUserState.AddOrUpdate(channel.Trim(), deserializeUserState, (oldState, newState) => deserializeUserState);
                }
            }

            if (string.IsNullOrEmpty(uuid))
            {
                VerifyOrSetSessionUUID();
                uuid = this.sessionUUID;
            }

            Uri request = BuildSetUserStateRequest(channel, uuid, jsonUserState);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
            requestState.Type = ResponseType.SetUserState;
            requestState.UserCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

		internal void GetUserState<T> (string channel, string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
				throw new ArgumentException ("Missing Channel");
			}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			if (string.IsNullOrEmpty (uuid)) {
				VerifyOrSetSessionUUID ();
				uuid = this.sessionUUID;
			}

			Uri request = BuildGetUserStateRequest (channel, uuid);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			;
			requestState.Type = ResponseType.GetUserState;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T> (request, requestState);
		}

		private Uri BuildSetUserStateRequest (string channel, string uuid, string jsonUserState)
		{
			setUserStateparameters = string.Format ("?state={0}", EncodeUricomponent (jsonUserState, ResponseType.SetUserState, false));

			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (channel);
			url.Add ("uuid");
			url.Add (uuid);
			url.Add ("data");

			return BuildRestApiRequest<Uri> (url, ResponseType.SetUserState);
		}

		private Uri BuildGetUserStateRequest (string channel, string uuid)
		{
			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (channel);
			url.Add ("uuid");
			url.Add (uuid);

			return BuildRestApiRequest<Uri> (url, ResponseType.GetUserState);
		}

		#endregion

		#region "Exception handlers"

		protected void UrlRequestCommonExceptionHandler<T> (ResponseType type, string[] channels, bool requestTimeout, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback, bool resumeOnReconnect)
		{
			if (type == ResponseType.Subscribe || type == ResponseType.Presence) {
				MultiplexExceptionHandler<T> (type, channels, userCallback, connectCallback, errorCallback, false, resumeOnReconnect);
			} else if (type == ResponseType.Publish) {
				PublishExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			} else if (type == ResponseType.Here_Now) {
				HereNowExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			} else if (type == ResponseType.DetailedHistory) {
				DetailedHistoryExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			} else if (type == ResponseType.Time) {
				TimeExceptionHandler<T> (requestTimeout, errorCallback);
			} else if (type == ResponseType.Leave) {
				//no action at this time
			} else if (type == ResponseType.PresenceHeartbeat) {
				//no action at this time
			} else if (type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess) {
			} else if (type == ResponseType.GetUserState) {
				GetUserStateExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			} else if (type == ResponseType.SetUserState) {
				SetUserStateExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			} else if (type == ResponseType.GlobalHere_Now) {
				GlobalHereNowExceptionHandler<T> (requestTimeout, errorCallback);
			} else if (type == ResponseType.Where_Now) {
				WhereNowExceptionHandler<T> (channels [0], requestTimeout, errorCallback);
			}
		}

		protected void MultiplexExceptionHandler<T> (ResponseType type, string[] channels, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback, bool reconnectMaxTried, bool resumeOnReconnect)
		{
			string channel = "";
			if (channels != null) {
				channel = string.Join (",", channels);
			}

			if (reconnectMaxTried) {
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, MAX retries reached. Exiting the subscribe for channel(s) = {1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);

				string[] activeChannels = multiChannelSubscribe.Keys.ToArray<string> ();
				MultiChannelUnSubscribeInit<T> (ResponseType.Unsubscribe, string.Join (",", activeChannels), null, null, null, null);

				string[] subscribeChannels = activeChannels.Where (filterChannel => !filterChannel.Contains ("-pnpres")).ToArray ();
				string[] presenceChannels = activeChannels.Where (filterChannel => filterChannel.Contains ("-pnpres")).ToArray ();

				if (subscribeChannels != null && subscribeChannels.Length > 0) {
					for (int index = 0; index < subscribeChannels.Length; index++) {
						string message = string.Format ("Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannel = subscribeChannels [index].ToString ();

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
						callbackKey.Channel = activeChannel;
						callbackKey.Type = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
							PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.Callback != null) {
								CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
									activeChannel, currentPubnubCallback.ErrorCallback, message, 
									PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
							}
						}

						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Subscribe JSON network error response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);
					}

				}
				if (presenceChannels != null && presenceChannels.Length > 0) {
					for (int index = 0; index < presenceChannels.Length; index++) {
						string message = string.Format ("Presence Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannel = presenceChannels [index].ToString ();

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
						callbackKey.Channel = activeChannel;
						callbackKey.Type = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
							PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.Callback != null) {
								CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
									activeChannel, currentPubnubCallback.ErrorCallback, message, 
									PubnubErrorCode.PresenceUnsubscribedAfterMaxRetries, null, null);
							}
						}

						LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Presence-Subscribe JSON network error response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);
					}
				}

			} else {
				List<object> result = new List<object> ();
				result.Add ("0");
				if (resumeOnReconnect) {
					result.Add (0); //send 0 time token to enable presence event
				} else {
					result.Add (lastSubscribeTimetoken); //get last timetoken
				}
				result.Add (channels); //send channel name

				MultiplexInternalCallback<T> (type, result, userCallback, connectCallback, errorCallback);
			}
		}

		private void PublishExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, JSON publish response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message,
					PubnubErrorCode.PublishOperationTimeout, null, null);
			}
		}

		private void PAMAccessExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PAMAccessExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message,
					PubnubErrorCode.PAMAccessOperationTimeout, null, null);
			}
		}

		private void WhereNowExceptionHandler<T> (string uuid, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WhereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					uuid, errorCallback, message, PubnubErrorCode.WhereNowOperationTimeout, null, null);
			}
		}

		private void HereNowExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, HereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message,
					PubnubErrorCode.HereNowOperationTimeout, null, null);
			}
		}

		private void GlobalHereNowExceptionHandler<T> (bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GlobalHereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					"", errorCallback, message, PubnubErrorCode.GlobalHereNowOperationTimeout, null, null);
			}
		}

		private void DetailedHistoryExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, DetailedHistoryExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message, 
					PubnubErrorCode.DetailedHistoryOperationTimeout, null, null);
			}
		}

		private void TimeExceptionHandler<T> (bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, TimeExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					"", errorCallback, message, PubnubErrorCode.TimeOperationTimeout, null, null);
			}
		}

		private void SetUserStateExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SetUserStateExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message,
					PubnubErrorCode.SetUserStateTimeout, null, null);
			}
		}

		private void GetUserStateExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GetUserStateExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, errorCallback, message,
					PubnubErrorCode.GetUserStateTimeout, null, null);
			}
		}

		#endregion

		#region "Callbacks"

		protected virtual bool CheckInternetConnectionStatus<T> (bool systemActive, Action<PubnubClientError> errorCallback, string[] channels)
		{
			return ClientNetworkStatus.CheckInternetStatus<T> (pubnetSystemActive, errorCallback, channels);
		}

		protected void OnPresenceHeartbeatIntervalTimeout<T> (System.Object presenceHeartbeatState)
		{
			//Make presence heartbeat call
			RequestState<T> currentState = presenceHeartbeatState as RequestState<T>;
			if (currentState != null && currentState.Channels != null) {
				string channel = (currentState.Channels != null) ? string.Join (",", currentState.Channels) : "";

				bool networkConnection;
				if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) {
					networkConnection = true;
				} else {
					networkConnection = CheckInternetConnectionStatus<T> (pubnetSystemActive, currentState.ErrorCallback, currentState.Channels);
					if (networkConnection) {
						string[] subscriberChannels = currentState.Channels.Where (s => s.Contains ("-pnpres") == false).ToArray ();

						if (subscriberChannels != null && subscriberChannels.Length > 0) {
							Uri request = BuildPresenceHeartbeatRequest (subscriberChannels);

							RequestState<T> requestState = new RequestState<T> ();
							requestState.Channels = currentState.Channels;
							requestState.Type = ResponseType.PresenceHeartbeat;
							requestState.UserCallback = null;
							requestState.ErrorCallback = currentState.ErrorCallback;
							requestState.Reconnect = false;
                            requestState.Response = null;

							UrlProcessRequest<T> (request, requestState);
						}
					}
				}

			}

		}

		protected void OnPubnubLocalClientHeartBeatTimeoutCallback<T> (System.Object heartbeatState)
		{
			RequestState<T> currentState = heartbeatState as RequestState<T>;
			if (currentState != null) {
				string channel = (currentState.Channels != null) ? string.Join(",", currentState.Channels) : "";

				if (channelInternetStatus.ContainsKey(channel)
				        && (currentState.Type == ResponseType.Subscribe || currentState.Type == ResponseType.Presence || currentState.Type == ResponseType.PresenceHeartbeat)
				        && overrideTcpKeepAlive) 
                {
					bool networkConnection;
					if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) 
                    {
						networkConnection = true;
					} 
                    else 
                    {
						networkConnection = CheckInternetConnectionStatus<T>(pubnetSystemActive, currentState.ErrorCallback, currentState.Channels);
					}

					channelInternetStatus[channel] = networkConnection;

					LoggingMethod.WriteToLog(string.Format ("DateTime: {0}, OnPubnubLocalClientHeartBeatTimeoutCallback - Internet connection = {1}", DateTime.Now.ToString (), networkConnection), LoggingMethod.LevelVerbose);
					if (!networkConnection) 
                    {
						TerminatePendingWebRequest(currentState);
					}
				}
			}
		}

		/// <summary>
		/// Check the response of the REST API and call for re-subscribe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="multiplexResult"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="errorCallback"></param>
		protected void MultiplexInternalCallback<T> (ResponseType type, object multiplexResult, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			List<object> message = multiplexResult as List<object>;
			string[] channels = null;
			if (message != null && message.Count >= 3) {
				if (message [message.Count - 1] is string[]) {
					channels = message [message.Count - 1] as string[];
				} else {
					channels = message [message.Count - 1].ToString ().Split (',') as string[];
				}
			} else {
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Lost Channel Name for resubscribe", DateTime.Now.ToString ()), LoggingMethod.LevelError);
				return;
			}

			if (message != null && message.Count >= 3) {
				MultiChannelSubscribeRequest<T> (type, channels, (object)message [1], userCallback, connectCallback, errorCallback, false);
			}
		}

		private void ResponseToConnectCallback<T> (List<object> result, ResponseType type, string[] channels, Action<T> connectCallback)
		{
			//Check callback exists and make sure previous timetoken = 0
			if (channels != null && connectCallback != null
			       && channels.Length > 0) {
				IEnumerable<string> newChannels = from channel in multiChannelSubscribe
				                                      where channel.Value == 0
				                                      select channel.Key;
				foreach (string channel in newChannels) {
					string jsonString = "";
					List<object> connectResult = new List<object> ();
					switch (type) {
					case ResponseType.Subscribe:
						jsonString = string.Format ("[1, \"Connected\"]");
						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject (jsonString);
						connectResult.Add (channel);

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
						callbackKey.Channel = channel;
						callbackKey.Type = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
							PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null) {
								GoToCallback<T> (connectResult, currentPubnubCallback.ConnectCallback);
							}
						}
						break;
					case ResponseType.Presence:
						jsonString = string.Format ("[1, \"Presence Connected\"]");
						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject (jsonString);
						connectResult.Add (channel.Replace ("-pnpres", ""));

						PubnubChannelCallbackKey pCallbackKey = new PubnubChannelCallbackKey ();
						pCallbackKey.Channel = channel;
						pCallbackKey.Type = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (pCallbackKey)) {
							PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [pCallbackKey] as PubnubChannelCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null) {
								GoToCallback<T> (connectResult, currentPubnubCallback.ConnectCallback);
							}
						}
						break;
					default:
						break;
					}
				}
			}

		}

		protected void ProcessResponseCallbackExceptionHandler<T> (Exception ex, RequestState<T> asynchRequestState)
		{
			//common Exception handler
			if (asynchRequestState.Response != null)
				asynchRequestState.Response.Close ();

			LoggingMethod.WriteToLog (string.Format ("DateTime {0} Exception= {1} for URL: {2}", DateTime.Now.ToString (), ex.ToString (), asynchRequestState.Request.RequestUri.ToString ()), LoggingMethod.LevelError);
			UrlRequestCommonExceptionHandler<T> (asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout, asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, false);
		}

		protected abstract bool HandleWebException<T> (WebException webEx, RequestState<T> asynchRequestState, string channel);

		protected void ProcessResponseCallbackWebExceptionHandler<T> (WebException webEx, RequestState<T> asynchRequestState, string channel)
		{
			bool reconnect = false;
			LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WebException: {1}", DateTime.Now.ToString (), webEx.ToString ()), LoggingMethod.LevelError);
			if (asynchRequestState != null) {
				if (asynchRequestState.Response != null)
					asynchRequestState.Response.Close ();
				if (asynchRequestState.Request != null)
					TerminatePendingWebRequest (asynchRequestState);
			}
			//#elif (!SILVERLIGHT)
			reconnect = HandleWebException (webEx, asynchRequestState, channel);

			UrlRequestCommonExceptionHandler<T> (asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout,
				asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, reconnect);
		}

		protected void ProcessResponseCallbacks<T> (List<object> result, RequestState<T> asynchRequestState)
		{
			if (result != null && result.Count >= 1 && asynchRequestState.UserCallback != null) {
				ResponseToConnectCallback<T> (result, asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.ConnectCallback);
				ResponseToUserCallback<T> (result, asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.UserCallback);
			}
		}
		//#if (!UNITY_IOS)
		//TODO:refactor
		protected void UrlProcessResponseCallback<T> (IAsyncResult asynchronousResult)
		{
			List<object> result = new List<object> ();

			RequestState<T> asynchRequestState = asynchronousResult.AsyncState as RequestState<T>;

			string channel = "";
			if (asynchRequestState != null && asynchRequestState.Channels != null) {
				channel = string.Join (",", asynchRequestState.Channels);
			}

			PubnubWebRequest asyncWebRequest = asynchRequestState.Request as PubnubWebRequest;
			try {
				if (asyncWebRequest != null) {
					using (PubnubWebResponse asyncWebResponse = (PubnubWebResponse)asyncWebRequest.EndGetResponse (asynchronousResult)) {
						asynchRequestState.Response = asyncWebResponse;

						using (StreamReader streamReader = new StreamReader (asyncWebResponse.GetResponseStream ())) {
							if (asynchRequestState.Type == ResponseType.Subscribe || asynchRequestState.Type == ResponseType.Presence) {
								if (!overrideTcpKeepAlive && channelInternetStatus.ContainsKey (channel) && !channelInternetStatus [channel]) {
									if (asynchRequestState.Channels != null) {
										for (int index = 0; index < asynchRequestState.Channels.Length; index++) {
											string activeChannel = asynchRequestState.Channels [index].ToString ();

											string status = "Internet connection available";

											PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
											callbackKey.Channel = activeChannel;
											callbackKey.Type = asynchRequestState.Type;

											if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
												object callbackObject;
												bool channelAvailable = channelCallbacks.TryGetValue (callbackKey, out callbackObject);
												PubnubChannelCallback<T> currentPubnubCallback = null;
												if (channelAvailable) {
													currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
												}

												if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null) {
													CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
														activeChannel, asynchRequestState.ErrorCallback, 
														status, PubnubErrorCode.YesInternet, null, null);
												}
											}
										}
									}
								}

								channelInternetStatus.AddOrUpdate (channel, true, (key, oldValue) => true);
							}

							//Deserialize the result
							string jsonString = streamReader.ReadToEnd ();
							streamReader.Close ();

							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString (), channel, asynchRequestState.Type.ToString (), jsonString), LoggingMethod.LevelInfo);

							if (overrideTcpKeepAlive) {
								TerminateLocalClientHeartbeatTimer (asyncWebRequest.RequestUri);
							}

							if (asynchRequestState.Type == ResponseType.PresenceHeartbeat) {
								if (_jsonPluggableLibrary.IsDictionaryCompatible (jsonString)) {
									Dictionary<string, object> deserializeStatus = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
									int statusCode = 0; //default. assuming all is ok 
									if (deserializeStatus.ContainsKey ("status") && deserializeStatus.ContainsKey ("message")) {
										Int32.TryParse (deserializeStatus ["status"].ToString (), out statusCode);
										string statusMessage = deserializeStatus ["message"].ToString ();

										if (statusCode != 200) {
											PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType (statusCode, statusMessage);
											int pubnubStatusCode = (int)pubnubErrorType;
											string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);
                                            
											PubnubClientError error = new PubnubClientError (pubnubStatusCode, PubnubErrorSeverity.Critical, statusMessage, PubnubMessageSource.Server, asynchRequestState.Request, asynchRequestState.Response, errorDescription, channel);
											GoToCallback (error, asynchRequestState.ErrorCallback);
										}
									}
								}
							} else if (jsonString != "[]") {
								result = WrapResultBasedOnResponseType<T> (asynchRequestState.Type, jsonString, asynchRequestState.Channels, asynchRequestState.Reconnect, asynchRequestState.Timetoken, asynchRequestState.ErrorCallback);
							}
						}
						asyncWebResponse.Close ();
					}
				} else {
					LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Request aborted for channel={1}", DateTime.Now.ToString (), channel), LoggingMethod.LevelInfo);
				}

				ProcessResponseCallbacks<T> (result, asynchRequestState);

				if ((asynchRequestState.Type == ResponseType.Subscribe || asynchRequestState.Type == ResponseType.Presence) && (asynchRequestState.Channels != null) && (result != null) && (result.Count > 0)) {
					foreach (string currentChannel in asynchRequestState.Channels) {
						multiChannelSubscribe.AddOrUpdate (currentChannel, Convert.ToInt64 (result [1].ToString ()), (key, oldValue) => Convert.ToInt64 (result [1].ToString ()));
					}
				}

				switch (asynchRequestState.Type) {
				case ResponseType.Subscribe:
				case ResponseType.Presence:
					MultiplexInternalCallback<T> (asynchRequestState.Type, result, asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback);
					break;
				default:
					break;
				}
			} catch (WebException webEx) {
				HttpStatusCode currentHttpStatusCode;
				if (webEx.Response != null && asynchRequestState != null) {
					if (webEx.Response.GetType ().ToString () == "System.Net.HttpWebResponse"
					         || webEx.Response.GetType ().ToString () == "System.Net.Browser.ClientHttpWebResponse") {
						currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
					} else {
						currentHttpStatusCode = ((PubnubWebResponse)webEx.Response).HttpStatusCode;
					}
					PubnubWebResponse exceptionResponse = new PubnubWebResponse (webEx.Response, currentHttpStatusCode);
					if (exceptionResponse != null) {
						asynchRequestState.Response = exceptionResponse;

						using (StreamReader streamReader = new StreamReader (asynchRequestState.Response.GetResponseStream ())) {
							string jsonString = streamReader.ReadToEnd ();

							streamReader.Close ();

							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString (), channel, asynchRequestState.Type.ToString (), jsonString), LoggingMethod.LevelInfo);

							if (overrideTcpKeepAlive) {
								TerminateLocalClientHeartbeatTimer (asyncWebRequest.RequestUri);
							}

							if ((int)currentHttpStatusCode < 200 || (int)currentHttpStatusCode >= 300) {
								result = null;
								string errorDescription = "";
								int pubnubStatusCode = 0;

								if ((int)currentHttpStatusCode == 500 || (int)currentHttpStatusCode == 502 || (int)currentHttpStatusCode == 504 || (int)currentHttpStatusCode == 414) {
									//This status code is not giving json string.
									string statusMessage = currentHttpStatusCode.ToString ();
									PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);
									pubnubStatusCode = (int)pubnubErrorType;
									errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);
								} else if (_jsonPluggableLibrary.IsArrayCompatible (jsonString)) {
									List<object> deserializeStatus = _jsonPluggableLibrary.DeserializeToListOfObject (jsonString);
									string statusMessage = deserializeStatus [1].ToString ();
									PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);
									pubnubStatusCode = (int)pubnubErrorType;
									errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);
								} else if (_jsonPluggableLibrary.IsDictionaryCompatible (jsonString)) {
									Dictionary<string, object> deserializeStatus = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
									string statusMessage = deserializeStatus.ContainsKey ("message") ? deserializeStatus ["message"].ToString () : jsonString;
									PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);
									pubnubStatusCode = (int)pubnubErrorType;
									errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);
								}
                                else
                                {
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, jsonString);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }

								PubnubClientError error = new PubnubClientError (pubnubStatusCode, PubnubErrorSeverity.Critical, jsonString, PubnubMessageSource.Server, asynchRequestState.Request, asynchRequestState.Response, errorDescription, channel);
								GoToCallback (error, asynchRequestState.ErrorCallback);

							} else if (jsonString != "[]") 
                            {
								result = WrapResultBasedOnResponseType<T> (asynchRequestState.Type, jsonString, asynchRequestState.Channels, asynchRequestState.Reconnect, asynchRequestState.Timetoken, asynchRequestState.ErrorCallback);
							} 
                            else 
                            {
								result = null;
							}
						}
					}
					exceptionResponse.Close ();

					if (result != null && result.Count > 0) 
                    {
						ProcessResponseCallbacks<T> (result, asynchRequestState);
					}

                    if (result == null && currentHttpStatusCode == HttpStatusCode.NotFound
                        && (asynchRequestState.Type == ResponseType.Presence || asynchRequestState.Type == ResponseType.Subscribe)
                        && webEx.Response.GetType().ToString() == "System.Net.Browser.ClientHttpWebResponse")
                    {
                        ProcessResponseCallbackExceptionHandler(webEx, asynchRequestState);
                    }
				} 
                else 
                {
					if (asynchRequestState.Channels != null || asynchRequestState.Type == ResponseType.Time) 
                    {
						if (asynchRequestState.Type == ResponseType.Subscribe
						          || asynchRequestState.Type == ResponseType.Presence) 
                        {
                            if ((webEx.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                || webEx.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
                                && (webEx.Status != WebExceptionStatus.RequestCanceled)) 
                            {
								for (int index = 0; index < asynchRequestState.Channels.Length; index++) 
                                {
									string activeChannel = asynchRequestState.Channels [index].ToString ();
									PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
									callbackKey.Channel = activeChannel;
									callbackKey.Type = asynchRequestState.Type;

									if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) 
                                    {
										object callbackObject;
										bool channelAvailable = channelCallbacks.TryGetValue (callbackKey, out callbackObject);
										PubnubChannelCallback<T> currentPubnubCallback = null;
										if (channelAvailable) 
                                        {
											currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
										}
										if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
											PubnubClientError error = CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
												                                     activeChannel, currentPubnubCallback.ErrorCallback, 
												                                     webEx, asynchRequestState.Request, asynchRequestState.Response);
											LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString (), error.ToString ()), LoggingMethod.LevelInfo);
										}
									}

								}
							}
						} else {
							PubnubClientError error = CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
								                                 channel, asynchRequestState.ErrorCallback, 
								                                 webEx, asynchRequestState.Request, asynchRequestState.Response);
							LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString (), error.ToString ()), LoggingMethod.LevelInfo);
						}
					}
					ProcessResponseCallbackWebExceptionHandler<T> (webEx, asynchRequestState, channel);
				}
			} catch (Exception ex) {
				if (!pubnetSystemActive && ex.Message.IndexOf ("The IAsyncResult object was not returned from the corresponding asynchronous method on this class.") == -1) {
					if (asynchRequestState.Channels != null) {
						if (asynchRequestState.Type == ResponseType.Subscribe
						          || asynchRequestState.Type == ResponseType.Presence) {
							for (int index = 0; index < asynchRequestState.Channels.Length; index++) {
								string activeChannel = asynchRequestState.Channels [index].ToString ();

								PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
								callbackKey.Channel = activeChannel;
								callbackKey.Type = asynchRequestState.Type;

								if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
									object callbackObject;
									bool channelAvailable = channelCallbacks.TryGetValue (callbackKey, out callbackObject);
									PubnubChannelCallback<T> currentPubnubCallback = null;
									if (channelAvailable) {
										currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
									}
									if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
										CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
											activeChannel, currentPubnubCallback.ErrorCallback, ex, asynchRequestState.Request, asynchRequestState.Response);

									}
								}
							}
						} else {
							CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
								channel, asynchRequestState.ErrorCallback, ex, asynchRequestState.Request, asynchRequestState.Response);
						}

					}
				}
				ProcessResponseCallbackExceptionHandler<T> (ex, asynchRequestState);
			}
		}
		//#endif
		//TODO:refactor
		private void ResponseToUserCallback<T> (List<object> result, ResponseType type, string[] channels, Action<T> userCallback)
		{
			string[] messageChannels;
			switch (type) {
			case ResponseType.Subscribe:
			case ResponseType.Presence:
				var messages = (from item in result
				                    select item as object).ToArray ();
				if (messages != null && messages.Length > 0) {
					object[] messageList = messages [0] as object[];
					#if (USE_MiniJSON)
										int i=0;
										foreach (object o in result){
											if(i==0)
											{
												IList collection = (IList)o;
												messageList = new object[collection.Count];
												collection.CopyTo(messageList, 0);
											}
											i++;
										}
					#endif
					messageChannels = messages [2].ToString ().Split (',');

					if (messageList != null && messageList.Length > 0) {
						for (int messageIndex = 0; messageIndex < messageList.Length; messageIndex++) {
							string currentChannel = (messageChannels.Length == 1) ? (string)messageChannels [0] : (string)messageChannels [messageIndex];
							List<object> itemMessage = new List<object> ();
							if (currentChannel.Contains ("-pnpres")) {
								itemMessage.Add (messageList [messageIndex]);
							} else {
								//decrypt the subscriber message if cipherkey is available
								if (this.cipherKey.Length > 0) {
									PubnubCrypto aes = new PubnubCrypto (this.cipherKey);
									string decryptMessage = aes.Decrypt (messageList [messageIndex].ToString ());
									object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : _jsonPluggableLibrary.DeserializeToObject (decryptMessage);

									itemMessage.Add (decodeMessage);
								} else {
									itemMessage.Add (messageList [messageIndex]);
								}
							}
							itemMessage.Add (messages [1].ToString ());
							itemMessage.Add (currentChannel.Replace ("-pnpres", ""));

							PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
							callbackKey.Channel = currentChannel;
							callbackKey.Type = (currentChannel.LastIndexOf ("-pnpres") == -1) ? ResponseType.Subscribe : ResponseType.Presence;

							//callbackKey.Channel = (type == ResponseType.Subscribe) ? currentChannel.Replace("-pnpres", "") : currentChannel;
							//callbackKey.Type = (type == ResponseType.Presence && currentChannel.LastIndexOf("-pnpres") == -1) ? ResponseType.Subscribe : type;

							if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
								if ((typeof(T) == typeof(string) && channelCallbacks [callbackKey].GetType ().Name.Contains ("[System.String]")) ||
								            (typeof(T) == typeof(object) && channelCallbacks [callbackKey].GetType ().Name.Contains ("[System.Object]"))) {
									PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
									if (currentPubnubCallback != null && currentPubnubCallback.Callback != null) {
										GoToCallback<T> (itemMessage, currentPubnubCallback.Callback);
									}
								} else if (channelCallbacks [callbackKey].GetType ().FullName.Contains ("[System.String")) {
									PubnubChannelCallback<string> retryPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<string>;
									if (retryPubnubCallback != null && retryPubnubCallback.Callback != null) {
										GoToCallback (itemMessage, retryPubnubCallback.Callback);
									}
								}

							}
						}
					}
				}
				break;
			case ResponseType.Publish:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.DetailedHistory:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.Here_Now:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.GlobalHere_Now:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.Where_Now:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.Time:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			case ResponseType.Leave:
				    //No response to callback
				break;
			case ResponseType.GrantAccess:
			case ResponseType.AuditAccess:
			case ResponseType.RevokeAccess:
			case ResponseType.GetUserState:
			case ResponseType.SetUserState:
				if (result != null && result.Count > 0) {
					GoToCallback<T> (result, userCallback);
				}
				break;
			default:
				break;
			}
		}

		private void JsonResponseToCallback<T> (List<object> result, Action<T> callback)
		{
			string callbackJson = "";

			if (typeof(T) == typeof(string)) {
				callbackJson = _jsonPluggableLibrary.SerializeToJsonString (result);

				Action<string> castCallback = callback as Action<string>;
				castCallback (callbackJson);
			}
		}

		private void JsonResponseToCallback<T> (object result, Action<T> callback)
		{
			string callbackJson = "";

			if (typeof(T) == typeof(string)) {
				callbackJson = _jsonPluggableLibrary.SerializeToJsonString (result);

				Action<string> castCallback = callback as Action<string>;
				castCallback (callbackJson);
			}
		}

		protected void GoToCallback<T> (object result, Action<T> Callback)
		{
			if (Callback != null) {
				if (typeof(T) == typeof(string)) {
					JsonResponseToCallback (result, Callback);
				} else {
					Callback ((T)(object)result);
				}
			}
		}

		protected void GoToCallback (object result, Action<string> Callback)
		{
			if (Callback != null) {
				JsonResponseToCallback (result, Callback);
			}
		}

		protected void GoToCallback (object result, Action<object> Callback)
		{
			if (Callback != null) {
				Callback (result);
			}
		}

		protected void GoToCallback(PubnubClientError error, Action<PubnubClientError> Callback)
		{
			if (Callback != null && error != null) {
				if ((int)error.Severity <= (int)_errorLevel) { //Checks whether the error serverity falls in the range of error filter level
					//Do not send 107 = PubnubObjectDisposedException
					//Do not send 105 = WebRequestCancelled
					//Do not send 130 = PubnubClientMachineSleep
                    if (error.StatusCode != 107
                        && error.StatusCode != 105
                        && error.StatusCode != 130
                        && error.StatusCode != 4040) //Error Code that should not go out
                    { 
						Callback (error);
					}
				}
			}
		}

		#endregion

		#region "Simulate network fail and machine sleep"

		/// <summary>
		/// FOR TESTING ONLY - To Enable Simulation of Network Non-Availability
		/// </summary>
		public void EnableSimulateNetworkFailForTestingOnly ()
		{
			ClientNetworkStatus.SimulateNetworkFailForTesting = true;
			PubnubWebRequest.SimulateNetworkFailForTesting = true;
		}

		/// <summary>
		/// FOR TESTING ONLY - To Disable Simulation of Network Non-Availability
		/// </summary>
		public void DisableSimulateNetworkFailForTestingOnly ()
		{
			ClientNetworkStatus.SimulateNetworkFailForTesting = false;
			PubnubWebRequest.SimulateNetworkFailForTesting = false;
		}

		protected abstract void GeneratePowerSuspendEvent ();

		protected abstract void GeneratePowerResumeEvent ();

		public void EnableMachineSleepModeForTestingOnly ()
		{
			GeneratePowerSuspendEvent ();
			pubnetSystemActive = false;
		}

		public void DisableMachineSleepModeForTestingOnly ()
		{
			GeneratePowerResumeEvent ();
			pubnetSystemActive = true;
		}

		#endregion

		#region "Helpers"

		protected void VerifyOrSetSessionUUID ()
		{
			if (string.IsNullOrEmpty (this.sessionUUID) || string.IsNullOrEmpty (this.sessionUUID.Trim ())) {
				this.sessionUUID = Guid.NewGuid ().ToString ();
			}
		}

		protected bool IsUnsafe (char ch, bool ignoreComma)
		{
			if (ignoreComma) {
				return " ~`!@#$%^&*()+=[]\\{}|;':\"/<>?".IndexOf (ch) >= 0;
			} else {
				return " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?".IndexOf (ch) >= 0;
			}
		}

		public virtual Guid GenerateGuid ()
		{
			return Guid.NewGuid ();
		}

		protected int GetTimeoutInSecondsForResponseType (ResponseType type)
		{
			int timeout;
			if (type == ResponseType.Subscribe || type == ResponseType.Presence) {
				timeout = _pubnubWebRequestCallbackIntervalInSeconds;
			} else {
				timeout = _pubnubOperationTimeoutIntervalInSeconds;
			}
			return timeout;
		}

		public static long TranslateDateTimeToSeconds (DateTime dotNetUTCDateTime)
		{
			TimeSpan timeSpan = dotNetUTCDateTime - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long timeStamp = Convert.ToInt64 (timeSpan.TotalSeconds);
			return timeStamp;
		}

		/// <summary>
		/// Convert the UTC/GMT DateTime to Unix Nano Seconds format
		/// </summary>
		/// <param name="dotNetUTCDateTime"></param>
		/// <returns></returns>
		public static long TranslateDateTimeToPubnubUnixNanoSeconds (DateTime dotNetUTCDateTime)
		{
			TimeSpan timeSpan = dotNetUTCDateTime - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long timeStamp = Convert.ToInt64 (timeSpan.TotalSeconds) * 10000000;
			return timeStamp;
		}

		/// <summary>
		/// Convert the Unix Nano Seconds format time to UTC/GMT DateTime
		/// </summary>
		/// <param name="unixNanoSecondTime"></param>
		/// <returns></returns>
		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime (long unixNanoSecondTime)
		{
			double timeStamp = unixNanoSecondTime / 10000000;
			DateTime dateTime = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (timeStamp);
			return dateTime;
		}

		private bool IsPresenceChannel (string channel)
		{
			if (channel.LastIndexOf ("-pnpres") > 0) {
				return true;
			} else {
				return false;
			}
		}

		private string[] GetCurrentSubscriberChannels ()
		{
			string[] channels = null;
			if (multiChannelSubscribe != null && multiChannelSubscribe.Keys.Count > 0) {
				channels = multiChannelSubscribe.Keys.ToArray<string> ();
			}

			return channels;
		}

		/// <summary>
		/// Retrieves the channel name from the url components
		/// </summary>
		/// <param name="urlComponents"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private string GetChannelName (List<string> urlComponents, ResponseType type)
		{
			string channelName = "";
			switch (type) {
			case ResponseType.Subscribe:
			case ResponseType.Presence:
				channelName = urlComponents [2];
				break;
			case ResponseType.Publish:
				channelName = urlComponents [4];
				break;
			case ResponseType.DetailedHistory:
				channelName = urlComponents [5];
				break;
			case ResponseType.Here_Now:
				channelName = urlComponents [5];
				break;
			case ResponseType.Leave:
				channelName = urlComponents [5];
				break;
			case ResponseType.Where_Now:
				channelName = urlComponents [5];
				break;
			default:
				break;
			}
			;
			return channelName;
		}

		#endregion

		#region "PAM"

		private Uri BuildGrantAccessRequest (string channel, bool read, bool write, int ttl)
		{
			string signature = "0";
			long timeStamp = TranslateDateTimeToSeconds (DateTime.UtcNow);
			string queryString = "";
			StringBuilder queryStringBuilder = new StringBuilder ();
			if (!string.IsNullOrEmpty (_authenticationKey)) {
				queryStringBuilder.AppendFormat ("auth={0}", EncodeUricomponent (_authenticationKey, ResponseType.GrantAccess, false));
			}

			if (!string.IsNullOrEmpty (channel)) {
				queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent (channel, ResponseType.GrantAccess, false));
			}

			queryStringBuilder.AppendFormat ("{0}", (queryStringBuilder.Length > 0) ? "&" : "");
			queryStringBuilder.AppendFormat ("r={0}&timestamp={1}{2}&w={3}", Convert.ToInt32 (read), timeStamp.ToString (), (ttl > -1) ? "&ttl=" + ttl.ToString () : "", Convert.ToInt32 (write));

			if (this.secretKey.Length > 0) {
				StringBuilder string_to_sign = new StringBuilder ();
				string_to_sign.Append (this.subscribeKey)
					.Append ("\n")
						.Append (this.publishKey)
						.Append ("\n")
						.Append ("grant")
						.Append ("\n")
						.Append (queryStringBuilder.ToString ());

				PubnubCrypto pubnubCrypto = new PubnubCrypto (this.cipherKey);
				signature = pubnubCrypto.PubnubAccessManagerSign (this.secretKey, string_to_sign.ToString ());
				queryString = string.Format ("signature={0}&{1}", signature, queryStringBuilder.ToString ());
			}

			parameters = "";
			parameters += "?" + queryString;

			List<string> url = new List<string> ();
			url.Add ("v1");
			url.Add ("auth");
			url.Add ("grant");
			url.Add ("sub-key");
			url.Add (this.subscribeKey);

			return BuildRestApiRequest<Uri> (url, ResponseType.GrantAccess);
		}

		private Uri BuildAuditAccessRequest (string channel)
		{
			string signature = "0";
			long timeStamp = ((_pubnubUnitTest == null) || (_pubnubUnitTest is IPubnubUnitTest && !_pubnubUnitTest.EnableStubTest))
				? TranslateDateTimeToSeconds (DateTime.UtcNow) 
					: TranslateDateTimeToSeconds (new DateTime (2013, 01, 01));
			string queryString = "";
			StringBuilder queryStringBuilder = new StringBuilder ();
			if (!string.IsNullOrEmpty (_authenticationKey)) {
				queryStringBuilder.AppendFormat ("auth={0}", EncodeUricomponent (_authenticationKey, ResponseType.AuditAccess, false));
			}
			if (!string.IsNullOrEmpty (channel)) {
				queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent (channel, ResponseType.AuditAccess, false));
			}
			queryStringBuilder.AppendFormat ("{0}timestamp={1}", (queryStringBuilder.Length > 0) ? "&" : "", timeStamp.ToString ());

			if (this.secretKey.Length > 0) {
				StringBuilder string_to_sign = new StringBuilder ();
				string_to_sign.Append (this.subscribeKey)
					.Append ("\n")
						.Append (this.publishKey)
						.Append ("\n")
						.Append ("audit")
						.Append ("\n")
						.Append (queryStringBuilder.ToString ());

				PubnubCrypto pubnubCrypto = new PubnubCrypto (this.cipherKey);
				signature = pubnubCrypto.PubnubAccessManagerSign (this.secretKey, string_to_sign.ToString ());
				queryString = string.Format ("signature={0}&{1}", signature, queryStringBuilder.ToString ());
			}

			parameters = "";
			parameters += "?" + queryString;

			List<string> url = new List<string> ();
			url.Add ("v1");
			url.Add ("auth");
			url.Add ("audit");
			url.Add ("sub-key");
			url.Add (this.subscribeKey);

			return BuildRestApiRequest<Uri> (url, ResponseType.AuditAccess);
		}

		public bool GrantAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return GrantAccess (channel, read, write, -1, userCallback, errorCallback);
		}

		public bool GrantAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (this.secretKey) || string.IsNullOrEmpty (this.secretKey.Trim ()) || this.secretKey.Length <= 0) {
				throw new MissingFieldException ("Invalid secret key");
			}

			Uri request = BuildGrantAccessRequest (channel, read, write, ttl);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			requestState.Type = ResponseType.GrantAccess;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState); 
		}

		public bool GrantPresenceAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return GrantPresenceAccess (channel, read, write, -1, userCallback, errorCallback);
		}

		public bool GrantPresenceAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			string[] multiChannels = channel.Split (',');
			if (multiChannels.Length > 0) {
				for (int index = 0; index < multiChannels.Length; index++) {
					if (!string.IsNullOrEmpty (multiChannels [index]) && multiChannels [index].Trim ().Length > 0) {
						multiChannels [index] = string.Format ("{0}-pnpres", multiChannels [index]);
					} else {
						throw new MissingFieldException ("Invalid channel");
					}
				}
			}
			string presenceChannel = string.Join (",", multiChannels);
			return GrantAccess (presenceChannel, read, write, ttl, userCallback, errorCallback);
		}

		public void AuditAccess<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			AuditAccess ("", userCallback, errorCallback);
		}

		public void AuditAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (this.secretKey) || string.IsNullOrEmpty (this.secretKey.Trim ()) || this.secretKey.Length <= 0) {
				throw new MissingFieldException ("Invalid secret key");
			}

			Uri request = BuildAuditAccessRequest (channel);

			RequestState<T> requestState = new RequestState<T> ();
			if (!string.IsNullOrEmpty (channel)) {
				requestState.Channels = new string[] { channel };
			}
			requestState.Type = ResponseType.AuditAccess;
			requestState.UserCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T> (request, requestState);
		}

		public void AuditPresenceAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			string[] multiChannels = channel.Split (',');
			if (multiChannels.Length > 0) {
				for (int index = 0; index < multiChannels.Length; index++) {
					multiChannels [index] = string.Format ("{0}-pnpres", multiChannels [index]);
				}
			}
			string presenceChannel = string.Join (",", multiChannels);
			AuditAccess (presenceChannel, userCallback, errorCallback);
		}

		#endregion

		#region "Response"

		protected void OnPubnubWebRequestTimeout<T> (object state, bool timeout)
		{
			if (timeout && state != null) {
				RequestState<T> currentState = state as RequestState<T>;
				if (currentState != null) {
					PubnubWebRequest request = currentState.Request;
					if (request != null) {
						string currentMultiChannel = (currentState.Channels == null) ? "" : string.Join (",", currentState.Channels);
						LoggingMethod.WriteToLog (string.Format ("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel = {1}", DateTime.Now.ToString (), currentMultiChannel), LoggingMethod.LevelInfo);
						currentState.Timeout = true;
						TerminatePendingWebRequest (currentState);
					}
				} else {
					LoggingMethod.WriteToLog (string.Format ("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached. However state is unknown", DateTime.Now.ToString ()), LoggingMethod.LevelError);
				}
			}
		}

		protected void OnPubnubWebRequestTimeout<T> (System.Object requestState)
		{
			RequestState<T> currentState = requestState as RequestState<T>;
			if (currentState != null && currentState.Response == null && currentState.Request != null) {
				currentState.Timeout = true;
				TerminatePendingWebRequest (currentState);
				LoggingMethod.WriteToLog (string.Format ("DateTime: {0}, **WP7 OnPubnubWebRequestTimeout**", DateTime.Now.ToString ()), LoggingMethod.LevelError);
			}
		}

		/// <summary>
		/// Gets the result by wrapping the json response based on the request
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="jsonString"></param>
		/// <param name="channels"></param>
		/// <param name="reconnect"></param>
		/// <param name="lastTimetoken"></param>
		/// <param name="errorCallback"></param>
		/// <returns></returns>
		protected List<object> WrapResultBasedOnResponseType<T> (ResponseType type, string jsonString, string[] channels, bool reconnect, long lastTimetoken, Action<PubnubClientError> errorCallback)
		{
			List<object> result = new List<object> ();

			try {
				string multiChannel = (channels != null) ? string.Join (",", channels) : "";
				if (!string.IsNullOrEmpty (jsonString)) {
					if (!string.IsNullOrEmpty (jsonString)) {
						object deSerializedResult = _jsonPluggableLibrary.DeserializeToObject (jsonString);
						List<object> result1 = ((IEnumerable)deSerializedResult).Cast<object> ().ToList ();

						if (result1 != null && result1.Count > 0) {
							result = result1;
						}

						switch (type) {
						case ResponseType.Publish:
							result.Add (multiChannel);
							break;
						case ResponseType.History:
							if (this.cipherKey.Length > 0) {
								List<object> historyDecrypted = new List<object> ();
								PubnubCrypto aes = new PubnubCrypto (this.cipherKey);
								foreach (object message in result) {
									historyDecrypted.Add (aes.Decrypt (message.ToString ()));
								}
								History = historyDecrypted;
							} else {
								History = result;
							}
							break;
						case ResponseType.DetailedHistory:
							result = DecodeDecryptLoop (result, channels, errorCallback);
							result.Add (multiChannel);
							break;
						case ResponseType.Here_Now:
							Dictionary<string, object> dictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (dictionary);
							result.Add (multiChannel);
							break;
						case ResponseType.GlobalHere_Now:
							Dictionary<string, object> globalHereNowDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (globalHereNowDictionary);
							break;
						case ResponseType.Where_Now:
							Dictionary<string, object> whereNowDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (whereNowDictionary);
							result.Add (multiChannel);
							break;
						case ResponseType.Time:
							break;
						case ResponseType.Subscribe:
						case ResponseType.Presence:
							result.Add (multiChannel);
							long receivedTimetoken = (result.Count > 1) ? Convert.ToInt64 (result [1].ToString ()) : 0;
							long minimumTimetoken = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Min (token => token.Value) : 0;
							long maximumTimetoken = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Max (token => token.Value) : 0;

							if (minimumTimetoken == 0 || lastTimetoken == 0) {
								if (maximumTimetoken == 0) {
									lastSubscribeTimetoken = receivedTimetoken;
								} else {
									if (!_enableResumeOnReconnect) {
										lastSubscribeTimetoken = receivedTimetoken;
									} else {
										//do nothing. keep last subscribe token
									}
								}
							} else {
								if (reconnect) {
									if (_enableResumeOnReconnect) {
										//do nothing. keep last subscribe token
									} else {
										lastSubscribeTimetoken = receivedTimetoken;
									}
								} else {
									lastSubscribeTimetoken = receivedTimetoken;
								}
							}
							break;
						case ResponseType.Leave:
							result.Add (multiChannel);
							break;
						case ResponseType.GrantAccess:
						case ResponseType.AuditAccess:
						case ResponseType.RevokeAccess:
							Dictionary<string, object> grantDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (grantDictionary);
							result.Add (multiChannel);
							break;
						case ResponseType.GetUserState:
						case ResponseType.SetUserState:
							Dictionary<string, object> userStateDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (userStateDictionary);
							result.Add (multiChannel);
							break;
						default:
							break;
						}
						;//switch stmt end
					}
				}
			} catch (Exception ex) {
				if (channels != null) {
					if (type == ResponseType.Subscribe
					         || type == ResponseType.Presence) {
						for (int index = 0; index < channels.Length; index++) {
							string activeChannel = channels [index].ToString ();
							PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
							callbackKey.Channel = activeChannel;
							callbackKey.Type = type;

							if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
								PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
									CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										activeChannel, currentPubnubCallback.ErrorCallback, ex, null, null);
								}
							}
						}
					} else {
						if (errorCallback != null) {
							CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
								string.Join (",", channels), errorCallback, ex, null, null);
						}
					}
				}
			}
			return result;
		}

		#endregion

		#region "Build, process and send request"

		protected virtual void ForceCanonicalPathAndQuery (Uri requestUri)
		{
			//#if ((!__MonoCS__) && (!SILVERLIGHT) && (!WINDOWS_PHONE) && (!UNITY_STANDALONE) && (!UNITY_WEBPLAYER) && (!UNITY_IOS) && (!UNITY_ANDROID))

			// Force canonical path and query
			FieldInfo flagsFieldInfo = typeof(Uri).GetField ("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			ulong flags = (ulong)flagsFieldInfo.GetValue (requestUri);
			flags &= ~((ulong)0x30);
			// Flags.PathNotCanonical|Flags.QueryNotCanonical
			flagsFieldInfo.SetValue (requestUri, flags);
			//#endif
		}

		protected abstract PubnubWebRequest SetProxy<T> (PubnubWebRequest request);

		protected abstract PubnubWebRequest SetTimeout<T> (RequestState<T> pubnubRequestState, PubnubWebRequest request);

		protected virtual void TimerWhenOverrideTcpKeepAlive<T> (Uri requestUri, RequestState<T> pubnubRequestState)
		{
			//Eventhough heart-beat is disabled, run one time to check internet connection by setting dueTime=0
			localClientHeartBeatTimer = new System.Threading.Timer (
				new TimerCallback (OnPubnubLocalClientHeartBeatTimeoutCallback<T>), pubnubRequestState, 0,
				(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
			channelLocalClientHeartbeatTimer.AddOrUpdate (requestUri, localClientHeartBeatTimer, (key, oldState) => localClientHeartBeatTimer);
		}

		protected abstract PubnubWebRequest SetServicePointSetTcpKeepAlive (PubnubWebRequest request);
		/*{
					#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE)
					request.ServicePoint.SetTcpKeepAlive(true, _pubnubNetworkTcpCheckIntervalInSeconds * 1000, 1000);
					#endif
				}*/
		protected virtual void SendRequestAndGetResult<T> (Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request)
		{
			IAsyncResult asyncResult = request.BeginGetResponse (new AsyncCallback (UrlProcessResponseCallback<T>), pubnubRequestState);
			ThreadPool.RegisterWaitForSingleObject (asyncResult.AsyncWaitHandle, new WaitOrTimerCallback (OnPubnubWebRequestTimeout<T>), pubnubRequestState, GetTimeoutInSecondsForResponseType (pubnubRequestState.Type) * 1000, true);
		}

		private bool UrlProcessRequest<T> (Uri requestUri, RequestState<T> pubnubRequestState)
		{
			string channel = "";
			if (pubnubRequestState != null && pubnubRequestState.Channels != null) {
				channel = string.Join (",", pubnubRequestState.Channels);
			}

			try {
				if (!_channelRequest.ContainsKey (channel) && (pubnubRequestState.Type == ResponseType.Subscribe || pubnubRequestState.Type == ResponseType.Presence)) {
					return false;
				}

				// Create Request
				PubnubWebRequestCreator requestCreator = new PubnubWebRequestCreator (_pubnubUnitTest);
				PubnubWebRequest request = (PubnubWebRequest)requestCreator.Create (requestUri);

				request = SetProxy<T> (request);
				request = SetTimeout<T> (pubnubRequestState, request);

				pubnubRequestState.Request = request;

				if (pubnubRequestState.Type == ResponseType.Subscribe || pubnubRequestState.Type == ResponseType.Presence) {
					_channelRequest.AddOrUpdate (channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
				}


				if (overrideTcpKeepAlive) {
					TimerWhenOverrideTcpKeepAlive (requestUri, pubnubRequestState);
				} else {
					request = SetServicePointSetTcpKeepAlive (request);
				}
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Request={1}", DateTime.Now.ToString (), requestUri.ToString ()), LoggingMethod.LevelInfo);


				SendRequestAndGetResult (requestUri, pubnubRequestState, request);

				return true;
			} catch (System.Exception ex) {
				if (pubnubRequestState != null && pubnubRequestState.ErrorCallback != null) {
					string multiChannel = (pubnubRequestState.Channels != null) ? string.Join (",", pubnubRequestState.Channels) : "";

					CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
						multiChannel, pubnubRequestState.ErrorCallback, ex, pubnubRequestState.Request, pubnubRequestState.Response);
				}
				LoggingMethod.WriteToLog (string.Format ("DateTime {0} Exception={1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
				UrlRequestCommonExceptionHandler<T> (pubnubRequestState.Type, pubnubRequestState.Channels, false, pubnubRequestState.UserCallback, pubnubRequestState.ConnectCallback, pubnubRequestState.ErrorCallback, false);
				return false;
			}
		}

		private Uri BuildRestApiRequest<T> (List<string> urlComponents, ResponseType type)
		{
			VerifyOrSetSessionUUID ();

			return BuildRestApiRequest<T> (urlComponents, type, this.sessionUUID);
		}

		private Uri BuildRestApiRequest<T> (List<string> urlComponents, ResponseType type, string uuid)
		{
			bool queryParamExist = false;
			StringBuilder url = new StringBuilder ();

			if (string.IsNullOrEmpty (uuid)) {
				VerifyOrSetSessionUUID ();
				uuid = this.sessionUUID;
			}

			// Add http or https based on SSL flag
			if (this.ssl) {
				url.Append ("https://");
			} else {
				url.Append ("http://");
			}

			// Add Origin To The Request
			url.Append (this._origin);

			// Generate URL with UTF-8 Encoding
			for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++) 
            {
				url.Append ("/");

				if (type == ResponseType.Publish && componentIndex == urlComponents.Count - 1) {
					url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, false));
				} else {
					url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, true));
				}
			}

			if (type == ResponseType.Presence || type == ResponseType.Subscribe || type == ResponseType.Leave) 
            {
				queryParamExist = true;
				url.AppendFormat("?uuid={0}", uuid);
				url.Append(subscribeParameters);
				if (!string.IsNullOrEmpty(_authenticationKey)) {
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
				if (_pubnubPresenceHeartbeatInSeconds != 0) {
					url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
				}
			}

			if (type == ResponseType.PresenceHeartbeat) 
            {
				queryParamExist = true;
				url.AppendFormat("?uuid={0}", uuid);
				url.Append(presenceHeartbeatParameters);
				if (_pubnubPresenceHeartbeatInSeconds != 0) 
                {
					url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
				}
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
			}
			if (type == ResponseType.SetUserState) 
            {
				queryParamExist = true;
				url.Append(setUserStateparameters);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
			}
			if (type == ResponseType.GetUserState) 
            {
				if (!string.IsNullOrEmpty (_authenticationKey)) 
                {
					queryParamExist = true;
					url.AppendFormat ("?auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}

			}

			if (type == ResponseType.Here_Now) 
            {
				url.Append(hereNowParameters);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					queryParamExist = true;
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
			}
			if (type == ResponseType.GlobalHere_Now) 
            {
				url.Append(globalHereNowParameters);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					queryParamExist = true;
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
			}
			if (type == ResponseType.Where_Now) 
            {
				if (!string.IsNullOrEmpty(_authenticationKey)) {
					queryParamExist = true;
					url.AppendFormat ("?auth={0}", EncodeUricomponent(_authenticationKey, type, false));
				}
			}

			if (type == ResponseType.Publish && !string.IsNullOrEmpty(_authenticationKey)) 
            {
				queryParamExist = true;
				url.AppendFormat("?auth={0}", EncodeUricomponent(_authenticationKey, type, false));
			}

			if (type == ResponseType.DetailedHistory || type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess) {
				url.Append(parameters);
				queryParamExist = true;
			}

			Uri requestUri = new Uri (url.ToString());

			if ((type == ResponseType.Publish || type == ResponseType.Subscribe || type == ResponseType.Presence)) 
            {
				ForceCanonicalPathAndQuery(requestUri);
			}

			return requestUri;

		}

		#endregion

	}
	#region "Unit test interface"
	public interface IPubnubUnitTest
	{
		bool EnableStubTest {
			get;
			set;
		}

		string TestClassName {
			get;
			set;
		}

		string TestCaseName {
			get;
			set;
		}

		string GetStubResponse (HttpWebRequest request);
	}
	#endregion
	#region "Webrequest and webresponse"
	internal abstract class PubnubWebRequestCreatorBase : IWebRequestCreate
	{
		private IPubnubUnitTest pubnubUnitTest = null;

		public PubnubWebRequestCreatorBase ()
		{
		}

		public PubnubWebRequestCreatorBase (IPubnubUnitTest pubnubUnitTest)
		{
			this.pubnubUnitTest = pubnubUnitTest;
		}

		protected abstract HttpWebRequest SetUserAgent (HttpWebRequest req, bool keepAliveRequest, OperatingSystem userOS);

        protected abstract HttpWebRequest SetNoCache(HttpWebRequest req, bool nocache);

        private WebRequest CreateRequest(Uri uri, bool keepAliveRequest, bool nocache)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            OperatingSystem userOS = System.Environment.OSVersion;

            req = SetUserAgent(req, keepAliveRequest, userOS);

            req = SetNoCache(req, nocache);
            if (this.pubnubUnitTest is IPubnubUnitTest)
            {
                return new PubnubWebRequest(req, pubnubUnitTest);
            }
            else
            {
                return new PubnubWebRequest(req);
            }
        }

        public WebRequest Create(Uri uri)
        {
            return CreateRequest(uri, true, true);
        }
        public WebRequest Create(Uri uri, bool keepAliveRequest)
        {
            return CreateRequest(uri, keepAliveRequest, true);
        }
        public WebRequest Create(Uri uri, bool keepAliveRequest, bool nocache)
        {
            return CreateRequest(uri, keepAliveRequest, nocache);
        }
	}

	public abstract class PubnubWebRequestBase : WebRequest
	{
		internal IPubnubUnitTest pubnubUnitTest = null;
		private static bool simulateNetworkFailForTesting = false;
		private static bool machineSuspendMode = false;
		private bool terminated = false;
		PubnubErrorFilter.Level filterErrorLevel = PubnubErrorFilter.Level.Info;
		internal HttpWebRequest request;

		internal static bool SimulateNetworkFailForTesting 
        {
			get 
            {
				return simulateNetworkFailForTesting;
			}
			set 
            {
				simulateNetworkFailForTesting = value;
			}
		}

		internal static bool MachineSuspendMode {
			get {
				return machineSuspendMode;
			}
			set {
				machineSuspendMode = value;
			}
		}

		public PubnubWebRequestBase (HttpWebRequest request)
		{
			this.request = request;
		}

		public PubnubWebRequestBase (HttpWebRequest request, IPubnubUnitTest pubnubUnitTest)
		{
			this.request = request;
			this.pubnubUnitTest = pubnubUnitTest;
		}

		public override void Abort ()
		{
			if (request != null) {
				terminated = true;
				request.Abort ();
			}
		}

		public void Abort (Action<PubnubClientError> errorCallback, PubnubErrorFilter.Level errorLevel)
		{
			if (request != null) {
				terminated = true;
				try {
					request.Abort ();
				} catch (WebException webEx) {
					if (errorCallback != null) {
						HttpStatusCode currentHttpStatusCode;

						filterErrorLevel = errorLevel;
						if (webEx.Response.GetType ().ToString () == "System.Net.HttpWebResponse"
						          || webEx.Response.GetType ().ToString () == "System.Net.Browser.ClientHttpWebResponse") {
							currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
						} else {
							currentHttpStatusCode = ((PubnubWebResponse)webEx.Response).HttpStatusCode;
						}
						string statusMessage = currentHttpStatusCode.ToString ();
						PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);
						int pubnubStatusCode = (int)pubnubErrorType;
						string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);

						PubnubClientError error = new PubnubClientError (pubnubStatusCode, PubnubErrorSeverity.Critical, true, webEx.Message, webEx, PubnubMessageSource.Client, null, null, errorDescription, "");
						GoToCallback (error, errorCallback);
					}
				} catch (Exception ex) {
					if (errorCallback != null) {
						filterErrorLevel = errorLevel;
						PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);
						int statusCode = (int)errorType;
						string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);
						PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Critical, true, ex.Message, ex, PubnubMessageSource.Client, null, null, errorDescription, "");
						GoToCallback (error, errorCallback);
					}
				}
			}
		}

		private void GoToCallback (PubnubClientError error, Action<PubnubClientError> Callback)
		{
			if (Callback != null && error != null) {
				if ((int)error.Severity <= (int)filterErrorLevel) { //Checks whether the error serverity falls in the range of error filter level
					//Do not send 107 = PubnubObjectDisposedException
					//Do not send 105 = WebRequestCancelled
					//Do not send 130 = PubnubClientMachineSleep
					if (error.StatusCode != 107
					         && error.StatusCode != 105
					         && error.StatusCode != 130) { //Error Code that should not go out
						Callback (error);
					}
				}
			}
		}

		public override WebHeaderCollection Headers {
			get {
				return request.Headers;
			}
			set {
				request.Headers = value;
			}
		}

		public override string Method {
			get {
				return request.Method;
			}
			set {
				request.Method = value;
			}
		}

		public override string ContentType {
			get {
				return request.ContentType;
			}
			set {
				request.ContentType = value;
			}
		}

		public override ICredentials Credentials {
			get {
				return request.Credentials;
			}
			set {
				request.Credentials = value;
			}
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			return request.BeginGetRequestStream (callback, state);
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			return request.EndGetRequestStream (asyncResult);
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest) {
				return new PubnubWebAsyncResult (callback, state);
			} else if (machineSuspendMode) {
				return new PubnubWebAsyncResult (callback, state);
			} else {
				return request.BeginGetResponse (callback, state);
			}
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest) {
				string stubResponse = pubnubUnitTest.GetStubResponse (request);
				return new PubnubWebResponse (new MemoryStream (Encoding.UTF8.GetBytes (stubResponse)));
			} else if (machineSuspendMode) {
				WebException simulateException = new WebException ("Machine suspend mode enabled. No request will be processed.", WebExceptionStatus.Pending);
				throw simulateException;
			} else if (simulateNetworkFailForTesting) {
				WebException simulateException = new WebException ("For simulating network fail, the remote name could not be resolved", WebExceptionStatus.ConnectFailure);
				throw simulateException;
			} else {
				return new PubnubWebResponse (request.EndGetResponse (asyncResult));
			}
		}

		public override Uri RequestUri {
			get {
				return request.RequestUri;
			}
		}

		public override bool UseDefaultCredentials {
			get {
				return request.UseDefaultCredentials;
			}
		}

		public bool Terminated {
			get {
				return terminated;
			}
		}
	}

	public class PubnubWebResponse : WebResponse
	{
		WebResponse response;
		readonly Stream _responseStream;
		HttpStatusCode httpStatusCode;

		public PubnubWebResponse (WebResponse response)
		{
			this.response = response;
		}

		public PubnubWebResponse (WebResponse response, HttpStatusCode statusCode)
		{
			this.response = response;
			this.httpStatusCode = statusCode;
		}

		public PubnubWebResponse (Stream responseStream)
		{
			_responseStream = responseStream;
		}

		public PubnubWebResponse (Stream responseStream, HttpStatusCode statusCode)
		{
			_responseStream = responseStream;
			this.httpStatusCode = statusCode;
		}

		public override Stream GetResponseStream ()
		{
			if (response != null)
				return response.GetResponseStream ();
			else
				return _responseStream;
		}

		public override void Close ()
		{
			if (response != null) {
				response.Close ();
			}
		}

		public override WebHeaderCollection Headers {
			get {
				return response.Headers;
			}
		}

		public override long ContentLength {
			get {
				return response.ContentLength;
			}
		}

		public override string ContentType {
			get {
				return response.ContentType;
			}
		}

		public override Uri ResponseUri {
			get {
				return response.ResponseUri;
			}
		}

		public HttpStatusCode HttpStatusCode {
			get {
				return httpStatusCode;
			}
		}
	}

	internal class PubnubWebAsyncResult : IAsyncResult
	{
		private const int pubnubDefaultLatencyInMilliSeconds = 1;
		//PubnubDefaultLatencyInMilliSeconds
		private readonly AsyncCallback _callback;
		private readonly object _state;
		private readonly ManualResetEvent _waitHandle;
		private readonly Timer _timer;

		public bool IsCompleted { 
			get;
			private set; 
		}

		public WaitHandle AsyncWaitHandle {
			get { return _waitHandle; }
		}

		public object AsyncState {
			get { return _state; }
		}

		public bool CompletedSynchronously {
			get { return IsCompleted; }
		}

		public PubnubWebAsyncResult (AsyncCallback callback, object state)
			: this (callback, state, TimeSpan.FromMilliseconds (pubnubDefaultLatencyInMilliSeconds))
		{
		}

		public PubnubWebAsyncResult (AsyncCallback callback, object state, TimeSpan latency)
		{
			IsCompleted = false;
			_callback = callback;
			_state = state;
			_waitHandle = new ManualResetEvent (false);
			_timer = new Timer (onTimer => NotifyComplete (), null, latency, TimeSpan.FromMilliseconds (-1));
		}

		public void Abort ()
		{
			_timer.Dispose ();
			NotifyComplete ();
		}

		private void NotifyComplete ()
		{
			IsCompleted = true;
			_waitHandle.Set ();
			if (_callback != null)
				_callback (this);
		}
	}
	#endregion
	#region "Proxy"
	public class PubnubProxy
	{
		string proxyServer;
		int proxyPort;
		string proxyUserName;
		string proxyPassword;

		public string ProxyServer {
			get {
				return proxyServer;
			}
			set {
				proxyServer = value;
			}
		}

		public int ProxyPort {
			get {
				return proxyPort;
			}
			set {
				proxyPort = value;
			}
		}

		public string ProxyUserName {
			get {
				return proxyUserName;
			}
			set {
				proxyUserName = value;
			}
		}

		public string ProxyPassword {
			get {
				return proxyPassword;
			}
			set {
				proxyPassword = value;
			}
		}
	}
	#endregion
	#region "Json Pluggable Library"
	public interface IJsonPluggableLibrary
	{
		bool IsArrayCompatible (string jsonString);

		bool IsDictionaryCompatible (string jsonString);

		string SerializeToJsonString (object objectToSerialize);

		List<object> DeserializeToListOfObject (string jsonString);

		object DeserializeToObject (string jsonString);
		//T DeserializeToObject<T>(string jsonString);
		Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString);
	}
	#if (USE_JSONFX)|| (USE_JSONFX_UNITY)
	public class JsonFXDotNet : IJsonPluggableLibrary
	{
		public bool IsArrayCompatible (string jsonString)
		{
			return false;
		}

		public bool IsDictionaryCompatible (string jsonString)
		{
			return true;
		}

		public string SerializeToJsonString (object objectToSerialize)
		{
			#if(__MonoCS__)
			var writer = new JsonFx.Json.JsonWriter ();
			string json = writer.Write (objectToSerialize);
			return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
			#else
			string json = "";
			var resolver = new JsonFx.Serialization.Resolvers.CombinedResolverStrategy(new JsonFx.Serialization.Resolvers.DataContractResolverStrategy());
			JsonFx.Serialization.DataWriterSettings dataWriterSettings = new JsonFx.Serialization.DataWriterSettings(resolver);
			var writer = new JsonFx.Json.JsonWriter(dataWriterSettings, new string[] { "PubnubClientError" });
			json = writer.Write(objectToSerialize);

			return json;
			#endif
		}

		public List<object> DeserializeToListOfObject (string jsonString)
		{
			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars (jsonString);
			var reader = new JsonFx.Json.JsonReader ();
			var output = reader.Read<List<object>> (jsonString);
			return output;
		}

		public object DeserializeToObject (string jsonString)
		{
			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars (jsonString);
			var reader = new JsonFx.Json.JsonReader ();
			var output = reader.Read<object> (jsonString);
			return output;
		}

		public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
		{
			#if USE_JSONFX_UNITY
			LoggingMethod.WriteToLog ("jsonstring:"+jsonString, LoggingMethod.LevelInfo);
			object obj = DeserializeToObject(jsonString);
			Dictionary<string, object> stateDictionary = new Dictionary<string, object> ();
			Dictionary<string, object> message = (Dictionary<string, object>)obj;
			if(message != null){
				foreach (KeyValuePair<String, object> kvp in message) {
					stateDictionary.Add (kvp.Key, kvp.Value);
				}
			}
			return stateDictionary;
			#else
			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars (jsonString);
			var reader = new JsonFx.Json.JsonReader ();
			var output = reader.Read<object> (jsonString);
			Type valueType = null;
			valueType = output.GetType ();
			var expectedType = typeof(System.Dynamic.ExpandoObject);
			if (expectedType.IsAssignableFrom (valueType)) {
				var d = output as IDictionary<string, object>;
				Dictionary<string, object> stateDictionary = new Dictionary<string, object> ();
				foreach (KeyValuePair<string, object> kvp in d) {
					stateDictionary.Add (kvp.Key, kvp.Value);
				}
				return stateDictionary;
			} else {
				LoggingMethod.WriteToLog ("jsonstring:"+jsonString, LoggingMethod.LevelInfo);
				object obj = DeserializeToObject(jsonString);
				Dictionary<string, object> stateDictionary  = new Dictionary<string, object> ();
				Dictionary<string, object> message = (Dictionary<string, object>)obj;
				if(message != null){
					foreach (KeyValuePair<String, object> kvp in message) {
						stateDictionary.Add (kvp.Key, kvp.Value);
					}
				}
				return stateDictionary;
			}
			#endif
		}
	}
	#elif (USE_DOTNET_SERIALIZATION)
	public class JscriptSerializer : IJsonPluggableLibrary
	{
		public bool IsArrayCompatible(string jsonString){
			return false;
		}
		public bool IsDictionaryCompatible(string jsonString){
			return false;
		}

		public string SerializeToJsonString(object objectToSerialize)
		{
			JavaScriptSerializer jS = new JavaScriptSerializer();
			return jS.Serialize(objectToSerialize);
		}

		public List<object> DeserializeToListOfObject(string jsonString)
		{
			JavaScriptSerializer jS = new JavaScriptSerializer();
			return (List<object>)jS.Deserialize<List<object>>(jsonString);
		}

		public object DeserializeToObject(string jsonString)
		{
			JavaScriptSerializer jS = new JavaScriptSerializer();
			return (object)jS.Deserialize<object>(jsonString);
		}

		public Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
		{
			JavaScriptSerializer jS = new JavaScriptSerializer();
			return (Dictionary<string, object>)jS.Deserialize<Dictionary<string, object>>(jsonString);
		}
	}
	#elif (USE_MiniJSON)
	public class MiniJSONObjectSerializer : IJsonPluggableLibrary
	{
		public bool IsArrayCompatible(string jsonString){
			return false;
		}
		public bool IsDictionaryCompatible(string jsonString){
			return true;
		}

		public string SerializeToJsonString(object objectToSerialize)
		{
			string json =  Json.Serialize(objectToSerialize); 
			return PubnubCryptoBase.ConvertHexToUnicodeChars(json);
		}

		public List<object> DeserializeToListOfObject(string jsonString)
		{
			return Json.Deserialize(jsonString) as List<object>;
		}

		public object DeserializeToObject (string jsonString)
		{
			return Json.Deserialize (jsonString) as object;
		}

		public Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
		{
			return Json.Deserialize(jsonString) as Dictionary<string, object>;
		}
	}
	#elif (USE_JSONFX_UNITY_IOS)
	public class JsonFxUnitySerializer : IJsonPluggableLibrary
	{
		public bool IsArrayCompatible (string jsonString)
		{
				return false;
		}

		public bool IsDictionaryCompatible (string jsonString)
		{
				return true;
		}

		public string SerializeToJsonString (object objectToSerialize)
		{
				string json = JsonWriter.Serialize (objectToSerialize); 
				return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
		}

		public List<object> DeserializeToListOfObject (string jsonString)
		{
				var output = JsonReader.Deserialize<object[]> (jsonString) as object[];
				List<object> messageList = output.Cast<object> ().ToList ();
				return messageList;
		}

		public object DeserializeToObject (string jsonString)
		{
				var output = JsonReader.Deserialize<object> (jsonString) as object;
				return output;
		}

		public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
		{
			LoggingMethod.WriteToLog ("jsonstring:"+jsonString, LoggingMethod.LevelInfo);					
			object obj = DeserializeToObject(jsonString);
			Dictionary<string, object> stateDictionary = new Dictionary<string, object> ();
			Dictionary<string, object> message = (Dictionary<string, object>)obj;
			if(message != null){
				foreach (KeyValuePair<String, object> kvp in message) {
					stateDictionary.Add (kvp.Key, kvp.Value);
				}
			}
			return stateDictionary;
		}
	}
	#else
	public class NewtonsoftJsonDotNet : IJsonPluggableLibrary
	{
	#region IJsonPlugableLibrary methods implementation
				
		public bool IsArrayCompatible (string jsonString)
		{
			bool ret = false;
			JsonTextReader reader = new JsonTextReader (new StringReader (jsonString));
			while (reader.Read ()) {
				if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartArray) {
					ret = true;
					break;
				} else {
					break;
				}
			}

			return ret;
		}

		public bool IsDictionaryCompatible (string jsonString)
		{
			bool ret = false;
			JsonTextReader reader = new JsonTextReader (new StringReader (jsonString));
			while (reader.Read ()) {
				if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartObject) {
					ret = true;
					break;
				} else {
					break;
				}
			}

			return ret;
		}

		public string SerializeToJsonString (object objectToSerialize)
		{
			return JsonConvert.SerializeObject (objectToSerialize);
		}

		public List<object> DeserializeToListOfObject (string jsonString)
		{
			List<object> result = JsonConvert.DeserializeObject<List<object>> (jsonString);

			return result;
		}

		public object DeserializeToObject (string jsonString)
		{
			object result = JsonConvert.DeserializeObject<object> (jsonString);
			if (result.GetType ().ToString () == "Newtonsoft.Json.Linq.JArray") {
				JArray jarrayResult = result as JArray;
				List<object> objectContainer = jarrayResult.ToObject<List<object>> ();
				if (objectContainer != null && objectContainer.Count > 0) {
					for (int index = 0; index < objectContainer.Count; index++) {
						if (objectContainer [index].GetType ().ToString () == "Newtonsoft.Json.Linq.JArray") {
							JArray internalItem = objectContainer [index] as JArray;
							objectContainer [index] = internalItem.Select (item => (object)item).ToArray ();
						}
					}
					result = objectContainer;
				}
			}
			return result;
		}

		public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, object>> (jsonString);
		}
	#endregion
	
	}
	#endif
	#endregion
	#region "States and ResposeTypes"
	public enum ResponseType
	{
		Publish,
		History,
		Time,
		Subscribe,
		Presence,
		Here_Now,
		DetailedHistory,
		Leave,
		Unsubscribe,
		PresenceUnsubscribe,
		GrantAccess,
		AuditAccess,
		RevokeAccess,
		PresenceHeartbeat,
		SetUserState,
		GetUserState,
		Where_Now,
		GlobalHere_Now
	}

	internal class InternetState<T>
	{
		public Action<bool> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public string[] Channels;

		public InternetState ()
		{
			Callback = null;
			ErrorCallback = null;
			Channels = null;
		}
	}

	public class RequestState<T>
	{
		public Action<T> UserCallback;
		public Action<PubnubClientError> ErrorCallback;
		public Action<T> ConnectCallback;
		public PubnubWebRequest Request;
		public PubnubWebResponse Response;
		public ResponseType Type;
		public string[] Channels;
		public bool Timeout;
		public bool Reconnect;
		public long Timetoken;

		public RequestState ()
		{
			UserCallback = null;
			ConnectCallback = null;
			Request = null;
			Response = null;
			Channels = null;
		}
	}
	#endregion
	#region "Channel callback"
	internal struct PubnubChannelCallbackKey
	{
		public string Channel;
		public ResponseType Type;
	}

	internal class PubnubChannelCallback<T>
	{
		public Action<T> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public Action<T> ConnectCallback;
		public Action<T> DisconnectCallback;
		//public ResponseType Type;
		public PubnubChannelCallback ()
		{
			Callback = null;
			ConnectCallback = null;
			DisconnectCallback = null;
			ErrorCallback = null;
		}
	}
	#endregion
}
