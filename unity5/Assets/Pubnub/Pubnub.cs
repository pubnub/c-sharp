//Build Date: Feb. 3, 2015
//ver3.6.2/Unity5
#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID || UNITY_IOS || UNITY_5)
#define USE_JSONFX_UNITY_IOS
#endif
#if (__MonoCS__ && !UNITY_STANDALONE && !UNITY_WEBPLAYER)
#define TRACE
#endif
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pathfinding.Serialization.JsonFx;
using System.Security.Cryptography;
using System.Net;
using System.ComponentModel;

namespace PubNubMessaging.Core
{
    public class Pubnub
    {

        #region "Events"

        // Common property changed event
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged (string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler (this, new PropertyChangedEventArgs (propertyName));
            }
        }

        #endregion

        #region "Class variables"

        private GameObject gobj;
        private CoroutineClass subCoroutine;
        private CoroutineClass nonSubCoroutine;
        private CoroutineClass heartbeatCoroutine;
        private CoroutineClass presenceHeartbeatCoroutine;

        private string _origin = "pubsub.pubnub.com";
        private string publishKey = "";
        private string subscribeKey = "";
        private string secretKey = "";
        private string cipherKey = "";
        private bool ssl = false;
        private static long lastSubscribeTimetoken = 0;
        private string parameters = "";
        private string subscribeParameters = "";
        private string presenceHeartbeatParameters = "";
        private string hereNowParameters = "";
        private string setUserStateparameters = "";
        private string globalHereNowParameters = "";
        private string _pnsdkVersion = "PubNub-CSharp-Unity5/3.6.2";

        private int _pubnubWebRequestCallbackIntervalInSeconds = 310;
        private int _pubnubOperationTimeoutIntervalInSeconds = 15;
        private int _pubnubHeartbeatTimeoutIntervalInSeconds = 10;
        private int _pubnubNetworkTcpCheckIntervalInSeconds = 15;
        private int _pubnubNetworkCheckRetries = 50;
        private int _pubnubWebRequestRetryIntervalInSeconds = 10;
        private int _pubnubPresenceHeartbeatInSeconds = 0;
        private int _presenceHeartbeatIntervalInSeconds = 0;
        private bool _enableResumeOnReconnect = true;
        private bool _uuidChanged = false;
        public bool overrideTcpKeepAlive = true;
        private bool _enableJsonEncodingForPublish = true;
        private LoggingMethod.Level _pubnubLogLevel = LoggingMethod.Level.Info;
        private PubnubErrorFilter.Level _errorLevel = PubnubErrorFilter.Level.Info;
        private SafeDictionary<string, long> multiChannelSubscribe = new SafeDictionary<string, long> ();
        private SafeDictionary<string, PubnubWebRequest> _channelRequest = new SafeDictionary<string, PubnubWebRequest> ();
        //private SafeDictionary<string, bool> channelInternetStatus = new SafeDictionary<string, bool> ();
        //private SafeDictionary<string, int> channelInternetRetry = new SafeDictionary<string, int> ();
        //SafeDictionary<string, Timer> _channelReconnectTimer = new SafeDictionary<string, Timer> ();
        //protected SafeDictionary<Uri, Timer> channelLocalClientHeartbeatTimer = new SafeDictionary<Uri, Timer> ();
        private SafeDictionary<PubnubChannelCallbackKey, object> channelCallbacks = new SafeDictionary<PubnubChannelCallbackKey, object> ();
        private SafeDictionary<string, Dictionary<string, object>> _channelLocalUserState = new SafeDictionary<string, Dictionary<string, object>> ();
        private SafeDictionary<string, Dictionary<string, object>> _channelUserState = new SafeDictionary<string, Dictionary<string, object>> ();
        private SafeDictionary<string, List<string>> _channelSubscribedAuthKeys = new SafeDictionary<string, List<string>> ();
        //private System.Threading.Timer localClientHeartBeatTimer;
        //private System.Threading.Timer presenceHeartbeatTimer = null;
        private static bool pubnetSystemActive = true;

        private bool keepHearbeatRunning = false;
        private bool isHearbeatRunning = false;

        private bool keepPresenceHearbeatRunning = false;
        private bool isPresenceHearbeatRunning = false;

        private bool internetStatus = true;
        private bool retriesExceeded = false;


        private int retryCount = 0;

        #endregion

        #region "Properties"

        private static bool _failClientNetworkForTesting = false;
        private static bool _machineSuspendMode = false;
        private static bool SimulateNetworkFailForTesting {
            get {
                return _failClientNetworkForTesting;
            }

            set {
                _failClientNetworkForTesting = value;
            }

        }

        private static bool MachineSuspendMode {
            get {
                return _machineSuspendMode;
            }
            set {
                _machineSuspendMode = value;
            }
        }

        public string Version {
            get {
                return _pnsdkVersion;
            }
            set {
                _pnsdkVersion = value;
            }
        }

        private List<object> _history = new List<object> ();

        public List<object> History {
            get { return _history; }
            set {
                _history = value;
                RaisePropertyChanged ("History");
            }
        }

        public int SubscribeTimeout {
            get {
                return _pubnubWebRequestCallbackIntervalInSeconds;
            }

            set {
                _pubnubWebRequestCallbackIntervalInSeconds = value;
            }
        }

        public int HeartbeatTimeout {
            get {
                return _pubnubHeartbeatTimeoutIntervalInSeconds;
            }

            set {
                _pubnubHeartbeatTimeoutIntervalInSeconds = value;
            }
        }

        public int NonSubscribeTimeout {
            get {
                return _pubnubOperationTimeoutIntervalInSeconds;
            }

            set {
                _pubnubOperationTimeoutIntervalInSeconds = value;
            }
        }

        public int NetworkCheckMaxRetries {
            get {
                return _pubnubNetworkCheckRetries;
            }

            set {
                _pubnubNetworkCheckRetries = value;
            }
        }

        public int NetworkCheckRetryInterval {
            get {
                return _pubnubWebRequestRetryIntervalInSeconds;
            }

            set {
                _pubnubWebRequestRetryIntervalInSeconds = value;
            }
        }

        public int LocalClientHeartbeatInterval {
            get {
                return _pubnubNetworkTcpCheckIntervalInSeconds;
            }

            set {
                _pubnubNetworkTcpCheckIntervalInSeconds = value;
            }
        }

        public bool EnableResumeOnReconnect {
            get {
                return _enableResumeOnReconnect;
            }
            set {
                _enableResumeOnReconnect = value;
            }
        }

        public bool EnableJsonEncodingForPublish {
            get {
                return _enableJsonEncodingForPublish;
            }
            set {
                _enableJsonEncodingForPublish = value;
            }
        }

        private string _authenticationKey = "";

        public string AuthenticationKey {
            get {
                return _authenticationKey;
            }

            set {
                _authenticationKey = value;
            }
        }

        private IPubnubUnitTest _pubnubUnitTest;

        public virtual IPubnubUnitTest PubnubUnitTest {
            get {
                return _pubnubUnitTest;
            }
            set {
                _pubnubUnitTest = value;
            }
        }

        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public IJsonPluggableLibrary JsonPluggableLibrary {
            get {
                return _jsonPluggableLibrary;
            }

            set {
                _jsonPluggableLibrary = value;
                if (_jsonPluggableLibrary is IJsonPluggableLibrary) {
                    this._jsonPluggableLibrary = _jsonPluggableLibrary;
                } else {
                    _jsonPluggableLibrary = null;
                    throw new ArgumentException ("Missing or Incorrect JsonPluggableLibrary value");
                }
            }
        }

        public string Origin {
            get {
                return _origin;
            }

            set {
                _origin = value;
            }
        }

        private string sessionUUID = "";

        public string SessionUUID {
            get {
                return sessionUUID;
            }
            set {
                sessionUUID = value;
            }
        }

        /// <summary>
        /// This property sets presence expiry timeout.
        /// Presence expiry value in seconds.
        /// </summary>
        public int PresenceHeartbeat {
            get {
                return _pubnubPresenceHeartbeatInSeconds;
            }

            set {
                if (value <= 0 || value > 320) {
                    _pubnubPresenceHeartbeatInSeconds = 0;
                } else {
                    _pubnubPresenceHeartbeatInSeconds = value;
                }
                if (_pubnubPresenceHeartbeatInSeconds != 0) {
                    _presenceHeartbeatIntervalInSeconds = (_pubnubPresenceHeartbeatInSeconds / 2) - 1;
                }
            }
        }

