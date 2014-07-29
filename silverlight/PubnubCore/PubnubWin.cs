//Build Date: July 22, 2014
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;
#if NETFX_CORE
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using System.Threading.Tasks;
#else
using System.Net.Sockets;
using Microsoft.Win32;
using System.Configuration;
using System.Security.Cryptography;
#endif
using System.Threading;
using System.Reflection;
using System.Linq;

#if (SILVERLIGHT || WINDOWS_PHONE)
using System.Windows.Threading;
using System.IO.IsolatedStorage;
using System.Net.Browser;
#endif

namespace PubNubMessaging.Core
{
	internal class PubnubWin : PubnubCore
	{

		#region "Constants"
		const LoggingMethod.Level pubnubLogLevel = LoggingMethod.Level.Off;
		const PubnubErrorFilter.Level errorLevel = PubnubErrorFilter.Level.Info;

		#if (!SILVERLIGHT && !WINDOWS_PHONE)
		protected bool pubnubEnableProxyConfig = true;
		#endif
		
		#if (__MonoCS__)
		protected string _domainName = "pubsub.pubnub.com";
		#endif

        private object _reconnectFromSuspendMode = null;

		#endregion

		#region "Properties"
		//Proxy
		private PubnubProxy _pubnubProxy = null;
		public PubnubProxy Proxy
		{
			get
			{
		        #if (!SILVERLIGHT && !WINDOWS_PHONE)
                return _pubnubProxy;
                #else
                throw new NotSupportedException("Proxy is not supported");
                #endif
            }
			set
			{
                #if (!SILVERLIGHT && !WINDOWS_PHONE)
                _pubnubProxy = value;
				if (_pubnubProxy == null)
				{
					throw new ArgumentException("Missing Proxy Details");
				}
				if (string.IsNullOrEmpty(_pubnubProxy.ProxyServer) || (_pubnubProxy.ProxyPort <= 0) || string.IsNullOrEmpty(_pubnubProxy.ProxyUserName) || string.IsNullOrEmpty(_pubnubProxy.ProxyPassword))
				{
					_pubnubProxy = null;
					throw new MissingMemberException("Insufficient Proxy Details");
				}
                #else
                throw new NotSupportedException("Proxy is not supported");
                #endif
			}
		}
		#endregion

		#region "Constructors and destructors"

#if (!SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH && !__IOS__ && !MONODROID && !__ANDROID__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)
        ~PubnubWin()
		{
			//detach
			SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler (SystemEvents_PowerModeChanged);
		}
#endif

        public PubnubWin (string publishKey, string subscribeKey): 
			base(publishKey, subscribeKey)
		{
		}

		public PubnubWin(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn): 
			base(publishKey, subscribeKey, secretKey, cipherKey, sslOn)
		{
		}

		public PubnubWin(string publishKey, string subscribeKey, string secretKey):
			base(publishKey, subscribeKey, secretKey)
		{
		}
		#endregion

		#region "Abstract methods"
		protected override PubnubWebRequest SetServicePointSetTcpKeepAlive (PubnubWebRequest request)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
            request.ServicePoint.SetTcpKeepAlive(true, base.LocalClientHeartbeatInterval * 1000, 1000);
#endif
            //do nothing for mono
			return request;
		}
		protected override PubnubWebRequest SetProxy<T> (PubnubWebRequest request)
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
            if (pubnubEnableProxyConfig && _pubnubProxy != null)
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, ProxyServer={1}; ProxyPort={2}; ProxyUserName={3}", DateTime.Now.ToString(), _pubnubProxy.ProxyServer, _pubnubProxy.ProxyPort, _pubnubProxy.ProxyUserName), LoggingMethod.LevelInfo);
                WebProxy webProxy = new WebProxy(_pubnubProxy.ProxyServer, _pubnubProxy.ProxyPort);
                webProxy.Credentials = new NetworkCredential(_pubnubProxy.ProxyUserName, _pubnubProxy.ProxyPassword);
                request.Proxy = webProxy;
            }
#endif
            //No proxy setting for WP7
            return request;
		}

		protected override PubnubWebRequest SetTimeout<T>(RequestState<T> pubnubRequestState, PubnubWebRequest request)
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
            request.Timeout = GetTimeoutInSecondsForResponseType(pubnubRequestState.Type) * 1000;
#endif
            //No Timeout setting for WP7
            return request;
		}

		protected override void GeneratePowerSuspendEvent ()
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH && !__IOS__ && !MONODROID && !__ANDROID__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)

            PowerModeChangedEventArgs powerChangeEvent = new PowerModeChangedEventArgs(PowerModes.Suspend);
            SystemEvents_PowerModeChanged(null, powerChangeEvent);
#endif
            return;
        }

		protected override void GeneratePowerResumeEvent ()
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH && !__IOS__ && !MONODROID && !__ANDROID__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)
            PowerModeChangedEventArgs powerChangeEvent = new PowerModeChangedEventArgs(PowerModes.Resume);
            SystemEvents_PowerModeChanged(null, powerChangeEvent);
#endif
            return;
        }

		#endregion

		#region "Overridden methods"
		protected override sealed void Init(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
		{

			#if (USE_JSONFX)
			LoggingMethod.WriteToLog("Using USE_JSONFX", LoggingMethod.LevelInfo);
			this.JsonPluggableLibrary = new JsonFXDotNet();
			#elif (USE_DOTNET_SERIALIZATION)
			LoggingMethod.WriteToLog("Using USE_DOTNET_SERIALIZATION", LoggingMethod.LevelInfo);
			this.JsonPluggableLibrary = new JscriptSerializer();   
			#else
			LoggingMethod.WriteToLog("Using NewtonsoftJsonDotNet", LoggingMethod.LevelInfo);
			base.JsonPluggableLibrary = new NewtonsoftJsonDotNet();
			#endif

#if(SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            LoggingMethod.LogLevel = pubnubLogLevel;
            PubnubErrorFilter.ErrorLevel = errorLevel;
#else
            string configuredLogLevel = ConfigurationManager.AppSettings["PubnubMessaging.LogLevel"];
            int logLevelValue;
            if (!Int32.TryParse(configuredLogLevel, out logLevelValue))
            {
                base.PubnubLogLevel = pubnubLogLevel;
            }
            else
            {
                base.PubnubLogLevel = (LoggingMethod.Level)logLevelValue;
            }

            string configuredErrorFilter = ConfigurationManager.AppSettings["PubnubMessaging.PubnubErrorFilterLevel"];
            int errorFilterValue;
            if (!Int32.TryParse(configuredErrorFilter, out errorFilterValue))
            {
                base.PubnubErrorLevel = errorLevel;
            }
            else
            {
                base.PubnubErrorLevel = (PubnubErrorFilter.Level)errorFilterValue;
            }
#endif

#if (SILVERLIGHT || WINDOWS_PHONE)
            HttpWebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
#endif

			base.publishKey = publishKey;
			base.subscribeKey = subscribeKey;
			base.secretKey = secretKey;
			base.cipherKey = cipherKey;
			base.ssl = sslOn;

			base.VerifyOrSetSessionUUID();

			//Initiate System Events for PowerModeChanged - to monitor suspend/resume
			InitiatePowerModeCheck();

		}

		protected override bool InternetConnectionStatus<T> (string channel, Action<PubnubClientError> errorCallback, string[] rawChannels)
		{
            bool networkConnection;
            networkConnection = ClientNetworkStatus.CheckInternetStatus<T>(pubnetSystemActive, errorCallback, rawChannels);
            return networkConnection;
        }

		public override Guid GenerateGuid()
		{
			return base.GenerateGuid();
		}

		protected override void ForceCanonicalPathAndQuery (Uri requestUri)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)
            // Force canonical path and query
            string paq = requestUri.PathAndQuery;
            FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(requestUri);
            flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
            flagsFieldInfo.SetValue(requestUri, flags);
