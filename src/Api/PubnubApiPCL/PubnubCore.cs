//Build Date: June 06, 2016
#region "Header"
#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID || UNITY_IOS)
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
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

namespace PubnubApi
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
		int _pubnubPresenceHeartbeatInSeconds = 0;
		int _presenceHeartbeatIntervalInSeconds = 0;
		bool _enableResumeOnReconnect = true;
		bool _uuidChanged = false;
		protected bool overrideTcpKeepAlive = true;
		bool _enableJsonEncodingForPublish = true;
        bool _enableDebugForPushPublish = false;
		LoggingMethod.Level _pubnubLogLevel = LoggingMethod.Level.Off;
		PubnubErrorFilter.Level _errorLevel = PubnubErrorFilter.Level.Info;
		protected ConcurrentDictionary<string, long> multiChannelSubscribe = new ConcurrentDictionary<string, long>();
        protected ConcurrentDictionary<string, long> multiChannelGroupSubscribe = new ConcurrentDictionary<string, long>();
		ConcurrentDictionary<string, PubnubWebRequest> _channelRequest = new ConcurrentDictionary<string, PubnubWebRequest>();
		protected ConcurrentDictionary<string, bool> channelInternetStatus = new ConcurrentDictionary<string, bool>();
        protected ConcurrentDictionary<string, bool> channelGroupInternetStatus = new ConcurrentDictionary<string, bool>();
		protected ConcurrentDictionary<string, int> channelInternetRetry = new ConcurrentDictionary<string, int>();
        protected ConcurrentDictionary<string, int> channelGroupInternetRetry = new ConcurrentDictionary<string, int>();
		ConcurrentDictionary<string, Timer> _channelReconnectTimer = new ConcurrentDictionary<string, Timer>();
        ConcurrentDictionary<string, Timer> _channelGroupReconnectTimer = new ConcurrentDictionary<string, Timer>();
		protected ConcurrentDictionary<Uri, Timer> channelLocalClientHeartbeatTimer = new ConcurrentDictionary<Uri, Timer>();
		protected ConcurrentDictionary<PubnubChannelCallbackKey, object> channelCallbacks = new ConcurrentDictionary<PubnubChannelCallbackKey, object>();
        protected ConcurrentDictionary<PubnubChannelGroupCallbackKey, object> channelGroupCallbacks = new ConcurrentDictionary<PubnubChannelGroupCallbackKey, object>();
		ConcurrentDictionary<string, Dictionary<string, object>> _channelLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        ConcurrentDictionary<string, Dictionary<string, object>> _channelUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        ConcurrentDictionary<string, Dictionary<string, object>> _channelGroupLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        ConcurrentDictionary<string, Dictionary<string, object>> _channelGroupUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        ConcurrentDictionary<string, List<string>> _channelSubscribedAuthKeys = new ConcurrentDictionary<string, List<string>>();
		ConcurrentDictionary<string, Type> _channelSubscribeObjectType = new ConcurrentDictionary<string, Type>();
		ConcurrentDictionary<string, Type> _channelGroupSubscribeObjectType = new ConcurrentDictionary<string, Type>();
		protected System.Threading.Timer localClientHeartBeatTimer;
		protected System.Threading.Timer presenceHeartbeatTimer = null;
		protected static bool pubnetSystemActive = true;
        protected Collection<Uri> pushRemoteImageDomainUri = new Collection<Uri>();
        // History of Messages (Obsolete)
		private List<object> _history = new List<object>();

        public List<object> History { get { return _history; } set { _history = value; RaisePropertyChanged("History"); } }

		private static long lastSubscribeTimetoken = 0;
		// Pubnub Core API implementation
		private string _origin = "pubsub.pubnub.com";
        protected string publishKey = "";
		protected string subscribeKey = "";
		protected string secretKey = "";
		protected string cipherKey = "";
		protected bool ssl = false;
        protected string parameters = "";
		private string subscribeParameters = "";
		private string presenceHeartbeatParameters = "";
		private string hereNowParameters = "";
		private string setUserStateParameters = "";
        private string getUserStateParameters = "";
		private string globalHereNowParameters = "";
        private string pushRegisterDeviceParameters = "";
        private string pushRemoveChannelParameters = "";
        private string pushGetChannelsParameters = "";
        private string pushUnregisterDeviceParameters = "";
        private string channelGroupAddParameters = "";
        private string channelGroupRemoveParameters = "";
        private string _pnsdkVersion = "PubNub-CSharp-.NET/3.7.1";
        private string _pushServiceName = "push.pubnub.com";
        private bool _addPayloadToPublishResponse = false; 

		#endregion

		#region "Properties"

        protected string Version
        {
            get
            {
                return _pnsdkVersion;
            }
            set
            {
                _pnsdkVersion = value;
            }
        }

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

        public bool EnableDebugForPushPublish
        {
            get
            {
                return _enableDebugForPushPublish;
            }
            set
            {
                _enableDebugForPushPublish = value;
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

		private IPubnubLog _pubnubLog = null;
		public IPubnubLog PubnubLog
		{
			get{
				return _pubnubLog;
			}
			set {
				_pubnubLog = value;
				if (_pubnubLog != null) {
					LoggingMethod.PubnubLog = _pubnubLog;
					this.PubnubLogLevel = _pubnubLog.LogLevel;
				} else {
					_pubnubLog = null;
					throw new ArgumentException("Missing or Incorrect PubnubLog value");
				}
			}
		}

		private IPubnubSubscribeMessageType _subscribeMessageType = null;
		public IPubnubSubscribeMessageType SubscribeMessageType
		{
			get
			{
				return _subscribeMessageType;
			}
			set
			{
				_subscribeMessageType = value;
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
                if (_presenceHeartbeatIntervalInSeconds >= _pubnubPresenceHeartbeatInSeconds) 
                {
					_presenceHeartbeatIntervalInSeconds = (_pubnubPresenceHeartbeatInSeconds / 2) - 1;
				}
			}
		}

		public LoggingMethod.Level PubnubLogLevel 
        {
			get 
            {
				return _pubnubLogLevel;
			}
			
            set 
            {
				_pubnubLogLevel = value;
                LoggingMethod.LogLevel = _pubnubLogLevel;
			}
		}

		public PubnubErrorFilter.Level PubnubErrorLevel 
        {
			get 
            {
				return _errorLevel;
			}
			
            set 
            {
				_errorLevel = value;
                PubnubErrorFilter.ErrorLevel = _errorLevel;
			}
		}

        public Collection<Uri> PushRemoteImageDomainUri
        {
            get
            {
                return pushRemoteImageDomainUri;
            }
            set
            {
                pushRemoteImageDomainUri = value;
            }
        }

        public string PushServiceName
        {
            get
            {
                return _pushServiceName;
            }

            set
            {
                _pushServiceName = value;
            }
        }

        public bool AddPayloadToPublishResponse
        {
            get
            {
                return _addPayloadToPublishResponse;
            }
            set
            {
                _addPayloadToPublishResponse = value;
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

        protected virtual void ReconnectFromSuspendMode(object netState)
        {
            if (netState == null) return;
            ReconnectFromSuspendModeCallback<string>(netState);
        }
        
        protected virtual void ReconnectNetwork<T>(ReconnectState<T> netState)
		{
            if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)))
            {
                System.Threading.Timer timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
                                                      (-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);

                if (netState.Channels != null && netState.Channels.Length > 0)
                {
                    _channelReconnectTimer.AddOrUpdate(string.Join(",", netState.Channels), timer, (key, oldState) => timer);
                }
                if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                {
                    _channelGroupReconnectTimer.AddOrUpdate(string.Join(",", netState.ChannelGroups), timer, (key, oldState) => timer);
                }
			}
        }

		protected virtual void ReconnectNetworkCallback<T>(System.Object reconnectState)
		{
			string channel = "";
			string channelGroup = "";

			ReconnectState<T> netState = reconnectState as ReconnectState<T>;
			try
			{
				if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)))
				{
					if (netState.Channels != null && netState.Channels.Length > 0)
					{
						channel = (netState.Channels.Length > 0) ? string.Join(",", netState.Channels) : ",";

						if (channelInternetStatus.ContainsKey(channel)
							&& (netState.ResponseType == ResponseType.Subscribe || netState.ResponseType == ResponseType.Presence))
						{
							bool networkConnection;
							if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest)
							{
								networkConnection = true;
							}
							else
							{
								networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, netState.ErrorCallback, netState.Channels, netState.ChannelGroups);
							}

							if (channelInternetStatus[channel])
							{
								//Reset Retry if previous state is true
								channelInternetRetry.AddOrUpdate(channel, 0, (key, oldValue) => 0);
							}
							else
							{
								channelInternetStatus.AddOrUpdate(channel, networkConnection, (key, oldValue) => networkConnection);

								channelInternetRetry.AddOrUpdate(channel, 1, (key, oldValue) => oldValue + 1);
								LoggingMethod.WriteToLog(string.Format("DateTime {0}, channel={1} {2} reconnectNetworkCallback. Retry {3} of {4}", DateTime.Now.ToString(), channel, netState.ResponseType, channelInternetRetry[channel], _pubnubNetworkCheckRetries), LoggingMethod.LevelInfo);

								if (netState.Channels != null && netState.Channels.Length > 0)
								{
									for (int index = 0; index < netState.Channels.Length; index++)
									{
										string activeChannel = (netState.Channels != null && netState.Channels.Length > 0) ? netState.Channels[index].ToString() : "";
										string activeChannelGroup = (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) ? netState.ChannelGroups[index].ToString() : "";

										string message = string.Format("Detected internet connection problem. Retrying connection attempt {0} of {1}", channelInternetRetry[channel], _pubnubNetworkCheckRetries);

										PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
										callbackKey.Channel = activeChannel;
										callbackKey.ResponseType = netState.ResponseType;

										if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
										{
											if (netState.ResponseType == ResponseType.Presence)
											{
												PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
												if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
												{
													CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
														activeChannel, activeChannelGroup, currentPubnubCallback.ErrorCallback, message, PubnubErrorCode.NoInternet,
														null, null);
												}
											}
											else
											{
												PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
												if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
												{
													CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
														activeChannel, activeChannelGroup, currentPubnubCallback.ErrorCallback, message, PubnubErrorCode.NoInternet,
														null, null);
												}
											}
										}
									}
								}

							}
						}

						if (channelInternetStatus.ContainsKey(channel) && channelInternetStatus[channel])
						{
							if (_channelReconnectTimer.ContainsKey(channel))
							{
								try
								{
									_channelReconnectTimer[channel].Change(Timeout.Infinite, Timeout.Infinite);
									_channelReconnectTimer[channel].Dispose();
								}
								catch { }
							}
							string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";
							string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups) : "";
							string message = "Internet connection available";

							CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
								multiChannel, multiChannelGroup, netState.ErrorCallback, message, PubnubErrorCode.YesInternet, null, null);

							LoggingMethod.WriteToLog(string.Format("DateTime {0}, {1} {2} reconnectNetworkCallback. Internet Available : {3}", DateTime.Now.ToString(), channel, netState.ResponseType, channelInternetStatus[channel]), LoggingMethod.LevelInfo);
							switch (netState.ResponseType)
							{
							case ResponseType.Subscribe:
							case ResponseType.Presence:
								MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, netState.SubscribeRegularCallback, netState.PresenceRegularCallback, netState.ConnectCallback, netState.WildcardPresenceCallback, netState.ErrorCallback, true);
								break;
							default:
								break;
							}
						}
						else if (channelInternetRetry.ContainsKey(channel) && channelInternetRetry[channel] >= _pubnubNetworkCheckRetries)
						{
							if (_channelReconnectTimer.ContainsKey(channel))
							{
								try
								{
									_channelReconnectTimer[channel].Change(Timeout.Infinite, Timeout.Infinite);
									_channelReconnectTimer[channel].Dispose();
								}
								catch { }
							}
							switch (netState.ResponseType)
							{
							case ResponseType.Subscribe:
							case ResponseType.Presence:
								MultiplexExceptionHandler<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.SubscribeRegularCallback, netState.PresenceRegularCallback, netState.ConnectCallback, netState.WildcardPresenceCallback, netState.ErrorCallback, true, false);
								break;
							default:
								break;
							}
						}
					}
					else if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
					{
						channelGroup = string.Join(",", netState.ChannelGroups);

						if (channelGroup != "" && channelGroupInternetStatus.ContainsKey(channelGroup)
							&& (netState.ResponseType == ResponseType.Subscribe || netState.ResponseType == ResponseType.Presence))
						{
                            bool networkConnection;
                            if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest)
                            {
                                networkConnection = true;
                            }
                            else
                            {
                                networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, netState.ErrorCallback, netState.Channels, netState.ChannelGroups);
                            }

                            if (channelGroupInternetStatus[channelGroup])
							{
								//Reset Retry if previous state is true
								channelGroupInternetRetry.AddOrUpdate(channelGroup, 0, (key, oldValue) => 0);
							}
							else
							{
                                channelGroupInternetStatus.AddOrUpdate(channelGroup, networkConnection, (key, oldValue) => networkConnection);

                                channelGroupInternetRetry.AddOrUpdate(channelGroup, 1, (key, oldValue) => oldValue + 1);
								LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Retry {3} of {4}", DateTime.Now.ToString(), channelGroup, netState.ResponseType, channelGroupInternetRetry[channelGroup], _pubnubNetworkCheckRetries), LoggingMethod.LevelInfo);

								if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
								{
									for (int index = 0; index < netState.ChannelGroups.Length; index++)
									{
										string activeChannel = (netState.Channels != null && netState.Channels.Length > 0) ? netState.Channels[index].ToString() : "";
										string activeChannelGroup = (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) ? netState.ChannelGroups[index].ToString() : "";

										string message = string.Format("Detected internet connection problem. Retrying connection attempt {0} of {1}", channelGroupInternetRetry[channelGroup], _pubnubNetworkCheckRetries);

										PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
										callbackKey.ChannelGroup = activeChannelGroup;
										callbackKey.ResponseType = netState.ResponseType;

										if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
										{
											if (netState.ResponseType == ResponseType.Presence)
											{
												PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
												if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
												{
													CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
														activeChannel, activeChannelGroup, currentPubnubCallback.ErrorCallback, message, PubnubErrorCode.NoInternet,
														null, null);
												}
											}
											else
											{
												PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
												if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
												{
													CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
														activeChannel, activeChannelGroup, currentPubnubCallback.ErrorCallback, message, PubnubErrorCode.NoInternet,
														null, null);
												}
											}
										}
									}
								}
							}
						}

						if (channelGroupInternetStatus[channelGroup])
						{
							if (_channelGroupReconnectTimer.ContainsKey(channelGroup))
							{
								try
								{
									_channelGroupReconnectTimer[channelGroup].Change(Timeout.Infinite, Timeout.Infinite);
									_channelGroupReconnectTimer[channelGroup].Dispose();
								}
								catch { }
							}
							string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";
							string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups) : "";
							string message = "Internet connection available";

							CallErrorCallback(PubnubErrorSeverity.Warn, PubnubMessageSource.Client,
								multiChannel, multiChannelGroup, netState.ErrorCallback, message, PubnubErrorCode.YesInternet, null, null);

							LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Internet Available : {3}", DateTime.Now.ToString(), channelGroup, netState.ResponseType, channelGroupInternetRetry[channelGroup]), LoggingMethod.LevelInfo);
							switch (netState.ResponseType)
							{
							case ResponseType.Subscribe:
							case ResponseType.Presence:
								MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, netState.SubscribeRegularCallback, netState.PresenceRegularCallback, netState.ConnectCallback, netState.WildcardPresenceCallback, netState.ErrorCallback, true);
								break;
							default:
								break;
							}
						}
						else if (channelGroupInternetRetry[channelGroup] >= _pubnubNetworkCheckRetries)
						{
							if (_channelGroupReconnectTimer.ContainsKey(channelGroup))
							{
								try
								{
									_channelGroupReconnectTimer[channelGroup].Change(Timeout.Infinite, Timeout.Infinite);
									_channelGroupReconnectTimer[channelGroup].Dispose();
								}
								catch { }
							}
							switch (netState.ResponseType)
							{
							case ResponseType.Subscribe:
							case ResponseType.Presence:
								MultiplexExceptionHandler<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.SubscribeRegularCallback, netState.PresenceRegularCallback, netState.ConnectCallback, netState.WildcardPresenceCallback, netState.ErrorCallback, true, false);
								break;
							default:
								break;
							}
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
					string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups) : "";

					CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
						multiChannel, multiChannelGroup, netState.ErrorCallback, ex, null, null);
				}

				LoggingMethod.WriteToLog(string.Format("DateTime {0} method:reconnectNetworkCallback \n Exception Details={1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
			}
		}

        private bool InternetConnectionStatusWithUnitTestCheck(string channel, string channelGroup, Action<PubnubClientError> errorCallback, string[] rawChannels, string[] rawChannelGroups)
		{
			bool networkConnection;
			if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) 
            {
				networkConnection = true;
			} 
            else 
            {
				networkConnection = InternetConnectionStatus(channel, channelGroup, errorCallback, rawChannels, rawChannelGroups);
				if (!networkConnection) 
                {
					string message = "Network connnect error - Internet connection is not available.";
					CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                        channel, channelGroup, errorCallback, message,
						PubnubErrorCode.NoInternet, null, null);
				}
			}

			return networkConnection;
		}

        protected virtual bool InternetConnectionStatus(string channel, string channelGroup, Action<PubnubClientError> errorCallback, string[] rawChannels, string[] rawChannelGroups)
		{
			bool networkConnection;
			networkConnection = ClientNetworkStatus.CheckInternetStatus(pubnetSystemActive, errorCallback, rawChannels, rawChannelGroups);
			return networkConnection;
		}

		private void ResetInternetCheckSettings(string[] channels, string[] channelGroups)
		{
			if (channels == null && channelGroups == null)
				return;

            string multiChannel = (channels != null) ? string.Join(",", channels) : "";
            string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            //if (multiChannel == "")
            //{
            //    multiChannel = ",";
            //}
            if (multiChannel != "")
            {
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

            if (multiChannelGroup != "")
            {
                if (channelGroupInternetStatus.ContainsKey(multiChannelGroup))
                {
                    channelGroupInternetStatus.AddOrUpdate(multiChannelGroup, true, (key, oldValue) => true);
                }
                else
                {
                    channelGroupInternetStatus.GetOrAdd(multiChannelGroup, true); //Set to true for internet connection
                }

                if (channelGroupInternetRetry.ContainsKey(multiChannelGroup))
                {
                    channelGroupInternetRetry.AddOrUpdate(multiChannelGroup, 0, (key, oldValue) => 0);
                }
                else
                {
                    channelGroupInternetRetry.GetOrAdd(multiChannelGroup, 0); //Initialize the internet retry count
                }
            }
        }

		protected virtual bool ReconnectNetworkIfOverrideTcpKeepAlive<T>(ResponseType type, string[] channels, string[] channelGroups, object timetoken, Action<Message<T>> subscribeCallback, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> presenceWildcardCallback, Action<PubnubClientError> errorCallback)
		{
			if (overrideTcpKeepAlive)
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, Subscribe - No internet connection for channel={1} and channelgroup={2}", DateTime.Now.ToString(), string.Join(",", channels), ((channelGroups != null) ? string.Join(",", channelGroups) : "")), LoggingMethod.LevelInfo);
				ReconnectState<T> netState = new ReconnectState<T>();
				netState.Channels = channels;
				netState.ChannelGroups = channelGroups;
				netState.ResponseType = type;
				netState.SubscribeRegularCallback = subscribeCallback;
				netState.PresenceRegularCallback = presenceCallback;
				netState.WildcardPresenceCallback = presenceWildcardCallback;
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

        protected virtual void ReconnectFromSuspendModeCallback<T>(System.Object reconnectState)
        {
            if (PubnubWebRequest.MachineSuspendMode && ClientNetworkStatus.MachineSuspendMode)
            {
                return;
            }
            
            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Reconnect from Machine Suspend Mode.", DateTime.Now.ToString()), LoggingMethod.LevelInfo);

            ReconnectState<T> netState = reconnectState as ReconnectState<T>;
            try
            {
                if (netState != null)
                {
                    switch (netState.ResponseType)
                    {
                        case ResponseType.Subscribe:
                        case ResponseType.Presence:
							MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, netState.SubscribeRegularCallback, netState.PresenceRegularCallback, netState.ConnectCallback, netState.WildcardPresenceCallback, netState.ErrorCallback, netState.Reconnect);
							break;
                        default:
                            break;
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unknown request state in ReconnectFromSuspendModeCallback", DateTime.Now.ToString()), LoggingMethod.LevelError);
                }
            }
            catch (Exception ex)
            {
                if (netState != null)
                {
                    string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";
                    string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups) : "";

                    CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                        multiChannel, multiChannelGroup, netState.ErrorCallback, ex, null, null);
                }

                LoggingMethod.WriteToLog(string.Format("DateTime {0} method:ReconnectFromSuspendModeCallback \n Exception Details={1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
            }
        }
		#endregion

		#region "Error Callbacks"

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, string channelGroup, Action<PubnubClientError> errorCallback, 
		                                                 string message, PubnubErrorCode errorType, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			int statusCode = (int)errorType;

			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel, channelGroup);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, string channelGroup, Action<PubnubClientError> errorCallback, 
		                                                 string message, int currentHttpStatusCode, string statusMessage,
		                                                 PubnubWebRequest req, PubnubWebResponse res)
		{
			PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType ((int)currentHttpStatusCode, statusMessage);

			int statusCode = (int)pubnubErrorType;

			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (pubnubErrorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel, channelGroup);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, string channelGroup, Action<PubnubClientError> errorCallback, 
		                                                 Exception ex, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);

			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, true, ex.Message, ex, msgSource, req, res, errorDescription, channel, channelGroup);
			GoToCallback (error, errorCallback);
			return error;
		}

		protected PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
		                                                 string channel, string channelGroup, Action<PubnubClientError> errorCallback, 
		                                                 WebException webex, PubnubWebRequest req, 
		                                                 PubnubWebResponse res)
		{
			PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (webex.Status, webex.Message);
			int statusCode = (int)errorType;
			string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);

			PubnubClientError error = new PubnubClientError (statusCode, errSeverity, true, webex.Message, webex, msgSource, req, res, errorDescription, channel, channelGroup);
			GoToCallback (error, errorCallback);
			return error;
		}

		#endregion

		#region "Terminate requests and Timers"

		protected void TerminatePendingWebRequest ()
		{
			TerminatePendingWebRequest<object> (null);
		}

		protected void TerminatePendingWebRequest<T>(RequestState<T> state)
		{
			if (state != null && state.Request != null)
			{
				if (state.Channels != null && state.Channels.Length > 0)
				{
					string activeChannel = state.Channels[0].ToString(); //Assuming one channel exist, else will refactor later
					PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
					callbackKey.Channel = (state.ResponseType == ResponseType.Subscribe) ? activeChannel.Replace("-pnpres", "") : activeChannel;
					callbackKey.ResponseType = state.ResponseType;

					if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
					{
						object callbackObject;
						bool channelAvailable = channelCallbacks.TryGetValue(callbackKey, out callbackObject);
						if (channelAvailable)
						{
							if (state.ResponseType == ResponseType.Presence)
							{
								PubnubPresenceChannelCallback currentPubnubCallback = callbackObject as PubnubPresenceChannelCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
								{
									state.Request.Abort(currentPubnubCallback.ErrorCallback, _errorLevel);
								}
							}
							else
							{
								PubnubSubscribeChannelCallback<T> currentPubnubCallback = callbackObject as PubnubSubscribeChannelCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
								{
									state.Request.Abort(currentPubnubCallback.ErrorCallback, _errorLevel);
								}
							}
						}
					}
				}
				if (state.ChannelGroups != null && state.ChannelGroups.Length > 0 && state.ChannelGroups[0] != null)
				{
					string activeChannelGroup = state.ChannelGroups[0].ToString(); //Assuming one channel exist, else will refactor later
					PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
					callbackKey.ChannelGroup = (state.ResponseType == ResponseType.Subscribe) ? activeChannelGroup.Replace("-pnpres", "") : activeChannelGroup;
					callbackKey.ResponseType = state.ResponseType;

					if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
					{
						object callbackObject;
						bool channelAvailable = channelGroupCallbacks.TryGetValue(callbackKey, out callbackObject);
						if (channelAvailable)
						{
							if (state.ResponseType == ResponseType.Presence)
							{
								PubnubPresenceChannelGroupCallback currentPubnubCallback = callbackObject as PubnubPresenceChannelGroupCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
								{
									state.Request.Abort(currentPubnubCallback.ErrorCallback, _errorLevel);
								}
							}
							else
							{
								PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = callbackObject as PubnubSubscribeChannelGroupCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
								{
									state.Request.Abort(currentPubnubCallback.ErrorCallback, _errorLevel);
								}
							}
						}
					}
				}
			}
			else
			{
				ICollection<string> keyCollection = _channelRequest.Keys;
				foreach (string key in keyCollection)
				{
					PubnubWebRequest currentRequest = _channelRequest[key];
					if (currentRequest != null)
					{
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
				string channel = (state.Channels != null) ? string.Join (",", state.Channels) : ",";

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
                if (keyCollection != null && keyCollection.Count > 0)
                {
                    List<string> keysList = keyCollection.ToList();
                    foreach (string key in keysList)
                    {
                        PubnubWebRequest currentRequest = _channelRequest[key];
                        if (currentRequest != null)
                        {
                            bool removeKey = _channelRequest.TryRemove(key, out currentRequest);
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelInfo);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelError);
                            }
                        }
                    }
                }
			}
		}

		private void RemoveChannelCallback<T>(string channel, ResponseType type)
		{
			string[] arrChannels = channel.Split(',');
			if (arrChannels != null && arrChannels.Length > 0)
			{
				foreach (string arrChannel in arrChannels)
				{
					PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
					callbackKey.Channel = arrChannel;
					switch (type)
					{
					case ResponseType.Unsubscribe:
						callbackKey.ResponseType = ResponseType.Subscribe;
						break;
					case ResponseType.PresenceUnsubscribe:
						callbackKey.ResponseType = ResponseType.Presence;
						break;
					default:
						callbackKey.ResponseType = ResponseType.Time; //overriding the default
						break;
					}

					if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
					{
						if (type == ResponseType.Presence)
						{
							PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
							if (currentPubnubCallback != null)
							{
								currentPubnubCallback.PresenceRegularCallback = null;
								currentPubnubCallback.ConnectCallback = null;
							}
						}
						else
						{
							PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
							if (currentPubnubCallback != null)
							{
								currentPubnubCallback.SubscribeRegularCallback = null;
								currentPubnubCallback.ConnectCallback = null;
							}
						}
					}

				}
			}

		}

		private void RemoveChannelCallback()
		{
			ICollection<PubnubChannelCallbackKey> channelCollection = channelCallbacks.Keys;
            if (channelCollection != null && channelCollection.Count > 0)
            {
                List<PubnubChannelCallbackKey> channelList = channelCollection.ToList();
                foreach (PubnubChannelCallbackKey keyChannel in channelList)
                {
                    if (channelCallbacks.ContainsKey(keyChannel))
                    {
                        object tempChannelCallback;
                        bool removeKey = channelCallbacks.TryRemove(keyChannel, out tempChannelCallback);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveChannelCallback from dictionary in RemoveChannelCallback for channel= {1}", DateTime.Now.ToString(), removeKey), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveChannelCallback from dictionary in RemoveChannelCallback for channel= {1}", DateTime.Now.ToString(), removeKey), LoggingMethod.LevelError);
                        }
                    }
                }
            }
		}

		private void RemoveChannelGroupCallback<T>(string channelGroup, ResponseType type)
		{
			string[] arrChannelGroups = channelGroup.Split(',');
			if (arrChannelGroups != null && arrChannelGroups.Length > 0)
			{
				foreach (string arrChannelGroup in arrChannelGroups)
				{
					PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
					callbackKey.ChannelGroup = arrChannelGroup;
					switch (type)
					{
					case ResponseType.Unsubscribe:
						callbackKey.ResponseType = ResponseType.Subscribe;
						break;
					case ResponseType.PresenceUnsubscribe:
						callbackKey.ResponseType = ResponseType.Presence;
						break;
					default:
						callbackKey.ResponseType = ResponseType.Time; //overriding the default
						break;
					}

					if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
					{
						if (type == ResponseType.Presence)
						{
							PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
							if (currentPubnubCallback != null)
							{
								currentPubnubCallback.PresenceRegularCallback = null;
								currentPubnubCallback.ConnectCallback = null;
							}
						}
						else
						{
							PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
							if (currentPubnubCallback != null)
							{
								currentPubnubCallback.SubscribeRegularCallback = null;
								currentPubnubCallback.ConnectCallback = null;
							}
						}
					}

				}
			}

		}

        private void RemoveChannelGroupCallback()
        {
            ICollection<PubnubChannelGroupCallbackKey> channelGroupCollection = channelGroupCallbacks.Keys;
            if (channelGroupCollection != null && channelGroupCollection.Count > 0)
            {
                List<PubnubChannelGroupCallbackKey> channelGroupCallbackList = channelGroupCollection.ToList();
                foreach (PubnubChannelGroupCallbackKey keyChannelGroup in channelGroupCallbackList)
                {
                    if (channelGroupCallbacks.ContainsKey(keyChannelGroup))
                    {
                        object tempChannelGroupCallback;
                        bool removeKey = channelGroupCallbacks.TryRemove(keyChannelGroup, out tempChannelGroupCallback);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveChannelGroupCallback from dictionary in RemoveChannelGroupCallback for channelgroup= {1}", DateTime.Now.ToString(), keyChannelGroup), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveChannelGroupCallback from dictionary in RemoveChannelGroupCallback for channelgroup= {1}", DateTime.Now.ToString(), keyChannelGroup), LoggingMethod.LevelError);
                        }
                    }
                }
            }
        }

        private void RemoveUserState()
        {
            ICollection<string> channelLocalUserStateCollection = _channelLocalUserState.Keys;
            ICollection<string> channelUserStateCollection = _channelUserState.Keys;

            ICollection<string> channelGroupLocalUserStateCollection = _channelGroupLocalUserState.Keys;
            ICollection<string> channelGroupUserStateCollection = _channelGroupUserState.Keys;

            if (channelLocalUserStateCollection != null && channelLocalUserStateCollection.Count > 0)
            {
                List<string> channelLocalStateList = channelLocalUserStateCollection.ToList();
                foreach (string key in channelLocalStateList)
                {
                    if (_channelLocalUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = _channelLocalUserState.TryRemove(key, out tempUserState);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelError);
                        }
                    }
                }
            }

            if (channelUserStateCollection != null && channelUserStateCollection.Count > 0)
            {
                List<string> channelStateList = channelUserStateCollection.ToList();
                foreach (string key in channelStateList)
                {
                    if (_channelUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = _channelUserState.TryRemove(key, out tempUserState);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelError);
                        }
                    }
                }
            }

            if (channelGroupLocalUserStateCollection != null && channelGroupLocalUserStateCollection.Count > 0)
            {
                List<string> channelGroupLocalStateList = channelGroupLocalUserStateCollection.ToList();
                foreach (string key in channelGroupLocalStateList)
                {
                    if (_channelGroupLocalUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = _channelGroupLocalUserState.TryRemove(key, out tempUserState);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelError);
                        }
                    }
                }
            }


            if (channelGroupUserStateCollection != null && channelGroupUserStateCollection.Count > 0)
            {
                List<string> channelGroupStateList = channelGroupUserStateCollection.ToList();

                foreach (string key in channelGroupStateList)
                {
                    if (_channelGroupUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = _channelGroupUserState.TryRemove(key, out tempUserState);
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), LoggingMethod.LevelError);
                        }
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
					Timer requestHeatbeatTimer = null;
                    if (channelLocalClientHeartbeatTimer.TryGetValue(requestUri, out requestHeatbeatTimer) && requestHeatbeatTimer != null)
                    {
						try {
							requestHeatbeatTimer.Change (
								(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000,
								(-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
							requestHeatbeatTimer.Dispose ();
						} catch (ObjectDisposedException ex) {
							//Known exception to be ignored
                            //LoggingMethod.WriteToLog (string.Format ("DateTime {0} Error while accessing requestHeatbeatTimer object in TerminateLocalClientHeartbeatTimer {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelInfo);
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
                if (keyCollection != null && keyCollection.Count > 0)
                {
                    List<Uri> keyList = keyCollection.ToList();
                    foreach (Uri key in keyList)
                    {
                        if (channelLocalClientHeartbeatTimer.ContainsKey(key))
                        {
                            Timer currentTimer = null;
                            if (channelLocalClientHeartbeatTimer.TryGetValue(key, out currentTimer) && currentTimer != null)
                            {
                                currentTimer.Dispose();
                                Timer removedTimer = null;
                                bool removed = channelLocalClientHeartbeatTimer.TryRemove(key, out removedTimer);
                                if (!removed)
                                {
                                    LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateLocalClientHeartbeatTimer(null) - Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), key.ToString()), LoggingMethod.LevelInfo);
                                }
                            }
                        }
                    }
                }
			}
		}

        private void TerminateReconnectTimer()
        {
            ConcurrentDictionary<string, Timer> channelReconnectCollection = _channelReconnectTimer;
            ICollection<string> keyCollection = channelReconnectCollection.Keys;
            if (keyCollection != null && keyCollection.Count > 0)
            {
                List<string> keyList = keyCollection.ToList();
                foreach (string key in keyList)
                {
                    if (_channelReconnectTimer.ContainsKey(key))
                    {
                        Timer currentTimer = _channelReconnectTimer[key];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = _channelReconnectTimer.TryRemove(key, out removedTimer);
                        if (!removed)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channel reconnect timer reference from collection for {1}", DateTime.Now.ToString(), key.ToString()), LoggingMethod.LevelInfo);
                        }
                    }
                }
            }


            ConcurrentDictionary<string, Timer> channelGroupReconnectCollection = _channelGroupReconnectTimer;
            ICollection<string> groupKeyCollection = channelGroupReconnectCollection.Keys;
            if (groupKeyCollection != null && groupKeyCollection.Count > 0)
            {
                List<string> groupKeyList = groupKeyCollection.ToList();
                foreach (string groupKey in groupKeyList)
                {
                    if (_channelGroupReconnectTimer.ContainsKey(groupKey))
                    {
                        Timer currentTimer = _channelGroupReconnectTimer[groupKey];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = _channelGroupReconnectTimer.TryRemove(groupKey, out removedTimer);
                        if (!removed)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channelgroup reconnect timer reference from collection for {1}", DateTime.Now.ToString(), groupKey.ToString()), LoggingMethod.LevelInfo);
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
            RemoveChannelGroupCallback();
            RemoveUserState();
            TerminatePresenceHeartbeatTimer();
		}

		public void TerminateCurrentSubscriberRequest()
		{
			string[] channels = GetCurrentSubscriberChannels ();
			if (channels != null) {
                string multiChannel = (channels.Length > 0) ? string.Join(",", channels) : ",";
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
            string[] channelGroups = GetCurrentSubscriberChannelGroups();

            channels = (channels != null) ? channels : new string[] { };
            channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

            if (channels.Length > 0 || channelGroups.Length > 0)
            {
                Uri request = BuildMultiChannelLeaveRequest(channels, channelGroups, oldUUID);

                RequestState<string> requestState = new RequestState<string>();
                requestState.Channels = channels;
                requestState.ChannelGroups = channelGroups;
				requestState.ResponseType = ResponseType.Leave;
				requestState.SubscribeRegularCallback = null;
				requestState.PresenceRegularCallback = null;
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
            if (IsNullOrWhiteSpace(publishKey)) { publishKey = ""; }
            if (IsNullOrWhiteSpace(subscribeKey)) { subscribeKey = ""; }
            if (IsNullOrWhiteSpace(secretKey)) { secretKey = ""; }
            if (IsNullOrWhiteSpace(cipherKey)) { cipherKey = ""; }
            
            this.Init(publishKey, subscribeKey, secretKey, cipherKey, sslOn);
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
            if (IsNullOrWhiteSpace(publishKey)) { publishKey = ""; }
            if (IsNullOrWhiteSpace(subscribeKey)) { subscribeKey = ""; }
            
			this.Init(publishKey, subscribeKey, "", "", true);
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
            if (IsNullOrWhiteSpace(publishKey)) { publishKey = ""; }
            if (IsNullOrWhiteSpace(subscribeKey)) { subscribeKey = ""; }
            if (IsNullOrWhiteSpace(secretKey)) { secretKey = ""; }
            
			this.Init(publishKey, subscribeKey, secretKey, "", true);
		}

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return string.IsNullOrEmpty(value.Trim());
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
			return false;
		}
		#endregion

		#region "Detailed History"

		/**
         * Detailed History
         */
		internal bool DetailedHistory(string channel, long start, long end, int count, bool reverse, bool includeToken, Action<DetailedHistoryAck> userCallback, Action<PubnubClientError> errorCallback)
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


			Uri request = BuildDetailedHistoryRequest (channel, start, end, count, reverse, includeToken);

			RequestState<DetailedHistoryAck> requestState = new RequestState<DetailedHistoryAck>();
			requestState.Channels = new string[] { channel };
			requestState.ResponseType = ResponseType.DetailedHistory;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<DetailedHistoryAck>(request, requestState);
		}

        private Uri BuildDetailedHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken)
		{
            StringBuilder parameterBuilder = new StringBuilder();
			parameters = "";
			if (count <= -1)
				count = 100;
            
            parameterBuilder.AppendFormat("?count={0}", count);
            if (reverse)
            {
                parameterBuilder.AppendFormat("&reverse={0}", reverse.ToString().ToLower());
            }
            if (start != -1)
            {
                parameterBuilder.AppendFormat("&start={0}", start.ToString().ToLower());
            }
            if (end != -1)
            {
                parameterBuilder.AppendFormat("&end={0}", end.ToString().ToLower());
            }
			if (!string.IsNullOrEmpty (_authenticationKey)) 
            {
                parameterBuilder.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, ResponseType.DetailedHistory, false, false));
			}

            parameterBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(sessionUUID, ResponseType.DetailedHistory, false, false));
            if (includeToken)
            {
                parameterBuilder.AppendFormat("&include_token={0}", includeToken.ToString().ToLower());
            }
            parameterBuilder.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, ResponseType.DetailedHistory, false, true));

            parameters = parameterBuilder.ToString();

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

        #region "Push"
        public void RegisterDeviceForPush(string channel, PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            RegisterDeviceForPush<object>(channel, pushType, pushToken, userCallback, errorCallback); 
        }

        public void RegisterDeviceForPush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            Uri request = BuildRegisterDevicePushRequest(channel, pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
			requestState.ResponseType = ResponseType.PushRegister;
			requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void UnregisterDeviceForPush(PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            UnregisterDeviceForPush<object>(pushType, pushToken, userCallback, errorCallback); 
        }

        public void UnregisterDeviceForPush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
			if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            Uri request = BuildUnregisterDevicePushRequest(pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
			requestState.ResponseType = ResponseType.PushUnregister;
			requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void RemoveChannelForDevicePush(string channel, PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            RemoveChannelForDevicePush<object>(channel, pushType, pushToken, userCallback, errorCallback); 
        }

		public void RemoveChannelForDevicePush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
			{
				throw new ArgumentException("Missing Channel");
			}
			if (pushType == PushTypeService.None)
			{
				throw new ArgumentException("Missing PushTypeService");
			}
			if (pushToken == null)
			{
				throw new ArgumentException("Missing Uri");
			}
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildRemoveChannelPushRequest(channel, pushType, pushToken);

			RequestState<T> requestState = new RequestState<T>();
			requestState.Channels = new string[] { channel };
			requestState.ResponseType = ResponseType.PushRemove;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T>(request, requestState);
		}

        public void GetChannelsForDevicePush(PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            GetChannelsForDevicePush<object>(pushType, pushToken, userCallback, errorCallback); 
        }

		public void GetChannelsForDevicePush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (pushType == PushTypeService.None)
			{
				throw new ArgumentException("Missing PushTypeService");
			}
			if (pushToken == null)
			{
				throw new ArgumentException("Missing Uri");
			}
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsPushRequest(pushType, pushToken);

			RequestState<T> requestState = new RequestState<T>();
			requestState.ResponseType = ResponseType.PushGet;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T>(request, requestState);
		}

        private Uri BuildRegisterDevicePushRequest(string channel, PushTypeService pushType, string pushToken)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            pushRegisterDeviceParameters = "";

            parameterBuilder.AppendFormat("?add={0}", EncodeUricomponent(channel, ResponseType.PushRegister, true, false));
            parameterBuilder.AppendFormat("&type={0}", pushType.ToString().ToLower());
            
            pushRegisterDeviceParameters = parameterBuilder.ToString();

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            return BuildRestApiRequest<Uri>(url, ResponseType.PushRegister);
        }

        private Uri BuildRemoveChannelPushRequest(string channel, PushTypeService pushType, string pushToken)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            pushRemoveChannelParameters = "";

            parameterBuilder.AppendFormat("?remove={0}", EncodeUricomponent(channel, ResponseType.PushRemove, true, false));
            parameterBuilder.AppendFormat("&type={0}", pushType.ToString().ToLower());

            pushRemoveChannelParameters = parameterBuilder.ToString();

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            return BuildRestApiRequest<Uri>(url, ResponseType.PushRemove);
        }

        private Uri BuildGetChannelsPushRequest(PushTypeService pushType, string pushToken)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            pushGetChannelsParameters = "";

            parameterBuilder.AppendFormat("?type={0}", pushType.ToString().ToLower());

            pushGetChannelsParameters = parameterBuilder.ToString();

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            return BuildRestApiRequest<Uri>(url, ResponseType.PushGet);
        }

        private Uri BuildUnregisterDevicePushRequest(PushTypeService pushType, string pushToken)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            pushUnregisterDeviceParameters = "";

            parameterBuilder.AppendFormat("?type={0}", pushType.ToString().ToLower());

            pushUnregisterDeviceParameters = parameterBuilder.ToString();

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());
            url.Add("remove");

            return BuildRestApiRequest<Uri>(url, ResponseType.PushUnregister);
        }
        #endregion

        #region "Channel Group"

		public void AddChannelsToChannelGroup(string[] channels, string groupName, Action<AddChannelToChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			AddChannelsToChannelGroup(channels, "", groupName, userCallback, errorCallback);
		}

		public void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Action<AddChannelToChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (channels == null || channels.Length == 0)
			{
				throw new ArgumentException("Missing channel(s)");
			}

			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
			{
				throw new ArgumentException("Missing groupName");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildAddChannelsToChannelGroupRequest(channels, nameSpace, groupName);

			RequestState<AddChannelToChannelGroupAck> requestState = new RequestState<AddChannelToChannelGroupAck>();
			requestState.ResponseType = ResponseType.ChannelGroupAdd;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<AddChannelToChannelGroupAck>(request, requestState);
		}

		public void RemoveChannelsFromChannelGroup(string[] channels, string groupName, Action<RemoveChannelFromChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			RemoveChannelsFromChannelGroup(channels, "", groupName, userCallback, errorCallback);
		}

        /// <summary>
        /// Remove channel(s) from group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channels"></param>
        /// <param name="nameSpace"></param>
        /// <param name="groupName"></param>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Action<RemoveChannelFromChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (channels == null || channels.Length == 0)
			{
				throw new ArgumentException("Missing channel(s)");
			}

			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
			{
				throw new ArgumentException("Missing groupName");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildRemoveChannelsFromChannelGroupRequest(channels, nameSpace, groupName);

			RequestState<RemoveChannelFromChannelGroupAck> requestState = new RequestState<RemoveChannelFromChannelGroupAck>();
			requestState.ResponseType = ResponseType.ChannelGroupRemove;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<RemoveChannelFromChannelGroupAck>(request, requestState);
		}

        /// <summary>
        /// Removes group and all its channels
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace"></param>
        /// <param name="groupName"></param>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void RemoveChannelGroup(string nameSpace, string groupName, Action<RemoveChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
			{
				throw new ArgumentException("Missing groupName");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildRemoveChannelsFromChannelGroupRequest(null, nameSpace, groupName);

			RequestState<RemoveChannelGroupAck> requestState = new RequestState<RemoveChannelGroupAck>();
			requestState.ResponseType = ResponseType.ChannelGroupRemove;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<RemoveChannelGroupAck>(request, requestState);
		}

        /// <summary>
        /// Removes namespace and all its group names and all channels
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace"></param>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void RemoveChannelGroupNameSpace(string nameSpace, Action<RemoveNamespaceAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildRemoveChannelsFromChannelGroupRequest(null, nameSpace, null);

			RequestState<RemoveNamespaceAck> requestState = new RequestState<RemoveNamespaceAck>();
			requestState.ResponseType = ResponseType.ChannelGroupRemove;
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}",nameSpace,"") };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<RemoveNamespaceAck>(request, requestState);
		}

        /// <summary>
        /// Get all channels for a given channel group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace"></param>
        /// <param name="groupName"></param>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void GetChannelsForChannelGroup<T>(string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
			{
				throw new ArgumentException("Missing groupName");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsForChannelGroupRequest(nameSpace, groupName, false);

			RequestState<T> requestState = new RequestState<T>();
			requestState.ResponseType = ResponseType.ChannelGroupGet;
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T>(request, requestState);
		}

		public void GetChannelsForChannelGroup(string groupName, Action<GetChannelGroupChannelsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
			{
				throw new ArgumentException("Missing groupName");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsForChannelGroupRequest(null, groupName, false);

			RequestState<GetChannelGroupChannelsAck> requestState = new RequestState<GetChannelGroupChannelsAck>();
			requestState.ResponseType = ResponseType.ChannelGroupGet;
			requestState.ChannelGroups = new string[] { groupName };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<GetChannelGroupChannelsAck>(request, requestState);
		}
        
        public void GetAllChannelGroups(string nameSpace, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            GetAllChannelGroups<object>(nameSpace, userCallback, errorCallback);
        }

        /// <summary>
        /// Get all channel group names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace"></param>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void GetAllChannelGroups<T>(string nameSpace, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (nameSpace == null)
			{
				throw new ArgumentException("Missing nameSpace");
			}

			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsForChannelGroupRequest(nameSpace, null, true);

			RequestState<T> requestState = new RequestState<T>();
			requestState.ResponseType = ResponseType.ChannelGroupGet;
			requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace,"") };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T>(request, requestState);
		}

		public void GetAllChannelGroups(Action<GetAllChannelGroupsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsForChannelGroupRequest(null, null, true);

			RequestState<GetAllChannelGroupsAck> requestState = new RequestState<GetAllChannelGroupsAck>();
			requestState.ResponseType = ResponseType.ChannelGroupGet;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new string[] { };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<GetAllChannelGroupsAck>(request, requestState);
		}

        public void GetAllChannelGroupNamespaces(Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            GetAllChannelGroupNamespaces<object>(userCallback, errorCallback);
        }

        /// <summary>
        /// Get all namespaces
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userCallback"></param>
        /// <param name="errorCallback"></param>
		public void GetAllChannelGroupNamespaces<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			Uri request = BuildGetChannelsForChannelGroupRequest(null, null, false);

			RequestState<T> requestState = new RequestState<T>();
			requestState.ResponseType = ResponseType.ChannelGroupGet;
			requestState.Channels = new string[] { };
			requestState.ChannelGroups = new string[] { };
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<T>(request, requestState);
		}

        private Uri BuildAddChannelsToChannelGroupRequest(string[] channels, string nameSpace, string groupName)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            channelGroupAddParameters = "";

            parameterBuilder.AppendFormat("?add={0}", string.Join(",",channels));

            channelGroupAddParameters = parameterBuilder.ToString();

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                url.Add("namespace");
                url.Add(nameSpace);
            }
            url.Add("channel-group");
            url.Add(groupName);

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupAdd);
        }

        private Uri BuildRemoveChannelsFromChannelGroupRequest(string[] channels, string nameSpace, string groupName)
        {
            bool groupNameAvailable = false;
            bool nameSpaceAvailable = false;
            bool channelAvaiable = false;

            StringBuilder parameterBuilder = new StringBuilder();
            channelGroupRemoveParameters = "";

            if (channels != null && channels.Length > 0)
            {
                channelAvaiable = true;
                parameterBuilder.AppendFormat("?remove={0}", string.Join(",", channels));
                channelGroupRemoveParameters = parameterBuilder.ToString();
            }

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                nameSpaceAvailable = true;
                url.Add("namespace");
                url.Add(nameSpace);
            }
            if (!string.IsNullOrEmpty(groupName) && groupName.Trim().Length > 0)
            {
                groupNameAvailable = true;
                url.Add("channel-group");
                url.Add(groupName);
            }
            if (nameSpaceAvailable && groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }
            else if (nameSpaceAvailable && !groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }
            else if (!nameSpaceAvailable && groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }  

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupRemove);
        }

        private Uri BuildGetChannelsForChannelGroupRequest(string nameSpace, string groupName, bool limitToChannelGroupScopeOnly)
        {
            bool groupNameAvailable = false;
            bool nameSpaceAvailable = false;

            // Build URL
            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(this.subscribeKey);
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                nameSpaceAvailable = true;
                url.Add("namespace");
                url.Add(nameSpace);
            }
            if (limitToChannelGroupScopeOnly)
            {
                url.Add("channel-group");
            }
            else
            {
                if (!string.IsNullOrEmpty(groupName) && groupName.Trim().Length > 0)
                {
                    groupNameAvailable = true;
                    url.Add("channel-group");
                    url.Add(groupName);
                }

                if (!nameSpaceAvailable && !groupNameAvailable)
                {
                    url.Add("namespace");
                }
                else if (nameSpaceAvailable && !groupNameAvailable)
                {
                    url.Add("channel-group");
                }
            }
            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupGet);
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
        public bool Publish(string channel, object message, bool storeInHistory, string jsonUserMetaData, Action<PublishAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
			{
				throw new ArgumentException("Missing Channel or Message");
			}

			if (string.IsNullOrEmpty(this.publishKey) || string.IsNullOrEmpty(this.publishKey.Trim()) || this.publishKey.Length <= 0)
			{
				throw new MissingMemberException("Invalid publish key");
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

			if (_enableDebugForPushPublish)
			{
				if (message is Dictionary<string,object>)
				{
					Dictionary<string, object> dicMessage = message as Dictionary<string, object>;
					dicMessage.Add("pn_debug", true);
					message = dicMessage;
				}
			}

            if (string.IsNullOrEmpty(jsonUserMetaData) || !_jsonPluggableLibrary.IsDictionaryCompatible(jsonUserMetaData))
            {
                jsonUserMetaData = "";
            }

            Uri request = BuildPublishRequest(channel, message, storeInHistory, jsonUserMetaData);

			RequestState<PublishAck> requestState = new RequestState<PublishAck>();
			requestState.Channels = new string[] { channel };
			requestState.ResponseType = ResponseType.Publish;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<PublishAck>(request, requestState);
		}

        private Uri BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, string jsonUserMetaData)
		{
			string message = (_enableJsonEncodingForPublish) ? JsonEncodePublishMsg (originalMessage) : originalMessage.ToString ();

            StringBuilder publishParamBuilder = new StringBuilder();
            if (!storeInHistory)
            {
                publishParamBuilder.Append("store=0");
            }
            if (!string.IsNullOrEmpty(jsonUserMetaData) && _jsonPluggableLibrary != null && _jsonPluggableLibrary.IsDictionaryCompatible(jsonUserMetaData))
            {
                if (publishParamBuilder.ToString().Length > 0)
                {
                    publishParamBuilder.AppendFormat("&meta={0}", EncodeUricomponent(jsonUserMetaData, ResponseType.Publish, false, false));
                }
                else
                {
                    publishParamBuilder.AppendFormat("meta={0}", EncodeUricomponent(jsonUserMetaData, ResponseType.Publish, false, false));
                }
            }
            parameters = publishParamBuilder.ToString();  

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
		private List<object> DecodeDecryptLoop (List<object> message, string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback)
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
                            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

							CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                multiChannel, multiChannelGroup, errorCallback, ex, null, null);
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

		protected virtual string EncodeUricomponent (string s, ResponseType type, bool ignoreComma, bool ignorePercent2fEncode)
		{
			string encodedUri = "";
			bool prevSurroagePair = false;
			StringBuilder o = new StringBuilder ();
			foreach (char ch in s) {
				if (prevSurroagePair) {
					prevSurroagePair = false;
					continue;
				}
				if (IsUnsafe (ch, ignoreComma)) {
					o.Append ('%');
					o.Append (ToHex (ch / 16));
					o.Append (ToHex (ch % 16));
				} else {
					int positionOfChar = s.IndexOf (ch);
					if (ch == ',' && ignoreComma) {
						o.Append (ch.ToString ());
					} 
					else if (Char.IsSurrogatePair(s,positionOfChar)){
						string codepoint = ConvertToUtf32(s, positionOfChar).ToString("X4");
						//string codepoint = get_char_code(ch).ToString("X4");
						//string codepoint = "U+" + ((int)ch).ToString("X4");

						int cpValue = int.Parse (codepoint, NumberStyles.HexNumber);
						if (cpValue <= 0x7F)
						{
							System.Diagnostics.Debug.WriteLine("0x7F");
							string utf8HexValue = string.Format("%{0}", cpValue);
							o.Append(utf8HexValue);
						}
						else if (cpValue <= 0x7FF)
						{
							string one   = (0xC0 | ((cpValue >> 6) & 0x1F)).ToString("X");
							string two   = (0x80 | ((cpValue) & 0x3F)).ToString("X");
							string utf8HexValue = string.Format("%{0}%{1}", one, two);
							o.Append(utf8HexValue);
						}
						else if (cpValue <= 0xFFFF)
						{
							string one   = (0xE0 | ((cpValue >> 12) & 0x0F)).ToString("X");
							string two   = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
							string three  = (0x80 | ((cpValue) & 0x3F)).ToString("X");
							string utf8HexValue = string.Format("%{0}%{1}%{2}", one, two, three);
							o.Append(utf8HexValue);
						}
						else if (cpValue <= 0x10FFFF)
						{
							string one    = (0xF0 | ((cpValue >> 18) & 0x07)).ToString("X");
							string two    = (0x80 | ((cpValue >> 12) & 0x3F)).ToString("X");
							string three  = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
							string four   = (0x80 | ((cpValue) & 0x3F)).ToString("X");
							string utf8HexValue = string.Format("%{0}%{1}%{2}%{3}", one, two, three, four);
							o.Append(utf8HexValue);
						}

//						string charTarget = char.ConvertFromUtf32 (int.Parse (codepoint, NumberStyles.HexNumber));
//						o.Append(charTarget);

						prevSurroagePair = true;
//					}
//					else if (Char.IsSurrogate (ch)) {
//						o.Append (ch);
					} else {
						string escapeChar = System.Uri.EscapeDataString (ch.ToString ());
						o.Append (escapeChar);
					}
				}
			}
			encodedUri = o.ToString ();
			if (type == ResponseType.Here_Now || type == ResponseType.DetailedHistory || type == ResponseType.Leave || type == ResponseType.PresenceHeartbeat || type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister) 
            {
                if (!ignorePercent2fEncode)
                {
                    encodedUri = encodedUri.Replace("%2F", "%252F");
                }
			}

			return encodedUri;
		}