        public int PresenceHeartbeatInterval {
            get {
                return _presenceHeartbeatIntervalInSeconds;
            }

            set {
                _presenceHeartbeatIntervalInSeconds = value;
                if (_presenceHeartbeatIntervalInSeconds >= _pubnubPresenceHeartbeatInSeconds) {
                    _presenceHeartbeatIntervalInSeconds = (_pubnubPresenceHeartbeatInSeconds / 2) - 1;
                }
            }
        }

        public LoggingMethod.Level PubnubLogLevel {
            get {
                return _pubnubLogLevel;
            }

            set {
                _pubnubLogLevel = value;
                LoggingMethod.LogLevel = _pubnubLogLevel;
            }
        }

        public PubnubErrorFilter.Level PubnubErrorLevel {
            get {
                return _errorLevel;
            }

            set {
                _errorLevel = value;
                PubnubErrorFilter.ErrorLevel = _errorLevel;
            }
        }

        #endregion

        #region "Constructors"

        public Pubnub (string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
        {
            this.Init (publishKey, subscribeKey, secretKey, cipherKey, sslOn);
        }

        public Pubnub (string publishKey, string subscribeKey, string secretKey)
        {
            this.Init (publishKey, subscribeKey, secretKey, "", false);
        }

        public Pubnub (string publishKey, string subscribeKey)
        {
            this.Init (publishKey, subscribeKey, "", "", false);
        }

        #endregion

        #region "Init"

        private void Init (string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
        {
            #if (USE_MiniJSON)
            LoggingMethod.WriteToLog("USE_MiniJSON", LoggingMethod.LevelInfo);
            this.JsonPluggableLibrary = new MiniJSONObjectSerializer();
            #elif (USE_JSONFX_UNITY_IOS)
            LoggingMethod.WriteToLog ("USE_JSONFX_UNITY_IOS", LoggingMethod.LevelInfo);
            this.JsonPluggableLibrary = new JsonFxUnitySerializer ();
            #endif

            #if(UNITY_IOS)
            this.Version = "PubNub-CSharp-Unity5IOS/3.6.2";
            #elif(UNITY_STANDALONE_WIN)
            this.Version = "PubNub-CSharp-Unity5Win/3.6.2";
            #elif(UNITY_STANDALONE_OSX)
            this.Version = "PubNub-CSharp-Unity5OSX/3.6.2";
            #elif(UNITY_ANDROID)
            this.Version = "PubNub-CSharp-Unity5Android/3.6.2";
            #elif(UNITY_STANDALONE_LINUX)
            this.Version = "PubNub-CSharp-Unity5Linux/3.6.2";
            #else
            this.Version = "PubNub-CSharp-Unity5/3.6.2";
            #endif

            gobj = new GameObject ();            
            subCoroutine = gobj.AddComponent<CoroutineClass> ();
            nonSubCoroutine = gobj.AddComponent<CoroutineClass> ();
            heartbeatCoroutine = gobj.AddComponent<CoroutineClass> ();
            presenceHeartbeatCoroutine = gobj.AddComponent<CoroutineClass> ();

            LoggingMethod.LogLevel = _pubnubLogLevel;
            PubnubErrorFilter.ErrorLevel = _errorLevel;

            this.publishKey = publishKey;
            this.subscribeKey = subscribeKey;
            this.secretKey = secretKey;
            this.cipherKey = cipherKey;
            this.ssl = sslOn;

            retriesExceeded = false;
            internetStatus = true;

            VerifyOrSetSessionUUID ();
        }

        #endregion

        #region "PubNub API Channel Methods"

        #region "Subscribe"
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

        public void Subscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
        {
            Subscribe<object> (channel, userCallback, connectCallback, errorCallback);
        }
        #endregion

        #region "Publish"

        public bool Publish<T>(string channel, object message, bool storeInHistory, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(this.publishKey) || string.IsNullOrEmpty(this.publishKey.Trim()) || this.publishKey.Length <= 0) {
                throw new MissingMemberException("Invalid publish key");
            }
            if (userCallback == null) {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null) {
                throw new ArgumentException("Missing errorCallback");
            }
            if (_jsonPluggableLibrary == null) {
                throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
            }

            Uri request = BuildPublishRequest(channel, message, storeInHistory);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
            requestState.Type = ResponseType.Publish;
            requestState.UserCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            return UrlProcessRequest<T>(request, requestState);
        }

        public bool Publish(string channel, object message, bool storeInHistory, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return Publish<object>(channel, message, storeInHistory, userCallback, errorCallback);
        }

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
            return Publish<object> (channel, message, true, userCallback, errorCallback);
        }

        public bool Publish<T> (string channel, object message, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return Publish<T>(channel, message, true, userCallback, errorCallback);
        }