#endif
            //don't do anything for mono and SL and WP
		}

		protected override sealed void SendRequestAndGetResult<T> (Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request)
        {
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            //For WP7, Ensure that the RequestURI length <= 1599
            //For SL, Ensure that the RequestURI length <= 1482 for Large Text Message. If RequestURI Length < 1343, Successful Publish occurs
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UrlProcessResponseCallback<T>), pubnubRequestState);
            Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.Type) * 1000, Timeout.Infinite);
#else
            if (!ClientNetworkStatus.MachineSuspendMode && !PubnubWebRequest.MachineSuspendMode)
            {
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UrlProcessResponseCallback<T>), pubnubRequestState);
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(OnPubnubWebRequestTimeout<T>), pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.Type) * 1000, true);
            }
            else
            {
                ReconnectState<T> netState = new ReconnectState<T>();
                netState.Channels = pubnubRequestState.Channels;
                netState.Type = pubnubRequestState.Type;
                netState.Callback = pubnubRequestState.UserCallback;
                netState.ErrorCallback = pubnubRequestState.ErrorCallback;
                netState.ConnectCallback = pubnubRequestState.ConnectCallback;
                netState.Timetoken = pubnubRequestState.Timetoken;
                netState.Reconnect = pubnubRequestState.Reconnect;

                _reconnectFromSuspendMode = netState;
                return;
            }
