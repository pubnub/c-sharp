//Build Date: June 06, 2016
#region "Header"
#if (__MonoCS__)
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
using PubnubApi.Interface;
#endregion



namespace PubnubApi
{
    internal abstract class PubnubCoreBase
    {
        IPubnubHttp _pubnubHttp = null;

        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubUnitTest pubnubUnitTest = null;

        public PubnubCoreBase(PNConfiguration pnConfiguation)
        {
            if (pnConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }

            InternalConstructor(pnConfiguation, new NewtonsoftJsonDotNet(), null);
        }

        public PubnubCoreBase(PNConfiguration pnConfiguation, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            if (pnConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }
            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pnConfiguation, new NewtonsoftJsonDotNet(), null);
            }
            else
            {
                InternalConstructor(pnConfiguation, jsonPluggableLibrary, null);
            }
        }

        public PubnubCoreBase(PNConfiguration pnConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest)
        {
            if (pnConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }

            if (pubnubUnitTest == null)
            {
                throw new ArgumentException("IPubnubUnitTest missing");
            }

            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pnConfiguation, new NewtonsoftJsonDotNet(), pubnubUnitTest);
            }
            else
            {
                InternalConstructor(pnConfiguation, jsonPluggableLibrary, pubnubUnitTest);
            }
        }


        private void InternalConstructor(PNConfiguration pnConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubConfig = pnConfiguation;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pubnubUnitTest;

            _pubnubHttp = new PubnubHttp(pnConfiguation, jsonLib);

            //if (!IsNullOrWhiteSpace(pnConfiguation.PublishKey)) { this.publishKey = pnConfiguation.PublishKey; }
            //if (!IsNullOrWhiteSpace(pnConfiguation.SubscribeKey)) { this.subscribeKey = pnConfiguation.SubscribeKey; }
            //if (!IsNullOrWhiteSpace(pnConfiguation.SecretKey)) { this.secretKey = pnConfiguation.SecretKey; }
            //if (!IsNullOrWhiteSpace(pnConfiguation.CiperKey)) { this.cipherKey = pnConfiguation.CiperKey; }
            ////if (!IsNullOrWhiteSpace(pnConfiguation.AuthKey)) { this._authenticationKey = pnConfiguation.AuthKey; }
            //if (!IsNullOrWhiteSpace(pnConfiguation.Origin)) { this._origin = pnConfiguation.Origin; }
            ////if (!IsNullOrWhiteSpace(pnConfiguation.Uuid)) { this.sessionUUID = pnConfiguation.Uuid; }

            //this.ssl = pnConfiguation.Secure;
            //this._pubnubWebRequestCallbackIntervalInSeconds = pnConfiguation.SubscribeTimeout;
            //this._pubnubOperationTimeoutIntervalInSeconds = pnConfiguation.NonSubscribeRequestTimeout;
            //this._pubnubPresenceHeartbeatInSeconds = pnConfiguation.PresenceHeartbeatTimeout;
            //this._presenceHeartbeatIntervalInSeconds = pnConfiguation.PresenceHeartbeatInterval;

            _pubnubLogLevel = pnConfiguation.LogVerbosity;
#if (SILVERLIGHT || WINDOWS_PHONE)
            HttpWebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
#endif
        }

        #region "Class variables"

        int _pubnubNetworkTcpCheckIntervalInSeconds = 15;
        int _pubnubNetworkCheckRetries = 50;
        bool _enableResumeOnReconnect = true;
        bool _uuidChanged = false;
        protected bool overrideTcpKeepAlive = true;
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

        private static long lastSubscribeTimetoken = 0;
        // Pubnub Core API implementation
        #endregion

        #region "Constructors"

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return string.IsNullOrEmpty(value.Trim());
        }
        #endregion


        #region "Exception handlers"

        protected void UrlRequestCommonExceptionHandler<T>(ResponseType type, string[] channels, string[] channelGroups, bool requestTimeout, Action<Message<T>> subscribeRegularCallback, Action<PresenceAck> presenceRegularCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback, bool resumeOnReconnect)
        {
            if (type == ResponseType.Subscribe || type == ResponseType.Presence)
            {
                //MultiplexExceptionHandler<T>(type, channels, channelGroups, subscribeRegularCallback, presenceRegularCallback, connectCallback, wildcardPresenceCallback, errorCallback, false, resumeOnReconnect);
            }
            else if (type == ResponseType.Publish)
            {
                //PublishExceptionHandler(channels[0], requestTimeout, errorCallback);
            }
            else if (type == ResponseType.Here_Now)
            {
                //HereNowExceptionHandler(channels[0], requestTimeout, errorCallback);
            }
            else if (type == ResponseType.DetailedHistory)
            {
                //DetailedHistoryExceptionHandler(channels[0], requestTimeout, errorCallback);
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
                //GetUserStateExceptionHandler(channels[0], requestTimeout, errorCallback);
            }
            else if (type == ResponseType.SetUserState)
            {
                //SetUserStateExceptionHandler(channels[0], requestTimeout, errorCallback);
            }
            else if (type == ResponseType.GlobalHere_Now)
            {
                //GlobalHereNowExceptionHandler(requestTimeout, errorCallback);
            }
            else if (type == ResponseType.Where_Now)
            {
                //WhereNowExceptionHandler(channels[0], requestTimeout, errorCallback);
            }
            else if (type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
            {
                //PushNotificationExceptionHandler(channels, requestTimeout, errorCallback);
            }
            else if (type == ResponseType.ChannelGroupAdd || type == ResponseType.ChannelGroupRemove || type == ResponseType.ChannelGroupGet)
            {
                //ChannelGroupExceptionHandler(channels, requestTimeout, errorCallback);
            }
        }

        private void TimeExceptionHandler(bool requestTimeout, Action<PubnubClientError> errorCallback)
        {
            if (requestTimeout)
            {
                string message = (requestTimeout) ? "Operation Timeout" : "Network connnect error";

                LoggingMethod.WriteToLog(string.Format("DateTime {0}, TimeExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

                new PNCallbackService(pubnubConfig, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                    "", "", errorCallback, message, PubnubErrorCode.TimeOperationTimeout, null, null);
            }
        }

        #endregion

        #region "Callbacks"

        protected virtual bool CheckInternetConnectionStatus(bool systemActive, Action<PubnubClientError> errorCallback, string[] channels, string[] channelGroups)
        {
            return ClientNetworkStatus.CheckInternetStatus(pubnetSystemActive, errorCallback, channels, channelGroups);
        }

        protected void OnPubnubLocalClientHeartBeatTimeoutCallback<T>(System.Object heartbeatState)
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
                    if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest)
                    {
                        networkConnection = true;
                    }
                    else
                    {
                        networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, currentState.ErrorCallback, currentState.Channels, currentState.ChannelGroups);
                    }
                    networkConnection = CheckInternetConnectionStatus(pubnetSystemActive, currentState.ErrorCallback, currentState.Channels, currentState.ChannelGroups);

                    channelInternetStatus[channel] = networkConnection;
                    channelGroupInternetStatus[channelGroup] = networkConnection;

                    LoggingMethod.WriteToLog(string.Format("DateTime: {0}, OnPubnubLocalClientHeartBeatTimeoutCallback - Internet connection = {1}", DateTime.Now.ToString(), networkConnection), LoggingMethod.LevelVerbose);
                    if (!networkConnection)
                    {
                        //REVISIT
                        //TerminatePendingWebRequest(currentState);
                    }
                }
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

                            connectResult = jsonLib.DeserializeToListOfObject(jsonString);
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
                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
                                }
                            }
                            break;
                        case ResponseType.Presence:
                            jsonString = string.Format("[1, \"Presence Connected\"]");
                            connectResult = jsonLib.DeserializeToListOfObject(jsonString);
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
                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
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
                            connectResult = jsonLib.DeserializeToListOfObject(jsonString);
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
                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
                                }

                            }
                            break;
                        case ResponseType.Presence:
                            jsonString = string.Format("[1, \"Presence Connected\"]");
                            connectResult = jsonLib.DeserializeToListOfObject(jsonString);
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
                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<ConnectOrDisconnectAck>(connectResult, targetCallback, true, type);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region "Simulate network fail and machine sleep"

        /// <summary>
        /// FOR TESTING ONLY - To Enable Simulation of Network Non-Availability
        /// </summary>
        public void EnableSimulateNetworkFailForTestingOnly()
        {
            ClientNetworkStatus.SimulateNetworkFailForTesting = true;
            PubnubWebRequest.SimulateNetworkFailForTesting = true;
        }

        /// <summary>
        /// FOR TESTING ONLY - To Disable Simulation of Network Non-Availability
        /// </summary>
        public void DisableSimulateNetworkFailForTestingOnly()
        {
            ClientNetworkStatus.SimulateNetworkFailForTesting = false;
            PubnubWebRequest.SimulateNetworkFailForTesting = false;
        }

        //protected abstract void GeneratePowerSuspendEvent();

        //protected abstract void GeneratePowerResumeEvent();

        public void EnableMachineSleepModeForTestingOnly()
        {
            //GeneratePowerSuspendEvent();
            pubnetSystemActive = false;
        }

        public void DisableMachineSleepModeForTestingOnly()
        {
            //GeneratePowerResumeEvent();
            pubnetSystemActive = true;
        }

        #endregion

        #region "Helpers"

        public static long TranslateDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

        /// <summary>
        /// Convert the UTC/GMT DateTime to Unix Nano Seconds format
        /// </summary>
        /// <param name="dotNetUTCDateTime"></param>
        /// <returns></returns>
        public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds) * 10000000;
            return timeStamp;
        }

        /// <summary>
        /// Convert the Unix Nano Seconds format time to UTC/GMT DateTime
        /// </summary>
        /// <param name="unixNanoSecondTime"></param>
        /// <returns></returns>
        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
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

        private bool IsPresenceChannel(string channel)
        {
            if (channel.LastIndexOf("-pnpres") > 0)
            {
                return true;
            }
            else
            {
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
        private string GetChannelName(List<string> urlComponents, ResponseType type)
        {
            //This method is not in use
            string channelName = "";
            switch (type)
            {
                case ResponseType.Subscribe:
                case ResponseType.Presence:
                    channelName = urlComponents[2];
                    break;
                case ResponseType.Publish:
                    channelName = urlComponents[4];
                    break;
                case ResponseType.DetailedHistory:
                    channelName = urlComponents[5];
                    break;
                case ResponseType.Here_Now:
                    channelName = urlComponents[5];
                    break;
                case ResponseType.Leave:
                    channelName = urlComponents[5];
                    break;
                case ResponseType.Where_Now:
                    channelName = urlComponents[5];
                    break;
                default:
                    break;
            }
            ;
            return channelName;
        }

        #endregion

        #region "Build, process and send request"

        internal protected void UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest)
        {
            string channel = "";
            string channelGroup = "";

            if (terminateCurrentSubRequest)
            {
                TerminateCurrentSubscriberRequest();
            }

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

            try
            {
                if (!_channelRequest.ContainsKey(channel) && (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence))
                {
                    return;
                }

                // Create Request
                PubnubWebRequestCreator requestCreator = new PubnubWebRequestCreator(pubnubUnitTest);
                PubnubWebRequest request = (PubnubWebRequest)requestCreator.Create(requestUri);

                //REVISIT
                request = _pubnubHttp.SetProxy<T>(request);
                request = _pubnubHttp.SetTimeout<T>(pubnubRequestState, request);

                pubnubRequestState.Request = request;

                if (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence)
                {
                    _channelRequest.AddOrUpdate(channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
                }


                //overrideTcpKeepAlive must be true
                if (overrideTcpKeepAlive)
                {
                    //Eventhough heart-beat is disabled, run one time to check internet connection by setting dueTime=0
                    if (localClientHeartBeatTimer != null)
                    {
                        try
                        {
                            localClientHeartBeatTimer.Dispose();
                        }
                        catch { }
                    }
                    localClientHeartBeatTimer = new System.Threading.Timer(
                        new TimerCallback(OnPubnubLocalClientHeartBeatTimeoutCallback<T>), pubnubRequestState, 0,
                        (-1 == _pubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : _pubnubNetworkTcpCheckIntervalInSeconds * 1000);
                    channelLocalClientHeartbeatTimer.AddOrUpdate(requestUri, localClientHeartBeatTimer, (key, oldState) => localClientHeartBeatTimer);
                }
                else
                {
                    //REVISIT
                    request = _pubnubHttp.SetServicePointSetTcpKeepAlive(request);
                }
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Request={1}", DateTime.Now.ToString(), requestUri.ToString()), LoggingMethod.LevelInfo);

                //REVISIT
                _pubnubHttp.SendRequestAndGetResult(requestUri, pubnubRequestState, request);

                return;
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

                        new PNCallbackService(pubnubConfig, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                            multiChannel, multiChannelGroup, pubnubRequestState.ErrorCallback, ex, pubnubRequestState.Request, pubnubRequestState.Response);
                    }
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} Exception={1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelError);
                    UrlRequestCommonExceptionHandler<T>(pubnubRequestState.ResponseType, pubnubRequestState.Channels, pubnubRequestState.ChannelGroups, false, pubnubRequestState.SubscribeRegularCallback, pubnubRequestState.PresenceRegularCallback, pubnubRequestState.ConnectCallback, pubnubRequestState.WildcardPresenceCallback, pubnubRequestState.ErrorCallback, false);
                }
                return;
            }
        }

        #endregion

        public void TerminateCurrentSubscriberRequest()
        {
            string[] channels = GetCurrentSubscriberChannels();
            if (channels != null)
            {
                string multiChannel = (channels.Length > 0) ? string.Join(",", channels) : ",";
                PubnubWebRequest request = (_channelRequest.ContainsKey(multiChannel)) ? _channelRequest[multiChannel] : null;
                if (request != null)
                {
                    request.Abort(null, _errorLevel);

                    LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateCurrentSubsciberRequest {1}", DateTime.Now.ToString(), request.RequestUri.ToString()), LoggingMethod.LevelInfo);
                }
            }
        }
    }
}