        private Uri BuildPublishRequest (string channel, object originalMessage, bool storeInHistory)
        {
            string message = (_enableJsonEncodingForPublish) ? JsonEncodePublishMsg (originalMessage) : originalMessage.ToString ();

            parameters = (storeInHistory) ? "" : "store=0";

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

        #region "Presence"
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
        #endregion

        #region "Detailed History"

        /*      *
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
            StringBuilder parameterBuilder = new StringBuilder();
            parameters = "";
            if (count <= -1)
                count = 100;

            parameterBuilder.AppendFormat("?count={0}", count);
            if (reverse) {
                parameterBuilder.AppendFormat ("&reverse={0}", reverse.ToString ().ToLower ());
            }
            if (start != -1) {
                parameterBuilder.AppendFormat("&start={0}", start.ToString().ToLower());
            }
            if (end != -1) {
                parameterBuilder.AppendFormat("&end={0}", end.ToString().ToLower());
            }
            if (!string.IsNullOrEmpty (_authenticationKey)) {
                parameterBuilder.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, ResponseType.DetailedHistory, false, false));
            }

            parameterBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(sessionUUID, ResponseType.DetailedHistory, false, false));
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

        #region "HereNow"

        public bool HereNow (string channel, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return HereNow<object> (channel, true, false, userCallback, errorCallback);
        }

        public bool HereNow<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return HereNow<T> (channel, true, false, userCallback, errorCallback);
        }

        public bool HereNow<T> (string channel, bool showUUIDList, bool includeUserState, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return HereNow<T>  (channel, showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public bool HereNow<T> (string channel, bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
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
        #endregion

        #region "Global Here Now"

        public void GlobalHereNow (Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            GlobalHereNow<object> (true, false, userCallback, errorCallback);
        }

        public bool GlobalHereNow<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GlobalHereNow<T> (true, false, userCallback, errorCallback);
        }

        public bool GlobalHereNow<T> (bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
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

        public void WhereNow (string uuid, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            WhereNow<object> (uuid, userCallback, errorCallback);
        }

        public void WhereNow<T> (string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
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
        #endregion

        #region "Time"

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

        private Uri BuildTimeRequest ()
        {
            List<string> url = new List<string> ();

            url.Add ("time");
            url.Add ("0");

            return BuildRestApiRequest<Uri> (url, ResponseType.Time);
        }
        #endregion

        #region "PAM"

        public static long TranslateDateTimeToSeconds (DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64 (timeSpan.TotalSeconds);
            return timeStamp;
        }

        private Uri BuildGrantAccessRequest (string channel, string authenticationKey, bool read, bool write, int ttl)
        {
            string signature = "0";
            long timeStamp = TranslateDateTimeToSeconds (DateTime.UtcNow);
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder ();
            if (!string.IsNullOrEmpty (authenticationKey)) {
                queryStringBuilder.AppendFormat ("auth={0}", EncodeUricomponent (authenticationKey, ResponseType.GrantAccess, false, false));
            }

            if (!string.IsNullOrEmpty (channel)) {
                queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent (channel, ResponseType.GrantAccess, false, false));
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

        private Uri BuildAuditAccessRequest (string channel, string authenticationKey)
        {
            string signature = "0";
            long timeStamp = ((_pubnubUnitTest == null) || (_pubnubUnitTest is IPubnubUnitTest && !_pubnubUnitTest.EnableStubTest))
                ? TranslateDateTimeToSeconds (DateTime.UtcNow) 
                : TranslateDateTimeToSeconds (new DateTime (2013, 01, 01));
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder ();
            if (!string.IsNullOrEmpty (authenticationKey)) {
                queryStringBuilder.AppendFormat ("auth={0}", EncodeUricomponent (authenticationKey, ResponseType.AuditAccess, false, false));
            }
            if (!string.IsNullOrEmpty (channel)) {
                queryStringBuilder.AppendFormat ("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent (channel, ResponseType.AuditAccess, false, false));
            }
            queryStringBuilder.AppendFormat("{0}pnsdk={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(_pnsdkVersion, ResponseType.AuditAccess, false, true));
            queryStringBuilder.AppendFormat ("{0}timestamp={1}", (queryStringBuilder.Length > 0) ? "&" : "", timeStamp.ToString ());
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

        #region "Grant Access"
        public bool GrantAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantAccess (channel, "", read, write, -1, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantAccess<T> (channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantAccess (channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty (this.secretKey) || string.IsNullOrEmpty (this.secretKey.Trim ()) || this.secretKey.Length <= 0) {
                throw new MissingMemberException ("Invalid secret key");
            }

            Uri request = BuildGrantAccessRequest (channel, authenticationKey, read, write, ttl);

            RequestState<T> requestState = new RequestState<T> ();
            requestState.Channels = new string[] { channel };
            requestState.Type = ResponseType.GrantAccess;
            requestState.UserCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            return UrlProcessRequest<T> (request, requestState); 
        }
        #endregion

        #region "Grant Presence Access"
        public bool GrantPresenceAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantPresenceAccess (channel, "", read, write, -1, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantPresenceAccess (channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return GrantPresenceAccess<T> (channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannels = channel.Split (',');
            if (multiChannels.Length > 0) {
                for (int index = 0; index < multiChannels.Length; index++) {
                    if (!string.IsNullOrEmpty (multiChannels [index]) && multiChannels [index].Trim ().Length > 0) {
                        multiChannels [index] = string.Format ("{0}-pnpres", multiChannels [index]);
                    } else {
                        throw new MissingMemberException ("Invalid channel");
                    }
                }
            }
            string presenceChannel = string.Join (",", multiChannels);
            return GrantAccess (presenceChannel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        #endregion

        #region "Audit Access"
        public void AuditAccess<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditAccess ("", "", userCallback, errorCallback);
        }

        public void AuditAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditAccess (channel, "", userCallback, errorCallback);
        }

        public void AuditAccess<T> (string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty (this.secretKey) || string.IsNullOrEmpty (this.secretKey.Trim ()) || this.secretKey.Length <= 0) {
                throw new MissingMemberException ("Invalid secret key");
            }

            Uri request = BuildAuditAccessRequest (channel, authenticationKey);

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
        #endregion

        #region "Audit Presence"
        public void AuditPresenceAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditPresenceAccess (channel, "", userCallback, errorCallback);
        }

        public void AuditPresenceAccess<T> (string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannels = channel.Split (',');
            if (multiChannels.Length > 0) {
                for (int index = 0; index < multiChannels.Length; index++) {
                    multiChannels [index] = string.Format ("{0}-pnpres", multiChannels [index]);
                }
            }
            string presenceChannel = string.Join (",", multiChannels);
            AuditAccess (presenceChannel, authenticationKey, userCallback, errorCallback);
        }
        #endregion

        #endregion

        #region "Set User State"
        public void SetUserState<T> (string channel, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty (channel) || string.IsNullOrEmpty (channel.Trim ())) {
                throw new ArgumentException ("Missing Channel");
            }
            if (string.IsNullOrEmpty (jsonUserState) || string.IsNullOrEmpty (jsonUserState.Trim ())) {
                throw new ArgumentException ("Missing User State");
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

            if (!_jsonPluggableLibrary.IsDictionaryCompatible (jsonUserState)) {
                throw new MissingMemberException ("Missing json format for user state");
            } else {
                Dictionary<string, object> deserializeUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonUserState);
                if (deserializeUserState == null) {
                    throw new MissingMemberException ("Missing json format user state");
                } else {
                    string oldJsonState = GetLocalUserState (channel);
                    if (oldJsonState == jsonUserState) {
                        string message = "No change in User State";

                        CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            channel, errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
                        return;
                    }

                }
            }

            SharedSetUserState (channel, uuid, jsonUserState, userCallback, errorCallback);
        }

        public void SetUserState<T> (string channel, string uuid, KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
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

            string key = keyValuePair.Key;

            int valueInt;
            double valueDouble;
            string currentUserState = "";

            string oldJsonState = GetLocalUserState (channel);
            if (keyValuePair.Value == null) {
                currentUserState = SetLocalUserState (channel, key, null);
            } else if (Int32.TryParse (keyValuePair.Value.ToString (), out valueInt)) {
                currentUserState = SetLocalUserState (channel, key, valueInt);
            } else if (Double.TryParse (keyValuePair.Value.ToString (), out valueDouble)) {
                currentUserState = SetLocalUserState (channel, key, valueDouble);
            } else {
                currentUserState = SetLocalUserState (channel, key, keyValuePair.Value.ToString ());
            }

            if (oldJsonState == currentUserState) {
                string message = "No change in User State";

                CallErrorCallback (PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                    channel, errorCallback, message, PubnubErrorCode.UserStateUnchanged, null, null);
                return;
            }

            if (currentUserState.Trim () == "") {
                currentUserState = "{}";
            }

            SharedSetUserState<T> (channel, uuid, currentUserState, userCallback, errorCallback);
        }

        /*public void SetUserState<T> (string channel, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            //pubnub.SetUserState<T> (channel, uuid, jsonUserState, userCallback, errorCallback);
        }*/

        public void SetUserState<T> (string channel, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            //pubnub.SetUserState<T> (channel, "", jsonUserState, userCallback, errorCallback);
        }

        /*public void SetUserState<T> (string channel, string uuid, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            //pubnub.SetUserState<T> (channel, uuid, keyValuePair, userCallback, errorCallback);
        }*/

        public void SetUserState<T> (string channel, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            //pubnub.SetUserState<T> (channel, "", keyValuePair, userCallback, errorCallback);
        }
        #endregion

        #region "Get User State"
        public void GetUserState<T> (string channel, string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
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
            requestState.Type = ResponseType.GetUserState;
            requestState.UserCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T> (request, requestState);
        }

        public void GetUserState<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GetUserState<T> (channel, "", userCallback, errorCallback);
        }
        #endregion    

        #region "PubNub API Other Methods"

        public void TerminateCurrentSubscriberRequest ()
        {
            //pubnub.TerminateCurrentSubscriberRequest ();
            subCoroutine.BounceRequest ();
        }

        /*public void EnableSimulateNetworkFailForTestingOnly ()
        {
            //pubnub.EnableSimulateNetworkFailForTestingOnly ();
        }

        public void DisableSimulateNetworkFailForTestingOnly ()
        {
            //pubnub.DisableSimulateNetworkFailForTestingOnly ();
        }

        public void EnableMachineSleepModeForTestingOnly ()
        {
            //pubnub.EnableMachineSleepModeForTestingOnly ();
        }

        public void DisableMachineSleepModeForTestingOnly ()
        {
            //pubnub.DisableMachineSleepModeForTestingOnly ();
        }*/