#endif
            if (pubnubRequestState.Type == ResponseType.Presence || pubnubRequestState.Type == ResponseType.Subscribe)
            {
                if (presenceHeartbeatTimer != null)
                {
                    presenceHeartbeatTimer.Dispose();
                    presenceHeartbeatTimer = null;
                }
                if (pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0 && pubnubRequestState.Channels.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0)
                {
                    RequestState<T> presenceHeartbeatState = new RequestState<T>();
                    presenceHeartbeatState.Channels = pubnubRequestState.Channels;
                    presenceHeartbeatState.Type = ResponseType.PresenceHeartbeat;
                    presenceHeartbeatState.ErrorCallback = pubnubRequestState.ErrorCallback;
                    presenceHeartbeatState.Request = null;
                    presenceHeartbeatState.Response = null;

                    if (base.PresenceHeartbeatInterval > 0)
                    {
                        presenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, base.PresenceHeartbeatInterval * 1000, base.PresenceHeartbeatInterval * 1000);
                    }
                }
            }
        }

		protected override void TimerWhenOverrideTcpKeepAlive<T> (Uri requestUri, RequestState<T> pubnubRequestState)
		{
			if(localClientHeartBeatTimer != null){
				localClientHeartBeatTimer.Dispose();
			}
			localClientHeartBeatTimer = new Timer(new TimerCallback(OnPubnubLocalClientHeartBeatTimeoutCallback<T>), pubnubRequestState, 0,
                                       base.LocalClientHeartbeatInterval * 1000);
			channelLocalClientHeartbeatTimer.AddOrUpdate(requestUri, localClientHeartBeatTimer, (key, oldState) => localClientHeartBeatTimer);
		}

        protected override void ProcessResponseCallbackExceptionHandler<T>(Exception ex, RequestState<T> asynchRequestState)
        {
            //common Exception handler
#if !NETFX_CORE
			if (asynchRequestState.Response != null)
				asynchRequestState.Response.Close ();
#endif

            LoggingMethod.WriteToLog(string.Format("DateTime {0} Exception= {1} for URL: {2}", DateTime.Now.ToString(), ex.ToString(), asynchRequestState.Request.RequestUri.ToString()), LoggingMethod.LevelError);
            UrlRequestCommonExceptionHandler<T>(asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout, asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, false);
        }

        protected override bool HandleWebException<T>(WebException webEx, RequestState<T> asynchRequestState, string channel)
        {
            bool reconnect = false;
#if SILVERLIGHT || NETFX_CORE
            if (webEx.Status == WebExceptionStatus.ConnectFailure //Sending Keep-alive packet failed (No network)/Server is down.
             && !overrideTcpKeepAlive)
#else
            if ((webEx.Status == WebExceptionStatus.NameResolutionFailure //No network
            || webEx.Status == WebExceptionStatus.ConnectFailure //Sending Keep-alive packet failed (No network)/Server is down.
            || webEx.Status == WebExceptionStatus.ServerProtocolViolation //Problem with proxy or ISP
            || webEx.Status == WebExceptionStatus.ProtocolError) && (!overrideTcpKeepAlive))
#endif
            {
                //internet connection problem.
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, _urlRequest - Internet connection problem", DateTime.Now.ToString()), LoggingMethod.LevelError);
                if (channelInternetStatus.ContainsKey(channel) && (asynchRequestState.Type == ResponseType.Subscribe || asynchRequestState.Type == ResponseType.Presence))
                {
                    reconnect = true;
                    if (channelInternetStatus[channel])
                    {
                        //Reset Retry if previous state is true
                        channelInternetRetry.AddOrUpdate(channel, 0, (key, oldValue) => 0);
                    }
                    else
                    {
                        channelInternetRetry.AddOrUpdate(channel, 1, (key, oldValue) => oldValue + 1);
                        string multiChannel = (asynchRequestState.Channels != null) ? string.Join(",", asynchRequestState.Channels) : "";
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} {1} channel = {2} _urlRequest - Internet connection retry {3} of {4}", DateTime.Now.ToString(), asynchRequestState.Type, multiChannel, channelInternetRetry[channel], base.NetworkCheckMaxRetries), LoggingMethod.LevelInfo);
                        string message = string.Format("Detected internet connection problem. Retrying connection attempt {0} of {1}", channelInternetRetry[channel], base.NetworkCheckMaxRetries);
                        CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client, multiChannel, asynchRequestState.ErrorCallback, message, PubnubErrorCode.NoInternetRetryConnect, null, null);
                    }
                    channelInternetStatus[channel] = false;
                }
                #if NETFX_CORE
                Task.Delay(base.NetworkCheckRetryInterval * 1000);
                #else
                Thread.Sleep(base.NetworkCheckRetryInterval * 1000);
                #endif
            }
            return reconnect;
        }

        protected override void ProcessResponseCallbackWebExceptionHandler<T>(WebException webEx, RequestState<T> asynchRequestState, string channel)
        {
            bool reconnect = false;
            LoggingMethod.WriteToLog(string.Format("DateTime {0}, WebException: {1}", DateTime.Now.ToString(), webEx.ToString()), LoggingMethod.LevelError);
            if (asynchRequestState != null)
            {
#if !NETFX_CORE
				if (asynchRequestState.Response != null)
					asynchRequestState.Response.Close ();
#endif
                if (asynchRequestState.Request != null)
                    TerminatePendingWebRequest(asynchRequestState);
            }
            //#elif (!SILVERLIGHT)
            reconnect = HandleWebException(webEx, asynchRequestState, channel);

            UrlRequestCommonExceptionHandler<T>(asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout,
                asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, reconnect);
        }

        protected override void UrlProcessResponseCallback<T>(IAsyncResult asynchronousResult)
        {
            List<object> result = new List<object>();

            RequestState<T> asynchRequestState = asynchronousResult.AsyncState as RequestState<T>;

            string channel = "";
            if (asynchRequestState != null && asynchRequestState.Channels != null)
            {
                channel = string.Join(",", asynchRequestState.Channels);
            }

            PubnubWebRequest asyncWebRequest = asynchRequestState.Request as PubnubWebRequest;
            try
            {
                if (asyncWebRequest != null)
                {
                    using (PubnubWebResponse asyncWebResponse = (PubnubWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult))
                    {
                        asynchRequestState.Response = asyncWebResponse;

                        using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                        {
                            if (asynchRequestState.Type == ResponseType.Subscribe || asynchRequestState.Type == ResponseType.Presence)
                            {
                                if (!overrideTcpKeepAlive && channelInternetStatus.ContainsKey(channel) && !channelInternetStatus[channel])
                                {
                                    if (asynchRequestState.Channels != null)
                                    {
                                        for (int index = 0; index < asynchRequestState.Channels.Length; index++)
                                        {
                                            string activeChannel = asynchRequestState.Channels[index].ToString();

                                            string status = "Internet connection available";

                                            PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                                            callbackKey.Channel = activeChannel;
                                            callbackKey.Type = asynchRequestState.Type;

                                            if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                                            {
                                                object callbackObject;
                                                bool channelAvailable = channelCallbacks.TryGetValue(callbackKey, out callbackObject);
                                                PubnubChannelCallback<T> currentPubnubCallback = null;
                                                if (channelAvailable)
                                                {
                                                    currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
                                                }

                                                if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
                                                {
                                                    CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                                                        activeChannel, asynchRequestState.ErrorCallback,
                                                        status, PubnubErrorCode.YesInternet, null, null);
                                                }
                                            }
                                        }
                                    }
                                }

                                channelInternetStatus.AddOrUpdate(channel, true, (key, oldValue) => true);
                            }

                            //Deserialize the result
                            string jsonString = streamReader.ReadToEnd();
#if !NETFX_CORE
                            streamReader.Close ();
#endif

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString(), channel, asynchRequestState.Type.ToString(), jsonString), LoggingMethod.LevelInfo);

                            if (overrideTcpKeepAlive)
                            {
                                TerminateLocalClientHeartbeatTimer(asyncWebRequest.RequestUri);
                            }

                            if (asynchRequestState.Type == ResponseType.PresenceHeartbeat)
                            {
                                if (base.JsonPluggableLibrary.IsDictionaryCompatible(jsonString))
                                {
                                    Dictionary<string, object> deserializeStatus = base.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonString);
                                    int statusCode = 0; //default. assuming all is ok 
                                    if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                                    {
                                        Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                                        string statusMessage = deserializeStatus["message"].ToString();

                                        if (statusCode != 200)
                                        {
                                            PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType(statusCode, statusMessage);
                                            int pubnubStatusCode = (int)pubnubErrorType;
                                            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);

                                            PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, statusMessage, PubnubMessageSource.Server, asynchRequestState.Request, asynchRequestState.Response, errorDescription, channel);
                                            GoToCallback(error, asynchRequestState.ErrorCallback);
                                        }
                                    }
                                }
                            }
                            else if (jsonString != "[]")
                            {
                                result = WrapResultBasedOnResponseType<T>(asynchRequestState.Type, jsonString, asynchRequestState.Channels, asynchRequestState.Reconnect, asynchRequestState.Timetoken, asynchRequestState.ErrorCallback);
                            }
                        }
#if !NETFX_CORE
                        asyncWebResponse.Close ();