//		public int get_char_code(char character){ 
//			System.Text.UTF8Encoding encoding = new System.Text.UnicodeEncoding(); 
//			byte[] bytes = encoding.GetBytes(character.ToString().ToCharArray()); 
//			return BitConverter.ToInt32(bytes, 0); 
//		}
		internal const int HIGH_SURROGATE_START  = 0x00d800;
		internal const int LOW_SURROGATE_END     = 0x00dfff;
		internal const int LOW_SURROGATE_START   = 0x00dc00;
		internal const int UNICODE_PLANE01_START = 0x10000;

//		internal const char  HIGH_SURROGATE_START  = '\ud800';
//		internal const char  HIGH_SURROGATE_END    = '\udbff';
//		internal const char  LOW_SURROGATE_START   = '\udc00';
//		internal const char  LOW_SURROGATE_END     = '\udfff';

		public static int ConvertToUtf32(String s, int index) {
			if (s == null) {
				throw new ArgumentNullException("s");
			}

			if (index < 0 || index >= s.Length) {
				throw new ArgumentOutOfRangeException("index");
			}
			//Contract.EndContractBlock();
			// Check if the character at index is a high surrogate.
			int temp1 = (int)s[index] - HIGH_SURROGATE_START;
			if (temp1 >= 0 && temp1 <= 0x7ff) {
				// Found a surrogate char.
				if (temp1 <= 0x3ff) {
					// Found a high surrogate.
					if (index < s.Length - 1) {
						int temp2 = (int)s[index+1] - LOW_SURROGATE_START;
						if (temp2 >= 0 && temp2 <= 0x3ff) {
							// Found a low surrogate.
							return ((temp1 * 0x400) + temp2 + UNICODE_PLANE01_START);
						} else {
							throw new ArgumentException("index"); 
						}
					} else {
						// Found a high surrogate at the end of the string.
						throw new ArgumentException("index"); 
					}
				} else {
					// Find a low surrogate at the character pointed by index.
					throw new ArgumentException("index"); 
				}
			}
			// Not a high-surrogate or low-surrogate. Genereate the UTF32 value for the BMP characters.
			return ((int)s[index]);
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
		/// <param name="subscribeCallback"></param>
		/// <param name="connectCallback"></param>
		public void Subscribe<T>(string channel, string channelGroup, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildPresenceCallback, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}
			if (subscribeCallback == null)
			{
				throw new ArgumentException("Missing subscribeCallback");
			}
			if (connectCallback == null)
			{
				throw new ArgumentException("Missing connectCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null)
			{
				throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested subscribe for channel={1} and channel group={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);

			string[] arrayChannel = new string[] { };
			string[] arrayChannelGroup = new string[] { };

			if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
			{
				arrayChannel = channel.Trim().Split(',');
			}

			if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
			{
				arrayChannelGroup = channelGroup.Trim().Split(',');
			}

			//Action<object> anyPresenceCallback = null;
			//PubnubChannelCallbackKey anyPresenceKey = new PubnubChannelCallbackKey() { Channel = string.Format("{0}-pnpres",channel), ResponseType = ResponseType.Presence };
			//if (channelCallbacks != null && channelCallbacks.ContainsKey(anyPresenceKey))
			//{
			//    var currentType = Activator.CreateInstance(channelCallbacks[anyPresenceKey].GetType());
			//    anyPresenceCallback = channelCallbacks[anyPresenceKey] as Action<object>;
			//}

			MultiChannelSubscribeInit<T>(ResponseType.Subscribe, arrayChannel, arrayChannelGroup, subscribeCallback, null, connectCallback, disconnectCallback, wildPresenceCallback, errorCallback);
		}

		/// <summary>
		/// Presence
		/// Listen for a presence message on a channel or comma delimited channels
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="userCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="errorCallback"></param>
		public void Presence(string channel, string channelGroup, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}
			if (presenceCallback == null)
			{
				throw new ArgumentException("Missing presenceCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null)
			{
				throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested presence for channel={1} and channel group={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);
			string[] arrayChannel = new string[] { };
			string[] arrayChannelGroup = new string[] { };

			if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
			{
				arrayChannel = channel.Trim().Split(',');
			}

			if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
			{
				arrayChannelGroup = channelGroup.Trim().Split(',');
			}
			MultiChannelSubscribeInit<object>(ResponseType.Presence, arrayChannel, arrayChannelGroup, null, presenceCallback, connectCallback, disconnectCallback, null, errorCallback);
		}

		private void MultiChannelSubscribeInit<T>(ResponseType responseType, string[] rawChannels, string[] rawChannelGroups, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
			bool channelGroupSubscribeOnly = false;
			bool channelSubscribeOnly = false;

			string channel = (rawChannels != null) ? string.Join(",", rawChannels) : "";
			string channelGroup = (rawChannelGroups != null) ? string.Join(",", rawChannelGroups) : "";

			List<string> validChannels = new List<string>();
			List<string> validChannelGroups = new List<string>();

			bool networkConnection = InternetConnectionStatusWithUnitTestCheck(channel, channelGroup, errorCallback, rawChannels, rawChannelGroups);

			if (rawChannels.Length > 0 && networkConnection)
			{
				if (rawChannels.Length != rawChannels.Distinct().Count())
				{
					rawChannels = rawChannels.Distinct().ToArray();
					string message = "Detected and removed duplicate channels";

					CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
						channel, channelGroup, errorCallback, message, PubnubErrorCode.DuplicateChannel, null, null);
				}

				for (int index = 0; index < rawChannels.Length; index++)
				{
					if (rawChannels[index].Trim().Length > 0)
					{
						string channelName = rawChannels[index].Trim();

						if (responseType == ResponseType.Presence)
						{
							channelName = string.Format("{0}-pnpres", channelName);
						}
						if (multiChannelSubscribe.ContainsKey(channelName))
						{
							string message = string.Format("{0}Already subscribed", (IsPresenceChannel(channelName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel(channelName)) ? PubnubErrorCode.AlreadyPresenceSubscribed : PubnubErrorCode.AlreadySubscribed;

							CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								channelName.Replace("-pnpres", ""), "", errorCallback, message, errorType, null, null);
						}
						else
						{
							validChannels.Add(channelName);
						}
					}
				}
			}

			if (rawChannelGroups != null && rawChannelGroups.Length > 0 && networkConnection)
			{
				if (rawChannelGroups.Length != rawChannelGroups.Distinct().Count())
				{
					rawChannelGroups = rawChannelGroups.Distinct().ToArray();
					string message = "Detected and removed duplicate channel groups";

					CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
						channel, channelGroup, errorCallback, message, PubnubErrorCode.DuplicateChannel, null, null);
				}

				for (int index = 0; index < rawChannelGroups.Length; index++)
				{
					if (rawChannelGroups[index].Trim().Length > 0)
					{
						string channelGroupName = rawChannelGroups[index].Trim();

						if (responseType == ResponseType.Presence)
						{
							channelGroupName = string.Format("{0}-pnpres", channelGroupName);
						}
						if (multiChannelGroupSubscribe.ContainsKey(channelGroupName))
						{
							string message = string.Format("{0}Already subscribed", (IsPresenceChannel(channelGroupName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel(channelGroupName)) ? PubnubErrorCode.AlreadyPresenceSubscribed : PubnubErrorCode.AlreadySubscribed;

							CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								"", channelGroupName.Replace("-pnpres", ""), errorCallback, message, errorType, null, null);
						}
						else
						{
							validChannelGroups.Add(channelGroupName);
						}
					}
				}
			}

			if (validChannels.Count > 0 || validChannelGroups.Count > 0)
			{
				//Retrieve the current channels already subscribed previously and terminate them
				string[] currentChannels = multiChannelSubscribe.Keys.ToArray<string>();
				string[] currentChannelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();

				if (currentChannels != null && currentChannels.Length >= 0)
				{
					string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels) : ",";
					string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups) : "";

					if (_channelRequest.ContainsKey(multiChannelName))
					{
						LoggingMethod.WriteToLog(string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
						PubnubWebRequest webRequest = _channelRequest[multiChannelName];
						_channelRequest[multiChannelName] = null;

						if (webRequest != null)
							TerminateLocalClientHeartbeatTimer(webRequest.RequestUri);

						PubnubWebRequest removedRequest;
						_channelRequest.TryRemove(multiChannelName, out removedRequest);
						bool removedChannel = _channelRequest.TryRemove(multiChannelName, out removedRequest);
						if (removedChannel)
						{
							LoggingMethod.WriteToLog(string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
						}
						else
						{
							LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
						}
						if (webRequest != null)
							TerminatePendingWebRequest(webRequest, errorCallback);
					}
					else
					{
						LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
					}
				}


				//Add the valid channels to the channels subscribe list for tracking
				for (int index = 0; index < validChannels.Count; index++)
				{
					string currentLoopChannel = validChannels[index].ToString();
					multiChannelSubscribe.GetOrAdd(currentLoopChannel, 0);


					if (responseType == ResponseType.Presence)
					{
						PubnubChannelCallbackKey callbackPresenceKey = new PubnubChannelCallbackKey();
						callbackPresenceKey.Channel = currentLoopChannel;
						callbackPresenceKey.ResponseType = responseType;

						PubnubPresenceChannelCallback pubnubChannelCallbacks = new PubnubPresenceChannelCallback();
						pubnubChannelCallbacks.PresenceRegularCallback = presenceRegularCallback;
						pubnubChannelCallbacks.ConnectCallback = connectCallback;
						pubnubChannelCallbacks.DisconnectCallback = disconnectCallback;
						pubnubChannelCallbacks.ErrorCallback = errorCallback;

						channelCallbacks.AddOrUpdate(callbackPresenceKey, pubnubChannelCallbacks, (key, oldValue) => pubnubChannelCallbacks);
					}
					else
					{
						PubnubChannelCallbackKey callbackSubscribeKey = new PubnubChannelCallbackKey();
						callbackSubscribeKey.Channel = currentLoopChannel;
						callbackSubscribeKey.ResponseType = responseType;

						PubnubSubscribeChannelCallback<T> pubnubChannelCallbacks = new PubnubSubscribeChannelCallback<T>();
						pubnubChannelCallbacks.SubscribeRegularCallback = subscribeRegularCallback;
						pubnubChannelCallbacks.ConnectCallback = connectCallback;
						pubnubChannelCallbacks.DisconnectCallback = disconnectCallback;
						pubnubChannelCallbacks.WildcardPresenceCallback = wildcardPresenceCallback;
						pubnubChannelCallbacks.ErrorCallback = errorCallback;

						channelCallbacks.AddOrUpdate(callbackSubscribeKey, pubnubChannelCallbacks, (key, oldValue) => pubnubChannelCallbacks);

						//var ctor = typeof(T).GetConstructor(new Type[] {  });
						//var channelSubscribeObject = ctor.Invoke(new object[] { });

						//var type = typeof(Message<>).MakeGenericType(typeof(T));
						//var channelSubscribeObject = Activator.CreateInstance(type);

						//var channelSubscribeObject = (T)Activator.CreateInstance(typeof(T), new object[] {});
						_channelSubscribeObjectType.AddOrUpdate(currentLoopChannel, typeof(T), (key, oldValue) => typeof(T));
					}
				}


				for (int index = 0; index < validChannelGroups.Count; index++)
				{
					string currentLoopChannelGroup = validChannelGroups[index].ToString();
					multiChannelGroupSubscribe.GetOrAdd(currentLoopChannelGroup, 0);

					PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
					callbackKey.ChannelGroup = currentLoopChannelGroup;
					callbackKey.ResponseType = responseType;

					if (responseType == ResponseType.Presence)
					{
						PubnubPresenceChannelGroupCallback pubnubChannelGroupCallbacks = new PubnubPresenceChannelGroupCallback();
						pubnubChannelGroupCallbacks.PresenceRegularCallback = presenceRegularCallback;
						pubnubChannelGroupCallbacks.ConnectCallback = connectCallback;
						pubnubChannelGroupCallbacks.DisconnectCallback = disconnectCallback;
						pubnubChannelGroupCallbacks.ErrorCallback = errorCallback;

						channelGroupCallbacks.AddOrUpdate(callbackKey, pubnubChannelGroupCallbacks, (key, oldValue) => pubnubChannelGroupCallbacks);
					}
					else
					{
						PubnubSubscribeChannelGroupCallback<T> pubnubChannelGroupCallbacks = new PubnubSubscribeChannelGroupCallback<T>();
						pubnubChannelGroupCallbacks.SubscribeRegularCallback = subscribeRegularCallback;
						pubnubChannelGroupCallbacks.WildcardPresenceCallback = wildcardPresenceCallback;
						pubnubChannelGroupCallbacks.ConnectCallback = connectCallback;
						pubnubChannelGroupCallbacks.DisconnectCallback = disconnectCallback;
						pubnubChannelGroupCallbacks.ErrorCallback = errorCallback;

						channelGroupCallbacks.AddOrUpdate(callbackKey, pubnubChannelGroupCallbacks, (key, oldValue) => pubnubChannelGroupCallbacks);

						_channelGroupSubscribeObjectType.AddOrUpdate(currentLoopChannelGroup, typeof(T), (key, oldValue) => typeof(T));
					}
				}

				//Get all the channels
				string[] channels = multiChannelSubscribe.Keys.ToArray<string>();
				string[] channelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();

				if (channels != null && channels.Length > 0 && (channelGroups == null || channelGroups.Length == 0))
				{
					channelSubscribeOnly = true;
				}
				if (channelGroups != null && channelGroups.Length > 0 && (channels == null || channels.Length == 0))
				{
					channelGroupSubscribeOnly = true;
				}

				RequestState<T> state = new RequestState<T>();
				if (channelGroupSubscribeOnly)
				{
					_channelRequest.AddOrUpdate(",", state.Request, (key, oldValue) => state.Request);
				}
				else
				{
					_channelRequest.AddOrUpdate(string.Join(",", channels), state.Request, (key, oldValue) => state.Request);
				}

				ResetInternetCheckSettings(channels, channelGroups);
				MultiChannelSubscribeRequest<T>(responseType, channels, channelGroups, 0, subscribeRegularCallback, presenceRegularCallback,  connectCallback, wildcardPresenceCallback, errorCallback, false);
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
		private void MultiChannelSubscribeRequest<T>(ResponseType type, string[] channels, string[] channelGroups, object timetoken, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback, bool reconnect)
		{
			//Exit if the channel is unsubscribed
			if (multiChannelSubscribe != null && multiChannelSubscribe.Count <= 0 && multiChannelGroupSubscribe != null && multiChannelGroupSubscribe.Count <= 0)
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
				return;
			}

			string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
			string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";
			if (!_channelRequest.ContainsKey(multiChannel))
			{
				return;
			}

			bool networkConnection;
			if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest)
			{
				networkConnection = true;
			}
			else
			{
				networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, errorCallback, channels, channelGroups);
			}
			if (!networkConnection)
			{
				channelInternetStatus.AddOrUpdate(multiChannel, networkConnection, (key, oldValue) => networkConnection);
				channelGroupInternetStatus.AddOrUpdate(multiChannelGroup, networkConnection, (key, oldValue) => networkConnection);
			}

			if (((channelInternetStatus.ContainsKey(multiChannel) && !channelInternetStatus[multiChannel])
				|| (multiChannelGroup != "" && channelGroupInternetStatus.ContainsKey(multiChannelGroup) && !channelGroupInternetStatus[multiChannelGroup]))
				&& pubnetSystemActive)
			{
				if (channelInternetRetry.ContainsKey(multiChannel) && (channelInternetRetry[multiChannel] >= _pubnubNetworkCheckRetries))
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, Subscribe channel={1} - No internet connection. MAXed retries for internet ", DateTime.Now.ToString(), multiChannel), LoggingMethod.LevelInfo);
					MultiplexExceptionHandler<T>(type, channels, channelGroups, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, true, false);
					return;
				}
				else if (channelGroupInternetRetry.ContainsKey(multiChannelGroup) && (channelGroupInternetRetry[multiChannelGroup] >= _pubnubNetworkCheckRetries))
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, Subscribe channelgroup={1} - No internet connection. MAXed retries for internet ", DateTime.Now.ToString(), multiChannelGroup), LoggingMethod.LevelInfo);
					MultiplexExceptionHandler<T>(type, channels, channelGroups, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, true, false);
					return;
				}

				if (ReconnectNetworkIfOverrideTcpKeepAlive<T>(type, channels, channelGroups, timetoken, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback))
				{
					return;
				}

			}

			// Begin recursive subscribe
			try
			{
				long lastTimetoken = 0;
				long minimumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Min(token => token.Value) : 0;
				long minimumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Min(token => token.Value) : 0;
				long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

				long maximumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Max(token => token.Value) : 0;
				long maximumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Max(token => token.Value) : 0;
				long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);


				if (minimumTimetoken == 0 || reconnect || _uuidChanged)
				{
					lastTimetoken = 0;
					_uuidChanged = false;
				}
				else
				{
					if (lastSubscribeTimetoken == maximumTimetoken)
					{
						lastTimetoken = maximumTimetoken;
					}
					else
					{
						lastTimetoken = lastSubscribeTimetoken;
					}
				}
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, Building request for channel(s)={1}, channelgroup(s)={2} with timetoken={3}", DateTime.Now.ToString(), multiChannel, multiChannelGroup, lastTimetoken), LoggingMethod.LevelInfo);
				// Build URL
				Uri requestUrl = BuildMultiChannelSubscribeRequest(channels, channelGroups, (Convert.ToInt64(timetoken.ToString()) == 0) ? Convert.ToInt64(timetoken.ToString()) : lastTimetoken);

				RequestState<T> pubnubRequestState = new RequestState<T>();
				pubnubRequestState.Channels = channels;
				pubnubRequestState.ChannelGroups = channelGroups;
				pubnubRequestState.ResponseType = type;
				pubnubRequestState.ConnectCallback = connectCallback;
				pubnubRequestState.SubscribeRegularCallback = subscribeRegularCallback;
				pubnubRequestState.PresenceRegularCallback = presenceRegularCallback;
				pubnubRequestState.WildcardPresenceCallback = wildcardPresenceCallback;
				pubnubRequestState.ErrorCallback = errorCallback;
				pubnubRequestState.Reconnect = reconnect;
				pubnubRequestState.Timetoken = Convert.ToInt64(timetoken.ToString());

				// Wait for message
				UrlProcessRequest<T>(requestUrl, pubnubRequestState);
			}
			catch (Exception ex)
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0} method:_subscribe \n channel={1} \n timetoken={2} \n Exception Details={3}", DateTime.Now.ToString(), string.Join(",", channels), timetoken.ToString(), ex.ToString()), LoggingMethod.LevelError);

				CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					string.Join(",", channels), string.Join(",", channelGroups), errorCallback, ex, null, null);

				this.MultiChannelSubscribeRequest<T>(type, channels, channelGroups, timetoken, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, false);
			}
		}

		private Uri BuildMultiChannelSubscribeRequest (string[] channels, string[] channelGroups, object timetoken)
		{
            StringBuilder subscribeParamBuilder = new StringBuilder();
            subscribeParameters = "";
			string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
			if (channelsJsonState != "{}" && channelsJsonState != "") {
                subscribeParamBuilder.AppendFormat("&state={0}", EncodeUricomponent(channelsJsonState, ResponseType.Subscribe, false, false));
			}
            if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "")
            {
                subscribeParamBuilder.AppendFormat("&channel-group={0}", string.Join(",", channelGroups));
            }
            subscribeParameters = subscribeParamBuilder.ToString();

			List<string> url = new List<string> ();
			url.Add ("subscribe");
			url.Add (this.subscribeKey);
			url.Add ((channels.Length > 0) ? string.Join (",", channels) : ",");
			url.Add ("0");
			url.Add (timetoken.ToString ());

			return BuildRestApiRequest<Uri> (url, ResponseType.Subscribe);
		}

		#endregion

		#region "Unsubscribe Presence And Subscribe"

		public void PresenceUnsubscribe(string channel, string channelGroup, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}
			if (disconnectCallback == null)
			{
				throw new ArgumentException("Missing disconnectCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null)
			{
				throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested presence-unsubscribe for channel(s)={1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);
			MultiChannelUnSubscribeInit<object>(ResponseType.PresenceUnsubscribe, channel, channelGroup, errorCallback);
		}

		private void MultiChannelUnSubscribeInit<T>(ResponseType type, string channel, string channelGroup, Action<PubnubClientError> errorCallback)
		{
			bool channelGroupUnsubscribeOnly = false;
			bool channelUnsubscribeOnly = false;

			string[] rawChannels = (channel != null && channel.Trim().Length > 0) ? channel.Split(',') : new string[] { };
			string[] rawChannelGroups = (channelGroup != null && channelGroup.Trim().Length > 0) ? channelGroup.Split(',') : new string[] { };

			if (rawChannels.Length > 0 && rawChannelGroups.Length <= 0)
			{
				channelUnsubscribeOnly = true;
			}
			if (rawChannels.Length <= 0 && rawChannelGroups.Length > 0)
			{
				channelGroupUnsubscribeOnly = true;
			}

			List<string> validChannels = new List<string>();
			List<string> validChannelGroups = new List<string>();

			if (rawChannels.Length > 0)
			{
				for (int index = 0; index < rawChannels.Length; index++)
				{
					if (rawChannels[index].Trim().Length > 0)
					{
						string channelName = rawChannels[index].Trim();
						if (type == ResponseType.PresenceUnsubscribe)
						{
							channelName = string.Format("{0}-pnpres", channelName);
						}
						if (!multiChannelSubscribe.ContainsKey(channelName))
						{
							string message = string.Format("{0}Channel Not Subscribed", (IsPresenceChannel(channelName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel(channelName)) ? PubnubErrorCode.NotPresenceSubscribed : PubnubErrorCode.NotSubscribed;

							LoggingMethod.WriteToLog(string.Format("DateTime {0}, channel={1} unsubscribe response={2}", DateTime.Now.ToString(), channelName, message), LoggingMethod.LevelInfo);

							CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								channelName, "", errorCallback, message, errorType, null, null);
						}
						else
						{
							validChannels.Add(channelName);
						}
					}
					else
					{
						string message = "Invalid Channel Name For Unsubscribe";

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, channel={1} unsubscribe response={2}", DateTime.Now.ToString(), rawChannels[index], message), LoggingMethod.LevelInfo);

						CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
							rawChannels[index], "", errorCallback, message, PubnubErrorCode.InvalidChannel,
							null, null);
					}
				}
			}
			//
			if (rawChannelGroups.Length > 0)
			{
				for (int index = 0; index < rawChannelGroups.Length; index++)
				{
					if (rawChannelGroups[index].Trim().Length > 0)
					{
						string channelGroupName = rawChannelGroups[index].Trim();
						if (type == ResponseType.PresenceUnsubscribe)
						{
							channelGroupName = string.Format("{0}-pnpres", channelGroupName);
						}
						if (!multiChannelGroupSubscribe.ContainsKey(channelGroupName))
						{
							string message = string.Format("{0}ChannelGroup Not Subscribed", (IsPresenceChannel(channelGroupName)) ? "Presence " : "");

							PubnubErrorCode errorType = (IsPresenceChannel(channelGroupName)) ? PubnubErrorCode.NotPresenceSubscribed : PubnubErrorCode.NotSubscribed;

							LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} unsubscribe response={2}", DateTime.Now.ToString(), channelGroupName, message), LoggingMethod.LevelInfo);

							CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
								"", channelGroupName, errorCallback, message, errorType, null, null);
						}
						else
						{
							validChannelGroups.Add(channelGroupName);
						}
					}
					else
					{
						string message = "Invalid ChannelGroup Name For Unsubscribe";

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} unsubscribe response={2}", DateTime.Now.ToString(), rawChannelGroups[index], message), LoggingMethod.LevelInfo);

						CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
							"", rawChannelGroups[index], errorCallback, message, PubnubErrorCode.InvalidChannel,
							null, null);
					}
				}
			}

			if (validChannels.Count > 0 || validChannelGroups.Count > 0)
			{
				//Retrieve the current channels already subscribed previously and terminate them
				string[] currentChannels = multiChannelSubscribe.Keys.ToArray<string>();
				string[] currentChannelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();

				if (currentChannels != null && currentChannels.Length >= 0)
				{
					string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels) : ",";
					string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups) : "";

					System.Threading.Tasks.Task.Factory.StartNew(() =>
						{
							if (_channelRequest.ContainsKey(multiChannelName))
							{
								string[] arrValidChannels = validChannels.ToArray();
								RemoveChannelCallback<T>(string.Join(",", arrValidChannels), type);

								string[] arrValidChannelGroups = validChannels.ToArray();
								RemoveChannelGroupCallback<T>(string.Join(",", arrValidChannelGroups), type);

								LoggingMethod.WriteToLog(string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);

								PubnubWebRequest webRequest = _channelRequest[multiChannelName];
								_channelRequest[multiChannelName] = null;

								if (webRequest != null)
								{
									TerminateLocalClientHeartbeatTimer(webRequest.RequestUri);
								}

								PubnubWebRequest removedRequest;
								bool removedChannel = _channelRequest.TryRemove(multiChannelName, out removedRequest);
								if (removedChannel)
								{
									LoggingMethod.WriteToLog(string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
								}
								else
								{
									LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
								}
								if (webRequest != null)
									TerminatePendingWebRequest(webRequest, errorCallback);
							}
							else
							{
								LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), LoggingMethod.LevelInfo);
							}
						});

					if (type == ResponseType.Unsubscribe)
					{
						//just fire leave() event to REST API for safeguard
						Uri request = BuildMultiChannelLeaveRequest(validChannels.ToArray(), validChannelGroups.ToArray());

						RequestState<T> requestState = new RequestState<T>();
						requestState.Channels = new string[] { channel };
						requestState.ChannelGroups = new string[] { channelGroup };
						requestState.ResponseType = ResponseType.Leave;
						requestState.SubscribeRegularCallback = null;
						requestState.PresenceRegularCallback = null;
						requestState.WildcardPresenceCallback = null;
						requestState.ErrorCallback = null;
						requestState.ConnectCallback = null;
						requestState.Reconnect = false;

						UrlProcessRequest<T>(request, requestState); // connectCallback = null
					}
				}

				Dictionary<string, long> originalMultiChannelSubscribe = multiChannelSubscribe.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				Dictionary<string, long> originalMultiChannelGroupSubscribe = multiChannelGroupSubscribe.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

				//Remove the valid channels from subscribe list for unsubscribe 
				for (int index = 0; index < validChannels.Count; index++)
				{
					long timetokenValue;
					string channelToBeRemoved = validChannels[index].ToString();
					bool unsubscribeStatus = multiChannelSubscribe.TryRemove(channelToBeRemoved, out timetokenValue);
					if (unsubscribeStatus)
					{
						List<object> result = new List<object>();
						string jsonString = string.Format("[1, \"Channel {0}Unsubscribed from {1}\"]", (IsPresenceChannel(channelToBeRemoved)) ? "Presence " : "", channelToBeRemoved.Replace("-pnpres", ""));
						result = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
						result.Add(channelToBeRemoved.Replace("-pnpres", ""));
						LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON response={1}", DateTime.Now.ToString(), jsonString), LoggingMethod.LevelInfo);

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
						callbackKey.Channel = channelToBeRemoved;//.Replace("-pnpres", "");
                        if (type == ResponseType.Unsubscribe)
                        {
                            callbackKey.ResponseType = ResponseType.Subscribe;
                            if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                            {
                                PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
                                if (currentPubnubCallback != null && currentPubnubCallback.DisconnectCallback != null)
                                {
                                    Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.DisconnectCallback;
                                    currentPubnubCallback.ConnectCallback = null;
                                    GoToCallback<ConnectOrDisconnectAck>(result, targetCallback, true, type);
                                }
                            }
                        }
                        else if (type == ResponseType.PresenceUnsubscribe)
                        {
                            callbackKey.ResponseType = ResponseType.Presence;
                            if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                            {
                                PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
                                if (currentPubnubCallback != null && currentPubnubCallback.DisconnectCallback != null)
                                {
                                    Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.DisconnectCallback;
                                    currentPubnubCallback.ConnectCallback = null;
                                    GoToCallback<ConnectOrDisconnectAck>(result, targetCallback, true, type);
                                }
                            }
                        }
                        else
                        {
                            callbackKey.ResponseType = type;
                        }
						
						DeleteLocalChannelUserState(channelToBeRemoved);
					}
					else
					{
						string message = "Unsubscribe Error. Please retry the channel unsubscribe operation.";

						PubnubErrorCode errorType = (IsPresenceChannel(channelToBeRemoved)) ? PubnubErrorCode.PresenceUnsubscribeFailed : PubnubErrorCode.UnsubscribeFailed;

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, channel={1} unsubscribe error", DateTime.Now.ToString(), channelToBeRemoved), LoggingMethod.LevelInfo);

						CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
							channelToBeRemoved, "", errorCallback, message, errorType, null, null);

					}
				}
				for (int index = 0; index < validChannelGroups.Count; index++)
				{
					long timetokenValue;
					string channelGroupToBeRemoved = validChannelGroups[index].ToString();
					bool unsubscribeStatus = multiChannelGroupSubscribe.TryRemove(channelGroupToBeRemoved, out timetokenValue);
					if (unsubscribeStatus)
					{
						List<object> result = new List<object>();
						string jsonString = string.Format("[1, \"ChannelGroup {0}Unsubscribed from {1}\"]", (IsPresenceChannel(channelGroupToBeRemoved)) ? "Presence " : "", channelGroupToBeRemoved.Replace("-pnpres", ""));
						result = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
						result.Add(channelGroupToBeRemoved.Replace("-pnpres", ""));
                        result.Add("");
						LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON response={1}", DateTime.Now.ToString(), jsonString), LoggingMethod.LevelInfo);

						PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
						callbackKey.ChannelGroup = channelGroupToBeRemoved;//.Replace("-pnpres", "");
                        if (type == ResponseType.Unsubscribe)
                        {
                            callbackKey.ResponseType = ResponseType.Subscribe;
                            if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                            {
                                PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
                                if (currentPubnubCallback != null && currentPubnubCallback.DisconnectCallback != null)
                                {
                                    Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.DisconnectCallback;
                                    currentPubnubCallback.ConnectCallback = null;
                                    GoToCallback<ConnectOrDisconnectAck>(result, targetCallback, true, type);
                                }
                            }
                        }
                        else if (type == ResponseType.PresenceUnsubscribe)
                        {
                            callbackKey.ResponseType = ResponseType.Presence;
                            if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                            {
                                PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
                                if (currentPubnubCallback != null && currentPubnubCallback.DisconnectCallback != null)
                                {
                                    Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.DisconnectCallback;
                                    currentPubnubCallback.ConnectCallback = null;
                                    GoToCallback<ConnectOrDisconnectAck>(result, targetCallback, true, type);
                                }
                            }
                        }
						
						DeleteLocalChannelGroupUserState(channelGroupToBeRemoved);
					}
					else
					{
						string message = "Unsubscribe Error. Please retry the channelgroup unsubscribe operation.";

						PubnubErrorCode errorType = (IsPresenceChannel(channelGroupToBeRemoved)) ? PubnubErrorCode.PresenceUnsubscribeFailed : PubnubErrorCode.UnsubscribeFailed;

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} unsubscribe error", DateTime.Now.ToString(), channelGroupToBeRemoved), LoggingMethod.LevelInfo);

						CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
							"", channelGroupToBeRemoved, errorCallback, message, errorType, null, null);

					}
				}

				//Get all the channels
				string[] channels = multiChannelSubscribe.Keys.ToArray<string>();
				string[] channelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();

				//Check any chained subscribes while unsubscribe 
				foreach (string key in multiChannelSubscribe.Keys) 
				{ 
					if (!originalMultiChannelSubscribe.ContainsKey(key)) 
					{ 
						return; 
					} 
				} 
				foreach (string key in multiChannelGroupSubscribe.Keys) 
				{ 
					if (!originalMultiChannelGroupSubscribe.ContainsKey(key)) 
					{ 
						return; 
					} 
				}


				channels = (channels != null) ? channels : new string[] { };
				channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

				if (channels.Length > 0 || channelGroups.Length > 0)
				{
					string multiChannel = (channels.Length > 0) ? string.Join(",", channels) : ",";

					RequestState<T> state = new RequestState<T>();
					_channelRequest.AddOrUpdate(multiChannel, state.Request, (key, oldValue) => state.Request);

					ResetInternetCheckSettings(channels, channelGroups);

					//Modify the value for type ResponseType. Presence or Subscrie is ok, but sending the close value would make sense
					if (string.Join(",", channels).IndexOf("-pnpres") > 0 || string.Join(",", channelGroups).IndexOf("-pnpres") > 0)
					{
						type = ResponseType.Presence;
						//channelCallbacks.TryGetValue(
					}
					else
					{
						type = ResponseType.Subscribe;
					}

					//Continue with any remaining channels for subscribe/presence
					MultiChannelSubscribeRequest<T>(type, channels, channelGroups, 0, null, null, null, null, errorCallback, false);
				}
				else
				{
					if (presenceHeartbeatTimer != null)
					{
						// Stop the presence heartbeat timer if there are no channels subscribed
						presenceHeartbeatTimer.Dispose();
						presenceHeartbeatTimer = null;
					}
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString()), LoggingMethod.LevelInfo);
				}
			}

		}

        /// <summary>
        /// To unsubscribe a channel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscribeCallback"></param>
        /// <param name="connectCallback"></param>
        /// <param name="disconnectCallback"></param>
        /// <param name="errorCallback"></param>
		public void Unsubscribe<T>(string channel, string channelGroup, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}

			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null)
			{
				throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
			}

			LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested unsubscribe for channel(s)={1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);
			MultiChannelUnSubscribeInit<T>(ResponseType.Unsubscribe, channel, channelGroup, errorCallback);

		}

		private Uri BuildMultiChannelLeaveRequest (string[] channels, string[] channelGroups)
		{
			return BuildMultiChannelLeaveRequest(channels, channelGroups, "");
		}

		private Uri BuildMultiChannelLeaveRequest (string[] channels, string[] channelGroups, string uuid)
		{
            StringBuilder unsubscribeParamBuilder = new StringBuilder();
            subscribeParameters = "";
            string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                unsubscribeParamBuilder.AppendFormat("&state={0}", EncodeUricomponent(channelsJsonState, ResponseType.Leave, false, false));
            }
            if (channelGroups != null && channelGroups.Length > 0)
            {
                unsubscribeParamBuilder.AppendFormat("&channel-group={0}", string.Join(",", channelGroups));
            }
            subscribeParameters = unsubscribeParamBuilder.ToString();

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
            url.Add(multiChannel);
			url.Add ("leave");

			return BuildRestApiRequest<Uri> (url, ResponseType.Leave, uuid);
		}

		#endregion

		#region "HereNow"

		internal bool HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if ((channels == null && channelGroups == null) || (channels.Length == 0 && channelGroups.Length == 0))
			{
				throw new ArgumentException("Missing Channel/ChannelGroup");
			}
			//if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
			//    throw new ArgumentException ("Missing Channel");
			//}
			if (userCallback == null) {
				throw new ArgumentException ("Missing userCallback");
			}
			if (errorCallback == null) {
				throw new ArgumentException ("Missing errorCallback");
			}
			if (_jsonPluggableLibrary == null) {
				throw new NullReferenceException ("Missing Json Pluggable Library for Pubnub Instance");
			}

			Uri request = BuildHereNowRequest(channels, channelGroups, showUUIDList, includeUserState);

			RequestState<HereNowAck> requestState = new RequestState<HereNowAck>();
			requestState.Channels = channels;
			requestState.ChannelGroups = channelGroups;
			requestState.ResponseType = ResponseType.Here_Now;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<HereNowAck>(request, requestState);
		}

		private Uri BuildHereNowRequest (string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState)
		{
			string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
			if (channel.Trim() == "")
			{
				channel = ",";
			}
			string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

			int disableUUID = (showUUIDList) ? 0 : 1;
			int userState = (includeUserState) ? 1 : 0;

			if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
			{
				hereNowParameters = string.Format("?channel-group={0}&disable_uuids={1}&state={2}", channelGroup, disableUUID, userState);
			}
			else
			{
				hereNowParameters = string.Format("?disable_uuids={0}&state={1}", disableUUID, userState);
			}


			List<string> url = new List<string> ();

			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
			url.Add (channel);

			return BuildRestApiRequest<Uri> (url, ResponseType.Here_Now);
		}

		private Uri BuildPresenceHeartbeatRequest (string[] channels, string[] channelGroups)
		{
            StringBuilder presenceHeartbeatBuilder = new StringBuilder();
            presenceHeartbeatParameters = "";
            string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
			if (channelsJsonState != "{}" && channelsJsonState != "") 
            {
                presenceHeartbeatBuilder.AppendFormat("&state={0}", EncodeUricomponent(channelsJsonState, ResponseType.PresenceHeartbeat, false, false));
			}
            if (channelGroups != null && channelGroups.Length > 0)
            {
                presenceHeartbeatBuilder.AppendFormat("&channel-group={0}", string.Join(",", channelGroups));
            }
            presenceHeartbeatParameters = presenceHeartbeatBuilder.ToString();

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
			List<string> url = new List<string> ();
            
			url.Add ("v2");
			url.Add ("presence");
			url.Add ("sub_key");
			url.Add (this.subscribeKey);
			url.Add ("channel");
            url.Add(multiChannel);
			url.Add ("heartbeat");

			return BuildRestApiRequest<Uri> (url, ResponseType.PresenceHeartbeat);
		}

		#endregion

		#region "Global Here Now"

		internal bool GlobalHereNow(Action<GlobalHereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			return GlobalHereNow(true, false, userCallback, errorCallback);
		}

		internal bool GlobalHereNow(bool showUUIDList, bool includeUserState, Action<GlobalHereNowAck> userCallback, Action<PubnubClientError> errorCallback)
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

			RequestState<GlobalHereNowAck> requestState = new RequestState<GlobalHereNowAck>();
			requestState.Channels = null;
			requestState.ResponseType = ResponseType.GlobalHere_Now;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<GlobalHereNowAck>(request, requestState);
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

		internal void WhereNow(string uuid, Action<WhereNowAck> userCallback, Action<PubnubClientError> errorCallback)
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

			RequestState<WhereNowAck> requestState = new RequestState<WhereNowAck>();
			requestState.Channels = new string[] { uuid };
			requestState.ResponseType = ResponseType.Where_Now;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<WhereNowAck>(request, requestState);
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
		public bool Time(Action<long> userCallback, Action<PubnubClientError> errorCallback)
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

			RequestState<long> requestState = new RequestState<long>();
			requestState.Channels = null;
			requestState.ResponseType = ResponseType.Time;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<long>(request, requestState);
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

		private string AddOrUpdateOrDeleteLocalUserState (string channel, string channelGroup, string userStateKey, object userStateValue)
		{
            string retJsonUserState = "";

			Dictionary<string, object> channelUserStateDictionary = null;
            Dictionary<string, object> channelGroupUserStateDictionary = null;

            if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
            {
                if (_channelLocalUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = _channelLocalUserState[channel];
                    if (channelUserStateDictionary != null)
                    {
                        if (channelUserStateDictionary.ContainsKey(userStateKey))
                        {
                            if (userStateValue != null)
                            {
                                channelUserStateDictionary[userStateKey] = userStateValue;
                            }
                            else
                            {
                                channelUserStateDictionary.Remove(userStateKey);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                            {
                                channelUserStateDictionary.Add(userStateKey, userStateValue);
                            }
                        }
                    }
                    else
                    {
                        channelUserStateDictionary = new Dictionary<string, object>();
                        channelUserStateDictionary.Add(userStateKey, userStateValue);
                    }

                    _channelLocalUserState.AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelUserStateDictionary = new Dictionary<string, object>();
                        channelUserStateDictionary.Add(userStateKey, userStateValue);

                        _channelLocalUserState.AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                    }
                }
            }
            //
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                if (_channelGroupLocalUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = _channelGroupLocalUserState[channelGroup];
                    if (channelGroupUserStateDictionary != null)
                    {
                        if (channelGroupUserStateDictionary.ContainsKey(userStateKey))
                        {
                            if (userStateValue != null)
                            {
                                channelGroupUserStateDictionary[userStateKey] = userStateValue;
                            }
                            else
                            {
                                channelGroupUserStateDictionary.Remove(userStateKey);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                            {
                                channelGroupUserStateDictionary.Add(userStateKey, userStateValue);
                            }
                        }
                    }
                    else
                    {
                        channelGroupUserStateDictionary = new Dictionary<string, object>();
                        channelGroupUserStateDictionary.Add(userStateKey, userStateValue);
                    }

                    _channelGroupLocalUserState.AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelGroupUserStateDictionary = new Dictionary<string, object>();
                        channelGroupUserStateDictionary.Add(userStateKey, userStateValue);

                        _channelGroupLocalUserState.AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
                    }
                }
            }

            string jsonChannelUserState = BuildJsonUserState(channel, "", true);
            string jsonChannelGroupUserState = BuildJsonUserState("", channelGroup, true);
            if (jsonChannelUserState != "" && jsonChannelGroupUserState != "")
            {
                retJsonUserState = string.Format("{{\"{0}\":{{{1}}},\"{2}\":{{{3}}}}}", channel, jsonChannelUserState, channelGroup, jsonChannelGroupUserState);
            }
            else if (jsonChannelUserState != "")
            {
                retJsonUserState = string.Format("{{{0}}}", jsonChannelUserState);
            }
            else if (jsonChannelGroupUserState != "")
            {
                retJsonUserState = string.Format("{{{0}}}", jsonChannelGroupUserState);
            }
            return retJsonUserState;
		}

		private bool DeleteLocalChannelUserState (string channel)
		{
			bool userStateDeleted = false;

			if (_channelLocalUserState.ContainsKey(channel)) {
				Dictionary<string, object> returnedUserState = null;
				userStateDeleted = _channelLocalUserState.TryRemove (channel, out returnedUserState);
			}

			return userStateDeleted;
		}

        private bool DeleteLocalChannelGroupUserState(string channelGroup)
        {
            bool userStateDeleted = false;

            if (_channelGroupLocalUserState.ContainsKey(channelGroup))
            {
                Dictionary<string, object> returnedUserState = null;
                userStateDeleted = _channelGroupLocalUserState.TryRemove(channelGroup, out returnedUserState);
            }

            return userStateDeleted;
        }

        private string BuildJsonUserState(string channel, string channelGroup, bool local)
		{
			Dictionary<string, object> channelUserStateDictionary = null;
            Dictionary<string, object> channelGroupUserStateDictionary = null;

            if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(channelGroup))
            {
                throw new ArgumentException("BuildJsonUserState takes either channel or channelGroup at one time. Send one at a time by passing empty value for other.");
            }

            if (local)
            {
                if (!string.IsNullOrEmpty(channel) && _channelLocalUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = _channelLocalUserState[channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && _channelGroupLocalUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = _channelGroupLocalUserState[channelGroup];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(channel) && _channelUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = _channelUserState[channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && _channelGroupUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = _channelGroupUserState[channelGroup];
                }
            }

			StringBuilder jsonStateBuilder = new StringBuilder ();

			if (channelUserStateDictionary != null) 
            {
				string[] channelUserStateKeys = channelUserStateDictionary.Keys.ToArray<string> ();

				for (int keyIndex = 0; keyIndex < channelUserStateKeys.Length; keyIndex++) 
                {
					string channelUserStateKey = channelUserStateKeys [keyIndex];
					object channelUserStateValue = channelUserStateDictionary[channelUserStateKey];
                    if (channelUserStateValue == null)
                    {
                        jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelUserStateKey, string.Format("\"{0}\"", "null"));
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelUserStateKey, (channelUserStateValue.GetType().ToString() == "System.String") ? string.Format("\"{0}\"", channelUserStateValue) : channelUserStateValue);
                    }
					if (keyIndex < channelUserStateKeys.Length - 1) 
                    {
						jsonStateBuilder.Append (",");
					}
				}
			}
            if (channelGroupUserStateDictionary != null)
            {
                string[] channelGroupUserStateKeys = channelGroupUserStateDictionary.Keys.ToArray<string>();

                for (int keyIndex = 0; keyIndex < channelGroupUserStateKeys.Length; keyIndex++)
                {
                    string channelGroupUserStateKey = channelGroupUserStateKeys[keyIndex];
                    object channelGroupUserStateValue = channelGroupUserStateDictionary[channelGroupUserStateKey];
                    if (channelGroupUserStateValue == null)
                    {
                        jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelGroupUserStateKey, string.Format("\"{0}\"", "null"));
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelGroupUserStateKey, (channelGroupUserStateValue.GetType().ToString() == "System.String") ? string.Format("\"{0}\"", channelGroupUserStateValue) : channelGroupUserStateValue);
                    }
                    if (keyIndex < channelGroupUserStateKeys.Length - 1)
                    {
                        jsonStateBuilder.Append(",");
                    }
                }
            }

            return jsonStateBuilder.ToString();
		}

		private string BuildJsonUserState (string[] channels, string[] channelGroups, bool local)
		{
            string retJsonUserState = "";

			StringBuilder jsonStateBuilder = new StringBuilder ();

			if (channels != null && channels.Length > 0) 
            {
                for (int index = 0; index < channels.Length; index++)
                {
                    string currentJsonState = BuildJsonUserState(channels[index].ToString(), "", local);
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
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                for (int index = 0; index < channelGroups.Length; index++)
                {
                    string currentJsonState = BuildJsonUserState("", channelGroups[index].ToString(), local);
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format("\"{0}\":{{{1}}}", channelGroups[index].ToString(), currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(",");
                        }
                        jsonStateBuilder.Append(currentJsonState);
                    }
                }
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
            }

            return retJsonUserState;
		}

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, int userStateValue)
		{
			return AddOrUpdateOrDeleteLocalUserState (channel, channelGroup, userStateKey, userStateValue);
		}

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, double userStateValue)
		{
			return AddOrUpdateOrDeleteLocalUserState (channel, channelGroup, userStateKey, userStateValue);
		}

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, string userStateValue)
		{
            return AddOrUpdateOrDeleteLocalUserState(channel, channelGroup, userStateKey, userStateValue);
		}

        internal string GetLocalUserState(string channel, string channelGroup)
        {
            string retJsonUserState = "";
            StringBuilder jsonStateBuilder = new StringBuilder();

            string channelJsonUserState = BuildJsonUserState(channel, "", false);
            string channelGroupJsonUserState = BuildJsonUserState("", channelGroup, false);

            if (channelJsonUserState.Trim().Length > 0 && channelGroupJsonUserState.Trim().Length <= 0)
            {
                jsonStateBuilder.Append(channelJsonUserState);
            }
            else if (channelJsonUserState.Trim().Length <= 0 && channelGroupJsonUserState.Trim().Length > 0)
            {
                jsonStateBuilder.Append(channelGroupJsonUserState);
            }
            else if (channelJsonUserState.Trim().Length > 0 && channelGroupJsonUserState.Trim().Length > 0)
            {
                jsonStateBuilder.AppendFormat("{0}:{1},{2}:{3}", channel, channelJsonUserState, channelGroup, channelGroupJsonUserState);
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
            }

            return retJsonUserState;
        }

		internal void SetUserState(string channel, string uuid, string jsonUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
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
				throw new MissingMemberException("Missing json format for user state");
			}
			else
			{
				Dictionary<string, object> deserializeUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonUserState);
				if (deserializeUserState == null)
				{
					throw new MissingMemberException("Missing json format user state");
				}
				else
				{
					string oldJsonState = GetLocalUserState(channel, "");
					if (oldJsonState == jsonUserState)
					{
						string message = "No change in User State";

						CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
							channel, "", errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
						return;
					}

				}
			}

			SharedSetUserState(channel, null, uuid, jsonUserState,"{}", userCallback, errorCallback);
		}

		internal void SetUserState(string channel, string channelGroup, string uuid, string jsonUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
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
				throw new MissingMemberException("Missing json format for user state");
			}
			else
			{
				Dictionary<string, object> deserializeUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonUserState);
				if (deserializeUserState == null)
				{
					throw new MissingMemberException("Missing json format user state");
				}
				else
				{
					string oldChannelJsonState = GetLocalUserState(channel, "");
					string oldChannelGroupJsonState = GetLocalUserState("", channelGroup);
					if (oldChannelJsonState == jsonUserState && oldChannelGroupJsonState == jsonUserState)
					{
						string message = "No change in User State";

						CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
							channel, channelGroup, errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
						return;
					}

				}
			}

			SharedSetUserState(channel, channelGroup, uuid, jsonUserState, jsonUserState, userCallback, errorCallback);
		}

		internal void SetUserState(string channel, string uuid, KeyValuePair<string, object> keyValuePair, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Missing Channel");
			}
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			string key = keyValuePair.Key;

			int valueInt;
			double valueDouble;
			string currentChannelUserState = "";

			string oldJsonState = GetLocalUserState(channel, "");
			if (keyValuePair.Value == null)
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, null);
			}
			else if (Int32.TryParse(keyValuePair.Value.ToString(), out valueInt))
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, valueInt);
			}
			else if (Double.TryParse(keyValuePair.Value.ToString(), out valueDouble))
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, valueDouble);
			}
			else
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, keyValuePair.Value.ToString());
			}

			if (oldJsonState == currentChannelUserState)
			{
				string message = "No change in User State";

				CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
					channel, "", errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
				return;
			}

			if (currentChannelUserState.Trim() == "")
			{
				currentChannelUserState = "{}";
			}

			SharedSetUserState(channel, null, uuid, currentChannelUserState,"{}", userCallback, errorCallback);
		}

		internal void SetUserState(string channel, string channelGroup, string uuid, KeyValuePair<string, object> keyValuePair, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
			{
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}
			if (userCallback == null)
			{
				throw new ArgumentException("Missing userCallback");
			}
			if (errorCallback == null)
			{
				throw new ArgumentException("Missing errorCallback");
			}

			string key = keyValuePair.Key;

			int valueInt;
			double valueDouble;
			string currentChannelUserState = "";
			string currentChannelGroupUserState = "";

			string oldJsonChannelState = GetLocalUserState(channel, "");
			string oldJsonChannelGroupState = GetLocalUserState("", channelGroup);

			if (keyValuePair.Value == null)
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, null);
				currentChannelGroupUserState = SetLocalUserState("", channelGroup, key, null);
			}
			else if (Int32.TryParse(keyValuePair.Value.ToString(), out valueInt))
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, valueInt);
				currentChannelGroupUserState = SetLocalUserState("", channelGroup, key, valueInt);
			}
			else if (Double.TryParse(keyValuePair.Value.ToString(), out valueDouble))
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, valueDouble);
				currentChannelGroupUserState = SetLocalUserState("", channelGroup, key, valueDouble);
			}
			else
			{
				currentChannelUserState = SetLocalUserState(channel, "", key, keyValuePair.Value.ToString());
				currentChannelGroupUserState = SetLocalUserState("", channelGroup, key, keyValuePair.Value.ToString());
			}

			if (oldJsonChannelState == currentChannelUserState && oldJsonChannelGroupState == currentChannelGroupUserState)
			{
				string message = "No change in User State";

				CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
					channel, "", errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
				return;
			}

			if (currentChannelUserState.Trim() == "")
			{
				currentChannelUserState = "{}";
			}
			if (currentChannelGroupUserState == "")
			{
				currentChannelGroupUserState = "{}";
			}

			SharedSetUserState(channel, channelGroup, uuid, currentChannelUserState, currentChannelGroupUserState, userCallback, errorCallback);
		}

		private void SharedSetUserState(string channel, string channelGroup, string uuid, string jsonChannelUserState, string jsonChannelGroupUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			channel = (string.IsNullOrEmpty(channel)) ? "" : channel;
			channelGroup = (string.IsNullOrEmpty(channelGroup)) ? "" : channelGroup;

			if (string.IsNullOrEmpty(uuid))
			{
				VerifyOrSetSessionUUID();
				uuid = this.sessionUUID;
			}

			Dictionary<string, object> deserializeChannelUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonChannelUserState);
			Dictionary<string, object> deserializeChannelGroupUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonChannelGroupUserState);

			if (_channelUserState != null && !string.IsNullOrEmpty(channel))
			{
				_channelUserState.AddOrUpdate(channel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
			}
			if (_channelLocalUserState != null && !string.IsNullOrEmpty(channel))
			{
				_channelLocalUserState.AddOrUpdate(channel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
			}

			if (_channelGroupUserState != null && !string.IsNullOrEmpty(channelGroup))
			{
				_channelGroupUserState.AddOrUpdate(channelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
			}
			if (_channelGroupLocalUserState != null && !string.IsNullOrEmpty(channelGroup))
			{
				_channelGroupLocalUserState.AddOrUpdate(channelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
			}

			string jsonUserState = "{}";

			if (jsonChannelUserState == jsonChannelGroupUserState)
			{
				jsonUserState = jsonChannelUserState;
			}
			else if (jsonChannelUserState == "{}" && jsonChannelGroupUserState != "{}")
			{
				jsonUserState = jsonChannelGroupUserState;
			}
			else if (jsonChannelUserState != "{}" && jsonChannelGroupUserState == "{}")
			{
				jsonUserState = jsonChannelUserState;
			}
			else if (jsonChannelUserState != "{}" && jsonChannelGroupUserState != "{}")
			{
				jsonUserState = string.Format("{{\"{0}\":{{{1}}},\"{2}\":{{{3}}}}}", channel, jsonChannelUserState, channelGroup, jsonChannelGroupUserState);
			}

			Uri request = BuildSetUserStateRequest(channel, channelGroup, uuid, jsonUserState);

			RequestState<SetUserStateAck> requestState = new RequestState<SetUserStateAck>();
			requestState.Channels = new string[] { channel };
			requestState.ChannelGroups = new string[] { channelGroup };
			requestState.ResponseType = ResponseType.SetUserState;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<SetUserStateAck>(request, requestState);

			//bounce the long-polling subscribe requests to update user state
			TerminateCurrentSubscriberRequest();
		}

		internal void GetUserState(string channel, string uuid, Action<GetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
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

            
			Uri request = BuildGetUserStateRequest (channel, null, uuid);

			RequestState<GetUserStateAck> requestState = new RequestState<GetUserStateAck>();
			requestState.Channels = new string[] { channel };
			requestState.ChannelGroups = new string[] { };
			requestState.ResponseType = ResponseType.GetUserState;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<GetUserStateAck>(request, requestState);
		}

		internal void GetUserState(string channel, string channelGroup, string uuid, Action<GetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
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

            if (string.IsNullOrEmpty(uuid))
            {
                VerifyOrSetSessionUUID();
                uuid = this.sessionUUID;
            }
            channel = (string.IsNullOrEmpty(channel)) ? "" : channel;
            channelGroup = (string.IsNullOrEmpty(channelGroup)) ? "" : channelGroup;

            Uri request = BuildGetUserStateRequest(channel, channelGroup, uuid);

			RequestState<GetUserStateAck> requestState = new RequestState<GetUserStateAck>();
			requestState.Channels = new string[] { channel };
			requestState.ChannelGroups = new string[] { channelGroup };
			requestState.ResponseType = ResponseType.GetUserState;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			UrlProcessRequest<GetUserStateAck>(request, requestState);
        }
        
        private Uri BuildSetUserStateRequest(string channel, string channelGroup, string uuid, string jsonUserState)
		{
            if (string.IsNullOrEmpty(channel) && channel.Trim().Length <= 0)
            {
                channel = ",";
            }
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                setUserStateParameters = string.Format("?state={0}&channel-group={1}", EncodeUricomponent(jsonUserState, ResponseType.SetUserState, false, false), EncodeUricomponent(channelGroup, ResponseType.SetUserState, false, false));
            }
            else
            {
                setUserStateParameters = string.Format("?state={0}", EncodeUricomponent(jsonUserState, ResponseType.SetUserState, false, false));
            }

			List<string> url = new List<string>();

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

		private Uri BuildGetUserStateRequest (string channel, string channelGroup, string uuid)
		{
            getUserStateParameters = "";
            if (string.IsNullOrEmpty(channel) && channel.Trim().Length <= 0)
            {
                channel = ",";
            }

            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                getUserStateParameters = string.Format("&channel-group={0}", EncodeUricomponent(channelGroup, ResponseType.GetUserState, false, false));
            }

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

		protected void UrlRequestCommonExceptionHandler<T>(ResponseType type, string[] channels, string[] channelGroups, bool requestTimeout, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback, bool resumeOnReconnect)
		{
			if (type == ResponseType.Subscribe || type == ResponseType.Presence)
			{
				MultiplexExceptionHandler<T>(type, channels, channelGroups, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, false, resumeOnReconnect);
			}
			else if (type == ResponseType.Publish)
			{
				PublishExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.Here_Now)
			{
				HereNowExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.DetailedHistory)
			{
				DetailedHistoryExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.Time)
			{
				TimeExceptionHandler(requestTimeout, errorCallback);
			}
			else if (type == ResponseType.Leave)
			{
				//no action at this time
			}
			else if (type == ResponseType.PresenceHeartbeat)
			{
				//no action at this time
			}
			else if (type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess)
			{
			}
			else if (type == ResponseType.ChannelGroupGrantAccess || type == ResponseType.ChannelGroupAuditAccess || type == ResponseType.ChannelGroupRevokeAccess)
			{
			}
			else if (type == ResponseType.GetUserState)
			{
				GetUserStateExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.SetUserState)
			{
				SetUserStateExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.GlobalHere_Now)
			{
				GlobalHereNowExceptionHandler(requestTimeout, errorCallback);
			}
			else if (type == ResponseType.Where_Now)
			{
				WhereNowExceptionHandler(channels[0], requestTimeout, errorCallback);
			}
			else if (type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
			{
				PushNotificationExceptionHandler(channels, requestTimeout, errorCallback);
			}
			else if (type == ResponseType.ChannelGroupAdd || type == ResponseType.ChannelGroupRemove || type == ResponseType.ChannelGroupGet)
			{
				ChannelGroupExceptionHandler(channels, requestTimeout, errorCallback);
			}
		}

		protected void MultiplexExceptionHandler<T>(ResponseType type, string[] channels, string[] channelGroups, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback, bool reconnectMaxTried, bool resumeOnReconnect)
		{
			string channel = "";
			string channelGroup = "";
			if (channels != null)
			{
				channel = string.Join(",", channels);
			}
			if (channelGroups != null)
			{
				channelGroup = string.Join(",", channelGroups);
			}

			if (reconnectMaxTried)
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, MAX retries reached. Exiting the subscribe for channel(s) = {1}; channelgroup(s)={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);

				string[] activeChannels = multiChannelSubscribe.Keys.ToArray<string>();
				string[] activeChannelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();
				MultiChannelUnSubscribeInit<T>(ResponseType.Unsubscribe, string.Join(",", activeChannels), string.Join(",", activeChannelGroups), null);

				if (channelInternetStatus.ContainsKey(string.Join(",", activeChannels)) || channelGroupInternetStatus.ContainsKey(string.Join(",", activeChannelGroups)))
				{
					ResetInternetCheckSettings(activeChannels, activeChannelGroups);
				}

				string[] subscribeChannels = activeChannels.Where(filterChannel => !filterChannel.Contains("-pnpres")).ToArray();
				string[] presenceChannels = activeChannels.Where(filterChannel => filterChannel.Contains("-pnpres")).ToArray();

				string[] subscribeChannelGroups = activeChannelGroups.Where(filterChannelGroup => !filterChannelGroup.Contains("-pnpres")).ToArray();
				string[] presenceChannelGroups = activeChannelGroups.Where(filterChannelGroup => filterChannelGroup.Contains("-pnpres")).ToArray();

				if (subscribeChannels != null && subscribeChannels.Length > 0)
				{
					for (int index = 0; index < subscribeChannels.Length; index++)
					{
						string message = string.Format("Channel(s) Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannel = subscribeChannels[index].ToString();

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
						callbackKey.Channel = activeChannel;
						callbackKey.ResponseType = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
						{
							if (type == ResponseType.Presence)
							{
								PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.PresenceRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										activeChannel, "", currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
							else
							{
								PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.SubscribeRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										activeChannel, "", currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
						}

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, Channel Subscribe JSON network error response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);
					}
				}
				if (presenceChannels != null && presenceChannels.Length > 0)
				{
					for (int index = 0; index < presenceChannels.Length; index++)
					{
						string message = string.Format("Channel(s) Presence Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannel = presenceChannels[index].ToString();

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
						callbackKey.Channel = activeChannel;
						callbackKey.ResponseType = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
						{
							if (type == ResponseType.Presence)
							{
								PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.PresenceRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										activeChannel, "", currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.PresenceUnsubscribedAfterMaxRetries, null, null);
								}
							}
							else
							{
								PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.SubscribeRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										activeChannel, "", currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.PresenceUnsubscribedAfterMaxRetries, null, null);
								}
							}
						}

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, Channel(s) Presence-Subscribe JSON network error response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);
					}
				}

				if (subscribeChannelGroups != null && subscribeChannelGroups.Length > 0)
				{
					for (int index = 0; index < subscribeChannelGroups.Length; index++)
					{
						string message = string.Format("ChannelGroup(s) Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannelGroup = subscribeChannelGroups[index].ToString();

						PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
						callbackKey.ChannelGroup = activeChannelGroup;
						callbackKey.ResponseType = type;

						if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
						{
							if (type == ResponseType.Presence)
							{
								PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.PresenceRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										"", activeChannelGroup, currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
							else
							{
								PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.SubscribeRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										"", activeChannelGroup, currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
						}

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, ChannelGroup(s) Subscribe JSON network error response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);
					}
				}
				if (presenceChannelGroups != null && presenceChannelGroups.Length > 0)
				{
					for (int index = 0; index < presenceChannelGroups.Length; index++)
					{
						string message = string.Format("ChannelGroup(s) Presence Unsubscribed after {0} failed retries", _pubnubNetworkCheckRetries);
						string activeChannelGroup = presenceChannelGroups[index].ToString();

						PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
						callbackKey.ChannelGroup = activeChannelGroup;
						callbackKey.ResponseType = type;

						if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
						{
							if (type == ResponseType.Presence)
							{
								PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
								if (currentPubnubCallback != null && currentPubnubCallback.PresenceRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										"", activeChannelGroup, currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
							else
							{
								PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
								if (currentPubnubCallback != null && currentPubnubCallback.SubscribeRegularCallback != null)
								{
									CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
										"", activeChannelGroup, currentPubnubCallback.ErrorCallback, message,
										PubnubErrorCode.UnsubscribedAfterMaxRetries, null, null);
								}
							}
						}

						LoggingMethod.WriteToLog(string.Format("DateTime {0}, ChannelGroup(s) Presence-Subscribe JSON network error response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);
					}
				}

			}
			else
			{
				List<object> result = new List<object>();
				result.Add("0");
				if (resumeOnReconnect)
				{
					result.Add(0); //send 0 time token to enable presence event
				}
				else
				{
					result.Add(lastSubscribeTimetoken); //get last timetoken
				}
				if (channelGroups != null && channelGroups.Length > 0)
				{
					result.Add(channelGroups);
				}
				result.Add(channels); //send channel name

				MultiplexInternalCallback<T>(type, result, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback);
			}
		}

		private void PublishExceptionHandler (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, JSON publish response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message,
					PubnubErrorCode.PublishOperationTimeout, null, null);
			}
		}

		private void PAMAccessExceptionHandler<T> (string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PAMAccessExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message,
					PubnubErrorCode.PAMAccessOperationTimeout, null, null);
			}
		}

		private void WhereNowExceptionHandler(string uuid, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WhereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					uuid, "", errorCallback, message, PubnubErrorCode.WhereNowOperationTimeout, null, null);
			}
		}

		private void HereNowExceptionHandler(string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, HereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message,
					PubnubErrorCode.HereNowOperationTimeout, null, null);
			}
		}

		private void GlobalHereNowExceptionHandler(bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GlobalHereNowExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					"", "", errorCallback, message, PubnubErrorCode.GlobalHereNowOperationTimeout, null, null);
			}
		}

		private void DetailedHistoryExceptionHandler(string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, DetailedHistoryExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message, 
					PubnubErrorCode.DetailedHistoryOperationTimeout, null, null);
			}
		}

		private void TimeExceptionHandler(bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, TimeExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					"", "", errorCallback, message, PubnubErrorCode.TimeOperationTimeout, null, null);
			}
		}

		private void SetUserStateExceptionHandler(string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SetUserStateExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message,
					PubnubErrorCode.SetUserStateTimeout, null, null);
			}
		}

		private void GetUserStateExceptionHandler(string channelName, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			if (requestTimeout) {
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GetUserStateExceptionHandler response={1}", DateTime.Now.ToString (), message), LoggingMethod.LevelInfo);

				CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channelName, "", errorCallback, message,
					PubnubErrorCode.GetUserStateTimeout, null, null);
			}
		}

		private void PushNotificationExceptionHandler(string[] channels, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			string channel = "";
			if (channels != null)
			{
				channel = string.Join(",", channels);
			}
			if (requestTimeout)
			{
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog(string.Format("DateTime {0}, PushExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

				CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channel, "", errorCallback, message,
					PubnubErrorCode.PushNotificationTimeout, null, null);
			}
		}

		private void ChannelGroupExceptionHandler(string[] channels, bool requestTimeout, Action<PubnubClientError> errorCallback)
		{
			string channel = "";
			if (channels != null)
			{
				channel = string.Join(",", channels);
			}
			if (requestTimeout)
			{
				string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

				LoggingMethod.WriteToLog(string.Format("DateTime {0}, ChannelGroupExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

				CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
					channel, "", errorCallback, message,
					PubnubErrorCode.ChannelGroupTimeout, null, null);
			}
		}
		#endregion

		#region "Callbacks"

		protected virtual bool CheckInternetConnectionStatus (bool systemActive, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
		{
			return ClientNetworkStatus.CheckInternetStatus(pubnetSystemActive, errorCallback, channels, channelGroups);
		}

		protected void OnPresenceHeartbeatIntervalTimeout<T> (System.Object presenceHeartbeatState)
		{
			//Make presence heartbeat call
			RequestState<T> currentState = presenceHeartbeatState as RequestState<T>;
			if (currentState != null) 
            {
				bool networkConnection;
				if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) 
                {
					networkConnection = true;
				} else 
                {
                    networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, currentState.ErrorCallback, currentState.Channels, currentState.ChannelGroups);
					if (networkConnection) 
                    {
						string[] subscriberChannels = (currentState.Channels != null) ? currentState.Channels.Where (s => s.Contains ("-pnpres") == false).ToArray() : null;
                        string[] subscriberChannelGroups = (currentState.ChannelGroups != null) ? currentState.ChannelGroups.Where(s => s.Contains("-pnpres") == false).ToArray() : null;

						if ((subscriberChannels != null && subscriberChannels.Length > 0) || (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0))
                        {
                            Uri request = BuildPresenceHeartbeatRequest(subscriberChannels, subscriberChannelGroups);

							RequestState<T> requestState = new RequestState<T> ();
							requestState.Channels = currentState.Channels;
                            requestState.ChannelGroups = currentState.ChannelGroups;
							requestState.ResponseType = ResponseType.PresenceHeartbeat;
							requestState.SubscribeRegularCallback = null;
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
			if (currentState != null) 
            {
				string channel = (currentState.Channels != null) ? string.Join(",", currentState.Channels) : "";
                string channelGroup = (currentState.ChannelGroups != null) ? string.Join(",", currentState.ChannelGroups) : "";

				if ((channelInternetStatus.ContainsKey(channel) || channelGroupInternetStatus.ContainsKey(channelGroup))
				        && (currentState.ResponseType == ResponseType.Subscribe || currentState.ResponseType == ResponseType.Presence || currentState.ResponseType == ResponseType.PresenceHeartbeat)
				        && overrideTcpKeepAlive) 
                {
					bool networkConnection;
					if (_pubnubUnitTest is IPubnubUnitTest && _pubnubUnitTest.EnableStubTest) 
                    {
						networkConnection = true;
					} 
                    else 
                    {
                        networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, currentState.ErrorCallback, currentState.Channels, currentState.ChannelGroups);
					}

					channelInternetStatus[channel] = networkConnection;
                    channelGroupInternetStatus[channelGroup] = networkConnection;

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
		/// <param name="subscribeOrPresenceRegularCallback"></param>
		/// <param name="connectCallback"></param>
		/// <param name="errorCallback"></param>
		protected void MultiplexInternalCallback<T>(ResponseType type, object multiplexResult, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
            List<object> message = multiplexResult as List<object>;
            string[] channels = null;
            string[] channelGroups = null;
            if (message != null && message.Count >= 3)
            {
                if (message[message.Count - 1] is string[])
                {
                    channels = message[message.Count - 1] as string[];
                }
                else
                {
                    channels = message[message.Count - 1].ToString().Split(',') as string[];
                }

                if (channels.Length == 1 && channels[0] == "")
                {
                    channels = new string[] { };
                }
                if (message.Count >= 4)
                {
                    if (message[message.Count - 2] is string[])
                    {
                        channelGroups = message[message.Count - 2] as string[];
                    }
                    else if (message[message.Count - 2].ToString() != "")
                    {
                        channelGroups = message[message.Count - 2].ToString().Split(',') as string[];
                    }
                }
            }
            else
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Lost Channel Name for resubscribe", DateTime.Now.ToString()), LoggingMethod.LevelError);
                return;
            }

            if (message != null && message.Count >= 3)
            {
				MultiChannelSubscribeRequest<T>(type, channels, channelGroups, (object)message[1], subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, false);
            }
        }

		private void ResponseToConnectCallback<T>(List<object> result, ResponseType type, string[] channels, string[] channelGroups, Action<ConnectOrDisconnectAck> connectCallback)
		{
			//Check callback exists and make sure previous timetoken = 0
			if (channels != null && channels.Length > 0 && connectCallback != null)
			{
				IEnumerable<string> newChannels = from channel in multiChannelSubscribe
						where channel.Value == 0
					select channel.Key;
				foreach (string channel in newChannels)
				{
					string jsonString = "";
					List<object> connectResult = new List<object>();
					switch (type)
					{
					case ResponseType.Subscribe:
						jsonString = string.Format("[1, \"Connected\"]");

						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
                        connectResult.Add("");
						connectResult.Add(channel);

						PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
						callbackKey.Channel = channel;
						callbackKey.ResponseType = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
						{
							PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
							{
								Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.ConnectCallback;
								currentPubnubCallback.ConnectCallback = null;
								GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
							}
						}
						break;
					case ResponseType.Presence:
						jsonString = string.Format("[1, \"Presence Connected\"]");
						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
                        connectResult.Add("");
						connectResult.Add(channel.Replace("-pnpres", ""));

						PubnubChannelCallbackKey pCallbackKey = new PubnubChannelCallbackKey();
						pCallbackKey.Channel = channel;
						pCallbackKey.ResponseType = type;

						if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(pCallbackKey))
						{
							PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[pCallbackKey] as PubnubPresenceChannelCallback;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
							{
								Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.ConnectCallback;
								currentPubnubCallback.ConnectCallback = null;
								GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
							}
						}
						break;
					default:
						break;
					}
				}
			}

			if (channelGroups != null && channelGroups.Length > 0 && connectCallback != null)
			{
				IEnumerable<string> newChannelGroups = from channelGroup in multiChannelGroupSubscribe
						where channelGroup.Value == 0
					select channelGroup.Key;
				foreach (string channelGroup in newChannelGroups)
				{
					string jsonString = "";
					List<object> connectResult = new List<object>();
					switch (type)
					{
					case ResponseType.Subscribe:
						jsonString = string.Format("[1, \"Connected\"]");
						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
						connectResult.Add(channelGroup);
                        connectResult.Add("");

						PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
						callbackKey.ChannelGroup = channelGroup;
						callbackKey.ResponseType = type;

						if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
						{
							PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
							{
								Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.ConnectCallback;
								currentPubnubCallback.ConnectCallback = null;
								GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
							}

						}
						break;
					case ResponseType.Presence:
						jsonString = string.Format("[1, \"Presence Connected\"]");
						connectResult = _jsonPluggableLibrary.DeserializeToListOfObject(jsonString);
						connectResult.Add(channelGroup.Replace("-pnpres", ""));
                        connectResult.Add("");

						PubnubChannelGroupCallbackKey pCallbackKey = new PubnubChannelGroupCallbackKey();
						pCallbackKey.ChannelGroup = channelGroup;
						pCallbackKey.ResponseType = type;

						if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(pCallbackKey))
						{
							PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[pCallbackKey] as PubnubPresenceChannelGroupCallback;
							if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
							{
								Action<ConnectOrDisconnectAck> targetCallback = currentPubnubCallback.ConnectCallback;
								currentPubnubCallback.ConnectCallback = null;
								GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
							}
						}
						break;
					default:
						break;
					}
				}
			}
		}

		protected abstract void ProcessResponseCallbackExceptionHandler<T> (Exception ex, RequestState<T> asynchRequestState);

		protected abstract bool HandleWebException<T> (WebException webEx, RequestState<T> asynchRequestState, string channel, string channelGroup);

        protected abstract void ProcessResponseCallbackWebExceptionHandler<T>(WebException webEx, RequestState<T> asyncRequestState, string channel, string channelGroup);

		protected void ProcessResponseCallbacks<T> (List<object> result, RequestState<T> asyncRequestState)
		{
            bool callbackAvailable = false;
			if (result != null && result.Count >= 1 )
			{
                if (asyncRequestState.SubscribeRegularCallback != null || asyncRequestState.PresenceRegularCallback != null || asyncRequestState.NonSubscribeRegularCallback != null)
                {
                    callbackAvailable = true;
                }
                else
                {
                    if (asyncRequestState.ResponseType == ResponseType.Subscribe || asyncRequestState.ResponseType == ResponseType.Presence)
                    {
                        if (asyncRequestState.Channels != null && asyncRequestState.Channels.Length > 0)
                        {
                            List<string> chList = asyncRequestState.Channels.ToList();
                            foreach (string ch in chList)
                            {
                                PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                                callbackKey.Channel = ch;
                                callbackKey.ResponseType = asyncRequestState.ResponseType;

                                if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                                {
                                    callbackAvailable = true;
                                    break;
                                }
                            }
                        }
                        if (!callbackAvailable && asyncRequestState.ChannelGroups != null && asyncRequestState.ChannelGroups.Length > 0)
                        {
                            List<string> cgList = asyncRequestState.ChannelGroups.ToList();
                            foreach (string cg in cgList)
                            {
                                PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
                                callbackKey.ChannelGroup = cg;
                                callbackKey.ResponseType = asyncRequestState.ResponseType;

                                if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                                {
                                    callbackAvailable = true;
                                    break;
                                }
                            }
                        }
                    }
                }
			}
            if (callbackAvailable)
            {
                ResponseToConnectCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.ConnectCallback);
                ResponseToUserCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.NonSubscribeRegularCallback);
            }
		}
		//#if (!UNITY_IOS)
		//TODO:refactor
		protected abstract void UrlProcessResponseCallback<T> (IAsyncResult asynchronousResult);

        //#endif
		//TODO:refactor
		private void ResponseToUserCallback<T>(List<object> result, ResponseType type, string[] channels, string[] channelGroups, Action<T> userCallback)
		{
			string[] messageChannels = null;
			string[] messageChannelGroups = null;
			string[] messageWildcardPresenceChannels = null;
			switch (type)
			{
			case ResponseType.Subscribe:
			case ResponseType.Presence:
				var messages = (from item in result
					select item as object).ToArray();
				if (messages != null && messages.Length > 0)
				{
					object[] messageList = messages[0] as object[];
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
					if (messageList != null && messageList.Length > 0)
					{
						if (messages.Length == 4 || messages.Length == 6)
						{
							messageChannelGroups = messages[2].ToString().Split(',');
							messageChannels = messages[3].ToString().Split(',');
						}
						else
						{
							messageChannels = messages[2].ToString().Split(',');
							messageChannelGroups = null;
						}
						for (int messageIndex = 0; messageIndex < messageList.Length; messageIndex++)
						{
							string currentChannel = (messageChannels.Length == 1) ? (string)messageChannels[0] : (string)messageChannels[messageIndex];
							string currentChannelGroup = "";
							if (messageChannelGroups != null && messageChannelGroups.Length > 0)
							{
								currentChannelGroup = (messageChannelGroups.Length == 1) ? (string)messageChannelGroups[0] : (string)messageChannelGroups[messageIndex];
							}
							List<object> itemMessage = new List<object>();
							if (currentChannel.Contains(".*-pnpres"))
							{
								itemMessage.Add(messageList[messageIndex]);
							}
							else if (currentChannel.Contains("-pnpres"))
							{
								itemMessage.Add(messageList[messageIndex]);
							}
							else
							{
								//decrypt the subscriber message if cipherkey is available
								if (this.cipherKey.Length > 0)
								{
									PubnubCrypto aes = new PubnubCrypto(this.cipherKey);
									string decryptMessage = aes.Decrypt(messageList[messageIndex].ToString());
									object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : _jsonPluggableLibrary.DeserializeToObject(decryptMessage);

									itemMessage.Add(decodeMessage);
								}
								else
								{
									itemMessage.Add(messageList[messageIndex]);
								}
							}
							itemMessage.Add(messages[1].ToString());

							//if (messageWildcardPresenceChannels != null)
							//{
							//    string wildPresenceChannel = (messageWildcardPresenceChannels.Length == 1) ? (string)messageWildcardPresenceChannels[0] : (string)messageWildcardPresenceChannels[messageIndex];
							//    itemMessage.Add(wildPresenceChannel);
							//}

							if (currentChannel == currentChannelGroup)
							{
								itemMessage.Add(currentChannel.Replace("-pnpres", ""));
							}
							else
							{
								if (currentChannelGroup != "")
								{
									itemMessage.Add(currentChannelGroup.Replace("-pnpres", ""));
								}
								if (currentChannel != "")
								{
									itemMessage.Add(currentChannel.Replace("-pnpres", ""));
								}
							}

							PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();

							if (!string.IsNullOrEmpty(currentChannelGroup) && currentChannelGroup.Contains(".*"))
							{
								callbackKey.Channel = currentChannelGroup;
								callbackKey.ResponseType = ResponseType.Subscribe;
							}
							else
							{
								callbackKey.Channel = currentChannel;
								callbackKey.ResponseType = (currentChannel.LastIndexOf("-pnpres") == -1) ? ResponseType.Subscribe : ResponseType.Presence;
							}

							if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
							{
								//TODO: PANDU REFACTOR REPEAT LOGIC
								if (callbackKey.ResponseType == ResponseType.Presence)
								{
									PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
									if (currentPubnubCallback != null)
									{
										if (currentPubnubCallback.PresenceRegularCallback != null)
										{
											GoToCallback(itemMessage, currentPubnubCallback.PresenceRegularCallback, true, type);
										}
									}

								}
								else
								{
                                    PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
									//object pubnubSubscribeCallbackObject = channelCallbacks[callbackKey];
                                    //if (pubnubSubscribeCallbackObject is PubnubSubscribeChannelCallback<string>)
                                    //{
                                    //    currentPubnubCallback = pubnubSubscribeCallbackObject as PubnubSubscribeChannelCallback<string>;
                                    //}
                                    //else if (pubnubSubscribeCallbackObject is PubnubSubscribeChannelCallback<object>)
                                    //{
                                    //    currentPubnubCallback = pubnubSubscribeCallbackObject as PubnubSubscribeChannelCallback<object>;
                                    //}
                                    //else
                                    //{
                                    //    Type targetType = _channelSubscribeObjectType[currentChannel];

                                    //    if (_subscribeMessageType != null)
                                    //    {
                                    //        currentPubnubCallback = _subscribeMessageType.GetSubscribeMessageType(targetType, pubnubSubscribeCallbackObject, false);
                                    //    }
                                    //    else
                                    //    {
                                    //        currentPubnubCallback = null;
                                    //    }
                                    //}

									if (currentPubnubCallback != null)
									{
										if (itemMessage.Count >= 4 && currentChannel.Contains(".*") && currentChannel.Contains("-pnpres"))
										{
											if (currentPubnubCallback.WildcardPresenceCallback != null)
											{
												GoToCallback(itemMessage, currentPubnubCallback.WildcardPresenceCallback, true, type);
											}
										}
										else
										{
											if (currentPubnubCallback.SubscribeRegularCallback != null)
											{
                                                GoToCallback<Message<T>>(itemMessage, currentPubnubCallback.SubscribeRegularCallback, false, type);
											}
										}
									}
								}
							}

							PubnubChannelGroupCallbackKey callbackGroupKey = new PubnubChannelGroupCallbackKey();
							callbackGroupKey.ChannelGroup = currentChannelGroup;
							callbackGroupKey.ResponseType = (currentChannelGroup.LastIndexOf("-pnpres") == -1) ? ResponseType.Subscribe : ResponseType.Presence;

							if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackGroupKey))
							{
								if (callbackGroupKey.ResponseType == ResponseType.Presence)
								{
									PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackGroupKey] as PubnubPresenceChannelGroupCallback;
									if (currentPubnubCallback != null)
									{
										if (itemMessage.Count >= 4 && currentChannelGroup.Contains(".*") && currentChannel.Contains("-pnpres"))
										{
											//if (currentPubnubCallback.WildcardPresenceCallback != null)
											//{
											//    GoToCallback(itemMessage, currentPubnubCallback.WildcardPresenceCallback);
											//}
										}
										else
										{
											if (currentPubnubCallback.PresenceRegularCallback != null)
											{
												GoToCallback(itemMessage, currentPubnubCallback.PresenceRegularCallback, true, type);
											}
										}
									}
								}
								else
								{
									PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackGroupKey] as PubnubSubscribeChannelGroupCallback<T>;
                                    //dynamic currentPubnubCallback;
                                    //object pubnubSubscribeCallbackObject = channelCallbacks[callbackKey];
                                    //if (pubnubSubscribeCallbackObject is PubnubSubscribeChannelGroupCallback<string>)
                                    //{
                                    //    currentPubnubCallback = pubnubSubscribeCallbackObject as PubnubSubscribeChannelGroupCallback<string>;
                                    //}
                                    //else if (pubnubSubscribeCallbackObject is PubnubSubscribeChannelGroupCallback<object>)
                                    //{
                                    //    currentPubnubCallback = pubnubSubscribeCallbackObject as PubnubSubscribeChannelGroupCallback<object>;
                                    //}
                                    //else
                                    //{
                                    //    Type targetType = _channelGroupSubscribeObjectType[currentChannelGroup];

                                    //    if (_subscribeMessageType != null)
                                    //    {
                                    //        currentPubnubCallback = _subscribeMessageType.GetSubscribeMessageType(targetType, pubnubSubscribeCallbackObject, true);
                                    //    }
                                    //    else
                                    //    {
                                    //        currentPubnubCallback = null;
                                    //    }
                                    //}

									if (currentPubnubCallback != null)
									{
										if (itemMessage.Count >= 4 && currentChannelGroup.Contains(".*") && currentChannel.Contains("-pnpres"))
										{
											if (currentPubnubCallback.WildcardPresenceCallback != null)
											{
												GoToCallback(itemMessage, currentPubnubCallback.WildcardPresenceCallback, true, type);
											}
										}
										else
										{
											if (currentPubnubCallback.SubscribeRegularCallback != null)
											{
												GoToCallback(itemMessage, currentPubnubCallback.SubscribeRegularCallback, false, type);
											}
										}
									}
								}
							}

						}
					}
				}
				break;
			case ResponseType.Publish:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.DetailedHistory:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.Here_Now:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.GlobalHere_Now:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.Where_Now:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.Time:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.Leave:
				//No response to callback
				break;
			case ResponseType.GrantAccess:
			case ResponseType.AuditAccess:
			case ResponseType.RevokeAccess:
			case ResponseType.ChannelGroupGrantAccess:
			case ResponseType.ChannelGroupAuditAccess:
			case ResponseType.ChannelGroupRevokeAccess:
			case ResponseType.GetUserState:
			case ResponseType.SetUserState:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.PushRegister:
			case ResponseType.PushRemove:
			case ResponseType.PushGet:
			case ResponseType.PushUnregister:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
				}
				break;
			case ResponseType.ChannelGroupAdd:
			case ResponseType.ChannelGroupRemove:
			case ResponseType.ChannelGroupGet:
				if (result != null && result.Count > 0)
				{
					GoToCallback<T>(result, userCallback, true, type);
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

		private void JsonResponseToCallback<T>(long result, Action<T> callback)
		{
			if (typeof(T) == typeof(long))
			{
				Action<long> castCallback = callback as Action<long>;
				castCallback(result);
			}
		}

//		protected void GoToCallback<T> (object result, Action<T> Callback)
//		{
//			if (Callback != null) {
//				if (typeof(T) == typeof(string)) {
//					JsonResponseToCallback (result, Callback);
//				} else {
//					Callback ((T)(object)result);
//				}
//			}
//		}
			
		protected void GoToCallback<T>(List<object> result, Action<T> Callback, bool internalObject, ResponseType type)
		{
			if (Callback != null)
			{
				if (typeof(T) == typeof(string))
				{
					JsonResponseToCallback(result, Callback);
				}
				else if (typeof(T) == typeof(long) && type == ResponseType.Time)
				{
					long timetoken;
					Int64.TryParse(result[0].ToString(), out timetoken);
					JsonResponseToCallback(timetoken, Callback);
				}
				else
				{
					T ret = default(T);
					if (!internalObject)
					{
						ret = _jsonPluggableLibrary.DeserializeToObject<T>(result);
					}
					else
					{
						#if (USE_JSONFX)|| (USE_JSONFX_UNITY)
						JsonFXDotNet jsonLib = new JsonFXDotNet();
						#elif (USE_DOTNET_SERIALIZATION)
						JscriptSerializer jsonLib = new JscriptSerializer();
						#elif (USE_MiniJSON)
						MiniJSONObjectSerializer jsonLib = new MiniJSONObjectSerializer();
						#elif (USE_JSONFX_UNITY_IOS)
						JsonFxUnitySerializer jsonLib = new JsonFxUnitySerializer();
						#else
						NewtonsoftJsonDotNet jsonLib = new NewtonsoftJsonDotNet();
						#endif

						ret = jsonLib.DeserializeToObject<T>(result);
					}

					Callback(ret);
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
			try
			{
				double timeStamp = unixNanoSecondTime / 10000000;
				DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
				return dateTime;
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
		{
			long numericTime;
			bool tried = Int64.TryParse(unixNanoSecondTime, out numericTime);
			if (tried)
			{
				try
				{
					double timeStamp = numericTime / 10000000;
					DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
					return dateTime;
				}
				catch 
				{
					return DateTime.MinValue;
				}
			}
			else
			{
				return DateTime.MinValue;
			}
		}

		private bool IsPresenceChannel (string channel)
		{
			if (channel.LastIndexOf ("-pnpres") > 0) {
				return true;
			} else {
				return false;
			}
		}

		private string[] GetCurrentSubscriberChannels()
		{
			string[] channels = null;
			if (multiChannelSubscribe != null && multiChannelSubscribe.Keys.Count > 0) 
            {
				channels = multiChannelSubscribe.Keys.ToArray<string>();
			}

			return channels;
		}

        private string[] GetCurrentSubscriberChannelGroups()
        {
            string[] channelGroups = null;
            if (multiChannelGroupSubscribe != null && multiChannelGroupSubscribe.Keys.Count > 0)
            {
                channelGroups = multiChannelGroupSubscribe.Keys.ToArray<string>();
            }

            return channelGroups;
        }

		/// <summary>
		/// Retrieves the channel name from the url components
		/// </summary>
		/// <param name="urlComponents"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private string GetChannelName (List<string> urlComponents, ResponseType type)
		{
            //This method is not in use
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

		#region "PAM Channel"

		private Uri BuildGrantAccessRequest(string channel, string authenticationKey, bool read, bool write, int ttl)
		{
			string signature = "0";
			long timeStamp = TranslateDateTimeToSeconds (DateTime.UtcNow);
			string queryString = "";
			StringBuilder queryStringBuilder = new StringBuilder ();
            if (!string.IsNullOrEmpty(authenticationKey))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authenticationKey, ResponseType.GrantAccess, false, false));
			}

			if (!string.IsNullOrEmpty(channel)) 
            {
				queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channel, ResponseType.GrantAccess, false,false));
			}

			queryStringBuilder.AppendFormat ("{0}", (queryStringBuilder.Length > 0) ? "&" : "");
            queryStringBuilder.AppendFormat("pnsdk={0}", EncodeUricomponent(_pnsdkVersion, ResponseType.GrantAccess, false, true));
            queryStringBuilder.AppendFormat("&r={0}", Convert.ToInt32(read));
            queryStringBuilder.AppendFormat("&timestamp={0}", timeStamp.ToString());
            if (ttl > -1)
            {
                queryStringBuilder.AppendFormat("&ttl={0}", ttl.ToString());
            }
            queryStringBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(sessionUUID, ResponseType.GrantAccess, false,false));
            queryStringBuilder.AppendFormat("&w={0}", Convert.ToInt32(write));

            if (this.secretKey.Length > 0) 
            {
				StringBuilder string_to_sign = new StringBuilder();
				string_to_sign.Append (this.subscribeKey)
					.Append("\n")
						.Append(this.publishKey)
						.Append("\n")
						.Append("grant")
						.Append("\n")
						.Append(queryStringBuilder.ToString());

				PubnubCrypto pubnubCrypto = new PubnubCrypto (this.cipherKey);
				signature = pubnubCrypto.PubnubAccessManagerSign (this.secretKey, string_to_sign.ToString());
				queryString = string.Format("signature={0}&{1}", signature, queryStringBuilder.ToString());
			}

			parameters = "";
			parameters += "?" + queryString;

			List<string> url = new List<string>();
			url.Add("v1");
			url.Add("auth");
			url.Add("grant");
			url.Add("sub-key");
			url.Add(this.subscribeKey);

			return BuildRestApiRequest<Uri>(url, ResponseType.GrantAccess);
		}

        private Uri BuildAuditAccessRequest(string channel, string authenticationKey)
		{
			string signature = "0";
			long timeStamp = ((_pubnubUnitTest == null) || (_pubnubUnitTest is IPubnubUnitTest && !_pubnubUnitTest.EnableStubTest))
				? TranslateDateTimeToSeconds (DateTime.UtcNow) 
					: TranslateDateTimeToSeconds (new DateTime (2013, 01, 01));
			string queryString = "";
			StringBuilder queryStringBuilder = new StringBuilder ();
            if (!string.IsNullOrEmpty(authenticationKey))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authenticationKey, ResponseType.AuditAccess, false, false));
			}
			if (!string.IsNullOrEmpty (channel)) {
				queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent (channel, ResponseType.AuditAccess, false, false));
			}
            queryStringBuilder.AppendFormat("{0}pnsdk={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(_pnsdkVersion, ResponseType.AuditAccess, false, true));
            queryStringBuilder.AppendFormat("{0}timestamp={1}", (queryStringBuilder.Length > 0) ? "&" : "", timeStamp.ToString());
            queryStringBuilder.AppendFormat("{0}uuid={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(sessionUUID, ResponseType.AuditAccess, false, false));

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
			return GrantAccess (channel, "", read, write, -1, userCallback, errorCallback);
		}

        public bool GrantAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantAccess<T>(channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantAccess(channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }
        
        public bool GrantAccess<T> (string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty (this.secretKey) || string.IsNullOrEmpty (this.secretKey.Trim ()) || this.secretKey.Length <= 0) {
                throw new MissingMemberException("Invalid secret key");
			}

			Uri request = BuildGrantAccessRequest(channel, authenticationKey, read, write, ttl);

			RequestState<T> requestState = new RequestState<T> ();
			requestState.Channels = new string[] { channel };
			requestState.ResponseType = ResponseType.GrantAccess;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

			return UrlProcessRequest<T> (request, requestState); 
		}

		public bool GrantPresenceAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return GrantPresenceAccess (channel, "", read, write, -1, userCallback, errorCallback);
		}

        public bool GrantPresenceAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantPresenceAccess(channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantPresenceAccess<T>(channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }

		public bool GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			string[] multiChannels = channel.Split (',');
			if (multiChannels.Length > 0) {
				for (int index = 0; index < multiChannels.Length; index++) {
					if (!string.IsNullOrEmpty (multiChannels [index]) && multiChannels [index].Trim ().Length > 0) {
						multiChannels [index] = string.Format ("{0}-pnpres", multiChannels [index]);
					} else {
                        throw new MissingMemberException("Invalid channel");
					}
				}
			}
			string presenceChannel = string.Join (",", multiChannels);
			return GrantAccess(presenceChannel, authenticationKey, read, write, ttl, userCallback, errorCallback);
		}

		public void AuditAccess<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			AuditAccess("", "", userCallback, errorCallback);
		}

		public void AuditAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            AuditAccess(channel, "", userCallback, errorCallback);
		}

        public void AuditAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(this.secretKey) || string.IsNullOrEmpty(this.secretKey.Trim()) || this.secretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            Uri request = BuildAuditAccessRequest(channel, authenticationKey);

            RequestState<T> requestState = new RequestState<T>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new string[] { channel };
            }
			requestState.ResponseType = ResponseType.AuditAccess;
			requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

		public void AuditPresenceAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            AuditPresenceAccess(channel, "", userCallback, errorCallback);
		}

        public void AuditPresenceAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannels = channel.Split(',');
            if (multiChannels.Length > 0)
            {
                for (int index = 0; index < multiChannels.Length; index++)
                {
                    multiChannels[index] = string.Format("{0}-pnpres", multiChannels[index]);
                }
            }
            string presenceChannel = string.Join(",", multiChannels);
            AuditAccess(presenceChannel, authenticationKey, userCallback, errorCallback);
        }

		#endregion

        #region "PAM ChannelGroup"

        private Uri BuildChannelGroupGrantAccessRequest(string channelGroup, string authenticationKey, bool read, bool write, bool manage, int ttl)
        {
            string signature = "0";
            long timeStamp = TranslateDateTimeToSeconds(DateTime.UtcNow);
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(authenticationKey))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authenticationKey, ResponseType.ChannelGroupGrantAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                queryStringBuilder.AppendFormat("{0}channel-group={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channelGroup, ResponseType.ChannelGroupGrantAccess, false, false));
            }

            queryStringBuilder.AppendFormat("{0}", (queryStringBuilder.Length > 0) ? "&" : "");
            queryStringBuilder.AppendFormat("m={0}", Convert.ToInt32(manage));
            queryStringBuilder.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, ResponseType.ChannelGroupGrantAccess, false, true));
            queryStringBuilder.AppendFormat("&r={0}", Convert.ToInt32(read));
            queryStringBuilder.AppendFormat("&timestamp={0}", timeStamp.ToString()  );
            if (ttl > -1)
            {
                queryStringBuilder.AppendFormat("&ttl={0}", ttl.ToString());
            }
            queryStringBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(sessionUUID, ResponseType.ChannelGroupGrantAccess, false, false));
            //queryStringBuilder.AppendFormat("&w={0}", Convert.ToInt32(write)); Not supported at this time.

            if (this.secretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign.Append(this.subscribeKey)
                    .Append("\n")
                        .Append(this.publishKey)
                        .Append("\n")
                        .Append("grant")
                        .Append("\n")
                        .Append(queryStringBuilder.ToString());

                PubnubCrypto pubnubCrypto = new PubnubCrypto(this.cipherKey);
                signature = pubnubCrypto.PubnubAccessManagerSign(this.secretKey, string_to_sign.ToString());
                queryString = string.Format("signature={0}&{1}", signature, queryStringBuilder.ToString());
            }

            parameters = "";
            parameters += "?" + queryString;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("auth");
            url.Add("grant");
            url.Add("sub-key");
            url.Add(this.subscribeKey);

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupGrantAccess);
        }

        private Uri BuildChannelGroupAuditAccessRequest(string channelGroup, string authenticationKey)
        {
            string signature = "0";
            long timeStamp = ((_pubnubUnitTest == null) || (_pubnubUnitTest is IPubnubUnitTest && !_pubnubUnitTest.EnableStubTest))
                ? TranslateDateTimeToSeconds(DateTime.UtcNow)
                    : TranslateDateTimeToSeconds(new DateTime(2013, 01, 01));
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(authenticationKey))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authenticationKey, ResponseType.ChannelGroupAuditAccess, false, false));
            }
            if (!string.IsNullOrEmpty(channelGroup))
            {
                queryStringBuilder.AppendFormat("{0}channel-group={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channelGroup, ResponseType.ChannelGroupAuditAccess, false, false));
            }
            queryStringBuilder.AppendFormat("{0}pnsdk={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(_pnsdkVersion, ResponseType.ChannelGroupAuditAccess, false, true));
            queryStringBuilder.AppendFormat("{0}timestamp={1}", (queryStringBuilder.Length > 0) ? "&" : "", timeStamp.ToString());
            queryStringBuilder.AppendFormat("{0}uuid={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(sessionUUID, ResponseType.ChannelGroupAuditAccess, false, false));

            if (this.secretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign.Append(this.subscribeKey)
                    .Append("\n")
                        .Append(this.publishKey)
                        .Append("\n")
                        .Append("audit")
                        .Append("\n")
                        .Append(queryStringBuilder.ToString());

                PubnubCrypto pubnubCrypto = new PubnubCrypto(this.cipherKey);
                signature = pubnubCrypto.PubnubAccessManagerSign(this.secretKey, string_to_sign.ToString());
                queryString = string.Format("signature={0}&{1}", signature, queryStringBuilder.ToString());
            }

            parameters = "";
            parameters += "?" + queryString;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("auth");
            url.Add("audit");
            url.Add("sub-key");
            url.Add(this.subscribeKey);

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupAuditAccess);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantAccess(channelGroup, "", read, write, manage, -1, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantAccess<T>(channelGroup, "", read, write, manage, ttl, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantAccess(channelGroup, authenticationKey, read, write, manage, -1, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(this.secretKey) || string.IsNullOrEmpty(this.secretKey.Trim()) || this.secretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            Uri request = BuildChannelGroupGrantAccessRequest(channelGroup, authenticationKey, read, write, manage, ttl);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { channelGroup };
			requestState.ResponseType = ResponseType.ChannelGroupGrantAccess;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

            return UrlProcessRequest<T>(request, requestState);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantPresenceAccess(channelGroup, "", read, write, manage, -1, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantPresenceAccess(channelGroup, "", read, write, manage, ttl, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return ChannelGroupGrantPresenceAccess<T>(channelGroup, authenticationKey, read, write, manage, -1, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannelGroups = channelGroup.Split(',');
            if (multiChannelGroups.Length > 0)
            {
                for (int index = 0; index < multiChannelGroups.Length; index++)
                {
                    if (!string.IsNullOrEmpty(multiChannelGroups[index]) && multiChannelGroups[index].Trim().Length > 0)
                    {
                        multiChannelGroups[index] = string.Format("{0}-pnpres", multiChannelGroups[index]);
                    }
                    else
                    {
                        throw new MissingMemberException("Invalid channelgroup");
                    }
                }
            }
            string presenceChannel = string.Join(",", multiChannelGroups);
            return ChannelGroupGrantAccess(presenceChannel, authenticationKey, read, write, manage, ttl, userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditAccess("", "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditAccess(channelGroup, "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(this.secretKey) || string.IsNullOrEmpty(this.secretKey.Trim()) || this.secretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            Uri request = BuildChannelGroupAuditAccessRequest(channelGroup, authenticationKey);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { };
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new string[] { channelGroup };
            }
			requestState.ResponseType = ResponseType.ChannelGroupAuditAccess;
			requestState.NonSubscribeRegularCallback = userCallback;
			requestState.ErrorCallback = errorCallback;
			requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditPresenceAccess(channelGroup, "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannelGroups = channelGroup.Split(',');
            if (multiChannelGroups.Length > 0)
            {
                for (int index = 0; index < multiChannelGroups.Length; index++)
                {
                    multiChannelGroups[index] = string.Format("{0}-pnpres", multiChannelGroups[index]);
                }
            }
            string presenceChannelGroup = string.Join(",", multiChannelGroups);
            ChannelGroupAuditAccess(presenceChannelGroup, authenticationKey, userCallback, errorCallback);
        }

        #endregion
		#region "Response"

		protected void OnPubnubWebRequestTimeout<T> (object state, bool timeout)
		{
			if (timeout && state != null) 
            {
				RequestState<T> currentState = state as RequestState<T>;
				if (currentState != null) 
                {
					PubnubWebRequest request = currentState.Request;
					if (request != null) 
                    {
						string currentMultiChannel = (currentState.Channels == null) ? "" : string.Join (",", currentState.Channels);
                        string currentMultiChannelGroup = (currentState.ChannelGroups == null) ? "" : string.Join(",", currentState.ChannelGroups);
                        LoggingMethod.WriteToLog(string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel={1} ;channelgroup={2}", DateTime.Now.ToString(), currentMultiChannel, currentMultiChannelGroup), LoggingMethod.LevelInfo);
						currentState.Timeout = true;
						TerminatePendingWebRequest (currentState);
					}
				} 
                else 
                {
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
        protected List<object> WrapResultBasedOnResponseType<T>(ResponseType type, string jsonString, string[] channels, string[] channelGroups, bool reconnect, long lastTimetoken, PubnubWebRequest request, Action<PubnubClientError> errorCallback)
		{
			List<object> result = new List<object> ();

			try {
				string multiChannel = (channels != null) ? string.Join (",", channels) : "";
                string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";
				
                if (!string.IsNullOrEmpty (jsonString)) 
                {
					if (!string.IsNullOrEmpty (jsonString)) 
                    {
						object deSerializedResult = _jsonPluggableLibrary.DeserializeToObject (jsonString);
						List<object> result1 = ((IEnumerable)deSerializedResult).Cast<object> ().ToList ();

						if (result1 != null && result1.Count > 0) {
							result = result1;
						}

						switch (type) {
						case ResponseType.Publish:
							result.Add (multiChannel);
                            if (_addPayloadToPublishResponse && request != null & request.RequestUri != null)
                            {
                                Uri webUri = request.RequestUri;
                                string absolutePath = webUri.AbsolutePath.ToString();
                                int posLastSlash = absolutePath.LastIndexOf("/");
                                if (posLastSlash > 1)
                                {
                                    bool stringType = false;
                                    //object publishMsg = absolutePath.Substring(posLastSlash + 1);
                                    string publishPayload = absolutePath.Substring(posLastSlash + 1);
                                    int posOfStartDQ = publishPayload.IndexOf("%22");
                                    int posOfEndDQ = publishPayload.LastIndexOf("%22");
                                    if (posOfStartDQ == 0 && posOfEndDQ + 3 == publishPayload.Length)
                                    {
                                        publishPayload = publishPayload.Remove(posOfEndDQ).Remove(posOfStartDQ, 3);
                                        stringType = true;
                                    }
                                    string publishMsg = System.Uri.UnescapeDataString(publishPayload);

                                    double doubleData;
                                    int intData;
                                    if (!stringType && int.TryParse(publishMsg, out intData)) //capture numeric data
                                    {
                                        result.Add(intData);
                                    }
                                    else if (!stringType && double.TryParse(publishMsg, out doubleData)) //capture numeric data
                                    {
                                        result.Add(doubleData);
                                    }
                                    else
                                    {
                                        result.Add(publishMsg);
                                    }
                                }
                            }
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
							result = DecodeDecryptLoop (result, channels, channelGroups, errorCallback);
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
                            if (result.Count == 3 && result[0] is object[] && (result[0] as object[]).Length == 0 && result[2].ToString() == "")
                            {
                                result.RemoveAt(2);
                            }
                            if (result.Count == 4 && result[0] is object[] && (result[0] as object[]).Length == 0 && result[2].ToString() == "" && result[3].ToString() == "")
                            {
                                result.RemoveRange(2, 2);
                            }
                            result.Add(multiChannelGroup);
                            result.Add (multiChannel);

                            long receivedTimetoken = (result.Count > 1 && result[1].ToString() != "") ? Convert.ToInt64(result[1].ToString()) : 0;
							
                            long minimumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Min (token => token.Value) : 0;
                            long minimumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Min(token => token.Value) : 0;
                            long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

							long maximumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Max (token => token.Value) : 0;
                            long maximumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Max(token => token.Value) : 0;
                            long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);

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
							Dictionary<string, object> grantDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonString);
							result = new List<object> ();
							result.Add (grantDictionary);
							result.Add (multiChannel);
							break;
                        case ResponseType.ChannelGroupGrantAccess:
                        case ResponseType.ChannelGroupAuditAccess:
                        case ResponseType.ChannelGroupRevokeAccess:
                            Dictionary<string, object> channelGroupPAMDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonString);
                            result = new List<object>();
                            result.Add(channelGroupPAMDictionary);
                            result.Add(multiChannelGroup);
                            break;
                        case ResponseType.GetUserState:
						case ResponseType.SetUserState:
							Dictionary<string, object> userStateDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
							result.Add (userStateDictionary);
                            result.Add(multiChannelGroup);
                            result.Add(multiChannel);
							break;
                        case ResponseType.PushRegister:
                        case ResponseType.PushRemove:
                        case ResponseType.PushGet:
                        case ResponseType.PushUnregister:
							result.Add (multiChannel);
                            break;
                        case ResponseType.ChannelGroupAdd:
                        case ResponseType.ChannelGroupRemove:
                        case ResponseType.ChannelGroupGet:
							Dictionary<string, object> channelGroupDictionary = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonString);
							result = new List<object> ();
                            result.Add(channelGroupDictionary);
                            if (multiChannelGroup != "")
                            {
                                result.Add(multiChannelGroup);
                            }
                            if (multiChannel != "")
                            {
                                result.Add(multiChannel);
                            }
                            break;
						default:
							break;
						}
						;//switch stmt end
					}
				}
			} catch (Exception ex) {
				if (channels != null && channels.Length > 0) 
                {
					if (type == ResponseType.Subscribe || type == ResponseType.Presence) 
                    {
						for (int index = 0; index < channels.Length; index++) 
                        {
							string activeChannel = channels[index].ToString();
							PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
							callbackKey.Channel = activeChannel;
							callbackKey.ResponseType = type;

							if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
								if (type == ResponseType.Presence)
								{
									PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[callbackKey] as PubnubPresenceChannelCallback;
									if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
									{
										CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
											activeChannel, "", currentPubnubCallback.ErrorCallback, ex, null, null);
									}
								}
								else
								{
									PubnubSubscribeChannelCallback<T> currentPubnubCallback = channelCallbacks[callbackKey] as PubnubSubscribeChannelCallback<T>;
									if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
									{
										CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
											activeChannel, "", currentPubnubCallback.ErrorCallback, ex, null, null);
									}
								}
							}
						}
					} 
                    else 
                    {
						if (errorCallback != null) {
							CallErrorCallback (PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                string.Join(",", channels), string.Join(",", channelGroups), errorCallback, ex, null, null);
						}
					}
				}
                if (channelGroups != null && channelGroups.Length > 0)
                {
                    if (type == ResponseType.Subscribe || type == ResponseType.Presence)
                    {
                        for (int index = 0; index < channelGroups.Length; index++)
                        {
                            string activeChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? channelGroups[index].ToString() : "";
                            PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
                            callbackKey.ChannelGroup = activeChannelGroup;
                            callbackKey.ResponseType = type;

                            if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                            {
								if (type == ResponseType.Presence)
								{
									PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubPresenceChannelGroupCallback;
									if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
									{
										CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
											"", activeChannelGroup, currentPubnubCallback.ErrorCallback, ex, null, null);
									}
								}
								else
								{
									PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = channelGroupCallbacks[callbackKey] as PubnubSubscribeChannelGroupCallback<T>;
									if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null)
									{
										CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
											"", activeChannelGroup, currentPubnubCallback.ErrorCallback, ex, null, null);
									}
								}
                            }
                        }
                    }
                    else
                    {
                        if (errorCallback != null)
                        {
                            CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                string.Join(",", channels), string.Join(",", channelGroups), errorCallback, ex, null, null);
                        }
                    }
                }
            }
			return result;
		}

		#endregion

		#region "Build, process and send request"

		protected abstract void ForceCanonicalPathAndQuery (Uri requestUri);

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

		protected abstract void SendRequestAndGetResult<T> (Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request);

		private bool UrlProcessRequest<T> (Uri requestUri, RequestState<T> pubnubRequestState)
		{
			string channel = "";
            string channelGroup = "";
			if (pubnubRequestState != null) 
            {
                if (pubnubRequestState.Channels != null)
                {
                    channel = (pubnubRequestState.Channels.Length > 0) ? string.Join(",", pubnubRequestState.Channels) : ",";
                }
                if (pubnubRequestState.ChannelGroups != null)
                {
                    channelGroup = string.Join(",", pubnubRequestState.ChannelGroups);
                }
            }

			try {
				if (!_channelRequest.ContainsKey (channel) && (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence)) {
					return false;
				}

				// Create Request
				PubnubWebRequestCreator requestCreator = new PubnubWebRequestCreator (_pubnubUnitTest);
				PubnubWebRequest request = (PubnubWebRequest)requestCreator.Create (requestUri);

				request = SetProxy<T> (request);
				request = SetTimeout<T> (pubnubRequestState, request);

				pubnubRequestState.Request = request;

				if (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence) {
					_channelRequest.AddOrUpdate (channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
				}


                //overrideTcpKeepAlive must be true
                if (overrideTcpKeepAlive) {
					TimerWhenOverrideTcpKeepAlive (requestUri, pubnubRequestState);
				} else {
					request = SetServicePointSetTcpKeepAlive (request);
				}
				LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Request={1}", DateTime.Now.ToString (), requestUri.ToString ()), LoggingMethod.LevelInfo);


				SendRequestAndGetResult (requestUri, pubnubRequestState, request);

				return true;
			} 
            catch (System.Exception ex) 
            {
                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
                {
                    if (pubnubRequestState != null && pubnubRequestState.ErrorCallback != null)
                    {
                        string multiChannel = (pubnubRequestState.Channels != null) ? string.Join(",", pubnubRequestState.Channels) : "";
                        string multiChannelGroup = (pubnubRequestState.ChannelGroups != null) ? string.Join(",", pubnubRequestState.ChannelGroups) : "";

                        CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                            multiChannel, multiChannelGroup, pubnubRequestState.ErrorCallback, ex, pubnubRequestState.Request, pubnubRequestState.Response);
                    }
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} Exception={1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
					UrlRequestCommonExceptionHandler<T>(pubnubRequestState.ResponseType, pubnubRequestState.Channels, pubnubRequestState.ChannelGroups, false, pubnubRequestState.SubscribeRegularCallback, pubnubRequestState.PresenceRegularCallback, pubnubRequestState.ConnectCallback, pubnubRequestState.WildcardPresenceCallback, pubnubRequestState.ErrorCallback, false);
                }
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
            
            uuid = EncodeUricomponent(uuid, type, false, false);

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
					url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, false, false));
				} else {
					url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, true, false));
				}
			}

			if (type == ResponseType.Presence || type == ResponseType.Subscribe || type == ResponseType.Leave) 
            {
				queryParamExist = true;
				url.AppendFormat("?uuid={0}", uuid);
				url.Append(subscribeParameters);
				if (!string.IsNullOrEmpty(_authenticationKey)) {
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
				}
				if (_pubnubPresenceHeartbeatInSeconds != 0) {
					url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
				}
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
			}
			else if (type == ResponseType.PresenceHeartbeat) 
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
					url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
				}
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
			}
			else if (type == ResponseType.SetUserState) 
            {
				queryParamExist = true;
				url.Append(setUserStateParameters);
                url.AppendFormat("&uuid={0}", uuid);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
				}
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
			}
			else if (type == ResponseType.GetUserState) 
            {
                queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                url.Append(getUserStateParameters);
                if (!string.IsNullOrEmpty(_authenticationKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                }
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));

			}
            else if (type == ResponseType.Here_Now) 
            {
                queryParamExist = true;
                url.Append(hereNowParameters);
                url.AppendFormat("&uuid={0}", uuid);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
				}
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
			}
			else if (type == ResponseType.GlobalHere_Now) 
            {
                queryParamExist = true;
                url.Append(globalHereNowParameters);
                url.AppendFormat("&uuid={0}", uuid);
				if (!string.IsNullOrEmpty(_authenticationKey)) 
                {
					url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
				}
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
			}
			else if (type == ResponseType.Where_Now) 
            {
                queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                if (!string.IsNullOrEmpty(_authenticationKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                }
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.Publish) 
            {
				queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                if (parameters != "")
                {
                    url.AppendFormat("&{0}", parameters);
                }
                if (!string.IsNullOrEmpty(_authenticationKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                }
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
            {
                queryParamExist = true;
                switch (type)
                {
                    case ResponseType.PushRegister:
                        url.Append(pushRegisterDeviceParameters);
                        break;
                    case ResponseType.PushRemove:
                        url.Append(pushRemoveChannelParameters);
                        break;
                    case ResponseType.PushUnregister:
                        url.Append(pushUnregisterDeviceParameters);
                        break;
                    default:
                        url.Append(pushGetChannelsParameters);
                        break;
                }
                url.AppendFormat("&uuid={0}", uuid);
                if (!string.IsNullOrEmpty(_authenticationKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                }
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.ChannelGroupAdd || type == ResponseType.ChannelGroupRemove || type == ResponseType.ChannelGroupGet)
            {
                queryParamExist = true;
                switch (type)
                {
                    case ResponseType.ChannelGroupAdd:
                        url.Append(channelGroupAddParameters);
                        break;
                    case ResponseType.ChannelGroupRemove:
                        url.Append(channelGroupRemoveParameters);
                        break;
                    case ResponseType.ChannelGroupGet:
                        break;
                    default:
                        break;
                }
            }
            else if (type == ResponseType.DetailedHistory 
                || type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess
                || type == ResponseType.ChannelGroupGrantAccess || type == ResponseType.ChannelGroupAuditAccess || type == ResponseType.ChannelGroupRevokeAccess)
            {
                url.Append(parameters);
                queryParamExist = true;
            }

            if (!queryParamExist)
            {
                url.AppendFormat("?uuid={0}", uuid);
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }


			Uri requestUri = new Uri (url.ToString());

			if (type == ResponseType.Publish || type == ResponseType.Subscribe || type == ResponseType.Presence)
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
		protected IPubnubUnitTest pubnubUnitTest = null;

		public PubnubWebRequestCreatorBase ()
		{
		}

		public PubnubWebRequestCreatorBase (IPubnubUnitTest pubnubUnitTest)
		{
			this.pubnubUnitTest = pubnubUnitTest;
		}

        protected abstract HttpWebRequest SetNoCache(HttpWebRequest req, bool nocache);

        protected abstract WebRequest CreateRequest(Uri uri, bool keepAliveRequest, bool nocache);

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

						PubnubClientError error = new PubnubClientError (pubnubStatusCode, PubnubErrorSeverity.Critical, true, webEx.Message, webEx, PubnubMessageSource.Client, null, null, errorDescription, "","");
						GoToCallback (error, errorCallback);
					}
				} catch (Exception ex) {
					if (errorCallback != null) {
						filterErrorLevel = errorLevel;
						PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);
						int statusCode = (int)errorType;
						string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);
						PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Critical, true, ex.Message, ex, PubnubMessageSource.Client, null, null, errorDescription, "","");
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

	public abstract class PubnubWebResponseBase : WebResponse
	{
		protected WebResponse response;
		readonly Stream _responseStream;
		HttpStatusCode httpStatusCode;

		public PubnubWebResponseBase (WebResponse response)
		{
			this.response = response;
		}

		public PubnubWebResponseBase (WebResponse response, HttpStatusCode statusCode)
		{
			this.response = response;
			this.httpStatusCode = statusCode;
		}

		public PubnubWebResponseBase (Stream responseStream)
		{
			_responseStream = responseStream;
		}

        public PubnubWebResponseBase(Stream responseStream, HttpStatusCode statusCode)
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

		T DeserializeToObject<T>(string jsonString);

		T DeserializeToObject<T>(List<object> listObject);

		Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString);

        Dictionary<string, object> ConvertToDictionaryObject(object localContainer);

        Dictionary<string, object>[] ConvertToDictionaryObjectArray(object localContainer);

        object[] ConvertToObjectArray(object localContainer);

		void PopulateObject(string value, object target);
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
            bool ret = false;

			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars (jsonString);
			var reader = new JsonFx.Json.JsonReader ();
			var output = reader.Read<object> (jsonString);
			Type valueType = null;
			valueType = output.GetType ();
			var expectedType = typeof(System.Dynamic.ExpandoObject);
            if (expectedType.IsAssignableFrom(valueType))
            {
                ret = true;
            }

            return ret;
		}

		public string SerializeToJsonString (object objectToSerialize)
		{
			#if(__MonoCS__)
			var writer = new JsonFx.Json.JsonWriter ();
			string json = writer.Write (objectToSerialize);
			return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
			#else
            
			string json = "";
			var resolver = new JsonFx.Serialization.Resolvers.CombinedResolverStrategy(
                new JsonFx.Json.Resolvers.JsonResolverStrategy(),
                new JsonFx.Serialization.Resolvers.DataContractResolverStrategy()
                );

            //JsonFx.Serialization.DataWriterSettings dataWriterSettings = new JsonFx.Serialization.DataWriterSettings(resolver);
            //var writer = new JsonFx.Json.JsonWriter(dataWriterSettings, new string[] { "PubnubClientError" });
            var writer = new JsonFx.Json.JsonWriter();
            try
            {
                json = writer.Write(objectToSerialize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

			return json;
			#endif
		}

		public List<object> DeserializeToListOfObject (string jsonString)
		{
			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars(jsonString);
			var reader = new JsonFx.Json.JsonReader ();
			var output = reader.Read<List<object>> (jsonString);
			return output;
		}

		public object DeserializeToObject (string jsonString)
		{
			jsonString = PubnubCryptoBase.ConvertHexToUnicodeChars(jsonString);
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

        public Dictionary<string, object> ConvertToDictionaryObject(object localContainer)
        {
            Dictionary<string, object> ret = null;

            if (localContainer != null && localContainer.GetType().ToString() == "System.Dynamic.ExpandoObject")
            {
                IDictionary<string, object> iDictionary = localContainer as IDictionary<string, object>;
                ret = iDictionary.ToDictionary(item => item.Key, item => item.Value);
            }

            return ret;
        }

        public Dictionary<string, object>[] ConvertToDictionaryObjectArray(object localContainer)
        {
            Dictionary<string, object>[] ret = null;

            if (localContainer != null && localContainer.GetType().ToString() == "System.Dynamic.ExpandoObject[]")
            {
                IDictionary<string, object>[] iDictionary = localContainer as IDictionary<string, object>[];
                if (iDictionary != null && iDictionary.Length > 0)
                {
                    ret = new Dictionary<string, object>[iDictionary.Length];

                    for(int index=0; index < iDictionary.Length; index++)
                    {
                        IDictionary<string, object> iItem = iDictionary[index];
                        ret[index] = iItem.ToDictionary(item => item.Key, item => item.Value);
                    }
                }
            }

            return ret;
        }

        public object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            if (localContainer != null)
            {
                ret = localContainer as object[];
                if (ret == null)
                {
                    if (localContainer.GetType().IsArray)
                    {
                        switch (localContainer.GetType().GetElementType().FullName)
                        {
                            case "System.Int32":
                                int[] intArray = localContainer as int[];
                                ret = new object[intArray.Length];
                                Array.Copy(intArray, ret, intArray.Length);
                                break;
                            case "System.Int64":
                                Int64[] int64Array = localContainer as Int64[];
                                ret = new object[int64Array.Length];
                                Array.Copy(int64Array, ret, int64Array.Length);
                                break;
                            case "System.Double":
                                double[] doubleArray = localContainer as double[];
                                ret = new object[doubleArray.Length];
                                Array.Copy(doubleArray, ret, doubleArray.Length);
                                break;
                            case "System.Decimal":
                                decimal[] decimalArray = localContainer as decimal[];
                                ret = new object[decimalArray.Length];
                                Array.Copy(decimalArray, ret, decimalArray.Length);
                                break;
                            case "System.Single":
                                float[] floatArray = localContainer as float[];
                                ret = new object[floatArray.Length];
                                Array.Copy(floatArray, ret, floatArray.Length);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return ret;
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
        private bool IsValidJson(string jsonString)
        {
            bool ret = false;
            try
            {
                JObject.Parse(jsonString);
                ret = true;
            }
            catch { }
            return ret;
        }

        public bool IsArrayCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
                while (reader.Read())
                {
                    if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartArray)
                    {
                        ret = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        public bool IsDictionaryCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
                while (reader.Read())
                {
                    if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartObject)
                    {
                        ret = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        public string SerializeToJsonString(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        public List<object> DeserializeToListOfObject(string jsonString)
        {
            List<object> result = JsonConvert.DeserializeObject<List<object>>(jsonString);

            return result;
        }

        public object DeserializeToObject(string jsonString)
        {
            object result = JsonConvert.DeserializeObject<object>(jsonString);
            if (result.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                JArray jarrayResult = result as JArray;
                List<object> objectContainer = jarrayResult.ToObject<List<object>>();
                if (objectContainer != null && objectContainer.Count > 0)
                {
                    for (int index = 0; index < objectContainer.Count; index++)
                    {
                        if (objectContainer[index].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                        {
                            JArray internalItem = objectContainer[index] as JArray;
                            objectContainer[index] = internalItem.Select(item => (object)item).ToArray();
                        }
                    }
                    result = objectContainer;
                }
            }
            return result;
        }

        public void PopulateObject(string value, object target)
        {
            JsonConvert.PopulateObject(value, target);
        }

        public virtual T DeserializeToObject<T>(string jsonString)
        {
            T ret = default(T);

            try
            {
                ret = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch { }

            return ret;
        }

        public virtual T DeserializeToObject<T>(List<object> listObject)
        {
            T ret = default(T);

            if (listObject == null)
            {
                return ret;
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Message<>))
            {
                #region "Subscribe Message<>"
                Type dataType = typeof(T).GetGenericArguments()[0];
                Type generic = typeof(Message<>);
                Type specific = generic.MakeGenericType(dataType);

                //ConstructorInfo ci = specific.GetConstructor(Type.EmptyTypes);
                ConstructorInfo ci = specific.GetConstructors().FirstOrDefault();
                if (ci != null)
                {
                    object message = ci.Invoke(new object[] { });

                    //Set data
                    PropertyInfo dataProp = specific.GetProperty("Data");

                    object userMessage = null;
                    if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                    {
                        JValue jValue = listObject[0] as JValue;
                        userMessage = jValue.Value;

                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                    {
                        JToken token = listObject[0] as JToken;
                        if (dataProp.PropertyType == typeof(string))
                        {
                            userMessage = JsonConvert.SerializeObject(token);
                        }
                        else
                        {
                            userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());
                        }

                        //userMessage = ConvertJTokenToObject(listObject[0] as JToken);
                        //userMessage = Activator.CreateInstance(
                        //PopulateObject(listObject[0].ToString(), message);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        JToken token = listObject[0] as JToken;
                        userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());

                        //userMessage = ConvertJTokenToObject(listObject[0] as JToken);
                        //userMessage = Activator.CreateInstance(
                        //PopulateObject(listObject[0].ToString(), message);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(System.String))
                    {
                        userMessage = listObject[0] as string;
                        dataProp.SetValue(message, userMessage, null);
                    }

                    //Set Time
                    PropertyInfo timeProp = specific.GetProperty("Time");
                    timeProp.SetValue(message, Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(listObject[1].ToString()), null);

                    // Set ChannelName
                    PropertyInfo channelNameProp = specific.GetProperty("ChannelName");
                    channelNameProp.SetValue(message, (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString(), null);

                    PropertyInfo typeProp = specific.GetProperty("Type");
                    typeProp.SetValue(message, dataType, null);

                    ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
                }
                #endregion
            }
            else if (typeof(T) == typeof(GrantAck))
            {
                #region "GrantAck"
                Dictionary<string, object> grantDicObj = ConvertToDictionaryObject(listObject[0]);

                GrantAck ack = null;

                int statusCode = 0; //For Grant, status code 200 = success

                if (grantDicObj != null)
                {
                    ack = new GrantAck();

                    if (int.TryParse(grantDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = grantDicObj["message"].ToString();

                    ack.Service = grantDicObj["service"].ToString();

                    if (grantDicObj.ContainsKey("warning"))
                    {
                        ack.Warning = Convert.ToBoolean(grantDicObj["warning"].ToString());
                    }

                    ack.Payload = new GrantAck.Data();

                    if (grantDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> grantAckPayloadDic = ConvertToDictionaryObject(grantDicObj["payload"]);
                        if (grantAckPayloadDic != null && grantAckPayloadDic.Count > 0)
                        {
                            if (grantAckPayloadDic.ContainsKey("level"))
                            {
                                ack.Payload.Level = grantAckPayloadDic["level"].ToString();
                            }

                            if (grantAckPayloadDic.ContainsKey("subscribe_key"))
                            {
                                ack.Payload.SubscribeKey = grantAckPayloadDic["subscribe_key"].ToString();
                            }

                            if (grantAckPayloadDic.ContainsKey("ttl"))
                            {
                                ack.Payload.TTL = Convert.ToInt32(grantAckPayloadDic["ttl"].ToString());
                            }

                            if (ack.Payload != null && ack.Payload.Level != null && ack.Payload.Level == "subkey")
                            {
                                ack.Payload.Access = new GrantAck.Data.SubkeyAccess();
                                ack.Payload.Access.read = grantAckPayloadDic["r"].ToString() == "1";
                                ack.Payload.Access.write = grantAckPayloadDic["w"].ToString() == "1";
                                ack.Payload.Access.manage = grantAckPayloadDic["m"].ToString() == "1";
                            }
                            else
                            {
                                if (grantAckPayloadDic.ContainsKey("channels"))
                                {
                                    ack.Payload.channels = new Dictionary<string, GrantAck.Data.ChannelData>();

                                    Dictionary<string, object> grantAckChannelListDic = ConvertToDictionaryObject(grantAckPayloadDic["channels"]);
                                    if (grantAckChannelListDic != null && grantAckChannelListDic.Count > 0)
                                    {
                                        foreach (string channel in grantAckChannelListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelDataDic = ConvertToDictionaryObject(grantAckChannelListDic[channel]);
                                            if (grantAckChannelDataDic != null && grantAckChannelDataDic.Count > 0)
                                            {
                                                GrantAck.Data.ChannelData grantAckChannelData = new GrantAck.Data.ChannelData();
                                                if (grantAckChannelDataDic.ContainsKey("auths"))
                                                {
                                                    grantAckChannelData.auths = new Dictionary<string, GrantAck.Data.ChannelData.AuthData>();

                                                    Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckChannelDataDic["auths"]);
                                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                            {
                                                                GrantAck.Data.ChannelData.AuthData authData = new GrantAck.Data.ChannelData.AuthData();
                                                                authData.Access = new GrantAck.Data.ChannelData.AuthData.AuthAccess();
                                                                authData.Access.read = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                                authData.Access.write = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                                authData.Access.manage = grantAckChannelAuthDataDic["m"].ToString() == "1";

                                                                grantAckChannelData.auths.Add(authKey, authData);
                                                            }

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    grantAckChannelData.Access = new GrantAck.Data.ChannelData.ChannelAccess();
                                                    grantAckChannelData.Access.read = grantAckChannelDataDic["r"].ToString() == "1";
                                                    grantAckChannelData.Access.write = grantAckChannelDataDic["w"].ToString() == "1";
                                                    grantAckChannelData.Access.manage = grantAckChannelDataDic["m"].ToString() == "1";
                                                }

                                                ack.Payload.channels.Add(channel, grantAckChannelData);
                                            }
                                        }
                                    }
                                }//end of if channels
                                else if (grantAckPayloadDic.ContainsKey("channel"))
                                {
                                    ack.Payload.channels = new Dictionary<string, GrantAck.Data.ChannelData>();

                                    string channelName = grantAckPayloadDic["channel"].ToString();
                                    if (grantAckPayloadDic.ContainsKey("auths"))
                                    {
                                        GrantAck.Data.ChannelData grantAckChannelData = new GrantAck.Data.ChannelData();

                                        grantAckChannelData.auths = new Dictionary<string, GrantAck.Data.ChannelData.AuthData>();

                                        Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckPayloadDic["auths"]);
                                        if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                {
                                                    GrantAck.Data.ChannelData.AuthData authData = new GrantAck.Data.ChannelData.AuthData();
                                                    authData.Access = new GrantAck.Data.ChannelData.AuthData.AuthAccess();
                                                    authData.Access.read = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                    authData.Access.write = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                    authData.Access.manage = grantAckChannelAuthDataDic["m"].ToString() == "1";

                                                    grantAckChannelData.auths.Add(authKey, authData);
                                                }

                                            }
                                            ack.Payload.channels.Add(channelName, grantAckChannelData);
                                        }
                                    }
                                }

                                if (grantAckPayloadDic.ContainsKey("channel-groups"))
                                {
                                    ack.Payload.channelgroups = new Dictionary<string, GrantAck.Data.ChannelGroupData>();

                                    Dictionary<string, object> grantAckCgListDic = ConvertToDictionaryObject(grantAckPayloadDic["channel-groups"]);
                                    if (grantAckCgListDic != null && grantAckCgListDic.Count > 0)
                                    {
                                        foreach (string channelgroup in grantAckCgListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckCgDataDic = ConvertToDictionaryObject(grantAckCgListDic[channelgroup]);
                                            if (grantAckCgDataDic != null && grantAckCgDataDic.Count > 0)
                                            {
                                                GrantAck.Data.ChannelGroupData grantAckCgData = new GrantAck.Data.ChannelGroupData();
                                                if (grantAckCgDataDic.ContainsKey("auths"))
                                                {
                                                    grantAckCgData.auths = new Dictionary<string, GrantAck.Data.ChannelGroupData.AuthData>();

                                                    Dictionary<string, object> grantAckCgAuthListDic = ConvertToDictionaryObject(grantAckCgDataDic["auths"]);
                                                    if (grantAckCgAuthListDic != null && grantAckCgAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckCgAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckCgAuthDataDic = ConvertToDictionaryObject(grantAckCgAuthListDic[authKey]);
                                                            if (grantAckCgAuthDataDic != null && grantAckCgAuthDataDic.Count > 0)
                                                            {
                                                                GrantAck.Data.ChannelGroupData.AuthData authData = new GrantAck.Data.ChannelGroupData.AuthData();
                                                                authData.Access = new GrantAck.Data.ChannelGroupData.AuthData.AuthAccess();
                                                                authData.Access.read = grantAckCgAuthDataDic["r"].ToString() == "1";
                                                                authData.Access.write = grantAckCgAuthDataDic["w"].ToString() == "1";
                                                                authData.Access.manage = grantAckCgAuthDataDic["m"].ToString() == "1";

                                                                grantAckCgData.auths.Add(authKey, authData);
                                                            }

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    grantAckCgData.Access = new GrantAck.Data.ChannelGroupData.ChannelGroupAccess();
                                                    grantAckCgData.Access.read = grantAckCgDataDic["r"].ToString() == "1";
                                                    grantAckCgData.Access.write = grantAckCgDataDic["w"].ToString() == "1";
                                                    grantAckCgData.Access.manage = grantAckCgDataDic["m"].ToString() == "1";
                                                }

                                                ack.Payload.channelgroups.Add(channelgroup, grantAckCgData);
                                            }
                                        }
                                    }
                                }//end of if channel-groups
                            } //end of else subkey

                        }

                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(GrantAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(AuditAck))
            {
                #region "AuditAck"
                Dictionary<string, object> auditDicObj = ConvertToDictionaryObject(listObject[0]);

                AuditAck ack = null;

                int statusCode = 0; //For Audit, status code 200 = success

                if (auditDicObj != null)
                {
                    ack = new AuditAck();

                    if (int.TryParse(auditDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = auditDicObj["message"].ToString();

                    ack.Service = auditDicObj["service"].ToString();

                    if (auditDicObj.ContainsKey("warning"))
                    {
                        ack.Warning = Convert.ToBoolean(auditDicObj["warning"].ToString());
                    }

                    ack.Payload = new AuditAck.Data();

                    //AuditAckPayload auditAckPayload = DeserializeToObject<AuditAckPayload>(ack.Payload);
                    if (auditDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> auditAckPayloadDic = ConvertToDictionaryObject(auditDicObj["payload"]);
                        if (auditAckPayloadDic != null && auditAckPayloadDic.Count > 0)
                        {
                            if (auditAckPayloadDic.ContainsKey("level"))
                            {
                                ack.Payload.Level = auditAckPayloadDic["level"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("subscribe_key"))
                            {
                                ack.Payload.SubscribeKey = auditAckPayloadDic["subscribe_key"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, AuditAck.Data.ChannelData>();

                                Dictionary<string, object> auditAckChannelListDic = ConvertToDictionaryObject(auditAckPayloadDic["channels"]);
                                if (auditAckChannelListDic != null && auditAckChannelListDic.Count > 0)
                                {
                                    foreach (string channel in auditAckChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> auditAckChannelDataDic = ConvertToDictionaryObject(auditAckChannelListDic[channel]);
                                        if (auditAckChannelDataDic != null && auditAckChannelDataDic.Count > 0)
                                        {
                                            AuditAck.Data.ChannelData auditAckChannelData = new AuditAck.Data.ChannelData();
                                            if (auditAckChannelDataDic.ContainsKey("auths"))
                                            {
                                                auditAckChannelData.auths = new Dictionary<string, AuditAck.Data.ChannelData.AuthData>();

                                                Dictionary<string, object> auditAckChannelAuthListDic = ConvertToDictionaryObject(auditAckChannelDataDic["auths"]);
                                                if (auditAckChannelAuthListDic != null && auditAckChannelAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in auditAckChannelAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> auditAckChannelAuthDataDic = ConvertToDictionaryObject(auditAckChannelAuthListDic[authKey]);
                                                        if (auditAckChannelAuthDataDic != null && auditAckChannelAuthDataDic.Count > 0)
                                                        {
                                                            AuditAck.Data.ChannelData.AuthData authData = new AuditAck.Data.ChannelData.AuthData();
                                                            authData.Access = new AuditAck.Data.ChannelData.AuthData.AuthAccess();
                                                            authData.Access.read = auditAckChannelAuthDataDic["r"].ToString() == "1";
                                                            authData.Access.write = auditAckChannelAuthDataDic["w"].ToString() == "1";
                                                            authData.Access.manage = auditAckChannelAuthDataDic.ContainsKey("m") ? auditAckChannelAuthDataDic["m"].ToString() == "1" : false;
                                                            if (auditAckChannelAuthDataDic.ContainsKey("ttl"))
                                                            {
                                                                authData.Access.TTL = Int32.Parse(auditAckChannelAuthDataDic["ttl"].ToString());
                                                            }

                                                            auditAckChannelData.auths.Add(authKey, authData);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                auditAckChannelData.Access = new AuditAck.Data.ChannelData.ChannelAccess();
                                                auditAckChannelData.Access.read = auditAckChannelDataDic["r"].ToString() == "1";
                                                auditAckChannelData.Access.write = auditAckChannelDataDic["w"].ToString() == "1";
                                                auditAckChannelData.Access.manage = auditAckChannelDataDic.ContainsKey("m") ? auditAckChannelDataDic["m"].ToString() == "1" : false;
                                                if (auditAckChannelDataDic.ContainsKey("ttl"))
                                                {
                                                    auditAckChannelData.Access.TTL = Int32.Parse(auditAckChannelDataDic["ttl"].ToString());
                                                }
                                            }

                                            ack.Payload.channels.Add(channel, auditAckChannelData);
                                        }
                                    }
                                }
                            }//end of if channels
                            if (auditAckPayloadDic.ContainsKey("channel-groups"))
                            {
                                ack.Payload.channelgroups = new Dictionary<string, AuditAck.Data.ChannelGroupData>();

                                Dictionary<string, object> auditAckCgListDic = ConvertToDictionaryObject(auditAckPayloadDic["channel-groups"]);
                                if (auditAckCgListDic != null && auditAckCgListDic.Count > 0)
                                {
                                    foreach (string channelgroup in auditAckCgListDic.Keys)
                                    {
                                        Dictionary<string, object> auditAckCgDataDic = ConvertToDictionaryObject(auditAckCgListDic[channelgroup]);
                                        if (auditAckCgDataDic != null && auditAckCgDataDic.Count > 0)
                                        {
                                            AuditAck.Data.ChannelGroupData auditAckCgData = new AuditAck.Data.ChannelGroupData();
                                            if (auditAckCgDataDic.ContainsKey("auths"))
                                            {
                                                auditAckCgData.auths = new Dictionary<string, AuditAck.Data.ChannelGroupData.AuthData>();

                                                Dictionary<string, object> auditAckCgAuthListDic = ConvertToDictionaryObject(auditAckCgDataDic["auths"]);
                                                if (auditAckCgAuthListDic != null && auditAckCgAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in auditAckCgAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> auditAckCgAuthDataDic = ConvertToDictionaryObject(auditAckCgAuthListDic[authKey]);
                                                        if (auditAckCgAuthDataDic != null && auditAckCgAuthDataDic.Count > 0)
                                                        {
                                                            AuditAck.Data.ChannelGroupData.AuthData authData = new AuditAck.Data.ChannelGroupData.AuthData();
                                                            authData.Access = new AuditAck.Data.ChannelGroupData.AuthData.AuthAccess();
                                                            authData.Access.read = auditAckCgAuthDataDic["r"].ToString() == "1";
                                                            authData.Access.write = auditAckCgAuthDataDic["w"].ToString() == "1";
                                                            authData.Access.manage = auditAckCgAuthDataDic.ContainsKey("m") ? auditAckCgAuthDataDic["m"].ToString() == "1" : false;
                                                            if (auditAckCgAuthDataDic.ContainsKey("ttl"))
                                                            {
                                                                authData.Access.TTL = Int32.Parse(auditAckCgAuthDataDic["ttl"].ToString());
                                                            }

                                                            auditAckCgData.auths.Add(authKey, authData);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                auditAckCgData.Access = new AuditAck.Data.ChannelGroupData.ChannelGroupAccess();
                                                auditAckCgData.Access.read = auditAckCgDataDic["r"].ToString() == "1";
                                                auditAckCgData.Access.write = auditAckCgDataDic["w"].ToString() == "1";
                                                auditAckCgData.Access.manage = auditAckCgDataDic.ContainsKey("m") ? auditAckCgDataDic["m"].ToString() == "1" : false;
                                                if (auditAckCgDataDic.ContainsKey("ttl"))
                                                {
                                                    auditAckCgData.Access.TTL = Int32.Parse(auditAckCgDataDic["ttl"].ToString());
                                                }
                                            }

                                            ack.Payload.channelgroups.Add(channelgroup, auditAckCgData);
                                        }
                                    }
                                }
                            }//end of if channel-groups

                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(AuditAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(ConnectOrDisconnectAck))
            {
                #region "ConnectOrDisconnectAck"
                var ack = new ConnectOrDisconnectAck
                {
                    StatusMessage = listObject[1].ToString(),
                    ChannelGroupName = (listObject.Count == 4) ? listObject[2].ToString() : "",
                    ChannelName = (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString()
                };
                int statusCode;
                if (int.TryParse(listObject[0].ToString(), out statusCode))
                    ack.StatusCode = statusCode;

                ret = (T)Convert.ChangeType(ack, typeof(ConnectOrDisconnectAck), CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(PublishAck))
            {
                var ack = new PublishAck
                {
                    StatusMessage = listObject[1].ToString(),
                    Timetoken = Int64.Parse(listObject[2].ToString()),
                    ChannelName = listObject[3].ToString(),
                    Payload = (listObject.Count == 5) ? listObject[4] : null
                };
                int statusCode;
                if (int.TryParse(listObject[0].ToString(), out statusCode))
                    ack.StatusCode = statusCode;

                ret = (T)Convert.ChangeType(ack, typeof(PublishAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(PresenceAck))
            {
                #region "PresenceAck"
                Dictionary<string, object> presenceDicObj = ConvertToDictionaryObject(listObject[0]);

                PresenceAck ack = null;

                if (presenceDicObj != null)
                {
                    ack = new PresenceAck();
                    ack.Action = presenceDicObj["action"].ToString();
                    ack.Timestamp = Convert.ToInt64(presenceDicObj["timestamp"].ToString());
                    ack.UUID = presenceDicObj["uuid"].ToString();
                    ack.Occupancy = Int32.Parse(presenceDicObj["occupancy"].ToString());

                    //ack.Timetoken = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(listObject[1].ToString()),
                    ack.Timetoken = Convert.ToInt64(listObject[1].ToString());
                    ack.ChannelGroupName = (listObject.Count == 4) ? listObject[2].ToString() : "";
                    ack.ChannelName = (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString();
                }


                ret = (T)Convert.ChangeType(ack, typeof(PresenceAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(DetailedHistoryAck))
            {
                #region "DetailedHistoryAck"
                DetailedHistoryAck ack = new DetailedHistoryAck();
                ack.StartTimeToken = Convert.ToInt64(listObject[1].ToString());
                ack.EndTimeToken = Convert.ToInt64(listObject[2].ToString());
                ack.ChannelName = listObject[3].ToString();
                ack.Message = ConvertToObjectArray(listObject[0]);

                ret = (T)Convert.ChangeType(ack, typeof(DetailedHistoryAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(HereNowAck))
            {
                #region "HereNowAck"
                Dictionary<string, object> herenowDicObj = ConvertToDictionaryObject(listObject[0]);

                HereNowAck ack = null;

                int statusCode = 0;

                if (herenowDicObj != null)
                {
                    ack = new HereNowAck();

                    if (int.TryParse(herenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = herenowDicObj["message"].ToString();

                    ack.Service = herenowDicObj["service"].ToString();

                    ack.ChannelName = listObject[1].ToString();

                    ack.Payload = new HereNowAck.Data();

                    if (herenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> hereNowPayloadDic = ConvertToDictionaryObject(herenowDicObj["payload"]);
                        if (hereNowPayloadDic != null && hereNowPayloadDic.Count > 0)
                        {
                            ack.Payload.total_occupancy = Int32.Parse(hereNowPayloadDic["total_occupancy"].ToString());
                            ack.Payload.total_channels = Int32.Parse(hereNowPayloadDic["total_channels"].ToString());
                            if (hereNowPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, HereNowAck.Data.ChannelData>();

                                Dictionary<string, object> hereNowChannelListDic = ConvertToDictionaryObject(hereNowPayloadDic["channels"]);
                                if (hereNowChannelListDic != null && hereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in hereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> hereNowChannelItemDic = ConvertToDictionaryObject(hereNowChannelListDic[channel]);
                                        if (hereNowChannelItemDic != null && hereNowChannelItemDic.Count > 0)
                                        {
                                            HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                            channelData.occupancy = Convert.ToInt32(hereNowChannelItemDic["occupancy"].ToString());
                                            if (hereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] hereNowChannelUuidList = ConvertToObjectArray(hereNowChannelItemDic["uuids"]);
                                                if (hereNowChannelUuidList != null && hereNowChannelUuidList.Length > 0)
                                                {
                                                    List<HereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<HereNowAck.Data.ChannelData.UuidData>();

                                                    for (int index = 0; index < hereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (hereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                                            uuidData.uuid = hereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(hereNowChannelUuidList[index]);
                                                            if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                                                uuidData.uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                                                if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.state = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
                                                                }
                                                                uuidDataList.Add(uuidData);
                                                            }
                                                        }
                                                    }
                                                    channelData.uuids = uuidDataList.ToArray();
                                                }
                                            }
                                            ack.Payload.channels.Add(channel, channelData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (herenowDicObj.ContainsKey("occupancy"))
                    {
                        ack.Payload.total_occupancy = Int32.Parse(herenowDicObj["occupancy"].ToString());
                        ack.Payload.channels = new Dictionary<string, HereNowAck.Data.ChannelData>();
                        if (herenowDicObj.ContainsKey("uuids"))
                        {
                            object[] uuidArray = ConvertToObjectArray(herenowDicObj["uuids"]);
                            if (uuidArray != null && uuidArray.Length > 0)
                            {
                                List<HereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<HereNowAck.Data.ChannelData.UuidData>();
                                for (int index = 0; index < uuidArray.Length; index++)
                                {
                                    Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(uuidArray[index]);
                                    if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                    {
                                        HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                        uuidData.uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                        if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                        {
                                            uuidData.state = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
                                        }
                                        uuidDataList.Add(uuidData);
                                    }
                                    else
                                    {
                                        HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                        uuidData.uuid = uuidArray[index].ToString();
                                        uuidDataList.Add(uuidData);
                                    }
                                }
                                HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                channelData.uuids = uuidDataList.ToArray();
                                channelData.occupancy = ack.Payload.total_occupancy;

                                ack.Payload.channels.Add(ack.ChannelName, channelData);
                                ack.Payload.total_channels = ack.Payload.channels.Count;
                            }
                        }
                        else
                        {
                            string channels = listObject[1].ToString();
                            string[] arrChannel = channels.Split(',');
                            int totalChannels = 0;
                            foreach (string channel in arrChannel)
                            {
                                HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                channelData.occupancy = 1;
                                ack.Payload.channels.Add(channel, channelData);
                                totalChannels++;
                            }
                            ack.Payload.total_channels = totalChannels;


                        }
                    }

                }

                ret = (T)Convert.ChangeType(ack, typeof(HereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GlobalHereNowAck))
            {
                #region "GlobalHereNowAck"
                Dictionary<string, object> globalHerenowDicObj = ConvertToDictionaryObject(listObject[0]);

                GlobalHereNowAck ack = null;

                int statusCode = 0;

                if (globalHerenowDicObj != null)
                {
                    ack = new GlobalHereNowAck();

                    if (int.TryParse(globalHerenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = globalHerenowDicObj["message"].ToString();

                    ack.Service = globalHerenowDicObj["service"].ToString();

                    ack.Payload = new GlobalHereNowAck.Data();
                    if (globalHerenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> globalHereNowPayloadDic = ConvertToDictionaryObject(globalHerenowDicObj["payload"]);
                        if (globalHereNowPayloadDic != null && globalHereNowPayloadDic.Count > 0)
                        {
                            ack.Payload.total_occupancy = Int32.Parse(globalHereNowPayloadDic["total_occupancy"].ToString());
                            ack.Payload.total_channels = Int32.Parse(globalHereNowPayloadDic["total_channels"].ToString());
                            if (globalHereNowPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, GlobalHereNowAck.Data.ChannelData>();

                                Dictionary<string, object> globalHereNowChannelListDic = ConvertToDictionaryObject(globalHereNowPayloadDic["channels"]);
                                if (globalHereNowChannelListDic != null && globalHereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in globalHereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> globalHereNowChannelItemDic = ConvertToDictionaryObject(globalHereNowChannelListDic[channel]);
                                        if (globalHereNowChannelItemDic != null && globalHereNowChannelItemDic.Count > 0)
                                        {
                                            GlobalHereNowAck.Data.ChannelData channelData = new GlobalHereNowAck.Data.ChannelData();
                                            channelData.occupancy = Convert.ToInt32(globalHereNowChannelItemDic["occupancy"].ToString());
                                            if (globalHereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] globalHereNowChannelUuidList = ConvertToObjectArray(globalHereNowChannelItemDic["uuids"]);
                                                if (globalHereNowChannelUuidList != null && globalHereNowChannelUuidList.Length > 0)
                                                {
                                                    List<GlobalHereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<GlobalHereNowAck.Data.ChannelData.UuidData>();

                                                    for (int index = 0; index < globalHereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (globalHereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            GlobalHereNowAck.Data.ChannelData.UuidData uuidData = new GlobalHereNowAck.Data.ChannelData.UuidData();
                                                            uuidData.uuid = globalHereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> globalHereNowChannelItemUuidsDic = ConvertToDictionaryObject(globalHereNowChannelUuidList[index]);
                                                            if (globalHereNowChannelItemUuidsDic != null && globalHereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                GlobalHereNowAck.Data.ChannelData.UuidData uuidData = new GlobalHereNowAck.Data.ChannelData.UuidData();
                                                                uuidData.uuid = globalHereNowChannelItemUuidsDic["uuid"].ToString();
                                                                if (globalHereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.state = ConvertToDictionaryObject(globalHereNowChannelItemUuidsDic["state"]);
                                                                }
                                                                uuidDataList.Add(uuidData);
                                                            }
                                                        }
                                                    }
                                                    channelData.uuids = uuidDataList.ToArray();
                                                }
                                            }
                                            ack.Payload.channels.Add(channel, channelData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(GlobalHereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(WhereNowAck))
            {
                #region "WhereNowAck"
                Dictionary<string, object> wherenowDicObj = ConvertToDictionaryObject(listObject[0]);

                WhereNowAck ack = null;

                int statusCode = 0;

                if (wherenowDicObj != null)
                {
                    ack = new WhereNowAck();

                    if (int.TryParse(wherenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = wherenowDicObj["message"].ToString();

                    ack.Service = wherenowDicObj["service"].ToString();

                    ack.Payload = new WhereNowAck.Data();
                    if (wherenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> whereNowPayloadDic = ConvertToDictionaryObject(wherenowDicObj["payload"]);
                        if (whereNowPayloadDic != null && whereNowPayloadDic.Count > 0)
                        {
                            if (whereNowPayloadDic.ContainsKey("channels"))
                            {
                                //ack.Payload.channels = null;
                                object[] whereNowChannelList = ConvertToObjectArray(whereNowPayloadDic["channels"]);
                                if (whereNowChannelList != null && whereNowChannelList.Length >= 0)
                                {
                                    List<string> channelList = new List<string>();
                                    foreach (string channel in whereNowChannelList)
                                    {
                                        channelList.Add(channel);
                                    }
                                    ack.Payload.channels = channelList.ToArray();
                                }

                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(WhereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(SetUserStateAck))
            {
                #region "SetUserStateAck"
                Dictionary<string, object> setUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                SetUserStateAck ack = null;

                int statusCode = 0;

                if (setUserStatewDicObj != null)
                {
                    ack = new SetUserStateAck();

                    if (int.TryParse(setUserStatewDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = setUserStatewDicObj["message"].ToString();

                    ack.Service = setUserStatewDicObj["service"].ToString();

                    if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    {
                        ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    }

                    if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    {
                        ack.ChannelName = listObject[2].ToString().Split(',');
                    }

                    ack.Payload = new Dictionary<string, object>();

                    if (setUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> setStateDic = ConvertToDictionaryObject(setUserStatewDicObj["payload"]);
                        if (setStateDic != null)
                        {
                            ack.Payload = setStateDic;
                        }
                    }

                }

                ret = (T)Convert.ChangeType(ack, typeof(SetUserStateAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetUserStateAck))
            {
                #region "GetUserStateAck"
                Dictionary<string, object> getUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                GetUserStateAck ack = null;

                int statusCode = 0;

                if (getUserStatewDicObj != null)
                {
                    ack = new GetUserStateAck();

                    if (int.TryParse(getUserStatewDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = getUserStatewDicObj["message"].ToString();

                    ack.Service = getUserStatewDicObj["service"].ToString();

                    ack.UUID = getUserStatewDicObj["uuid"].ToString();

                    if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    {
                        ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    }
                    if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    {
                        ack.ChannelName = listObject[2].ToString().Split(',');
                    }

                    ack.Payload = new Dictionary<string, object>();

                    if (getUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> getStateDic = ConvertToDictionaryObject(getUserStatewDicObj["payload"]);
                        if (getStateDic != null)
                        {
                            ack.Payload = getStateDic;
                        }
                    }


                }

                ret = (T)Convert.ChangeType(ack, typeof(GetUserStateAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetChannelGroupChannelsAck))
            {
                #region "GetChannelGroupChannelsAck"
                Dictionary<string, object> getCgChannelsDicObj = ConvertToDictionaryObject(listObject[0]);

                GetChannelGroupChannelsAck ack = null;

                int statusCode = 0;

                if (getCgChannelsDicObj != null)
                {
                    ack = new GetChannelGroupChannelsAck();

                    if (int.TryParse(getCgChannelsDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getCgChannelsDicObj["service"].ToString();

                    Dictionary<string, object> getCgChannelPayloadDic = ConvertToDictionaryObject(getCgChannelsDicObj["payload"]);
                    if (getCgChannelPayloadDic != null && getCgChannelPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetChannelGroupChannelsAck.Data();
                        ack.Payload.ChannelGroupName = getCgChannelPayloadDic["group"].ToString();

                        object[] cgChPayloadChannels = ConvertToObjectArray(getCgChannelPayloadDic["channels"]);
                        if (cgChPayloadChannels != null && cgChPayloadChannels.Length > 0)
                        {
                            List<string> chList = new List<string>();
                            for (int index = 0; index < cgChPayloadChannels.Length; index++)
                            {
                                chList.Add(cgChPayloadChannels[index].ToString());
                            }
                            ack.Payload.ChannelName = chList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getCgChannelsDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetChannelGroupChannelsAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetAllChannelGroupsAck))
            {
                #region "GetAllChannelGroupsAck"
                Dictionary<string, object> getAllCgDicObj = ConvertToDictionaryObject(listObject[0]);

                GetAllChannelGroupsAck ack = null;

                int statusCode = 0;

                if (getAllCgDicObj != null)
                {
                    ack = new GetAllChannelGroupsAck();

                    if (int.TryParse(getAllCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getAllCgDicObj["service"].ToString();

                    Dictionary<string, object> getAllCgPayloadDic = ConvertToDictionaryObject(getAllCgDicObj["payload"]);
                    if (getAllCgPayloadDic != null && getAllCgPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetAllChannelGroupsAck.Data();
                        ack.Payload.Namespace = getAllCgPayloadDic["namespace"].ToString();

                        object[] cgAllCgPayloadChannels = ConvertToObjectArray(getAllCgPayloadDic["groups"]);
                        if (cgAllCgPayloadChannels != null && cgAllCgPayloadChannels.Length > 0)
                        {
                            List<string> allCgList = new List<string>();
                            for (int index = 0; index < cgAllCgPayloadChannels.Length; index++)
                            {
                                allCgList.Add(cgAllCgPayloadChannels[index].ToString());
                            }
                            ack.Payload.ChannelGroupName = allCgList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getAllCgDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetAllChannelGroupsAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetAllNamespacesAck))
            {
                #region "GetAllNamespacesAck"
                Dictionary<string, object> getAllNamespaceDicObj = ConvertToDictionaryObject(listObject[0]);

                GetAllNamespacesAck ack = null;

                int statusCode = 0;

                if (getAllNamespaceDicObj != null)
                {
                    ack = new GetAllNamespacesAck();

                    if (int.TryParse(getAllNamespaceDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getAllNamespaceDicObj["service"].ToString();

                    Dictionary<string, object> getAllNsPayloadDic = ConvertToDictionaryObject(getAllNamespaceDicObj["payload"]);
                    if (getAllNsPayloadDic != null && getAllNsPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetAllNamespacesAck.Data();
                        ack.Payload.SubKey = getAllNsPayloadDic["sub_key"].ToString();

                        object[] cgAllNsPayloadNamespaces = ConvertToObjectArray(getAllNsPayloadDic["namespaces"]);
                        if (cgAllNsPayloadNamespaces != null && cgAllNsPayloadNamespaces.Length > 0)
                        {
                            List<string> allCgList = new List<string>();
                            for (int index = 0; index < cgAllNsPayloadNamespaces.Length; index++)
                            {
                                allCgList.Add(cgAllNsPayloadNamespaces[index].ToString());
                            }
                            ack.Payload.NamespaceName = allCgList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getAllNamespaceDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetAllNamespacesAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(AddChannelToChannelGroupAck))
            {
                #region "AddChannelToChannelGroupAck"
                Dictionary<string, object> addChToCgDicObj = ConvertToDictionaryObject(listObject[0]);

                AddChannelToChannelGroupAck ack = null;

                int statusCode = 0;

                if (addChToCgDicObj != null)
                {
                    ack = new AddChannelToChannelGroupAck();

                    if (int.TryParse(addChToCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = addChToCgDicObj["message"].ToString();
                    ack.Service = addChToCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(addChToCgDicObj["error"].ToString());

                    ack.ChannelGroupName = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(AddChannelToChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveChannelFromChannelGroupAck))
            {
                #region "RemoveChannelFromChannelGroupAck"
                Dictionary<string, object> removeChFromCgDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveChannelFromChannelGroupAck ack = null;

                int statusCode = 0;

                if (removeChFromCgDicObj != null)
                {
                    ack = new RemoveChannelFromChannelGroupAck();

                    if (int.TryParse(removeChFromCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = removeChFromCgDicObj["message"].ToString();
                    ack.Service = removeChFromCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeChFromCgDicObj["error"].ToString());

                    ack.ChannelGroupName = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveChannelFromChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveChannelGroupAck))
            {
                #region "RemoveChannelGroupAck"
                Dictionary<string, object> removeCgDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveChannelGroupAck ack = null;

                int statusCode = 0;

                if (removeCgDicObj != null)
                {
                    ack = new RemoveChannelGroupAck();

                    if (int.TryParse(removeCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = removeCgDicObj["service"].ToString();
                    ack.StatusMessage = removeCgDicObj["message"].ToString();

                    //Dictionary<string, object> removeCgPayloadDic = ConvertToDictionaryObject(removeCgDicObj["payload"]);
                    //if (removeCgPayloadDic != null && removeCgPayloadDic.Count > 0)
                    //{
                    //    ack.Payload = new RemoveChannelGroupAck.Data();
                    //    ack.Payload.ChannelGroupName = removeCgPayloadDic["group"].ToString();

                    //    object[] cgChPayloadChannels = ConvertToObjectArray(removeCgPayloadDic["channels"]);
                    //    if (cgChPayloadChannels != null && cgChPayloadChannels.Length > 0)
                    //    {
                    //        List<string> chList = new List<string>();
                    //        for (int index = 0; index < cgChPayloadChannels.Length; index++)
                    //        {
                    //            chList.Add(cgChPayloadChannels[index].ToString());
                    //        }
                    //        ack.Payload.ChannelName = chList.ToArray();
                    //    }
                    //}

                    ack.Error = Convert.ToBoolean(removeCgDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveNamespaceAck))
            {
                #region "RemoveNamespaceAck"
                Dictionary<string, object> removeNsDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveNamespaceAck ack = null;

                int statusCode = 0;

                if (removeNsDicObj != null)
                {
                    ack = new RemoveNamespaceAck();

                    if (int.TryParse(removeNsDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = removeNsDicObj["message"].ToString();

                    ack.Service = removeNsDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeNsDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveNamespaceAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else
            {
                ret = (T)(object)listObject;
            }

            return ret;
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        }

        public Dictionary<string, object> ConvertToDictionaryObject(object localContainer)
        {
            Dictionary<string, object> ret = null;

            if (localContainer != null)
            {
                if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                {
                    ret = new Dictionary<string, object>();

                    IDictionary<string, JToken> jDictionary = localContainer as JObject;
                    if (jDictionary != null)
                    {
                        foreach (KeyValuePair<string, JToken> pair in jDictionary)
                        {
                            JToken token = pair.Value;
                            ret.Add(pair.Key, ConvertJTokenToObject(token));
                        }
                    }
                }
                else if (localContainer.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    ret = new Dictionary<string, object>();
                    Dictionary<string, object> dictionary = localContainer as Dictionary<string, object>;
                    foreach (string key in dictionary.Keys)
                    {
                        ret.Add(key, dictionary[key]);
                    }
                }
            }

            return ret;

        }

        public Dictionary<string, object>[] ConvertToDictionaryObjectArray(object localContainer)
        {
            Dictionary<string, object>[] ret = null;

            if (localContainer != null && localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject[]")
            {
                IDictionary<string, JToken>[] iDictionary = localContainer as IDictionary<string, JToken>[];
                if (iDictionary != null && iDictionary.Length > 0)
                {
                    ret = new Dictionary<string, object>[iDictionary.Length];

                    for (int index = 0; index < iDictionary.Length; index++)
                    {
                        IDictionary<string, JToken> iItem = iDictionary[index];
                        foreach (KeyValuePair<string, JToken> pair in iItem)
                        {
                            JToken token = pair.Value;
                            ret[index].Add(pair.Key, ConvertJTokenToObject(token));
                        }
                    }
                }
            }

            return ret;
        }

        public object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                JArray jarrayResult = localContainer as JArray;
                List<object> objectContainer = jarrayResult.ToObject<List<object>>();
                if (objectContainer != null && objectContainer.Count > 0)
                {
                    for (int index = 0; index < objectContainer.Count; index++)
                    {
                        if (objectContainer[index].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                        {
                            JArray internalItem = objectContainer[index] as JArray;
                            objectContainer[index] = internalItem.Select(item => (object)item).ToArray();
                        }
                    }
                    ret = objectContainer.ToArray<object>();
                }
            }
            else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
            {
                List<object> listResult = localContainer as List<object>;
                ret = listResult.ToArray<object>();
            }

            return ret;
        }

        private static object ConvertJTokenToObject(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            var jValue = token as JValue;
            if (jValue != null)
            {
                return jValue.Value;
            }

            var jContainer = token as JArray;
            if (jContainer != null)
            {
                List<object> jsonList = new List<object>();
                foreach (JToken arrayItem in jContainer)
                {
                    jsonList.Add(ConvertJTokenToObject(arrayItem));
                }
                return jsonList;
            }

            IDictionary<string, JToken> jsonObject = token as JObject;
            if (jsonObject != null)
            {
                var jsonDict = new Dictionary<string, object>();
                List<JProperty> propertyList = (from childToken in token
                                                where childToken is JProperty
                                                select childToken as JProperty).ToList();
                foreach (JProperty property in propertyList)
                {
                    jsonDict.Add(property.Name, ConvertJTokenToObject(property.Value));
                }

                //(from childToken in token 
                //    where childToken is JProperty select childToken as JProperty)
                //    .ToList()
                //    .ForEach(property => jsonDict.Add(property.Name, ConvertJTokenToDictionary(property.Value)));
                return jsonDict;
            }

            return null;
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
		GlobalHere_Now,
        PushRegister,
        PushRemove,
        PushGet,
        PushUnregister,
        ChannelGroupAdd,
        ChannelGroupRemove,
        ChannelGroupGet,
        ChannelGroupGrantAccess,
        ChannelGroupAuditAccess,
        ChannelGroupRevokeAccess
	}

	internal class InternetState
	{
		public Action<bool> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public string[] Channels;
		public string[] ChannelGroups;

		public InternetState ()
		{
			Callback = null;
			ErrorCallback = null;
			Channels = null;
			ChannelGroups = null;
		}
	}

	internal class RequestState<T>
	{
		public Action<T> NonSubscribeRegularCallback;
		public Action<Message<T>> SubscribeRegularCallback;
		public Action<PresenceAck> PresenceRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<PresenceAck> WildcardPresenceCallback;
		public Action<PubnubClientError> ErrorCallback;
		public PubnubWebRequest Request;
		public PubnubWebResponse Response;
		public ResponseType ResponseType;
		public string[] Channels;
		public string[] ChannelGroups;
		public bool Timeout;
		public bool Reconnect;
		public long Timetoken;

		public RequestState()
		{
			SubscribeRegularCallback = null;
			PresenceRegularCallback = null;
			WildcardPresenceCallback = null;
			ConnectCallback = null;
			Request = null;
			Response = null;
			Channels = null;
			ChannelGroups = null;
		}
	}
	#endregion
	#region "Channel callback"
	internal struct PubnubChannelCallbackKey
	{
		public string Channel;
		public ResponseType ResponseType;
	}

	public class PubnubSubscribeChannelCallback<T>
	{
		public Action<Message<T>> SubscribeRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<ConnectOrDisconnectAck> DisconnectCallback;
		public Action<PresenceAck> WildcardPresenceCallback;
		public Action<PubnubClientError> ErrorCallback;

		public PubnubSubscribeChannelCallback()
		{
			SubscribeRegularCallback = null;
			ConnectCallback = null;
			DisconnectCallback = null;
			WildcardPresenceCallback = null;
			ErrorCallback = null;
		}
	}

	internal class PubnubPresenceChannelCallback
	{
		public Action<PresenceAck> PresenceRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<ConnectOrDisconnectAck> DisconnectCallback;
		public Action<PubnubClientError> ErrorCallback;

		public PubnubPresenceChannelCallback()
		{
			PresenceRegularCallback = null;
			ConnectCallback = null;
			DisconnectCallback = null;
			ErrorCallback = null;
		}
	}
	#endregion

    #region "ChannelGroup callback"
    internal struct PubnubChannelGroupCallbackKey
    {
        public string ChannelGroup;
		public ResponseType ResponseType;
    }

	public class PubnubSubscribeChannelGroupCallback<T>
	{
		public Action<Message<T>> SubscribeRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<ConnectOrDisconnectAck> DisconnectCallback;
		public Action<PresenceAck> WildcardPresenceCallback;
		public Action<PubnubClientError> ErrorCallback;

		public PubnubSubscribeChannelGroupCallback()
		{
			SubscribeRegularCallback = null;
			ConnectCallback = null;
			DisconnectCallback = null;
			WildcardPresenceCallback = null;
			ErrorCallback = null;
		}
	}

	internal class PubnubPresenceChannelGroupCallback
	{
		public Action<PresenceAck> PresenceRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<ConnectOrDisconnectAck> DisconnectCallback;
		public Action<PubnubClientError> ErrorCallback;

		public PubnubPresenceChannelGroupCallback()
		{
			PresenceRegularCallback = null;
			ConnectCallback = null;
			DisconnectCallback = null;
			ErrorCallback = null;
		}
	}
    #endregion
    #region "Pubnub Push Notification"
    public enum PushTypeService
    {
        None,
        MPNS, //MicrosoftPushNotificationService
        WNS, //WindowsNotificationService,
        GCM,
        APNS
    }

    #endregion

    #region "public facing generic response types"

    public interface IPubnubSubscribeMessageType
    {
        dynamic GetSubscribeMessageType(Type messageType, object pubnubSubscribeCallbackObject, bool isChannelGroup);
    }

    public class PubnubSubscribeMessageType : IPubnubSubscribeMessageType
    {
        public virtual dynamic GetSubscribeMessageType(Type messageType, object pubnubSubscribeCallbackObject, bool isChannelGroup)
        {
            return null;
        }
    }

    public class Message<T>
    {
        public T Data { get; set; }
        public DateTime Time { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return Data.ToString();
        }
    }

    public class GrantAck
    {
        public GrantAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public Data()
            {
                this.Level = "";
            }

            public class SubkeyAccess
            {
                public bool read { get; set; }
                public bool write { get; set; }
                public bool manage { get; set; }
            }
            public string Level { get; set; }
            public string SubscribeKey { get; set; }
            public int TTL { get; set; }
            public Dictionary<string, ChannelData> channels { get; set; }
            public Dictionary<string, ChannelGroupData> channelgroups { get; set; }
            public SubkeyAccess Access { get; set; }

            public class ChannelData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                    }

                    public AuthAccess Access { get; set; }
                }

                public ChannelAccess Access { get; set; }
            }

            public class ChannelGroupData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelGroupAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                    }
                    public AuthAccess Access { get; set; }
                }

                public ChannelGroupAccess Access { get; set; }
            }


        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public bool Warning { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }

    public class AuditAck
    {
        public AuditAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public Data()
            {
                this.Level = "";
            }

            public class SubkeyAccess
            {
                public bool read { get; set; }
                public bool write { get; set; }
                public bool manage { get; set; }
                public int TTL { get; set; }
            }
            public string Level { get; set; }
            public string SubscribeKey { get; set; }
            public Dictionary<string, ChannelData> channels { get; set; }
            public Dictionary<string, ChannelGroupData> channelgroups { get; set; }
            public SubkeyAccess Access { get; set; }

            public class ChannelData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                    public int TTL { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                        public int TTL { get; set; }
                    }

                    public AuthAccess Access { get; set; }
                }

                public ChannelAccess Access { get; set; }
            }

            public class ChannelGroupData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelGroupAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                    public int TTL { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                        public int TTL { get; set; }
                    }
                    public AuthAccess Access { get; set; }
                }

                public ChannelGroupAccess Access { get; set; }
            }
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public bool Warning { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }

    public class PublishAck
    {
        public PublishAck()
        {
            this.StatusMessage = "";
            this.ChannelName = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public long Timetoken { get; set; }
        public object Payload { get; set; }
    }

    public class PresenceAck
    {
        public PresenceAck()
        {
            this.Action = "";
            this.UUID = "";
            this.ChannelName = "";
            this.ChannelGroupName = "";
        }
        public string Action { get; set; }
        public long Timestamp { get; set; }
        public string UUID { get; set; }
        public int Occupancy { get; set; }

        public string ChannelName { get; set; }
        public string ChannelGroupName { get; set; }

        public long Timetoken { get; set; }

    }

    public class DetailedHistoryAck
    {
        public DetailedHistoryAck()
        {
            this.ChannelName = "";
        }

        public object[] Message;
        public long StartTimeToken;
        public long EndTimeToken;
        public string ChannelName { get; set; }
    }

    public class ConnectOrDisconnectAck
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public string ChannelGroupName { get; set; }
    }

    public class HereNowAck
    {
        public HereNowAck()
        {
            this.StatusMessage = "";
            this.Service = "";
            this.ChannelName = "";
            //this.UUID = new string[0];
        }

        public class Data
        {
            public Data()
            {

            }
            public class ChannelData
            {
                public class UuidData
                {
                    public string uuid { get; set; }
                    public Dictionary<string, object> state { get; set; }
                }

                public int occupancy { get; set; }
                public UuidData[] uuids { get; set; }
            }
            public Dictionary<string, ChannelData> channels;
            public int total_channels { get; set; }
            public int total_occupancy { get; set; }
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
        public string ChannelName { get; set; }

    }

    public class GlobalHereNowAck
    {
        public GlobalHereNowAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public Data()
            {

            }
            public class ChannelData
            {
                public class UuidData
                {
                    public string uuid { get; set; }
                    public Dictionary<string, object> state { get; set; }
                }

                public int occupancy { get; set; }
                public UuidData[] uuids { get; set; }
            }
            public Dictionary<string, ChannelData> channels;
            public int total_channels { get; set; }
            public int total_occupancy { get; set; }
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }

    public class WhereNowAck
    {
        public WhereNowAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public string[] channels;
        }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }

    public class SetUserStateAck
    {
        public SetUserStateAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public string[] ChannelName { get; set; }
        public string[] ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }

    public class GetUserStateAck
    {
        public GetUserStateAck()
        {
            this.StatusMessage = "";
            this.Service = "";
            this.UUID = "";
        }

        public string[] ChannelName { get; set; }
        public string[] ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public string UUID { get; set; }
    }

    public class GetChannelGroupChannelsAck
    {
        public class Data
        {
            public Data()
            {
                this.ChannelGroupName = "";
            }

            public string[] ChannelName;
            public string ChannelGroupName;
        }

        public GetChannelGroupChannelsAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class GetAllChannelGroupsAck
    {
        public class Data
        {
            public Data()
            {
                this.Namespace = "";
            }

            public string[] ChannelGroupName;
            public string Namespace;
        }

        public GetAllChannelGroupsAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class GetAllNamespacesAck
    {
        public class Data
        {
            public Data()
            {
                this.SubKey = "";
            }

            public string[] NamespaceName;
            public string SubKey;
        }

        public GetAllNamespacesAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class AddChannelToChannelGroupAck
    {
        public AddChannelToChannelGroupAck()
        {
            this.ChannelGroupName = "";
            this.StatusMessage = "";
            this.Service = "";
        }

        public string ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class RemoveChannelFromChannelGroupAck
    {
        public RemoveChannelFromChannelGroupAck()
        {
            this.ChannelGroupName = "";
            this.StatusMessage = "";
            this.Service = "";
        }

        public string ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class RemoveChannelGroupAck
    {
        //public class Data
        //{
        //    public Data()
        //    {
        //        this.ChannelGroupName = "";
        //    }

        //    public string[] ChannelName;
        //    public string ChannelGroupName;
        //}

        public RemoveChannelGroupAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        //public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    public class RemoveNamespaceAck
    {
        public RemoveNamespaceAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }

    internal class GrantAckPayload
    {
        public GrantAckPayload()
        {
            this.Level = "";
            this.SubscribeKey = "";
        }

        public string Level { get; set; }
        public string SubscribeKey { get; set; }
        public int TTL { get; set; }
        public string[] ChannelName { get; set; }
        public string ChannelGroupName { get; set; }
    }

    internal class AuditAckPayload
    {
        public AuditAckPayload()
        {
            this.Level = "";
            this.SubscribeKey = "";
        }

        public string Level { get; set; }
        public string SubscribeKey { get; set; }
        public int TTL { get; set; }
        public string ChannelGroups { get; set; }
    }

    #endregion

}