        public void EndPendingRequests ()
        {
            //pubnub.EndPendingRequests ();
            RemoveChannelCallback ();
            RemoveUserState ();
            subCoroutine.BounceRequest ();
            LoggingMethod.WriteToLog (string.Format ("DateTime {0} Request bounced.", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
            StopHeartbeat ();
            LoggingMethod.WriteToLog (string.Format ("DateTime {0} StopHeartbeat.", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
            StopPresenceHeartbeat ();
            LoggingMethod.WriteToLog (string.Format ("DateTime {0} StopPresenceHeartbeat.", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
            UnityEngine.Object.Destroy (subCoroutine);
            UnityEngine.Object.Destroy (nonSubCoroutine);
            UnityEngine.Object.Destroy (heartbeatCoroutine);
            UnityEngine.Object.Destroy (presenceHeartbeatCoroutine);
            UnityEngine.Object.Destroy (gobj);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0} Clean up complete.", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
        }

        private void RemoveChannelCallback ()
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

        private void RemoveUserState ()
        {
            ICollection<string> channelLocalUserStateCollection = _channelLocalUserState.Keys;
            ICollection<string> channelUserStateCollection = _channelUserState.Keys;

            foreach (string key in channelLocalUserStateCollection) {
                if (_channelLocalUserState.ContainsKey (key)) {
                    Dictionary<string, object> tempUserState;
                    bool removeKey = _channelLocalUserState.TryRemove (key, out tempUserState);
                    if (removeKey) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelInfo);
                    } else {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelError);
                    }
                }
            }

            foreach (string key in channelUserStateCollection) {
                if (_channelUserState.ContainsKey (key)) {
                    Dictionary<string, object> tempUserState;
                    bool removeKey = _channelUserState.TryRemove (key, out tempUserState);
                    if (removeKey) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelInfo);
                    } else {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Unable to RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString (), removeKey), LoggingMethod.LevelError);
                    }
                }
            }
        }

        public Guid GenerateGuid ()
        {
            return Guid.NewGuid ();
        }

        public void ChangeUUID (string newUUID)
        {
            //pubnub.ChangeUUID (newUUID);
        }

        public static long TranslateDateTimeToPubnubUnixNanoSeconds (DateTime dotNetUTCDateTime)
        {
            return TranslateDateTimeToPubnubUnixNanoSeconds (dotNetUTCDateTime);
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime (long unixNanoSecondTime)
        {
            return TranslatePubnubUnixNanoSecondsToDateTime (unixNanoSecondTime);
        }

        #endregion

        #endregion

        #region "Heartbeats"

        /*protected virtual void TerminateLocalClientHeartbeatTimer ()
        {
            TerminateLocalClientHeartbeatTimer (null);
        }

        protected virtual void TerminateLocalClientHeartbeatTimer (Uri requestUri)
        {
           
        }*/

        void StopHeartbeat ()
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Stopping Heartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            keepHearbeatRunning = false;
            isHearbeatRunning = false;
        }

        void StopPresenceHeartbeat ()
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Stopping PresenceHeartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            keepPresenceHearbeatRunning = false;
            isPresenceHearbeatRunning = false;
        }

        void RunPresenceHeartbeat<T> (bool pause, int pauseTime, RequestState<T> pubnubRequestState)
        {
            keepPresenceHearbeatRunning = true;
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, In RunPresenceHeartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            if (!isPresenceHearbeatRunning) {
                string[] subscriberChannels = pubnubRequestState.Channels.Where (s => s.Contains ("-pnpres") == false).ToArray ();

                if (subscriberChannels != null && subscriberChannels.Length > 0) {

                    isPresenceHearbeatRunning = true;

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Running RunPresenceHeartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);

                    Uri requestUrl = BuildPresenceHeartbeatRequest (subscriberChannels);

                    presenceHeartbeatCoroutine.CoroutineComplete += CoroutineCompleteHandler<T>;
                    RequestState<T> requestState = new RequestState<T> ();
                    requestState.Channels = pubnubRequestState.Channels;
                    requestState.Type = ResponseType.PresenceHeartbeat;
                    requestState.UserCallback = null;
                    requestState.ErrorCallback = pubnubRequestState.ErrorCallback;
                    //for heartbeat and presence heartbeat treat reconnect as pause
                    requestState.Reconnect = pause;

                    presenceHeartbeatCoroutine.Run<T> (requestUrl.OriginalString, requestState, HeartbeatTimeout, pauseTime);
                }
            } else {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PresenceHeartbeat Running ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            }
        }

        void RunHeartbeat<T> (bool pause, int pauseTime)
        {
            keepHearbeatRunning = true;
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, In Runheartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            if (!isHearbeatRunning) {
                isHearbeatRunning = true;
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Running Runheartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                Uri requestUrl = BuildTimeRequest ();

                heartbeatCoroutine.CoroutineComplete += CoroutineCompleteHandler<T>;
                RequestState<T> requestState = new RequestState<T> ();
                requestState.Channels = null;
                requestState.Type = ResponseType.Heartbeat;
                requestState.UserCallback = null;
                requestState.ErrorCallback = null;
                //for heartbeat and presence heartbeat treat reconnect as pause
                requestState.Reconnect = pause;

                heartbeatCoroutine.Run<T> (requestUrl.OriginalString, requestState, HeartbeatTimeout, pauseTime);
            } else {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Heartbeat Running ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            }
        }
        #endregion

        #region "CoroutineHandlers"

        void CoroutineCompleteHandler<T> (object sender, EventArgs ea)
        {
            CustomEventArgs<T> cea = ea as CustomEventArgs<T>;
            try {
                if(cea.pubnubRequestState.Type == ResponseType.Heartbeat){
                    isHearbeatRunning = false;
                    if (cea.isTimeout || cea.isError) {
                        RetryLoop();
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Heartbeat timeout={1}", DateTime.Now.ToString (), cea.message.ToString ()), LoggingMethod.LevelError);
                    } else {
                        if(retryCount > 0){
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0} Internet Connection Available.", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                        }
                        retryCount = 0;
                        internetStatus = true;
                        retriesExceeded = false;
                    }
                    if(keepHearbeatRunning){
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Restarting Heartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                        if(internetStatus){
                            RunHeartbeat<T>(true, LocalClientHeartbeatInterval);
                        } else {
                            RunHeartbeat<T>(true, NetworkCheckRetryInterval);
                        }
                    }
                } if(cea.pubnubRequestState.Type == ResponseType.PresenceHeartbeat){
                    isPresenceHearbeatRunning = false;
                    if (cea.isTimeout || cea.isError) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Presence Heartbeat timeout={1}", DateTime.Now.ToString (), cea.message.ToString ()), LoggingMethod.LevelError);
                    } else {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Presence Heartbeat response: {1}", DateTime.Now.ToString (), cea.message.ToString ()), LoggingMethod.LevelInfo);
                    }
                    if(keepPresenceHearbeatRunning){
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Restarting PresenceHeartbeat ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                        RunPresenceHeartbeat<T>(true, PresenceHeartbeatInterval, cea.pubnubRequestState);
                    }
                } else if (cea.pubnubRequestState.Type == ResponseType.Subscribe || cea.pubnubRequestState.Type == ResponseType.Presence) {
                    if (cea.isTimeout) {
                        OnPubnubWebRequestTimeout<T> (cea.pubnubRequestState, true);
                        UrlRequestCommonExceptionHandler<T> (cea.pubnubRequestState.Type, cea.pubnubRequestState.Channels, true, cea.pubnubRequestState.UserCallback, cea.pubnubRequestState.ConnectCallback, cea.pubnubRequestState.ErrorCallback, false);
                    } else if (cea.isError) {
                        UrlRequestCommonExceptionHandler<T> (cea.pubnubRequestState.Type, cea.pubnubRequestState.Channels, false, cea.pubnubRequestState.UserCallback, cea.pubnubRequestState.ConnectCallback, cea.pubnubRequestState.ErrorCallback, false);
                    } else {
                        UrlProcessResponseCallbackNonAsync<T>(cea);
                    }
                } else {
                    if (cea.isTimeout) {
                        OnPubnubWebRequestTimeout<T> (cea.pubnubRequestState, true);
                        UrlRequestCommonExceptionHandler<T> (cea.pubnubRequestState.Type, cea.pubnubRequestState.Channels, true, cea.pubnubRequestState.UserCallback, cea.pubnubRequestState.ConnectCallback, cea.pubnubRequestState.ErrorCallback, false);
                    } else if (cea.isError) {
                        UrlRequestCommonExceptionHandler<T> (cea.pubnubRequestState.Type, cea.pubnubRequestState.Channels, false, cea.pubnubRequestState.UserCallback, cea.pubnubRequestState.ConnectCallback, cea.pubnubRequestState.ErrorCallback, false);
                    } else {
                        var result = WrapResultBasedOnResponseType<T> (cea.pubnubRequestState.Type, cea.message, cea.pubnubRequestState.Channels, cea.pubnubRequestState.Reconnect, cea.pubnubRequestState.Timetoken, cea.pubnubRequestState.ErrorCallback);
                        ProcessResponseCallbacks<T> (result, cea.pubnubRequestState);            
                    }
                }
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0} Exception={1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            CoroutineClass coroutine = sender as CoroutineClass;
            coroutine.CoroutineComplete -= CoroutineCompleteHandler<T>;
        }

        private void ProcessResponseCallbackExceptionHandler<T> (Exception ex, RequestState<T> asynchRequestState)
        {
            //common Exception handler
            /*if (asynchRequestState.Response != null)
                asynchRequestState.Response.Close ();*/

            LoggingMethod.WriteToLog (string.Format ("DateTime {0} Exception= {1} for URL: {2}", DateTime.Now.ToString (), ex.ToString (), asynchRequestState.Request.RequestUri.ToString ()), LoggingMethod.LevelError);
            UrlRequestCommonExceptionHandler<T> (asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout, asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, false);
        }

        private void ProcessResponseCallbackWebExceptionHandler<T> (WebException webEx, RequestState<T> asynchRequestState, string channel)
        {
            bool reconnect = false;
            if (webEx.ToString ().Contains ("Aborted")) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WebException: {1}", DateTime.Now.ToString (), webEx.ToString ()), LoggingMethod.LevelInfo);
            }else {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WebException: {1}", DateTime.Now.ToString (), webEx.ToString ()), LoggingMethod.LevelError);
            }
            /*if (asynchRequestState != null) {
                if (asynchRequestState.Response != null)
                    asynchRequestState.Response.Close ();
                if (asynchRequestState.Request != null)
                    TerminatePendingWebRequest (asynchRequestState);
            }*/
            //reconnect = HandleWebException (webEx, asynchRequestState, channel);

            UrlRequestCommonExceptionHandler<T> (asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.Timeout,
                asynchRequestState.UserCallback, asynchRequestState.ConnectCallback, asynchRequestState.ErrorCallback, reconnect);
        }

        private void UrlProcessResponseCallbackNonAsync<T> (CustomEventArgs<T> cea)
        {
            List<object> result = new List<object> ();

            RequestState<T> requestState = cea.pubnubRequestState;

            string channel = "";
            if (requestState != null && requestState.Channels != null) {
                channel = string.Join (",", requestState.Channels);
            }
            try {
                //if (requestState.Request != null) {
                    if (cea.isError) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Message: {1}", DateTime.Now.ToString (), cea.message), LoggingMethod.LevelError);
                        WebException webEx;
                        if ((cea.message.Contains ("NameResolutionFailure")
                            || cea.message.Contains ("ConnectFailure")
                            || cea.message.Contains ("ServerProtocolViolation")
                            || cea.message.Contains ("ProtocolError")
                        )) {
                            webEx = new WebException ("Network connnect error", WebExceptionStatus.ConnectFailure);
                        } else {
                            webEx = new WebException (cea.message);
                        }
                        ProcessResponseCallbackWebExceptionHandler<T> (webEx, requestState, channel);
                    } else {
                        //base.channelInternetStatus.AddOrUpdate (channel, true, (key, oldValue) => true);

                        string jsonString = cea.message;
                        if (overrideTcpKeepAlive) {
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Aborting previous subscribe/presence requests having channel(s) UrlProcessResponseCallbackNonAsync", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                            //TerminateLocalClientHeartbeatTimer (requestState.Request.RequestUri);
                            subCoroutine.BounceRequest();
                        }

                        if (jsonString != "[]") {
                            result = WrapResultBasedOnResponseType<T> (requestState.Type, jsonString, requestState.Channels, requestState.Reconnect, requestState.Timetoken, requestState.ErrorCallback);
                        }
                        ProcessResponseCallbacks<T> (result, requestState);
                        if (requestState.Type == ResponseType.Subscribe || requestState.Type == ResponseType.Presence) {
                            foreach (string currentChannel in requestState.Channels) {
                                multiChannelSubscribe.AddOrUpdate (currentChannel, Convert.ToInt64 (result [1].ToString ()), (key, oldValue) => Convert.ToInt64 (result [1].ToString ()));
                            }
                        }
                        switch (requestState.Type) {
                        case ResponseType.Subscribe:
                        case ResponseType.Presence:
                            MultiplexInternalCallback<T> (requestState.Type, result, requestState.UserCallback, requestState.ConnectCallback, requestState.ErrorCallback);
                            break;
                        default:
                            break;
                        }
                    }
                //} else {
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Request aborted for channel={1}", DateTime.Now.ToString (), requestState.Channels), LoggingMethod.LevelInfo);
                //}
            } catch (WebException webEx) {

                //TODO:fetch response code
                PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (webEx.Status, webEx.Message);
                int statusCode = (int)errorType;
                string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);
                if (requestState.Channels != null || requestState.Type == ResponseType.Time) {
                    if (requestState.Type == ResponseType.Subscribe
                        || requestState.Type == ResponseType.Presence) {
                        if (webEx.Message.IndexOf ("The request was aborted: The request was canceled") == -1
                            || webEx.Message.IndexOf ("Machine suspend mode enabled. No request will be processed.") == -1) {
                            for (int index = 0; index < requestState.Channels.Length; index++) {
                                string activeChannel = requestState.Channels [index].ToString ();
                                PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
                                callbackKey.Channel = activeChannel;
                                callbackKey.Type = requestState.Type;

                                if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
                                    object callbackObject;
                                    bool channelAvailable = channelCallbacks.TryGetValue (callbackKey, out callbackObject);
                                    PubnubChannelCallback<T> currentPubnubCallback = null;
                                    if (channelAvailable) {
                                        currentPubnubCallback = callbackObject as PubnubChannelCallback<T>;
                                    }
                                    if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
                                        PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Warn, true, webEx.Message, webEx, PubnubMessageSource.Client, requestState.Request, requestState.Response, errorDescription, activeChannel);
                                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString (), error.ToString ()), LoggingMethod.LevelInfo);
                                        GoToCallback (error, currentPubnubCallback.ErrorCallback);
                                    }
                                }

                            }
                        }
                    } else {
                        PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Warn, true, webEx.Message, webEx, PubnubMessageSource.Client, requestState.Request, requestState.Response, errorDescription, channel);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, PubnubClientError = {1}", DateTime.Now.ToString (), error.ToString ()), LoggingMethod.LevelInfo);
                        GoToCallback (error, requestState.ErrorCallback);
                    }
                }
                ProcessResponseCallbackWebExceptionHandler<T> (webEx, requestState, channel);
            } catch (Exception ex) {
                if (requestState.Channels != null) {
                    if (requestState.Type == ResponseType.Subscribe
                        || requestState.Type == ResponseType.Presence) {
                        for (int index = 0; index < requestState.Channels.Length; index++) {
                            string activeChannel = requestState.Channels [index].ToString ();
                            PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey ();
                            callbackKey.Channel = activeChannel;
                            callbackKey.Type = requestState.Type;


                            if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey (callbackKey)) {
                                PubnubChannelCallback<T> currentPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<T>;
                                PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);
                                int statusCode = (int)errorType;
                                string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);
                                PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Critical, true, ex.Message, ex, PubnubMessageSource.Client, requestState.Request, requestState.Response, errorDescription, channel);

                                if (currentPubnubCallback != null && currentPubnubCallback.ErrorCallback != null) {
                                    GoToCallback (error, currentPubnubCallback.ErrorCallback);
                                }
                            }
                        }
                    } else {
                        PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType (ex);
                        int statusCode = (int)errorType;
                        string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription (errorType);
                        PubnubClientError error = new PubnubClientError (statusCode, PubnubErrorSeverity.Critical, true, ex.Message, ex, PubnubMessageSource.Client, requestState.Request, requestState.Response, errorDescription, channel);


                        GoToCallback (error, requestState.ErrorCallback);
                    }

                }

                ProcessResponseCallbackExceptionHandler<T> (ex, requestState);
            }
        }
        #endregion

        #region BuildRequests

        private Uri BuildPresenceHeartbeatRequest (string[] channels)
        {
            presenceHeartbeatParameters = "";
            string channelsJsonState = BuildJsonUserState (channels, false);
            if (channelsJsonState != "{}" && channelsJsonState != "") {
                presenceHeartbeatParameters = string.Format ("&state={0}", EncodeUricomponent (channelsJsonState, ResponseType.PresenceHeartbeat, false, false));
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

        private Uri BuildMultiChannelSubscribeRequest (string[] channels, object timetoken)
        {
            subscribeParameters = "";
            string channelsJsonState = BuildJsonUserState (channels, false);
            if (channelsJsonState != "{}" && channelsJsonState != "") {
                subscribeParameters = string.Format ("&state={0}", EncodeUricomponent (channelsJsonState, ResponseType.Subscribe, false, false));
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

        #region "Exception Handlers"
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
        #endregion

        #region "User State"

        private string AddOrUpdateOrDeleteLocalUserState (string channel, string userStateKey, object userStateValue)
        {
            string retJsonUserState = "";

            Dictionary<string, object> userStateDictionary = null;

            if (_channelLocalUserState.ContainsKey (channel)) {
                userStateDictionary = _channelLocalUserState [channel];
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

                _channelLocalUserState.AddOrUpdate (channel, userStateDictionary, (oldData, newData) => userStateDictionary);
            } else {
                if (!string.IsNullOrEmpty (userStateKey) && userStateKey.Trim ().Length > 0 && userStateValue != null) {
                    userStateDictionary = new Dictionary<string, object> ();
                    userStateDictionary.Add (userStateKey, userStateValue);

                    _channelLocalUserState.AddOrUpdate (channel, userStateDictionary, (oldData, newData) => userStateDictionary);
                }
            }

            string jsonUserState = BuildJsonUserState (channel, true);
            if (jsonUserState != "") {
                retJsonUserState = string.Format ("{{{0}}}", jsonUserState);
            }
            return retJsonUserState;
        }

        private bool DeleteLocalUserState (string channel)
        {
            bool userStateDeleted = false;

            if (_channelLocalUserState.ContainsKey (channel)) {
                Dictionary<string, object> returnedUserState = null;
                userStateDeleted = _channelLocalUserState.TryRemove (channel, out returnedUserState);
            }

            return userStateDeleted;
        }

        private string BuildJsonUserState (string channel, bool local)
        {
            Dictionary<string, object> userStateDictionary = null;

            if (local) {
                if (_channelLocalUserState.ContainsKey (channel)) {
                    userStateDictionary = _channelLocalUserState [channel];
                }
            } else {
                if (_channelUserState.ContainsKey (channel)) {
                    userStateDictionary = _channelUserState [channel];
                }
            }

            StringBuilder jsonStateBuilder = new StringBuilder ();

            if (userStateDictionary != null) {
                string[] userStateKeys = userStateDictionary.Keys.ToArray<string> ();

                for (int keyIndex = 0; keyIndex < userStateKeys.Length; keyIndex++) {
                    string useStateKey = userStateKeys [keyIndex];
                    object userStateValue = userStateDictionary [useStateKey];
                    if (userStateValue == null) {
                        jsonStateBuilder.AppendFormat ("\"{0}\":{1}", useStateKey, string.Format ("\"{0}\"", "null"));
                    } else {
                        jsonStateBuilder.AppendFormat ("\"{0}\":{1}", useStateKey, (userStateValue.GetType ().ToString () == "System.String") ? string.Format ("\"{0}\"", userStateValue) : userStateValue);
                    }
                    if (keyIndex < userStateKeys.Length - 1) {
                        jsonStateBuilder.Append (",");
                    }
                }
            }

            return jsonStateBuilder.ToString ();
        }

        private string BuildJsonUserState (string[] channels, bool local)
        {
            string retJsonUserState = "";

            StringBuilder jsonStateBuilder = new StringBuilder ();

            if (channels != null) {
                for (int index = 0; index < channels.Length; index++) {
                    string currentJsonState = BuildJsonUserState (channels [index].ToString (), local);
                    if (!string.IsNullOrEmpty (currentJsonState)) {
                        currentJsonState = string.Format ("\"{0}\":{{{1}}}", channels [index].ToString (), currentJsonState);
                        if (jsonStateBuilder.Length > 0) {
                            jsonStateBuilder.Append (",");
                        }
                        jsonStateBuilder.Append (currentJsonState);
                    }
                }

                if (jsonStateBuilder.Length > 0) {
                    retJsonUserState = string.Format ("{{{0}}}", jsonStateBuilder.ToString ());
                }
            }

            return retJsonUserState;
        }

        private string SetLocalUserState (string channel, string userStateKey, int userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
        }

        private string SetLocalUserState (string channel, string userStateKey, double userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
        }

        private string SetLocalUserState (string channel, string userStateKey, string userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState (channel, userStateKey, userStateValue);
        }

        internal string GetLocalUserState (string channel)
        {
            string retJsonUserState = "";
            StringBuilder jsonStateBuilder = new StringBuilder ();

            jsonStateBuilder.Append (BuildJsonUserState (channel, false));
            if (jsonStateBuilder.Length > 0) {
                retJsonUserState = string.Format ("{{{0}}}", jsonStateBuilder.ToString ());
            }

            return retJsonUserState;
        }

        private void SharedSetUserState<T> (string channel, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty (uuid)) {
                VerifyOrSetSessionUUID ();
                uuid = this.sessionUUID;
            }

            Dictionary<string, object> deserializeUserState = _jsonPluggableLibrary.DeserializeToDictionaryOfObject (jsonUserState);
            if (_channelUserState != null) {
                _channelUserState.AddOrUpdate (channel.Trim (), deserializeUserState, (oldState, newState) => deserializeUserState);
            }
            if (_channelLocalUserState != null) {
                _channelLocalUserState.AddOrUpdate (channel.Trim (), deserializeUserState, (oldState, newState) => deserializeUserState);
            }

            Uri request = BuildSetUserStateRequest (channel, uuid, jsonUserState);

            RequestState<T> requestState = new RequestState<T> ();
            requestState.Channels = new string[] { channel };
            requestState.Type = ResponseType.SetUserState;
            requestState.UserCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T> (request, requestState);

            //bounce the long-polling subscribe requests to update user state
            TerminateCurrentSubscriberRequest ();
        }
            
        private Uri BuildSetUserStateRequest (string channel, string uuid, string jsonUserState)
        {
            setUserStateparameters = string.Format ("?state={0}", EncodeUricomponent (jsonUserState, ResponseType.SetUserState, false, false));

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

        #region "Helpers"

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
                        //TerminatePendingWebRequest (currentState);
                        subCoroutine.BounceRequest ();
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
                //TerminatePendingWebRequest (currentState);
                subCoroutine.BounceRequest ();
                LoggingMethod.WriteToLog (string.Format ("DateTime: {0}, **WP7 OnPubnubWebRequestTimeout**", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            }
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
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requestUriA: {1}", DateTime.Now.ToString (), requestUri.AbsoluteUri), LoggingMethod.LevelError);
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, requestUriO: {1}", DateTime.Now.ToString (), requestUri.OriginalString), LoggingMethod.LevelInfo);

                // Create Request
                /*PubnubWebRequestCreator requestCreator = new PubnubWebRequestCreator (_pubnubUnitTest);
                PubnubWebRequest request = (PubnubWebRequest)requestCreator.Create (requestUri);

                request = SetProxy<T> (request);
                request = SetTimeout<T> (pubnubRequestState, request);

                pubnubRequestState.Request = request;*/

                if (pubnubRequestState.Type == ResponseType.Subscribe || pubnubRequestState.Type == ResponseType.Presence) {
                    _channelRequest.AddOrUpdate (channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);

                    RunHeartbeat<T>(false, LocalClientHeartbeatInterval);

                    if (pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0 && pubnubRequestState.Channels.Where (s => s.Contains ("-pnpres") == false).ToArray ().Length > 0) {
                        if (PresenceHeartbeatInterval > 0) {
                            RunPresenceHeartbeat<T>(false, PresenceHeartbeatInterval, pubnubRequestState);
                        }
                    }
                    subCoroutine.CoroutineComplete += CoroutineCompleteHandler<T>;
                    subCoroutine.Run<T> (requestUri.OriginalString, pubnubRequestState, SubscribeTimeout, 0);
                } else {
                    nonSubCoroutine.CoroutineComplete += CoroutineCompleteHandler<T>;
                    nonSubCoroutine.Run<T> (requestUri.OriginalString, pubnubRequestState, NonSubscribeTimeout, 0);
                }

                /*if (overrideTcpKeepAlive) {
                    TimerWhenOverrideTcpKeepAlive (requestUri, pubnubRequestState);
                } else {
                    request = SetServicePointSetTcpKeepAlive (request);
                }*/

                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Request={1}", DateTime.Now.ToString (), requestUri.ToString ()), LoggingMethod.LevelInfo);

                //SendRequestAndGetResult (requestUri, pubnubRequestState, request);

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

                        /*if (webRequest != null) {
                            TerminateLocalClientHeartbeatTimer (webRequest.RequestUri);
                        }*/
                        StopHeartbeat ();

                        PubnubWebRequest removedRequest;
                        bool removedChannel = _channelRequest.TryRemove (multiChannelName, out removedRequest);
                        if (removedChannel) {
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Success to remove channel(s)={1} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
                        } else {
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to remove channel(s)={1} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
                        }
                        /*if (webRequest != null)
                            TerminatePendingWebRequest (webRequest, errorCallback);*/
                        subCoroutine.BounceRequest ();

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
                    /*if (presenceHeartbeatTimer != null) {
                        // Stop the presence heartbeat timer if there are no channels subscribed
                        presenceHeartbeatTimer.Dispose ();
                        presenceHeartbeatTimer = null;
                    }*/
                    if (isPresenceHearbeatRunning) {
                        StopPresenceHeartbeat ();
                    }
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                }
            }

        }

        private void ResetInternetCheckSettings (string[] channels)
        {
            if (channels == null)
                return;
            retryCount = 0;
            internetStatus = true;
            retriesExceeded = false;
        }

        private void MultiChannelSubscribeInit<T> (ResponseType type, string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
        {
            string[] rawChannels = channel.Split (',');
            List<string> validChannels = new List<string> ();

            bool networkConnection = internetStatus;

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

                        StopHeartbeat ();
                        //if (webRequest != null)
                            //TerminateLocalClientHeartbeatTimer (webRequest.RequestUri);

                        PubnubWebRequest removedRequest;
                        _channelRequest.TryRemove (multiChannelName, out removedRequest);
                        bool removedChannel = _channelRequest.TryRemove (multiChannelName, out removedRequest);
                        if (removedChannel) {
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Success to remove channel(s)={1} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
                        } else {
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Unable to remove channel(s)={1} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString (), multiChannelName), LoggingMethod.LevelInfo);
                        }
                        //if (webRequest != null)
                            //TerminatePendingWebRequest (webRequest, errorCallback);

                        subCoroutine.BounceRequest ();

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

            if (pubnetSystemActive && retriesExceeded) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Subscribe channel={1} - No internet connection. MAXed retries for internet ", DateTime.Now.ToString (), multiChannel), LoggingMethod.LevelInfo);
                MultiplexExceptionHandler<T> (type, channels, userCallback, connectCallback, errorCallback, true, false);
                retriesExceeded = false;
                return;
            }

            /*if (channelInternetStatus.ContainsKey (multiChannel) && (!channelInternetStatus [multiChannel]) && pubnetSystemActive) {
                if (channelInternetRetry.ContainsKey (multiChannel) && (channelInternetRetry [multiChannel] >= _pubnubNetworkCheckRetries)) {
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Subscribe channel={1} - No internet connection. MAXed retries for internet ", DateTime.Now.ToString (), multiChannel), LoggingMethod.LevelInfo);
                    MultiplexExceptionHandler<T> (type, channels, userCallback, connectCallback, errorCallback, true, false);
                    retriesExceeded = false;
                    return;
                }

                if (ReconnectNetworkIfOverrideTcpKeepAlive <T> (type, channels, timetoken, userCallback, connectCallback, errorCallback, multiChannel)) {
                    return;
                }

            }*/

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

        private void RetryLoop ()
        {
            internetStatus = false;
            retryCount++;
            if (retryCount <= NetworkCheckMaxRetries) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0} Internet Disconnected, retrying. Retry count {1} of {2}", DateTime.Now.ToString (), retryCount.ToString (), NetworkCheckMaxRetries), LoggingMethod.LevelError);
            } else {
                retriesExceeded = true;
                LoggingMethod.WriteToLog (string.Format ("DateTime {0} Internet Disconnected. Retries exceeded {1}. Unsubscribing connected channels.", DateTime.Now.ToString (), NetworkCheckMaxRetries), LoggingMethod.LevelError);

                //stop heartbeat.
                keepHearbeatRunning = false;
                //Unsubscribe channels here.
                subCoroutine.BounceRequest ();

                //TODO: Fire callbacks

                //TODO: reinit subscribe
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

        private void VerifyOrSetSessionUUID ()
        {
            if (string.IsNullOrEmpty (this.sessionUUID) || string.IsNullOrEmpty (this.sessionUUID.Trim ())) {
                this.sessionUUID = Guid.NewGuid ().ToString ();
            }
        }

        private bool IsUnsafe (char ch, bool ignoreComma)
        {
            if (ignoreComma) {
                return " ~`!@#$%^&*()+=[]\\{}|;':\"/<>?".IndexOf (ch) >= 0;
            } else {
                return " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?".IndexOf (ch) >= 0;
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

            uuid = EncodeUricomponent (uuid, type, false, false);

            // Add http or https based on SSL flag
            if (this.ssl) {
                url.Append ("https://");
            } else {
                url.Append ("http://");
            }

            // Add Origin To The Request
            url.Append (this.Origin);

            // Generate URL with UTF-8 Encoding
            for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++) {
                url.Append ("/");

                if (type == ResponseType.Publish && componentIndex == urlComponents.Count - 1) {
                    url.Append (EncodeUricomponent (urlComponents [componentIndex].ToString (), type, false, false));
                } else {
                    url.Append (EncodeUricomponent (urlComponents [componentIndex].ToString (), type, true, false));
                }
            }
            if (type == ResponseType.Presence || type == ResponseType.Subscribe || type == ResponseType.Leave) {
                        queryParamExist = true;
                        url.AppendFormat ("?uuid={0}", uuid);
                        url.Append (subscribeParameters);
                        if (!string.IsNullOrEmpty (_authenticationKey)) {
                                url.AppendFormat ("&auth={0}", EncodeUricomponent (_authenticationKey, type, false, false));
                        }
                        if (_pubnubPresenceHeartbeatInSeconds != 0) {
                                url.AppendFormat ("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }

                if (type == ResponseType.PresenceHeartbeat) {
                        queryParamExist = true;
                        url.AppendFormat ("?uuid={0}", uuid);
                        url.Append (presenceHeartbeatParameters);
                        if (_pubnubPresenceHeartbeatInSeconds != 0) {
                                url.AppendFormat ("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                        }
                        if (!string.IsNullOrEmpty (_authenticationKey)) {
                                url.AppendFormat ("&auth={0}", EncodeUricomponent (_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }
                if (type == ResponseType.SetUserState) {
                        queryParamExist = true;
                        url.Append (setUserStateparameters);
                        url.AppendFormat("&uuid={0}", uuid);
                        if (!string.IsNullOrEmpty (_authenticationKey)) {
                                url.AppendFormat ("&auth={0}", EncodeUricomponent (_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }
                if (type == ResponseType.GetUserState) {
                        queryParamExist = true;
                        url.AppendFormat("?uuid={0}", uuid);
                        if (!string.IsNullOrEmpty(_authenticationKey))
                        {
                                url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));

                }

                if (type == ResponseType.Here_Now) {
                        queryParamExist = true;
                        url.Append (hereNowParameters);
                        url.AppendFormat("&uuid={0}", uuid);
                        if (!string.IsNullOrEmpty (_authenticationKey)) {
                                url.AppendFormat ("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }
                if (type == ResponseType.GlobalHere_Now) {
                        queryParamExist = true;    
                        url.Append (globalHereNowParameters);
                        url.AppendFormat("&uuid={0}", uuid);
                        if (!string.IsNullOrEmpty (_authenticationKey)) {
                                url.AppendFormat ("&auth={0}", EncodeUricomponent (_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }
                if (type == ResponseType.Where_Now) {
                        queryParamExist = true;
                        url.AppendFormat("?uuid={0}", uuid);
                        if (!string.IsNullOrEmpty(_authenticationKey))
                        {
                                url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }

                if (type == ResponseType.Publish){ 
                        queryParamExist = true;
                        url.AppendFormat("?uuid={0}", uuid);
                        if (parameters != "") {
                                url.AppendFormat("&{0}", parameters);
                        }
                        if (!string.IsNullOrEmpty(_authenticationKey))
                        {
                                url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                        }
                        url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
                }

                if (type == ResponseType.DetailedHistory || type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess) {
                        url.Append (parameters);
                        queryParamExist = true;
                }

            if (!queryParamExist) {
                url.AppendFormat ("?uuid={0}", uuid);
                url.AppendFormat ("&pnsdk={0}", EncodeUricomponent (_pnsdkVersion, type, false, true));
            }
            Uri requestUri = new Uri (url.ToString ());

            return requestUri;

        }
        
        private void ProcessResponseCallbacks<T> (List<object> result, RequestState<T> asynchRequestState)
        {
            if (result != null && result.Count >= 1 && asynchRequestState.UserCallback != null) {
                ResponseToConnectCallback<T> (result, asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.ConnectCallback);
                ResponseToUserCallback<T> (result, asynchRequestState.Type, asynchRequestState.Channels, asynchRequestState.UserCallback);
            }
        }

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
                                } else if (channelCallbacks [callbackKey].GetType ().FullName.Contains ("[System.Object")) {
                                    PubnubChannelCallback<object> retryPubnubCallback = channelCallbacks [callbackKey] as PubnubChannelCallback<object>;
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

        private void GoToCallback<T> (object result, Action<T> Callback)
        {
            if (Callback != null) {
                if (typeof(T) == typeof(string)) {
                    JsonResponseToCallback (result, Callback);
                } else {
                    Callback ((T)(object)result);
                }
            }
        }

        private void GoToCallback (object result, Action<string> Callback)
        {
            if (Callback != null) {
                JsonResponseToCallback (result, Callback);
            }
        }

        private void GoToCallback (object result, Action<object> Callback)
        {
            if (Callback != null) {
                Callback (result);
            }
        }

        private void GoToCallback (PubnubClientError error, Action<PubnubClientError> Callback)
        {
            if (Callback != null && error != null) {
                if ((int)error.Severity <= (int)_errorLevel) { //Checks whether the error serverity falls in the range of error filter level
                    //Do not send 107 = PubnubObjectDisposedException
                    //Do not send 105 = WebRequestCancelled
                    //Do not send 130 = PubnubClientMachineSleep
                    if (error.StatusCode != 107
                        && error.StatusCode != 105
                        && error.StatusCode != 130
                        && error.StatusCode != 4040) { //Error Code that should not go out
                        Callback (error);
                    }
                }
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

        /// <summary>
        /// Check the response of the REST API and call for re-subscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="multiplexResult"></param>
        /// <param name="userCallback"></param>
        /// <param name="connectCallback"></param>
        /// <param name="errorCallback"></param>
        private void MultiplexInternalCallback<T> (ResponseType type, object multiplexResult, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
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
        private List<object> WrapResultBasedOnResponseType<T> (ResponseType type, string jsonString, string[] channels, bool reconnect, long lastTimetoken, Action<PubnubClientError> errorCallback)
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
                            //result = DecodeDecryptLoop (result, channels, errorCallback);
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
                            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, receivedTimetoken = {1}", DateTime.Now.ToString (), receivedTimetoken), LoggingMethod.LevelInfo);
                            //long minimumTimetoken = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Min (token => token.Value) : 0;
                            //long maximumTimetoken = (multiChannelSubscribe.Count > 0) ? multiChannelSubscribe.Max (token => token.Value) : 0;

                            //if (minimumTimetoken == 0 || lastTimetoken == 0) {
                                //if (maximumTimetoken == 0) {
                                    lastSubscribeTimetoken = receivedTimetoken;
                                //} else {
                                    if (!_enableResumeOnReconnect) {
                                        lastSubscribeTimetoken = receivedTimetoken;
                                    } else {
                                        //do nothing. keep last subscribe token
                                    }
                                //}
                            //} else {
                                if (reconnect) {
                                    if (_enableResumeOnReconnect) {
                                        //do nothing. keep last subscribe token
                                    } else {
                                        lastSubscribeTimetoken = receivedTimetoken;
                                    }
                                /*} else {
                                    lastSubscribeTimetoken = receivedTimetoken;*/
                                }
                            //}
                        //Debug.Log ("lastSubscribeTimetoken: " + lastSubscribeTimetoken);
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

        #region "Error Callbacks"

        private PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
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

        private PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
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

        private PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
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

        private PubnubClientError CallErrorCallback (PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
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

        private string EncodeUricomponent (string s, ResponseType type, bool ignoreComma, bool ignorePercent2fEncode)
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
                if (!ignorePercent2fEncode) {
                    encodedUri = encodedUri.Replace ("%2F", "%252F");
                }
            }

            return encodedUri;
        }

        private char ToHex (int ch)
        {
            return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
        }

        #endregion

    }

    #region EventExt and Args
    static class EventExtensions
    {
        public static void Raise<T> (this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            if (handler != null) {
                handler (sender, args);
            }
        }
    }

    internal class CustomEventArgs<T> : EventArgs
    {
        internal string message;
        internal RequestState<T> pubnubRequestState;
        internal bool isError;
        internal bool isTimeout;
    }
    #endregion

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

        //string GetStubResponse (HttpWebRequest request);
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

    #region "PubnubCrypto"
    public class PubnubCrypto: PubnubCryptoBase
    {
        public PubnubCrypto (string cipher_key)
            : base (cipher_key)
        {
        }

        protected override string ComputeHashRaw (string input)
        {
            #if (SILVERLIGHT || WINDOWS_PHONE || MONOTOUCH || __IOS__ || MONODROID || __ANDROID__ || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID || UNITY_5)
            HashAlgorithm algorithm = new System.Security.Cryptography.SHA256Managed ();
            #else
            HashAlgorithm algorithm = new SHA256CryptoServiceProvider ();
            #endif

            //Byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes (input);
            Byte[] hashedBytes = algorithm.ComputeHash (inputBytes);
            return BitConverter.ToString (hashedBytes);
        }

        protected override string EncryptOrDecrypt (bool type, string plainStr)
        {
            {
                RijndaelManaged aesEncryption = new RijndaelManaged ();
                aesEncryption.KeySize = 256;
                aesEncryption.BlockSize = 128;
                //Mode CBC
                aesEncryption.Mode = CipherMode.CBC;
                //padding
                aesEncryption.Padding = PaddingMode.PKCS7;
                //get ASCII bytes of the string
                aesEncryption.IV = System.Text.Encoding.ASCII.GetBytes ("0123456789012345");
                aesEncryption.Key = System.Text.Encoding.ASCII.GetBytes (GetEncryptionKey ());

                if (type) {
                    ICryptoTransform crypto = aesEncryption.CreateEncryptor ();
                    plainStr = EncodeNonAsciiCharacters (plainStr);
                    byte[] plainText = Encoding.UTF8.GetBytes (plainStr);

                    //encrypt
                    byte[] cipherText = crypto.TransformFinalBlock (plainText, 0, plainText.Length);
                    return Convert.ToBase64String (cipherText);
                } else {
                    try {
                        ICryptoTransform decrypto = aesEncryption.CreateDecryptor ();
                        //decode
                        byte[] decryptedBytes = Convert.FromBase64CharArray (plainStr.ToCharArray (), 0, plainStr.Length);

                        //decrypt
                        //string decrypted = System.Text.Encoding.ASCII.GetString(decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length));
                        string decrypted = System.Text.Encoding.UTF8.GetString (decrypto.TransformFinalBlock (decryptedBytes, 0, decryptedBytes.Length));

                        return decrypted;
                    } catch (Exception ex) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelVerbose);
                        throw ex;
                        //LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelVerbose);
                        //return "**DECRYPT ERROR**";
                    }
                }
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

    #if (USE_JSONFX_UNITY_IOS)
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
            LoggingMethod.WriteToLog ("jsonstring:" + jsonString, LoggingMethod.LevelInfo);                   
            object obj = DeserializeToObject (jsonString);
            Dictionary<string, object> stateDictionary = new Dictionary<string, object> ();
            Dictionary<string, object> message = (Dictionary<string, object>)obj;
            if (message != null) {
                foreach (KeyValuePair<String, object> kvp in message) {
                    stateDictionary.Add (kvp.Key, kvp.Value);
                }
            }
            return stateDictionary;
        }
    }
    #elif (USE_MiniJSON)
    public class MiniJSONObjectSerializer : IJsonPluggableLibrary
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
            string json = Json.Serialize (objectToSerialize); 
            return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
        }

        public List<object> DeserializeToListOfObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as List<object>;
        }

        public object DeserializeToObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as object;
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as Dictionary<string, object>;
        }
    }
    #endif
    #endregion
}