#endif
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, Request aborted for channel={1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);
                }

                ProcessResponseCallbacks<T>(result, asynchRequestState);

                if ((asynchRequestState.Type == ResponseType.Subscribe || asynchRequestState.Type == ResponseType.Presence) && (asynchRequestState.Channels != null) && (result != null) && (result.Count > 0))
                {
                    foreach (string currentChannel in asynchRequestState.Channels)
                    {
                        multiChannelSubscribe.AddOrUpdate(currentChannel, Convert.ToInt64(result[1].ToString()), (key, oldValue) => Convert.ToInt64(result[1].ToString()));
                    }
                }

                switch (asynchRequestState.Type)
                {
                    case ResponseType.Subscribe:
                    case ResponseType.Presence:
                        MultiplexInternalCallback<T>(asynchRequestState.Type, result, asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback);
                        break;
                    default:
                        break;
                }
            }
            catch (WebException webEx)
            {
                HttpStatusCode currentHttpStatusCode;
                if (webEx.Response != null && asynchRequestState != null)
                {
                    if (webEx.Response.GetType().ToString() == "System.Net.HttpWebResponse"
                             || webEx.Response.GetType().ToString() == "MS.Internal.Modern.ClientHttpWebResponse"
                             || webEx.Response.GetType().ToString() == "System.Net.Browser.ClientHttpWebResponse")
                    {
                        currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
                    }
                    else
                    {
                        currentHttpStatusCode = ((PubnubWebResponse)webEx.Response).HttpStatusCode;
                    }
                    PubnubWebResponse exceptionResponse = new PubnubWebResponse(webEx.Response, currentHttpStatusCode);
                    if (exceptionResponse != null)
                    {
                        asynchRequestState.Response = exceptionResponse;

                        using (StreamReader streamReader = new StreamReader(asynchRequestState.Response.GetResponseStream()))
                        {
                            string jsonString = streamReader.ReadToEnd();

#if !NETFX_CORE
                            streamReader.Close ();
#endif

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString(), channel, asynchRequestState.Type.ToString(), jsonString), LoggingMethod.LevelInfo);

                            if (overrideTcpKeepAlive)
                            {
                                TerminateLocalClientHeartbeatTimer(asyncWebRequest.RequestUri);
                            }

                            if ((int)currentHttpStatusCode < 200 || (int)currentHttpStatusCode >= 300)
                            {
                                result = null;
                                string errorDescription = "";
                                int pubnubStatusCode = 0;

                                if ((int)currentHttpStatusCode == 500 || (int)currentHttpStatusCode == 502 || (int)currentHttpStatusCode == 503 || (int)currentHttpStatusCode == 504 || (int)currentHttpStatusCode == 414)
                                {
                                    //This status code is not giving json string.
                                    string statusMessage = currentHttpStatusCode.ToString();
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else if (base.JsonPluggableLibrary.IsArrayCompatible(jsonString))
                                {
                                    List<object> deserializeStatus = base.JsonPluggableLibrary.DeserializeToListOfObject(jsonString);
                                    string statusMessage = deserializeStatus[1].ToString();
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else if (base.JsonPluggableLibrary.IsDictionaryCompatible(jsonString))
                                {
                                    Dictionary<string, object> deserializeStatus = base.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonString);
                                    string statusMessage = deserializeStatus.ContainsKey("message") ? deserializeStatus["message"].ToString() : jsonString;
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else
                                {
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, jsonString);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }

                                PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, jsonString, PubnubMessageSource.Server, asynchRequestState.Request, asynchRequestState.Response, errorDescription, channel);
                                GoToCallback(error, asynchRequestState.ErrorCallback);

                            }
                            else if (jsonString != "[]")
                            {
                                result = WrapResultBasedOnResponseType<T>(asynchRequestState.Type, jsonString, asynchRequestState.Channels, asynchRequestState.Reconnect, asynchRequestState.Timetoken, asynchRequestState.ErrorCallback);
                            }
                            else
                            {
                                result = null;
                            }
                        }
                    }
#if !NETFX_CORE
                    exceptionResponse.Close ();
#endif

                    if (result != null && result.Count > 0)
                    {
                        ProcessResponseCallbacks<T>(result, asynchRequestState);
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
                                    string activeChannel = asynchRequestState.Channels[index].ToString();
                                    PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                                    callbackKey.Channel = activeChannel;
                                    callbackKey.Type = asynchRequestState.Type;

                                    if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                                    {
                                        object callbackObject;
                                        bool channelAvailable = channelCallbacks.TryGetValue(callbackKey, out callbackObject);
                                        PubnubChannelCallback<T> currentPubnubCallback = null;
                                        if (channelAvailable)
                                        {
                                            currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
                                        }
                                        if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
                                        {
                                            PubnubClientError error = CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
                                                                                     activeChannel, currentPubnubCallback.ErrorCallback,
                                                                                     webEx, asynchRequestState.Request, asynchRequestState.Response);
                                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString(), error.ToString()), LoggingMethod.LevelInfo);
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            PubnubClientError error = CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
                                                                 channel, asynchRequestState.ErrorCallback,
                                                                 webEx, asynchRequestState.Request, asynchRequestState.Response);
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString(), error.ToString()), LoggingMethod.LevelInfo);
                        }
                    }
                    ProcessResponseCallbackWebExceptionHandler<T>(webEx, asynchRequestState, channel);
                }
            }
            catch (Exception ex)
            {
                if (!pubnetSystemActive && ex.Message.IndexOf("The IAsyncResult object was not returned from the corresponding asynchronous method on this class.") == -1)
                {
                    if (asynchRequestState.Channels != null)
                    {
                        if (asynchRequestState.Type == ResponseType.Subscribe
                                  || asynchRequestState.Type == ResponseType.Presence)
                        {
                            for (int index = 0; index < asynchRequestState.Channels.Length; index++)
                            {
                                string activeChannel = asynchRequestState.Channels[index].ToString();

                                PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                                callbackKey.Channel = activeChannel;
                                callbackKey.Type = asynchRequestState.Type;

                                if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                                {
                                    object callbackObject;
                                    bool channelAvailable = channelCallbacks.TryGetValue(callbackKey, out callbackObject);
                                    PubnubChannelCallback<T> currentPubnubCallback = null;
                                    if (channelAvailable)
                                    {
                                        currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
                                    }
                                    if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
                                    {
                                        CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                            activeChannel, currentPubnubCallback.ErrorCallback, ex, asynchRequestState.Request, asynchRequestState.Response);

                                    }
                                }
                            }
                        }
                        else
                        {
                            CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                channel, asynchRequestState.ErrorCallback, ex, asynchRequestState.Request, asynchRequestState.Response);
                        }

                    }
                }
                ProcessResponseCallbackExceptionHandler<T>(ex, asynchRequestState);
            }
        }

		#endregion

		#region "Overridden properties"
		public override IPubnubUnitTest PubnubUnitTest
		{
			get
			{
				return base.PubnubUnitTest;
			}
			set
			{
				base.PubnubUnitTest = value;
			}
		}
		#endregion

		#region "Other methods"

		private void InitiatePowerModeCheck()
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH && !__IOS__ && !MONODROID && !__ANDROID__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)
			try
			{
				SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, Initiated System Event - PowerModeChanged.", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
			}
			catch (Exception ex)
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0} No support for System Event - PowerModeChanged.", DateTime.Now.ToString()), LoggingMethod.LevelError);
				LoggingMethod.WriteToLog(string.Format("DateTime {0} {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
			}
#endif
        }

#if (!SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH && !__IOS__ && !MONODROID && !__ANDROID__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IOS && !UNITY_ANDROID && !NETFX_CORE)
		void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Suspend)
			{
				pubnetSystemActive = false;
				ClientNetworkStatus.MachineSuspendMode = true;
				PubnubWebRequest.MachineSuspendMode = true;
				TerminatePendingWebRequest();
                if (overrideTcpKeepAlive && localClientHeartBeatTimer != null)
				{
					localClientHeartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
				}

				LoggingMethod.WriteToLog(string.Format("DateTime {0}, System entered into Suspend Mode.", DateTime.Now.ToString()), LoggingMethod.LevelInfo);

				if (overrideTcpKeepAlive)
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, Disabled Timer for heartbeat ", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
				}
			}
			else if (e.Mode == PowerModes.Resume)
			{
				pubnetSystemActive = true;
				ClientNetworkStatus.MachineSuspendMode = false;
				PubnubWebRequest.MachineSuspendMode = false;
                if (overrideTcpKeepAlive && localClientHeartBeatTimer != null)
				{
					try
					{
						localClientHeartBeatTimer.Change(
                            (-1 == base.LocalClientHeartbeatInterval) ? -1 : base.LocalClientHeartbeatInterval * 1000,
                            (-1 == base.LocalClientHeartbeatInterval) ? -1 : base.LocalClientHeartbeatInterval * 1000);
					}
					catch { }
				}

				LoggingMethod.WriteToLog(string.Format("DateTime {0}, System entered into Resume/Awake Mode.", DateTime.Now.ToString()), LoggingMethod.LevelInfo);

				if (overrideTcpKeepAlive)
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, Enabled Timer for heartbeat ", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
				}

                ReconnectFromSuspendMode(_reconnectFromSuspendMode);
                _reconnectFromSuspendMode = null;

			}
		}
