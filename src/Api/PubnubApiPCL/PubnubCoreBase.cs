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
using System.Threading.Tasks;
#endregion



namespace PubnubApi
{
    internal abstract class PubnubCoreBase
    {
        private IPubnubHttp pubnubHttp = null;

        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubUnitTest pubnubUnitTest = null;

        //protected ConcurrentDictionary<string, Dictionary<string, object>> channelLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        //protected ConcurrentDictionary<string, Dictionary<string, object>> channelUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        //protected ConcurrentDictionary<string, Dictionary<string, object>> channelGroupLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
        //protected ConcurrentDictionary<string, Dictionary<string, object>> channelGroupUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();

        //protected ConcurrentDictionary<string, bool> channelInternetStatus = new ConcurrentDictionary<string, bool>();
        //protected ConcurrentDictionary<string, bool> channelGroupInternetStatus = new ConcurrentDictionary<string, bool>();
        //protected ConcurrentDictionary<string, int> channelInternetRetry = new ConcurrentDictionary<string, int>();
        //protected ConcurrentDictionary<string, int> channelGroupInternetRetry = new ConcurrentDictionary<string, int>();

        //protected ConcurrentDictionary<string, PubnubWebRequest> _channelRequest = new ConcurrentDictionary<string, PubnubWebRequest>();
        private int pubnubNetworkTcpCheckIntervalInSeconds = 15;
        private int pubnubNetworkCheckRetries = 50;
        //private static long lastSubscribeTimetoken = 0;
        //private bool _uuidChanged = false;
        //private ConcurrentDictionary<string, Type> _channelSubscribeObjectType = new ConcurrentDictionary<string, Type>();
        //private ConcurrentDictionary<string, Type> _channelGroupSubscribeObjectType = new ConcurrentDictionary<string, Type>();
        //private ConcurrentDictionary<string, Timer> _channelReconnectTimer = new ConcurrentDictionary<string, Timer>();
        //private ConcurrentDictionary<string, Timer> _channelGroupReconnectTimer = new ConcurrentDictionary<string, Timer>();

        protected ConcurrentDictionary<string, Timer> ChannelReconnectTimer
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Timer> ChannelGroupReconnectTimer
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Type> ChannelSubscribeObjectType
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Type> ChannelGroupSubscribeObjectType
        {
            get;
            set;
        }

        protected bool UuidChanged
        {
            get;
            set;
        }

        protected long LastSubscribeTimetoken
        {
            get;
            set;
        }

        protected int PubnubNetworkTcpCheckIntervalInSeconds
        {
            get
            {
                return pubnubNetworkTcpCheckIntervalInSeconds;
            }
            set
            {
                pubnubNetworkTcpCheckIntervalInSeconds = value;
            }
        }

        protected int PubnubNetworkCheckRetries
        {
            get
            {
                return pubnubNetworkCheckRetries;
            }
            set
            {
                pubnubNetworkCheckRetries = value;
            }
        }

        protected ConcurrentDictionary<string, PubnubWebRequest> ChannelRequest
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, bool> ChannelInternetStatus
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, bool> ChannelGroupInternetStatus
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, int> ChannelInternetRetry
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, int> ChannelGroupInternetRetry
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Dictionary<string, object>> ChannelLocalUserState
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Dictionary<string, object>> ChannelUserState
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Dictionary<string, object>> ChannelGroupLocalUserState
        {
            get;
            set;
        }

        protected ConcurrentDictionary<string, Dictionary<string, object>> ChannelGroupUserState
        {
            get;
            set;
        }

        public PubnubCoreBase(PNConfiguration pubnubConfiguation)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }

            InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(), null);
        }

        public PubnubCoreBase(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }
            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(), null);
            }
            else
            {
                InternalConstructor(pubnubConfiguation, jsonPluggableLibrary, null);
            }
        }

        public PubnubCoreBase(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }

            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(), pubnubUnitTest);
            }
            else
            {
                InternalConstructor(pubnubConfiguation, jsonPluggableLibrary, pubnubUnitTest);
            }
        }

        private void InternalConstructor(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest)
        {
            #region Initialize Properties having ConcurrentDictionary
            ChannelRequest = new ConcurrentDictionary<string, PubnubWebRequest>();
            ChannelInternetStatus = new ConcurrentDictionary<string, bool>();
            ChannelGroupInternetStatus = new ConcurrentDictionary<string, bool>();
            ChannelInternetRetry = new ConcurrentDictionary<string, int>();
            ChannelGroupInternetRetry = new ConcurrentDictionary<string, int>();
            ChannelLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
            ChannelUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
            ChannelGroupLocalUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
            ChannelGroupUserState = new ConcurrentDictionary<string, Dictionary<string, object>>();
            ChannelReconnectTimer = new ConcurrentDictionary<string, Timer>();
            ChannelGroupReconnectTimer = new ConcurrentDictionary<string, Timer>();
            ChannelSubscribeObjectType = new ConcurrentDictionary<string, Type>();
            ChannelGroupSubscribeObjectType = new ConcurrentDictionary<string, Type>();
            #endregion
            this.pubnubConfig = pubnubConfiguation;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pubnubUnitTest;

            pubnubHttp = new PubnubHttp(pubnubConfiguation, jsonLib);

            _pubnubLogLevel = pubnubConfiguation.LogVerbosity;
#if (SILVERLIGHT || WINDOWS_PHONE)
            HttpWebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
#endif
        }

        #region "Class variables"

        private bool _enableResumeOnReconnect = true;
        protected bool overrideTcpKeepAlive = true;
        private LoggingMethod.Level _pubnubLogLevel = LoggingMethod.Level.Off;
        private PubnubErrorFilter.Level _errorLevel = PubnubErrorFilter.Level.Info;
        protected ConcurrentDictionary<string, long> multiChannelSubscribe = new ConcurrentDictionary<string, long>();
        protected ConcurrentDictionary<string, long> multiChannelGroupSubscribe = new ConcurrentDictionary<string, long>();
        protected ConcurrentDictionary<Uri, Timer> channelLocalClientHeartbeatTimer = new ConcurrentDictionary<Uri, Timer>();
        protected ConcurrentDictionary<PubnubChannelCallbackKey, object> channelCallbacks = new ConcurrentDictionary<PubnubChannelCallbackKey, object>();
        protected ConcurrentDictionary<PubnubChannelGroupCallbackKey, object> channelGroupCallbacks = new ConcurrentDictionary<PubnubChannelGroupCallbackKey, object>();
        private ConcurrentDictionary<string, List<string>> _channelSubscribedAuthKeys = new ConcurrentDictionary<string, List<string>>();
        protected System.Threading.Timer localClientHeartBeatTimer;
        protected System.Threading.Timer presenceHeartbeatTimer = null;
        protected static bool pubnetSystemActive = true;
        protected Collection<Uri> pushRemoteImageDomainUri = new Collection<Uri>();

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
                string message = requestTimeout ? "Operation Timeout" : "Network connnect error";

                LoggingMethod.WriteToLog(string.Format("DateTime {0}, TimeExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

                new PNCallbackService(pubnubConfig, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                    "", "", errorCallback, message, PubnubErrorCode.TimeOperationTimeout, null, null);
            }
        }

        #endregion

        #region "Internet connection and Reconnect Network"

        private bool InternetConnectionStatusWithUnitTestCheck(string channel, string channelGroup, Action<PubnubClientError> errorCallback, string[] rawChannels, string[] rawChannelGroups)
        {
            bool networkConnection;
            if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest)
            {
                networkConnection = true;
            }
            else
            {
                networkConnection = InternetConnectionStatus(channel, channelGroup, errorCallback, rawChannels, rawChannelGroups);
                if (!networkConnection)
                {
                    string message = "Network connnect error - Internet connection is not available.";
                    new PNCallbackService(pubnubConfig, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
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

        protected void ResetInternetCheckSettings(string[] channels, string[] channelGroups)
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
                if (ChannelInternetStatus.ContainsKey(multiChannel))
                {
                    ChannelInternetStatus.AddOrUpdate(multiChannel, true, (key, oldValue) => true);
                }
                else
                {
                    ChannelInternetStatus.GetOrAdd(multiChannel, true); //Set to true for internet connection
                }
                if (ChannelInternetRetry.ContainsKey(multiChannel))
                {
                    ChannelInternetRetry.AddOrUpdate(multiChannel, 0, (key, oldValue) => 0);
                }
                else
                {
                    ChannelInternetRetry.GetOrAdd(multiChannel, 0); //Initialize the internet retry count
                }
            }

            if (multiChannelGroup != "")
            {
                if (ChannelGroupInternetStatus.ContainsKey(multiChannelGroup))
                {
                    ChannelGroupInternetStatus.AddOrUpdate(multiChannelGroup, true, (key, oldValue) => true);
                }
                else
                {
                    ChannelGroupInternetStatus.GetOrAdd(multiChannelGroup, true); //Set to true for internet connection
                }

                if (ChannelGroupInternetRetry.ContainsKey(multiChannelGroup))
                {
                    ChannelGroupInternetRetry.AddOrUpdate(multiChannelGroup, 0, (key, oldValue) => 0);
                }
                else
                {
                    ChannelGroupInternetRetry.GetOrAdd(multiChannelGroup, 0); //Initialize the internet retry count
                }
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

                if ((ChannelInternetStatus.ContainsKey(channel) || ChannelGroupInternetStatus.ContainsKey(channelGroup))
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

                    ChannelInternetStatus[channel] = networkConnection;
                    ChannelGroupInternetStatus[channelGroup] = networkConnection;

                    LoggingMethod.WriteToLog(string.Format("DateTime: {0}, OnPubnubLocalClientHeartBeatTimeoutCallback - Internet connection = {1}", DateTime.Now.ToString(), networkConnection), LoggingMethod.LevelVerbose);
                    if (!networkConnection)
                    {
                        TerminatePendingWebRequest(currentState);
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

                            PubnubChannelCallbackKey presenceCallbackKey = new PubnubChannelCallbackKey();
                            presenceCallbackKey.Channel = channel;
                            presenceCallbackKey.ResponseType = type;

                            if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(presenceCallbackKey))
                            {
                                PubnubPresenceChannelCallback currentPubnubCallback = channelCallbacks[presenceCallbackKey] as PubnubPresenceChannelCallback;
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

                            PubnubChannelGroupCallbackKey presenceCallbackKey = new PubnubChannelGroupCallbackKey();
                            presenceCallbackKey.ChannelGroup = channelGroup;
                            presenceCallbackKey.ResponseType = type;

                            if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(presenceCallbackKey))
                            {
                                PubnubPresenceChannelGroupCallback currentPubnubCallback = channelGroupCallbacks[presenceCallbackKey] as PubnubPresenceChannelGroupCallback;
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
                                    if (pubnubConfig.CiperKey.Length > 0)
                                    {
                                        PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CiperKey);
                                        string decryptMessage = aes.Decrypt(messageList[messageIndex].ToString());
                                        object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : jsonLib.DeserializeToObject(decryptMessage);

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
                                                new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(itemMessage, currentPubnubCallback.PresenceRegularCallback, true, type);
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
                                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(itemMessage, currentPubnubCallback.WildcardPresenceCallback, true, type);
                                                }
                                            }
                                            else
                                            {
                                                if (currentPubnubCallback.SubscribeRegularCallback != null)
                                                {
                                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<Message<T>>(itemMessage, currentPubnubCallback.SubscribeRegularCallback, false, type);
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
                                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(itemMessage, currentPubnubCallback.PresenceRegularCallback, true, type);
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
                                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(itemMessage, currentPubnubCallback.WildcardPresenceCallback, true, type);
                                                }
                                            }
                                            else
                                            {
                                                if (currentPubnubCallback.SubscribeRegularCallback != null)
                                                {
                                                    new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(itemMessage, currentPubnubCallback.SubscribeRegularCallback, false, type);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    break;
                case ResponseType.Time:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.Publish:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.DetailedHistory:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.Here_Now:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.GlobalHere_Now:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.Where_Now:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
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
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.PushRegister:
                case ResponseType.PushRemove:
                case ResponseType.PushGet:
                case ResponseType.PushUnregister:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                case ResponseType.ChannelGroupAdd:
                case ResponseType.ChannelGroupRemove:
                case ResponseType.ChannelGroupGet:
                    if (result != null && result.Count > 0)
                    {
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                default:
                    break;
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

        protected bool IsPresenceChannel(string channel)
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

        internal protected string UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest)
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
                if (!ChannelRequest.ContainsKey(channel) && (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence))
                {
                    return "";
                }

                // Create Request
                PubnubWebRequestCreator requestCreator = new PubnubWebRequestCreator(pubnubUnitTest);
                PubnubWebRequest request = (PubnubWebRequest)requestCreator.Create(requestUri);

                request = pubnubHttp.SetProxy<T>(request);
                request = pubnubHttp.SetTimeout<T>(pubnubRequestState, request);

                pubnubRequestState.Request = request;

                if (pubnubRequestState.ResponseType == ResponseType.Subscribe || pubnubRequestState.ResponseType == ResponseType.Presence)
                {
                    ChannelRequest.AddOrUpdate(channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
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
                        (-1 == pubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : pubnubNetworkTcpCheckIntervalInSeconds * 1000);
                    channelLocalClientHeartbeatTimer.AddOrUpdate(requestUri, localClientHeartBeatTimer, (key, oldState) => localClientHeartBeatTimer);
                }
                else
                {
                    request = pubnubHttp.SetServicePointSetTcpKeepAlive(request);
                }

                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Request={1}", DateTime.Now.ToString(), requestUri.ToString()), LoggingMethod.LevelInfo);

                //REVISIT
                //pubnubHttp.SendRequestAndGetResult(requestUri, pubnubRequestState, request);
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, START Request {1}", DateTime.Now.ToString(), requestUri));

                Task<string> jsonResponse = pubnubHttp.SendRequestAndGetJsonResponse(requestUri, pubnubRequestState, request);
                string jsonString = jsonResponse.Result;

                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Done Request", DateTime.Now.ToString()));

                LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON= {1} for request={2}", DateTime.Now.ToString(), jsonString, requestUri), LoggingMethod.LevelInfo);
                //ProcessJsonResponse(pubnubRequestState, jsonString);
                return jsonString; ;
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
                return "";
            }
        }

        internal protected List<object> ProcessJsonResponse<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            List<object> result = new List<object>();

            string channel = "";
            string channelGroup = "";
            if (asyncRequestState != null)
            {
                if (asyncRequestState.Channels != null)
                {
                    channel = (asyncRequestState.Channels.Length > 0) ? string.Join(",", asyncRequestState.Channels) : ",";
                }
                if (asyncRequestState.ChannelGroups != null)
                {
                    channelGroup = string.Join(",", asyncRequestState.ChannelGroups);
                }
            }

            bool errorCallbackRaised = false;
            if (jsonLib.IsDictionaryCompatible(jsonString))
            {
                Dictionary<string, object> deserializeStatus = jsonLib.DeserializeToDictionaryOfObject(jsonString);
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

                        PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, statusMessage, PubnubMessageSource.Server, asyncRequestState.Request, asyncRequestState.Response, errorDescription, channel, channelGroup);
                        errorCallbackRaised = true;
                        new PNCallbackService(pubnubConfig, jsonLib).GoToCallback(error, asyncRequestState.ErrorCallback);
                    }
                }
            }
            if (!errorCallbackRaised)
            {
                result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.Request, asyncRequestState.ErrorCallback);
            }

            return result;
            //ProcessResponseCallbacks<T>(result, asyncRequestState);

        }

        protected List<object> WrapResultBasedOnResponseType<T>(ResponseType type, string jsonString, string[] channels, string[] channelGroups, bool reconnect, long lastTimetoken, PubnubWebRequest request, Action<PubnubClientError> errorCallback)
        {
            List<object> result = new List<object>();

            try
            {
                string multiChannel = (channels != null) ? string.Join(",", channels) : "";
                string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

                if (!string.IsNullOrEmpty(jsonString))
                {
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        object deserializedResult = jsonLib.DeserializeToObject(jsonString);
                        List<object> result1 = ((IEnumerable)deserializedResult).Cast<object>().ToList();

                        if (result1 != null && result1.Count > 0)
                        {
                            result = result1;
                        }

                        switch (type)
                        {
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
                                result.Add(multiChannel);

                                long receivedTimetoken = (result.Count > 1 && result[1].ToString() != "") ? Convert.ToInt64(result[1].ToString()) : 0;

                                long minimumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Min(token => token.Value) : 0;
                                long minimumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Min(token => token.Value) : 0;
                                long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

                                long maximumTimetoken1 = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Max(token => token.Value) : 0;
                                long maximumTimetoken2 = (multiChannelGroupSubscribe.Count > 0) ? multiChannelGroupSubscribe.Max(token => token.Value) : 0;
                                long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);

                                if (minimumTimetoken == 0 || lastTimetoken == 0)
                                {
                                    if (maximumTimetoken == 0)
                                    {
                                        LastSubscribeTimetoken = receivedTimetoken;
                                    }
                                    else
                                    {
                                        if (!_enableResumeOnReconnect)
                                        {
                                            LastSubscribeTimetoken = receivedTimetoken;
                                        }
                                        else
                                        {
                                            //do nothing. keep last subscribe token
                                        }
                                    }
                                }
                                else
                                {
                                    if (reconnect)
                                    {
                                        if (_enableResumeOnReconnect)
                                        {
                                            //do nothing. keep last subscribe token
                                        }
                                        else
                                        {
                                            LastSubscribeTimetoken = receivedTimetoken;
                                        }
                                    }
                                    else
                                    {
                                        LastSubscribeTimetoken = receivedTimetoken;
                                    }
                                }
                                break;
                            case ResponseType.Leave:
                                result.Add(multiChannel);
                                break;

                            case ResponseType.Time:
                                break;
                            case ResponseType.Publish:
                                #region "Publish"
                                result.Add(multiChannel);
                                if (pubnubConfig.AddPayloadToPublishResponse && request != null & request.RequestUri != null)
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
                                #endregion
                                break;
                            case ResponseType.DetailedHistory:
                                result = DecodeDecryptLoop(result, channels, channelGroups, errorCallback);
                                result.Add(multiChannel);
                                break;
                            case ResponseType.Here_Now:
                                Dictionary<string, object> dictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(dictionary);
                                result.Add(multiChannel);
                                break;
                            case ResponseType.GlobalHere_Now:
                                Dictionary<string, object> globalHereNowDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(globalHereNowDictionary);
                                break;
                            case ResponseType.Where_Now:
                                Dictionary<string, object> whereNowDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(whereNowDictionary);
                                result.Add(multiChannel);
                                break;
                            case ResponseType.GrantAccess:
                            case ResponseType.AuditAccess:
                            case ResponseType.RevokeAccess:
                                Dictionary<string, object> grantDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(grantDictionary);
                                result.Add(multiChannel);
                                break;
                            case ResponseType.ChannelGroupGrantAccess:
                            case ResponseType.ChannelGroupAuditAccess:
                            case ResponseType.ChannelGroupRevokeAccess:
                                Dictionary<string, object> channelGroupPAMDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(channelGroupPAMDictionary);
                                result.Add(multiChannelGroup);
                                break;
                            case ResponseType.GetUserState:
                            case ResponseType.SetUserState:
                                Dictionary<string, object> userStateDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(userStateDictionary);
                                result.Add(multiChannelGroup);
                                result.Add(multiChannel);
                                break;
                            case ResponseType.PushRegister:
                            case ResponseType.PushRemove:
                            case ResponseType.PushGet:
                            case ResponseType.PushUnregister:
                                result.Add(multiChannel);
                                break;
                            case ResponseType.ChannelGroupAdd:
                            case ResponseType.ChannelGroupRemove:
                            case ResponseType.ChannelGroupGet:
                                Dictionary<string, object> channelGroupDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
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
                        //switch stmt end
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        protected void ProcessResponseCallbacks<T>(List<object> result, RequestState<T> asyncRequestState)
        {
            bool callbackAvailable = false;
            if (result != null && result.Count >= 1)
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
                            List<string> channelList = asyncRequestState.Channels.ToList();
                            foreach (string ch in channelList)
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
                            List<string> channelGroupList = asyncRequestState.ChannelGroups.ToList();
                            foreach (string cg in channelGroupList)
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

        private List<object> DecodeDecryptLoop(List<object> message, string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback)
        {
            List<object> returnMessage = new List<object>();
            if (pubnubConfig.CiperKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CiperKey);
                var myObjectArray = (from item in message
                                     select item as object).ToArray();
                IEnumerable enumerable = myObjectArray[0] as IEnumerable;
                if (enumerable != null)
                {
                    List<object> receivedMsg = new List<object>();
                    foreach (object element in enumerable)
                    {
                        string decryptMessage = "";
                        try
                        {
                            decryptMessage = aes.Decrypt(element.ToString());
                        }
                        catch (Exception ex)
                        {
                            decryptMessage = "**DECRYPT ERROR**";

                            string multiChannel = string.Join(",", channels);
                            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

                            new PNCallbackService(pubnubConfig, jsonLib).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                                multiChannel, multiChannelGroup, errorCallback, ex, null, null);
                        }
                        object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : jsonLib.DeserializeToObject(decryptMessage);
                        receivedMsg.Add(decodeMessage);
                    }
                    returnMessage.Add(receivedMsg);
                }

                for (int index = 1; index < myObjectArray.Length; index++)
                {
                    returnMessage.Add(myObjectArray[index]);
                }
                return returnMessage;
            }
            else
            {
                var myObjectArray = (from item in message
                                     select item as object).ToArray();
                IEnumerable enumerable = myObjectArray[0] as IEnumerable;
                if (enumerable != null)
                {
                    List<object> receivedMessage = new List<object>();
                    foreach (object element in enumerable)
                    {
                        receivedMessage.Add(element);
                    }
                    returnMessage.Add(receivedMessage);
                }
                for (int index = 1; index < myObjectArray.Length; index++)
                {
                    returnMessage.Add(myObjectArray[index]);
                }
                return returnMessage;
            }
        }

        #endregion

        protected string BuildJsonUserState(string channel, string channelGroup, bool local)
        {
            Dictionary<string, object> channelUserStateDictionary = null;
            Dictionary<string, object> channelGroupUserStateDictionary = null;

            if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(channelGroup))
            {
                throw new ArgumentException("BuildJsonUserState takes either channel or channelGroup at one time. Send one at a time by passing empty value for other.");
            }

            if (local)
            {
                if (!string.IsNullOrEmpty(channel) && this.ChannelLocalUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = this.ChannelLocalUserState[channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && this.ChannelGroupLocalUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = this.ChannelGroupLocalUserState[channelGroup];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(channel) && this.ChannelUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = this.ChannelUserState[channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && this.ChannelGroupUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = this.ChannelGroupUserState[channelGroup];
                }
            }

            StringBuilder jsonStateBuilder = new StringBuilder();

            if (channelUserStateDictionary != null)
            {
                string[] channelUserStateKeys = channelUserStateDictionary.Keys.ToArray<string>();

                for (int keyIndex = 0; keyIndex < channelUserStateKeys.Length; keyIndex++)
                {
                    string channelUserStateKey = channelUserStateKeys[keyIndex];
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
                        jsonStateBuilder.Append(",");
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

        protected string BuildJsonUserState(string[] channels, string[] channelGroups, bool local)
        {
            string retJsonUserState = "";

            StringBuilder jsonStateBuilder = new StringBuilder();

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

        #region "Terminate requests and Timers"

        protected void TerminatePendingWebRequest()
        {
            TerminatePendingWebRequest<object>(null);
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
                ICollection<string> keyCollection = ChannelRequest.Keys;
                foreach (string key in keyCollection)
                {
                    PubnubWebRequest currentRequest = ChannelRequest[key];
                    if (currentRequest != null)
                    {
                        TerminatePendingWebRequest(currentRequest, null);
                    }
                }
            }
        }

        protected void TerminatePendingWebRequest(PubnubWebRequest request, Action<PubnubClientError> errorCallback)
        {
            if (request != null)
            {
                request.Abort(errorCallback, _errorLevel);
            }
        }

        private void RemoveChannelDictionary()
        {
            RemoveChannelDictionary<object>(null);
        }

        private void RemoveChannelDictionary<T>(RequestState<T> state)
        {
            if (state != null && state.Request != null)
            {
                string channel = (state.Channels != null) ? string.Join(",", state.Channels) : ",";

                if (ChannelRequest.ContainsKey(channel))
                {
                    PubnubWebRequest removedRequest;
                    bool removeKey = ChannelRequest.TryRemove(channel, out removedRequest);
                    if (removeKey)
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelError);
                    }
                }
            }
            else
            {
                ICollection<string> keyCollection = ChannelRequest.Keys;
                if (keyCollection != null && keyCollection.Count > 0)
                {
                    List<string> keysList = keyCollection.ToList();
                    foreach (string key in keysList)
                    {
                        PubnubWebRequest currentRequest = ChannelRequest[key];
                        if (currentRequest != null)
                        {
                            bool removeKey = ChannelRequest.TryRemove(key, out currentRequest);
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
            ICollection<string> channelLocalUserStateCollection = ChannelLocalUserState.Keys;
            ICollection<string> channelUserStateCollection = ChannelUserState.Keys;

            ICollection<string> channelGroupLocalUserStateCollection = ChannelGroupLocalUserState.Keys;
            ICollection<string> channelGroupUserStateCollection = ChannelGroupUserState.Keys;

            if (channelLocalUserStateCollection != null && channelLocalUserStateCollection.Count > 0)
            {
                List<string> channelLocalStateList = channelLocalUserStateCollection.ToList();
                foreach (string key in channelLocalStateList)
                {
                    if (ChannelLocalUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelLocalUserState.TryRemove(key, out tempUserState);
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
                    if (ChannelUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelUserState.TryRemove(key, out tempUserState);
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
                    if (ChannelGroupLocalUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelGroupLocalUserState.TryRemove(key, out tempUserState);
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
                    if (ChannelGroupUserState.ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelGroupUserState.TryRemove(key, out tempUserState);
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
            if (requestUri != null)
            {
                if (channelLocalClientHeartbeatTimer.ContainsKey(requestUri))
                {
                    Timer requestHeatbeatTimer = null;
                    if (channelLocalClientHeartbeatTimer.TryGetValue(requestUri, out requestHeatbeatTimer) && requestHeatbeatTimer != null)
                    {
                        try
                        {
                            requestHeatbeatTimer.Change(
                                (-1 == pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : pubnubNetworkTcpCheckIntervalInSeconds * 1000,
                                (-1 == pubnubNetworkTcpCheckIntervalInSeconds) ? -1 : pubnubNetworkTcpCheckIntervalInSeconds * 1000);
                            requestHeatbeatTimer.Dispose();
                        }
                        catch (ObjectDisposedException ex)
                        {
                            //Known exception to be ignored
                            //LoggingMethod.WriteToLog (string.Format ("DateTime {0} Error while accessing requestHeatbeatTimer object in TerminateLocalClientHeartbeatTimer {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelInfo);
                        }

                        Timer removedTimer = null;
                        bool removed = channelLocalClientHeartbeatTimer.TryRemove(requestUri, out removedTimer);
                        if (removed)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), requestUri.ToString()), LoggingMethod.LevelInfo);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), requestUri.ToString()), LoggingMethod.LevelInfo);
                        }
                    }
                }
            }
            else
            {
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
            ConcurrentDictionary<string, Timer> channelReconnectCollection = ChannelReconnectTimer;
            ICollection<string> keyCollection = channelReconnectCollection.Keys;
            if (keyCollection != null && keyCollection.Count > 0)
            {
                List<string> keyList = keyCollection.ToList();
                foreach (string key in keyList)
                {
                    if (ChannelReconnectTimer.ContainsKey(key))
                    {
                        Timer currentTimer = ChannelReconnectTimer[key];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = ChannelReconnectTimer.TryRemove(key, out removedTimer);
                        if (!removed)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channel reconnect timer reference from collection for {1}", DateTime.Now.ToString(), key.ToString()), LoggingMethod.LevelInfo);
                        }
                    }
                }
            }


            ConcurrentDictionary<string, Timer> channelGroupReconnectCollection = ChannelGroupReconnectTimer;
            ICollection<string> groupKeyCollection = channelGroupReconnectCollection.Keys;
            if (groupKeyCollection != null && groupKeyCollection.Count > 0)
            {
                List<string> groupKeyList = groupKeyCollection.ToList();
                foreach (string groupKey in groupKeyList)
                {
                    if (ChannelGroupReconnectTimer.ContainsKey(groupKey))
                    {
                        Timer currentTimer = ChannelGroupReconnectTimer[groupKey];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = ChannelGroupReconnectTimer.TryRemove(groupKey, out removedTimer);
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
            string[] channels = GetCurrentSubscriberChannels();
            if (channels != null)
            {
                string multiChannel = (channels.Length > 0) ? string.Join(",", channels) : ",";
                PubnubWebRequest request = (ChannelRequest.ContainsKey(multiChannel)) ? ChannelRequest[multiChannel] : null;
                if (request != null)
                {
                    request.Abort(null, _errorLevel);

                    LoggingMethod.WriteToLog(string.Format("DateTime {0} TerminateCurrentSubsciberRequest {1}", DateTime.Now.ToString(), request.RequestUri.ToString()), LoggingMethod.LevelInfo);
                }
            }
        }

        #endregion

    }
}