#endif

#if (__MonoCS__)
		bool RequestIsUnsafe(Uri requestUri)
		{
			bool isUnsafe = false;
			StringBuilder requestMessage = new StringBuilder();
			if (requestUri.Segments.Length > 7)
			{
				for (int i = 7; i < requestUri.Segments.Length; i++)
				{
					requestMessage.Append(requestUri.Segments[i]);
				}
			}
			foreach (char ch in requestMessage.ToString().ToCharArray())
			{
				if (" ~`!@#$^&*()+=[]\\{}|;':\"./<>?".IndexOf(ch) >= 0)
				{
					isUnsafe = true;
					break;
				}
			}
			return isUnsafe;
		}
#endif

#if (__MonoCS__ && !UNITY_ANDROID && !UNITY_IOS) 
		string CreateRequest(Uri requestUri)
		{
			StringBuilder requestBuilder = new StringBuilder();
			requestBuilder.Append("GET ");
			requestBuilder.Append(requestUri.OriginalString);

			if (ssl)
			{
				requestBuilder.Append(string.Format(" HTTP/1.1\r\nConnection: close\r\nHost: {0}:443\r\n\r\n", this._domainName));
			}
			else
			{
				requestBuilder.Append(string.Format(" HTTP/1.1\r\nConnection: close\r\nHost: {0}:80\r\n\r\n", this._domainName));
			}
			return requestBuilder.ToString();
		}

		void ConnectToHostAndSendRequest<T>(bool sslEnabled, TcpClient tcpClient, RequestState<T> pubnubRequestState, string requestString)
		{
			NetworkStream stream = tcpClient.GetStream();

			string proxyAuth = string.Format("{0}:{1}", Proxy.ProxyUserName, Proxy.ProxyPassword);
			byte[] proxyAuthBytes = Encoding.UTF8.GetBytes(proxyAuth);

			//Proxy-Authenticate: authentication mode Basic, Digest and NTLM
			string connectRequest = "";
			if (sslEnabled)
			{
				connectRequest = string.Format("CONNECT {0}:443  HTTP/1.1\r\nProxy-Authorization: Basic {1}\r\nHost: {0}\r\n\r\n", this._domainName, Convert.ToBase64String(proxyAuthBytes));
			}
			else
			{
				connectRequest = string.Format("CONNECT {0}:80  HTTP/1.1\r\nProxy-Authorization: Basic {1}\r\nHost: {0}\r\n\r\n", this._domainName, Convert.ToBase64String(proxyAuthBytes));
			}

			byte[] tunnelRequest = Encoding.UTF8.GetBytes(connectRequest);
			stream.Write(tunnelRequest, 0, tunnelRequest.Length);
			stream.Flush();

			stream.ReadTimeout = pubnubRequestState.Request.Timeout * 5;

			StateObject<T> state = new StateObject<T>();
			state.tcpClient = tcpClient;
			state.RequestState = pubnubRequestState;
			state.requestString = requestString;
			state.netStream = stream;

			//stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ConnectToHostAndSendRequestCallback<T>), state);

			StringBuilder response = new StringBuilder();
			var responseStream = new StreamReader(stream);

			char[] buffer = new char[2048];

			int charsRead = responseStream.Read(buffer, 0, buffer.Length);
			bool connEstablished = false;
			while (charsRead > 0)
			{
				response.Append(buffer);
				if ((response.ToString().IndexOf("200 Connection established") > 0) || (response.ToString().IndexOf("200 OK") > 0))
				{
					connEstablished = true;
					break;
				}
				charsRead = responseStream.Read(buffer, 0, buffer.Length);
			}

			if (connEstablished)
			{
				if (sslEnabled)
				{
					SendSslRequest<T>(stream, tcpClient, pubnubRequestState, requestString);
				}
				else
				{
					SendRequest<T>(tcpClient, pubnubRequestState, requestString);
				}

			}
			else if (response.ToString().IndexOf("407 Proxy Authentication Required") > 0)
			{
				int pos = response.ToString().IndexOf("Proxy-Authenticate");
				string desc = "";
				if (pos > 0)
				{
					desc = response.ToString().Substring(pos, response.ToString().IndexOf("\r\n", pos) - pos);
				}
				throw new WebException(string.Format("Proxy Authentication Required. Desc: {0}", desc));
			}
			else
			{
				throw new WebException("Couldn't connect to the server");
			}
		}

		private void ConnectToHostAndSendRequestCallback<T>(IAsyncResult asynchronousResult)
		{
			StateObject<T> asynchStateObject = asynchronousResult.AsyncState as StateObject<T>;
			RequestState<T> asynchRequestState = asynchStateObject.RequestState;

			string channels = "";
			if (asynchRequestState != null && asynchRequestState.Channels != null)
			{
				channels = string.Join(",", asynchRequestState.Channels);
			}

			try
			{
				string requestString = asynchStateObject.requestString;
				TcpClient tcpClient = asynchStateObject.tcpClient;

				NetworkStream netStream = asynchStateObject.netStream;
				int bytesRead = netStream.EndRead(asynchronousResult);

				if (bytesRead > 0)
				{
					asynchStateObject.sb.Append(Encoding.ASCII.GetString(asynchStateObject.buffer, 0, bytesRead));

					netStream.BeginRead(asynchStateObject.buffer, 0, StateObject<T>.BufferSize,
					                    new AsyncCallback(ConnectToHostAndSendRequestCallback<T>), asynchStateObject);
				}
				else
				{
					string resp = asynchStateObject.sb.ToString();
					if (resp.IndexOf("200 Connection established") > 0)
					{
						SendSslRequest<T>(netStream, tcpClient, asynchRequestState, requestString);
					}
					else
					{
						throw new WebException("Couldn't connect to the server");
					}
				}
			}
			catch (WebException webEx)
			{
				if (asynchRequestState != null && asynchRequestState.ErrorCallback != null)
				{
					Action<PubnubClientError> errorCallback = asynchRequestState.ErrorCallback;

					CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
					                   channels, errorCallback, webEx, null, null);
				}
				ProcessResponseCallbackWebExceptionHandler<T>(webEx, asynchRequestState, channels);
			}
			catch (Exception ex)
			{
				if (asynchRequestState != null && asynchRequestState.ErrorCallback != null)
				{
					Action<PubnubClientError> errorCallback = asynchRequestState.ErrorCallback;
					CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
					                   channels, errorCallback, ex, null, null);
				}
				ProcessResponseCallbackExceptionHandler<T>(ex, asynchRequestState);
			}
		}

		void SendSslRequest<T>(NetworkStream netStream, TcpClient tcpClient, RequestState<T> pubnubRequestState, string requestString)
		{
#if(MONODROID || __ANDROID__)
			SslStream sslStream = new SslStream(netStream, true, Validator, null);
#elif(UNITY_ANDROID|| MONOTOUCH || __IOS__)
			ServicePointManager.ServerCertificateValidationCallback = ValidatorUnity;
			SslStream sslStream = new SslStream(netStream, true, ValidatorUnity, null);
#else
			SslStream sslStream = new SslStream(netStream);
#endif
			StateObject<T> state = new StateObject<T>();
			state.tcpClient = tcpClient;
			state.sslns = sslStream;
			state.RequestState = pubnubRequestState;
			state.requestString = requestString;
			sslStream.AuthenticateAsClient(this._domainName);
			AfterAuthentication(state);
		}

		void AfterAuthentication<T> (StateObject<T> state)
		{
			SslStream sslStream = state.sslns;
			byte[] sendBuffer = UTF8Encoding.UTF8.GetBytes(state.requestString);

			sslStream.Write(sendBuffer);
			sslStream.Flush();
#if(!MONODROID && !__ANDROID__ && !UNITY_ANDROID)         
			sslStream.ReadTimeout = state.RequestState.Request.Timeout;
#endif
			sslStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(SendRequestUsingTcpClientCallback<T>), state);
		}

		private void SendSslRequestAuthenticationCallback<T>(IAsyncResult asynchronousResult)
		{
			StateObject<T> state = asynchronousResult.AsyncState as StateObject<T>;
			RequestState<T> asynchRequestState = state.RequestState;
			string channels = "";

			if (asynchRequestState != null && asynchRequestState.Channels != null)
			{
				channels = string.Join(",", asynchRequestState.Channels);
			}
			try{
				AfterAuthentication(state);
			}
			catch (WebException webEx)
			{
				if (asynchRequestState != null && asynchRequestState.ErrorCallback != null)
				{
					Action<PubnubClientError> errorCallback = asynchRequestState.ErrorCallback;

					CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
					                   channels, errorCallback, webEx, null, null);
				}
				ProcessResponseCallbackWebExceptionHandler<T>(webEx, asynchRequestState, channels);
			}
			catch (Exception ex)
			{
				if (asynchRequestState != null && asynchRequestState.ErrorCallback != null)
				{
					Action<PubnubClientError> errorCallback = asynchRequestState.ErrorCallback;

					CallErrorCallback (PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
					                   channels, errorCallback, ex, null, null);
				}
				ProcessResponseCallbackExceptionHandler<T>(ex, asynchRequestState);
			}
		}

		void SendRequest<T>(TcpClient tcpClient, RequestState<T> pubnubRequestState, string requestString)
		{
			NetworkStream netStream = tcpClient.GetStream();

			StateObject<T> state = new StateObject<T>();
			state.tcpClient = tcpClient;
			state.netStream = netStream;
			state.RequestState = pubnubRequestState;

			System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(netStream);
			streamWriter.Write(requestString);
			streamWriter.Flush();
#if(!MONODROID && !__ANDROID__ && !UNITY_ANDROID)
			netStream.ReadTimeout = pubnubRequestState.Request.Timeout;
#endif
			netStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(SendRequestUsingTcpClientCallback<T>), state);

		}

		private void SendRequestUsingTcpClient<T>(Uri requestUri, RequestState<T> pubnubRequestState)
		{
			TcpClient tcpClient = new TcpClient();
			tcpClient.NoDelay = false;
#if(!MONODROID && !__ANDROID__ && !UNITY_ANDROID)
			tcpClient.SendTimeout = pubnubRequestState.Request.Timeout;
#endif          

			string requestString = CreateRequest(requestUri);

			if (ssl)
			{
				if (pubnubEnableProxyConfig && Proxy != null)
				{
					tcpClient.Connect(Proxy.ProxyServer, Proxy.ProxyPort);

					ConnectToHostAndSendRequest<T>(ssl, tcpClient, pubnubRequestState, requestString);
				}
				else
				{
					tcpClient.Connect(this._domainName, 443);
					NetworkStream netStream = tcpClient.GetStream();
					SendSslRequest<T>(netStream, tcpClient, pubnubRequestState, requestString);
				}
			}
			else
			{
				if (pubnubEnableProxyConfig && Proxy != null)
				{
					tcpClient.Connect(Proxy.ProxyServer, Proxy.ProxyPort);

					ConnectToHostAndSendRequest(ssl, tcpClient, pubnubRequestState, requestString);
				}
				else
				{
					tcpClient.Connect(this._domainName, 80);
					SendRequest<T>(tcpClient, pubnubRequestState, requestString);
				}
			}
		}

		private void SendRequestUsingTcpClientCallback<T>(IAsyncResult asynchronousResult)
		{
			StateObject<T> state = asynchronousResult.AsyncState as StateObject<T>;
			RequestState<T> asynchRequestState = state.RequestState;
			string channel = "";
			if (asynchRequestState != null && asynchRequestState.Channels != null)
			{
				channel = string.Join(",", asynchRequestState.Channels);
			}
			try
			{
				//StateObject<T> state = (StateObject<T>) asynchronousResult.AsyncState;
				if (ssl)
				{
					SslStream sslns = state.sslns;
					int bytesRead = sslns.EndRead(asynchronousResult);

					if (bytesRead > 0)
					{
						Decoder decoder = Encoding.UTF8.GetDecoder();
						char[] chars = new char[decoder.GetCharCount(state.buffer, 0, bytesRead)];
						decoder.GetChars(state.buffer, 0, bytesRead, chars, 0);
						state.sb.Append(chars);

						sslns.BeginRead(state.buffer, 0, StateObject<T>.BufferSize,
						                new AsyncCallback(SendRequestUsingTcpClientCallback<T>), state);
					}
					else
					{
						HandleTcpClientResponse(state, asynchRequestState, channel, asynchronousResult);
					}
				}
				else
				{
					NetworkStream netStream = state.netStream;
					int bytesRead = netStream.EndRead(asynchronousResult);

					if (bytesRead > 0)
					{
						state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

						netStream.BeginRead(state.buffer, 0, StateObject<T>.BufferSize,
						                    new AsyncCallback(SendRequestUsingTcpClientCallback<T>), state);
					}
					else
					{
						HandleTcpClientResponse(state, asynchRequestState, channel, asynchronousResult);
					}
				}
			}
			catch (WebException webEx)
			{
				ProcessResponseCallbackWebExceptionHandler<T>(webEx, asynchRequestState, channel);
			}
			catch (Exception ex)
			{
				ProcessResponseCallbackExceptionHandler<T>(ex, asynchRequestState);
			}
		}

		void HandleTcpClientResponse<T>(StateObject<T> state, RequestState<T> asynchRequestState, string channel, IAsyncResult asynchronousResult)
		{
			List<object> result = new List<object>();
			if (state.sb.Length > 1)
			{
				string jsonString = ParseResponse<T>(state.sb.ToString(), asynchronousResult);
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString(), channel, asynchRequestState.Type.ToString(), jsonString), LoggingMethod.LevelInfo);

				if (overrideTcpKeepAlive)
				{
					TerminateHeartbeatTimer(state.RequestState.Request.RequestUri);
				}

				if (jsonString != null && !string.IsNullOrEmpty(jsonString) && !string.IsNullOrEmpty(channel.Trim()))
				{
					result = WrapResultBasedOnResponseType<T>(asynchRequestState.Type, jsonString, asynchRequestState.Channels, asynchRequestState.Reconnect, asynchRequestState.Timetoken, asynchRequestState.ErrorCallback);
				}

				ProcessResponseCallbacks<T>(result, asynchRequestState);
			}
			if (state.tcpClient != null)
				state.tcpClient.Close();
		}

		string ParseResponse<T>(string responseString, IAsyncResult asynchronousResult)
		{
			string json = "";
			int pos = responseString.LastIndexOf('\n');
			if ((responseString.StartsWith("HTTP/1.1 ") || responseString.StartsWith("HTTP/1.0 "))
			    && (pos != -1) && responseString.Length >= pos + 1)

			{
				json = responseString.Substring(pos + 1);
			}
			return json;
		}
#endif

#if(UNITY_ANDROID || MONOTOUCH || __IOS__)      
		/// <summary>
		/// Workaround for the bug described here 
		/// https://bugzilla.xamarin.com/show_bug.cgi?id=6501
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="certificate">Certificate.</param>
		/// <param name="chain">Chain.</param>
		/// <param name="sslPolicyErrors">Ssl policy errors.</param>
		static bool ValidatorUnity (object sender,
		                            System.Security.Cryptography.X509Certificates.X509Certificate
		                            certificate,
		                            System.Security.Cryptography.X509Certificates.X509Chain chain,
		                            System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			//TODO:
			return true;
		}
#endif

#if(MONODROID || __ANDROID__)      
		/// <summary>
		/// Workaround for the bug described here 
		/// https://bugzilla.xamarin.com/show_bug.cgi?id=6501
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="certificate">Certificate.</param>
		/// <param name="chain">Chain.</param>
		/// <param name="sslPolicyErrors">Ssl policy errors.</param>
		static bool Validator (object sender,
		                       System.Security.Cryptography.X509Certificates.X509Certificate
		                       certificate,
		                       System.Security.Cryptography.X509Certificates.X509Chain chain,
		                       System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			var sslTrustManager = (IX509TrustManager) typeof (AndroidEnvironment)
				.GetField ("sslTrustManager",
				           System.Reflection.BindingFlags.NonPublic |
				           System.Reflection.BindingFlags.Static)
					.GetValue (null);

			Func<Java.Security.Cert.CertificateFactory,
			System.Security.Cryptography.X509Certificates.X509Certificate,
			Java.Security.Cert.X509Certificate> c = (f, v) =>
				f.GenerateCertificate (
					new System.IO.MemoryStream (v.GetRawCertData ()))
					.JavaCast<Java.Security.Cert.X509Certificate>();
			var cFactory = Java.Security.Cert.CertificateFactory.GetInstance (Javax.Net.Ssl.TrustManagerFactory.DefaultAlgorithm);
			var certs = new List<Java.Security.Cert.X509Certificate>(
				chain.ChainElements.Count + 1);
			certs.Add (c (cFactory, certificate));
			foreach (var ce in chain.ChainElements) {
				if (certificate.Equals (ce.Certificate))
					continue;
				certificate = ce.Certificate;
				certs.Add (c (cFactory, certificate));
			}
			try {
				//had to comment this out as sslTrustManager was returning null
				//working on the fix or a workaround
				//sslTrustManager.CheckServerTrusted (certs.ToArray (),
				//                                  Javax.Net.Ssl.TrustManagerFactory.DefaultAlgorithm);
				return true;
			}
			catch (Exception e) {
				throw new Exception("SSL error");
			}
		}
#endif

        #endregion

        #region "Nested Classes"
#if (__MonoCS__ && !UNITY_ANDROID && !UNITY_IOS)
		class StateObject<T>
		{
			public RequestState<T> RequestState
			{
				get;
				set;
			}

			public TcpClient tcpClient = null;
			public NetworkStream netStream = null;
			public SslStream sslns = null;
			public const int BufferSize = 2048;
			public byte[] buffer = new byte[BufferSize];
			public StringBuilder sb = new StringBuilder();
			public string requestString = null;
		}
#endif
        #endregion

    }


    #region "PubnubWebRequestCreator"
    internal class PubnubWebRequestCreator : PubnubWebRequestCreatorBase
    {

        public PubnubWebRequestCreator()
            : base()
        {
        }

        public PubnubWebRequestCreator(IPubnubUnitTest pubnubUnitTest)
            : base(pubnubUnitTest)
        {
        }

#if NETFX_CORE
        protected HttpWebRequest SetUserAgent(HttpWebRequest req, bool keepAliveRequest)
        {
            req.Headers["UserAgent"] = string.Format("ua_string=({0} {1}) PubNub-csharp/3.6", "WindowsStore", "Win8.1"); 
            return req;
        }
#else
        protected HttpWebRequest SetUserAgent(HttpWebRequest req, bool keepAliveRequest, OperatingSystem userOS)
        {
            #if (SILVERLIGHT || WINDOWS_PHONE)
            req.Headers["UserAgent"] = string.Format("ua_string=({0} {1}) PubNub-csharp/3.6", userOS.Platform.ToString(), userOS.Version.ToString());
            #else
			req.KeepAlive = keepAliveRequest;
			req.UserAgent = string.Format("ua_string=({0}) PubNub-csharp/3.6", userOS.VersionString);
            #endif
            return req;
        }
#endif

        protected override HttpWebRequest SetNoCache(HttpWebRequest req, bool nocache)
        {
            if (nocache)
            {
                req.Headers["Cache-Control"] = "no-cache";
                req.Headers["Pragma"] = "no-cache";
#if (WINDOWS_PHONE)
                req.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif
            }
            return req;
        }

        protected override WebRequest CreateRequest(Uri uri, bool keepAliveRequest, bool nocache)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
#if NETFX_CORE
            req = SetUserAgent(req, keepAliveRequest);
#else
            OperatingSystem userOS = System.Environment.OSVersion;
            req = SetUserAgent(req, keepAliveRequest, userOS);
#endif

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

    }
    #endregion

    #region "PubnubWebRequest"
    public class PubnubWebRequest : PubnubWebRequestBase
    {

#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
		public override long ContentLength
		{
			get
			{
				return request.ContentLength;
			}
		}
#endif
#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
        private int _timeout;
		public override int Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				_timeout = value;
				if (request != null)
				{
					request.Timeout = _timeout;
				}
			}
		}

		public override IWebProxy Proxy
		{
			get
			{
				return request.Proxy;
			}
			set
			{
				request.Proxy = value;
			}
		}

		public override bool PreAuthenticate
		{
			get
			{
				return request.PreAuthenticate;
			}
			set
			{
				request.PreAuthenticate = value;
			}
		}
		public override System.Net.Cache.RequestCachePolicy CachePolicy
		{
			get
			{
				return request.CachePolicy;
			}
		}

		public override string ConnectionGroupName
		{
			get
			{
				return request.ConnectionGroupName;
			}
		}
#endif

#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
        public ServicePoint ServicePoint;
#endif

#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
        public override WebResponse GetResponse()
		{
			return request.GetResponse();
		}
#endif

        public PubnubWebRequest(HttpWebRequest request)
            : base(request)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
			this.ServicePoint = this.request.ServicePoint;
#endif
        }
        public PubnubWebRequest(HttpWebRequest request, IPubnubUnitTest pubnubUnitTest)
            : base(request, pubnubUnitTest)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
			this.ServicePoint = this.request.ServicePoint;
#endif
        }
    }
    #endregion

    #region "PubnubWebResponse"
    public class PubnubWebResponse : PubnubWebResponseBase
    {
        public PubnubWebResponse(WebResponse response):base(response)
        {
        }

        public PubnubWebResponse(WebResponse response, HttpStatusCode statusCode):base(response,statusCode)
        {
        }

        public PubnubWebResponse(Stream responseStream):base(responseStream)
        {
        }

        public PubnubWebResponse(Stream responseStream, HttpStatusCode statusCode):base(responseStream, statusCode)
        {
        }

        #if !NETFX_CORE
		public override void Close ()
		{
			if (response != null) {
				response.Close ();
			}
		}
        #endif

    }
    #endregion

    #region "PubnubCrypto"

    public class PubnubCrypto : PubnubCryptoBase
    {
        public PubnubCrypto(string cipher_key)
            : base(cipher_key)
        {
        }

        protected override string ComputeHashRaw(string input)
        {
#if (SILVERLIGHT || WINDOWS_PHONE || MONOTOUCH || __IOS__ || MONODROID || __ANDROID__ || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID)
            HashAlgorithm algorithm = new System.Security.Cryptography.SHA256Managed();
#elif NETFX_CORE
            HashAlgorithmProvider algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
#else
			HashAlgorithm algorithm = new SHA256CryptoServiceProvider();
#endif

#if (SILVERLIGHT || WINDOWS_PHONE)
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
#elif NETFX_CORE
            IBuffer inputBuffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
#else
			Byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
#endif
#if NETFX_CORE
            IBuffer hashedBuffer = algorithm.HashData(inputBuffer);
            byte[] inputBytes;
            CryptographicBuffer.CopyToByteArray(hashedBuffer, out inputBytes);
            return BitConverter.ToString(inputBytes);
#else
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
#endif
        }

        protected override string EncryptOrDecrypt(bool type, string plainStr)
        {
            byte[] cipherText = null;

            #if (SILVERLIGHT || WINDOWS_PHONE)
                AesManaged aesEncryption = new AesManaged();
                aesEncryption.KeySize = 256;
                aesEncryption.BlockSize = 128;
                //get ASCII bytes of the string
                aesEncryption.IV = System.Text.Encoding.UTF8.GetBytes("0123456789012345");
                aesEncryption.Key = System.Text.Encoding.UTF8.GetBytes(GetEncryptionKey());
            #elif NETFX_CORE
            SymmetricKeyAlgorithmProvider algoritmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(GetEncryptionKey(), BinaryStringEncoding.Utf8);
            CryptographicKey key = algoritmProvider.CreateSymmetricKey(keyMaterial);
            IBuffer iv = CryptographicBuffer.ConvertStringToBinary("0123456789012345", BinaryStringEncoding.Utf8);
            #else
				RijndaelManaged aesEncryption = new RijndaelManaged();
				aesEncryption.KeySize = 256;
				aesEncryption.BlockSize = 128;
				//Mode CBC
				aesEncryption.Mode = CipherMode.CBC;
				//padding
				aesEncryption.Padding = PaddingMode.PKCS7;
				//get ASCII bytes of the string
				aesEncryption.IV = System.Text.Encoding.ASCII.GetBytes("0123456789012345");
				aesEncryption.Key = System.Text.Encoding.ASCII.GetBytes(GetEncryptionKey());
            #endif


            if (type)
            {
                plainStr = EncodeNonAsciiCharacters(plainStr);
                #if (SILVERLIGHT || WINDOWS_PHONE)
                ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(plainStr);
                
                //encrypt
                cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
                #elif NETFX_CORE
                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(plainStr, BinaryStringEncoding.Utf8);
                IBuffer buffEncrypt = CryptographicEngine.Encrypt(key, buffMsg, iv);
                CryptographicBuffer.CopyToByteArray(buffEncrypt, out cipherText);
                #else
                ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                byte[] plainText = Encoding.ASCII.GetBytes(plainStr);
                
                //encrypt
                cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
                #endif

                return Convert.ToBase64String(cipherText);
            }
            else
            {
                string decrypted = "";
                try
                {
                    //decode
                    byte[] decryptedBytes = Convert.FromBase64CharArray(plainStr.ToCharArray(), 0, plainStr.Length);

                    
                    #if (SILVERLIGHT || WINDOWS_PHONE)
                    ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
                    //decrypt
                    var data = decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    decrypted = Encoding.UTF8.GetString(data, 0, data.Length);
                    #elif NETFX_CORE
                    IBuffer buffMsg = CryptographicBuffer.DecodeFromBase64String(plainStr);
                    IBuffer buffDecrypted = CryptographicEngine.Decrypt(key, buffMsg, iv);
                    CryptographicBuffer.CopyToByteArray(buffDecrypted, out decryptedBytes);
                    decrypted = Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
                    #else
                    ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
                    //decrypt                    
                    decrypted = System.Text.Encoding.ASCII.GetString(decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length));
                    #endif

                    return decrypted;
                }
                catch (Exception ex)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelVerbose);
                    throw ex;
                    //LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelVerbose);
                    //return "**DECRYPT ERROR**";
                }
            }
        }

    }

    #endregion

}

