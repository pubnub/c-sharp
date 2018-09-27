//Build Date: April 13, 2017
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
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif
#endregion

namespace PubnubApi
{
    public abstract class PubnubCoreBase
    {
        #region "Class variables"
        private static bool enableResumeOnReconnect = true;
        protected static bool OverrideTcpKeepAlive { get; set; } = true;
        protected static System.Threading.Timer PresenceHeartbeatTimer { get; set; }
        protected static bool PubnetSystemActive { get; set; } = true;
        protected Collection<Uri> PushRemoteImageDomainUri { get; set; } = new Collection<Uri>();
        protected static int ConnectionErrors { get; set; }
        #endregion

        private const int MINEXPONENTIALBACKOFF = 1;
        private const int MAXEXPONENTIALBACKOFF = 32;
        private const int INTERVAL = 3;

        private static IPubnubHttp pubnubHttp;
        private static ConcurrentDictionary<string, PNConfiguration> pubnubConfig { get; } = new ConcurrentDictionary<string, PNConfiguration>();
        private static IJsonPluggableLibrary jsonLib;
        private static IPubnubUnitTest unitTest;
        private static ConcurrentDictionary<string, IPubnubLog> pubnubLog { get; } = new ConcurrentDictionary<string, IPubnubLog>();
        private static EndPoint.TelemetryManager pubnubTelemetryMgr;
        private static EndPoint.DuplicationManager pubnubSubscribeDuplicationManager { get; set; }
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClientSubscribe { get; set; }
        private static HttpClient httpClientNonsubscribe { get; set; }
        private static HttpClient httpClientNetworkStatus { get; set; }
        private static PubnubHttpClientHandler pubnubHttpClientHandler { get; set; }
#endif

        private bool clientNetworkStatusInternetStatus = true;
        protected static ConcurrentDictionary<string, bool> SubscribeDisconnected { get; set; } = new ConcurrentDictionary<string, bool>();

        protected Pubnub PubnubInstance { get; set; }

        protected bool UuidChanged { get; set; }

        protected string CurrentUuid { get; set; }

        protected static ConcurrentDictionary<string, long> LastSubscribeTimetoken { get; set; } = new ConcurrentDictionary<string, long>();

        protected static int PubnubNetworkTcpCheckIntervalInSeconds { get; set; } = 3;
        private static int PubnubLocalHeartbeatCheckIntervalInSeconds { get; set; } = 30;

        protected static ConcurrentDictionary<string, List<SubscribeCallback>> SubscribeCallbackListenerList
        {
            get;
            set;
        } = new ConcurrentDictionary<string, List<SubscribeCallback>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, long>> MultiChannelSubscribe
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, long>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, long>> MultiChannelGroupSubscribe
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, long>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Timer>> ChannelReconnectTimer
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Timer>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Timer>> ChannelGroupReconnectTimer
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Timer>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, HttpWebRequest>> ChannelRequest
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, HttpWebRequest>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> ChannelInternetStatus
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> ChannelGroupInternetStatus
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelLocalUserState
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelUserState
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelGroupLocalUserState
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelGroupUserState
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static ConcurrentDictionary<string, DateTime> SubscribeRequestTracker
        {
            get;
            set;
        } = new ConcurrentDictionary<string, DateTime>();

        protected PubnubCoreBase(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }
            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(pubnubConfiguation,log), pubnubUnitTest, log, telemetryManager, instance);
            }
            else
            {
                InternalConstructor(pubnubConfiguation, jsonPluggableLibrary, pubnubUnitTest, log, telemetryManager, instance);
            }
        }

        private void InternalConstructor(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance)
        {
            PubnubInstance = instance;
            pubnubConfig.AddOrUpdate(instance.InstanceId, pubnubConfiguation, (k,o)=> pubnubConfiguation);
            jsonLib = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
            pubnubLog.AddOrUpdate(instance.InstanceId, log, (k, o) => log);
            pubnubTelemetryMgr = telemetryManager;
            pubnubSubscribeDuplicationManager = new EndPoint.DuplicationManager(pubnubConfiguation, jsonPluggableLibrary, log);

            CurrentUuid = pubnubConfiguation.Uuid;

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientSubscribe == null)
            {
                if (pubnubConfiguation.Proxy != null)
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    if (httpClientHandler.SupportsProxy)
                    {
                        httpClientHandler.Proxy = pubnubConfiguation.Proxy;
                        httpClientHandler.UseProxy = true;
                    }
                    pubnubHttpClientHandler = new PubnubHttpClientHandler("PubnubHttpClientHandler", httpClientHandler, pubnubConfiguation, jsonLib, unitTest, log);
                    httpClientSubscribe = new HttpClient(pubnubHttpClientHandler);
                }
                else
                {
                    httpClientSubscribe = new HttpClient();
                }
                httpClientSubscribe.DefaultRequestHeaders.Accept.Clear();
                httpClientSubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientSubscribe.Timeout = TimeSpan.FromSeconds(pubnubConfiguation.SubscribeTimeout);
            }
            if (httpClientNonsubscribe == null)
            {
                if (pubnubConfiguation.Proxy != null)
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    if (httpClientHandler.SupportsProxy)
                    {
                        httpClientHandler.Proxy = pubnubConfiguation.Proxy;
                        httpClientHandler.UseProxy = true;
                    }
                    pubnubHttpClientHandler = new PubnubHttpClientHandler("PubnubHttpClientHandler", httpClientHandler, pubnubConfiguation, jsonLib, unitTest, log);
                    httpClientNonsubscribe = new HttpClient(pubnubHttpClientHandler);
                }
                else
                {
                    httpClientNonsubscribe = new HttpClient();
                }
                httpClientNonsubscribe.DefaultRequestHeaders.Accept.Clear();
                httpClientNonsubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientNonsubscribe.Timeout = TimeSpan.FromSeconds(pubnubConfiguation.NonSubscribeRequestTimeout);
            }
            pubnubHttp = new PubnubHttp(pubnubConfiguation, jsonLib, log, pubnubTelemetryMgr, httpClientSubscribe, httpClientNonsubscribe);
#else
            pubnubHttp = new PubnubHttp(pubnubConfiguation, jsonLib, log, pubnubTelemetryMgr);
#endif


            UpdatePubnubNetworkTcpCheckIntervalInSeconds();
            if (pubnubConfiguation.PresenceInterval > 10)
            {
                PubnubLocalHeartbeatCheckIntervalInSeconds = pubnubConfiguation.PresenceInterval;
            }
            enableResumeOnReconnect = pubnubConfiguation.ReconnectionPolicy != PNReconnectionPolicy.NONE;

#if (SILVERLIGHT || WINDOWS_PHONE)
            HttpWebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
#endif
        }


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

#region "Internet connection and Reconnect Network"

        protected void ResetInternetCheckSettings(string[] channels, string[] channelGroups)
        {
            if (channels == null && channelGroups == null)
            {
                return;
            }

            string multiChannel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            if (multiChannel != "")
            {
                if (ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(multiChannel))
                {
                    ChannelInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(multiChannel, true, (key, oldValue) => true);
                }
                else
                {
                    ChannelInternetStatus[PubnubInstance.InstanceId].GetOrAdd(multiChannel, true); //Set to true for internet connection
                }
            }

            if (multiChannelGroup != "")
            {
                if (ChannelGroupInternetStatus[PubnubInstance.InstanceId].ContainsKey(multiChannelGroup))
                {
                    ChannelGroupInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(multiChannelGroup, true, (key, oldValue) => true);
                }
                else
                {
                    ChannelGroupInternetStatus[PubnubInstance.InstanceId].GetOrAdd(multiChannelGroup, true); //Set to true for internet connection
                }
            }
        }

#endregion

#region "Callbacks"

        protected bool CheckInternetConnectionStatus<T>(bool systemActive, PNOperationType type, PNCallback<T> callback, string[] channels, string[] channelGroups)
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig);
            pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog);

            ClientNetworkStatus.PubnubConfiguation = currentConfig;
            ClientNetworkStatus.JsonLibrary = jsonLib;
            ClientNetworkStatus.PubnubUnitTest = unitTest;
            ClientNetworkStatus.PubnubLog = currentLog;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientNetworkStatus == null)
            {
                if (currentConfig.Proxy != null && pubnubHttpClientHandler != null)
                {
                    httpClientNetworkStatus = new HttpClient(pubnubHttpClientHandler);
                }
                else
                {
                    httpClientNetworkStatus = new HttpClient();
                }
                httpClientNetworkStatus.DefaultRequestHeaders.Accept.Clear();
                httpClientNetworkStatus.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientNetworkStatus.Timeout = TimeSpan.FromSeconds(currentConfig.NonSubscribeRequestTimeout);
            }
            ClientNetworkStatus.RefHttpClient = httpClientNetworkStatus;
#endif
            if (!ClientNetworkStatus.IsInternetCheckRunning())
            {
                clientNetworkStatusInternetStatus = ClientNetworkStatus.CheckInternetStatus<T>(PubnetSystemActive, type, callback, channels, channelGroups);
            }
            return clientNetworkStatusInternetStatus;
        }

        protected static long GetTimetokenFromMultiplexResult(List<object> result)
        {
            long jsonTimetoken = 0;
            Dictionary<string, object> timetokenObj = jsonLib.ConvertToDictionaryObject(result[0]);

            if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
            {
                Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("t"))
                {
                    Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
                }
            }
            else
            {
                timetokenObj = jsonLib.ConvertToDictionaryObject(result[1]);
                if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
                {
                    Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                    if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("t"))
                    {
                        Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
                    }
                }
            }

            return jsonTimetoken;
        }

        private static List<SubscribeMessage> GetMessageFromMultiplexResult(List<object> result)
        {
            List<object> jsonMessageList = null;
            List<SubscribeMessage> msgList = new List<SubscribeMessage>();

            Dictionary<string, object> messageDicObj = jsonLib.ConvertToDictionaryObject(result[1]);
            if (messageDicObj != null && messageDicObj.Count > 0 && messageDicObj.ContainsKey("m"))
            {
                jsonMessageList = messageDicObj["m"] as List<object>;
            }
            else
            {
                messageDicObj = jsonLib.ConvertToDictionaryObject(result[0]);
                if (messageDicObj != null && messageDicObj.Count > 0 && messageDicObj.ContainsKey("m"))
                {
                    jsonMessageList = messageDicObj["m"] as List<object>;
                }
            }

            if (jsonMessageList != null && jsonMessageList.Count > 0)
            {
                foreach (Dictionary<string, object> dicItem in jsonMessageList)
                {
                    if (dicItem.Count > 0)
                    {
                        SubscribeMessage msg = new SubscribeMessage();
                        foreach (string key in dicItem.Keys)
                        {
                            switch (key.ToLowerInvariant())
                            {
                                case "a":
                                    msg.Shard = dicItem[key].ToString();
                                    break;
                                case "b":
                                    msg.SubscriptionMatch = dicItem[key].ToString();
                                    break;
                                case "c":
                                    msg.Channel = dicItem[key].ToString();
                                    break;
                                case "d":
                                    msg.Payload = dicItem[key];
                                    break;
                                case "f":
                                    msg.Flags = dicItem[key].ToString();
                                    break;
                                case "i":
                                    msg.IssuingClientId = dicItem[key].ToString();
                                    break;
                                case "k":
                                    msg.SubscribeKey = dicItem[key].ToString();
                                    break;
                                case "s":
                                    int seqNum;
                                    Int32.TryParse(dicItem[key].ToString(), out seqNum);
                                    msg.SequenceNumber = seqNum;
                                    break;
                                case "o":
                                    Dictionary<string, object> ttOriginMetaData = jsonLib.ConvertToDictionaryObject(dicItem[key]);
                                    if (ttOriginMetaData != null && ttOriginMetaData.Count > 0)
                                    {
                                        TimetokenMetadata ttMeta = new TimetokenMetadata();

                                        foreach (string metaKey in ttOriginMetaData.Keys)
                                        {
                                            if (metaKey.ToLowerInvariant().Equals("t", StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                long timetoken;
                                                Int64.TryParse(ttOriginMetaData[metaKey].ToString(), out timetoken);
                                                ttMeta.Timetoken = timetoken;
                                            }
                                            else if (metaKey.ToLowerInvariant().Equals("r", StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                ttMeta.Region = ttOriginMetaData[metaKey].ToString();
                                            }
                                        }
                                        msg.OriginatingTimetoken = ttMeta;
                                    }
                                    break;
                                case "p":
                                    Dictionary<string, object> ttPublishMetaData = jsonLib.ConvertToDictionaryObject(dicItem[key]);
                                    if (ttPublishMetaData != null && ttPublishMetaData.Count > 0)
                                    {
                                        TimetokenMetadata ttMeta = new TimetokenMetadata();

                                        foreach (string metaKey in ttPublishMetaData.Keys)
                                        {
                                            string currentMetaKey = metaKey.ToLowerInvariant();
                                            
                                            if (currentMetaKey.Equals("t", StringComparison.OrdinalIgnoreCase))
                                            {
                                                long timetoken;
                                                Int64.TryParse(ttPublishMetaData[metaKey].ToString(), out timetoken);
                                                ttMeta.Timetoken = timetoken;
                                            }
                                            else if (currentMetaKey.Equals("r", StringComparison.OrdinalIgnoreCase))
                                            {
                                                ttMeta.Region = ttPublishMetaData[metaKey].ToString();
                                            }
                                        }
                                        msg.PublishTimetokenMetadata = ttMeta;
                                    }
                                    break;
                                case "u":
                                    msg.UserMetadata = dicItem[key];
                                    break;
                                default:
                                    break;
                            }
                        }

                        msgList.Add(msg);
                    }
                }
            }
            return msgList;
        }

        private bool IsTargetForDedup(SubscribeMessage message)
        {
            bool isTargetOfDedup = false;
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            try
            {
                if (pubnubSubscribeDuplicationManager.IsDuplicate(message))
                {
                    isTargetOfDedup = true;
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime: {0}, Dedupe - Duplicate skipped - msg = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLib.SerializeToJsonString(message)), currentConfig.LogVerbosity);
                    }
                }
                else
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime: {0}, Dedupe - AddEntry - msg = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLib.SerializeToJsonString(message)), currentConfig.LogVerbosity);
                    }
                    pubnubSubscribeDuplicationManager.AddEntry(message);
                }
            }
            catch (Exception ex)
            {
                //Log and ignore any exception due to Dedupe manager
                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                {
                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime: {0}, IsTargetForDedup - dedupe error = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), currentConfig.LogVerbosity);
                }
            }

            return isTargetOfDedup;
        }

        private bool IsZeroTimeTokenRequest<T>(RequestState<T> asyncRequestState, List<object> result)
        {
            bool ret = false;
            try
            {
                if (asyncRequestState != null && asyncRequestState.ResponseType == PNOperationType.PNSubscribeOperation && result != null && result.Count > 0)
                {
                    List<SubscribeMessage> message = GetMessageFromMultiplexResult(result);
                    if (message != null && message.Count == 0)
                    {
                        IEnumerable<string> newChannels = from channel in MultiChannelSubscribe[PubnubInstance.InstanceId]
                                                          where channel.Value == 0
                                                          select channel.Key;
                        IEnumerable<string> newChannelGroups = from channelGroup in MultiChannelGroupSubscribe[PubnubInstance.InstanceId]
                                                               where channelGroup.Value == 0
                                                               select channelGroup.Key;
                        if ((newChannels != null && newChannels.Count() > 0) || (newChannelGroups != null && newChannelGroups.Count() > 0))
                        {
                            ret = true;
                        }
                    }
                }
            }
            catch {  /* ignore */ }
            return ret;
        }

        private void ResponseToConnectCallback<T>(PNOperationType type, string[] channels, string[] channelGroups, RequestState<T> asyncRequestState)
        {
            PNConfiguration currentConfig;
            pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig);
            StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
            PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNConnectedCategory, asyncRequestState, (int)HttpStatusCode.OK, null);

            //Check callback exists and make sure previous timetoken = 0
            if (channels != null && channels.Length > 0)
            {
                IEnumerable<string> newChannels = from channel in MultiChannelSubscribe[PubnubInstance.InstanceId]
                                                  where channel.Value == 0
                                                  select channel.Key;
                if (newChannels != null && newChannels.Count() > 0)
                {
                    status.AffectedChannels = newChannels.ToList();
                }
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                IEnumerable<string> newChannelGroups = from channelGroup in MultiChannelGroupSubscribe[PubnubInstance.InstanceId]
                                                       where channelGroup.Value == 0
                                                       select channelGroup.Key;

                if (newChannelGroups != null && newChannelGroups.Count() > 0)
                {
                    status.AffectedChannelGroups = newChannelGroups.ToList();
                }
            }

            Announce(status);
        }

        private void ResponseToUserCallback<T>(List<object> result, PNOperationType type, RequestState<T> asyncRequestState)
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            try
            {
                pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig);
                pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog);

                var userCallback = (asyncRequestState != null) ? asyncRequestState.PubnubCallback : null;
                switch (type)
                {
                    case PNOperationType.PNSubscribeOperation:
                    case PNOperationType.Presence:
                        List<SubscribeMessage> messageList = GetMessageFromMultiplexResult(result);
                        if (messageList != null && messageList.Count > 0)
                        {
                            if (messageList.Count >= currentConfig.RequestMessageCountThreshold)
                            {
                                StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
                                PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNRequestMessageCountExceededCategory, asyncRequestState, (int)HttpStatusCode.OK, null);
                                Announce(status);
                            }

                            for (int messageIndex = 0; messageIndex < messageList.Count; messageIndex++)
                            {
                                SubscribeMessage currentMessage = messageList[messageIndex];
                                if (currentMessage != null)
                                {
                                    if (currentConfig.DedupOnSubscribe)
                                    {
                                        if (IsTargetForDedup(currentMessage))
                                        {
                                            continue;
                                        }
                                    }

                                    string currentMessageChannel = currentMessage.Channel;
                                    string currentMessageChannelGroup = currentMessage.SubscriptionMatch;

                                    if (currentMessageChannel.Replace("-pnpres", "") == currentMessageChannelGroup.Replace("-pnpres", ""))
                                    {
                                        currentMessageChannelGroup = "";
                                    }

                                    object payload = currentMessage.Payload;

                                    List<object> payloadContainer = new List<object>(); //First item always message
                                    if (currentMessageChannel.Contains("-pnpres") || currentMessageChannel.Contains(".*-pnpres"))
                                    {
                                        payloadContainer.Add(payload);
                                    }
                                    else
                                    {
                                        if (currentConfig.CipherKey.Length > 0) //decrypt the subscriber message if cipherkey is available
                                        {
                                            string decryptMessage = "";
                                            PubnubCrypto aes = new PubnubCrypto(currentConfig.CipherKey, currentConfig, currentLog);
                                            try
                                            {
                                                decryptMessage = aes.Decrypt(payload.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                decryptMessage = "**DECRYPT ERROR**";

                                                PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                                                PNStatus status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, category, null, (int)HttpStatusCode.NotFound, new PNException(ex));
                                                if (!string.IsNullOrEmpty(currentMessageChannel))
                                                {
                                                    status.AffectedChannels.Add(currentMessageChannel);
                                                    status.AffectedChannels = status.AffectedChannels.Distinct().ToList();
                                                }
                                                if (!string.IsNullOrEmpty(currentMessageChannelGroup))
                                                {
                                                    status.AffectedChannelGroups.Add(currentMessageChannelGroup);
                                                    status.AffectedChannelGroups = status.AffectedChannelGroups.Distinct().ToList();
                                                }

                                                Announce(status);
                                            }
                                            object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : jsonLib.DeserializeToObject(decryptMessage);

                                            payloadContainer.Add(decodeMessage);
                                        }
                                        else
                                        {
                                            string payloadJson = jsonLib.SerializeToJsonString(payload);
                                            object payloadJObject = jsonLib.BuildJsonObject(payloadJson);
                                            if (payloadJObject == null)
                                            {
                                                payloadContainer.Add(payload);
                                            }
                                            else
                                            {
                                                payloadContainer.Add(payloadJObject);
                                            }
                                        }
                                    }

                                    object userMetaData = currentMessage.UserMetadata;

                                    payloadContainer.Add(userMetaData); //Second one always user meta data

                                    payloadContainer.Add(currentMessage.PublishTimetokenMetadata.Timetoken); //Third one always Timetoken

                                    payloadContainer.Add(currentMessage.IssuingClientId); //Fourth one always Publisher

                                    if (!string.IsNullOrEmpty(currentMessageChannelGroup)) //Add cg first before channel
                                    {
                                        payloadContainer.Add(currentMessageChannelGroup);
                                    }

                                    if (!string.IsNullOrEmpty(currentMessageChannel))
                                    {
                                        payloadContainer.Add(currentMessageChannel);
                                    }

                                    if (currentMessageChannel.Contains("-pnpres"))
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib, currentLog);
                                        PNPresenceEventResult presenceEvent = responseBuilder.JsonToObject<PNPresenceEventResult>(payloadContainer, true);
                                        if (presenceEvent != null)
                                        {
                                            Announce(presenceEvent);
                                        }
                                    }
                                    else
                                    {
                                        if (currentConfig != null && currentLog != null)
                                        {
                                            LoggingMethod.WriteToLog(currentLog, string.Format("DateTime: {0}, ResponseToUserCallback - payload = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLib.SerializeToJsonString(payloadContainer)), currentConfig.LogVerbosity);
                                        }
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib, currentLog);
                                        PNMessageResult<T> userMessage = responseBuilder.JsonToObject<PNMessageResult<T>>(payloadContainer, true);
                                        if (userMessage != null)
                                        {
                                            Announce(userMessage);
                                        }
                                    }

                                }
                            }

                        }
                        break;
                    case PNOperationType.PNTimeOperation:
                    case PNOperationType.PNPublishOperation:
                    case PNOperationType.PNFireOperation:
                    case PNOperationType.PNHistoryOperation:
                    case PNOperationType.PNDeleteMessageOperation:
                    case PNOperationType.PNHereNowOperation:
                    case PNOperationType.PNWhereNowOperation:
                    case PNOperationType.PNAccessManagerGrant:
                    case PNOperationType.PNAccessManagerAudit:
                    case PNOperationType.RevokeAccess:
                    case PNOperationType.ChannelGroupGrantAccess:
                    case PNOperationType.ChannelGroupAuditAccess:
                    case PNOperationType.ChannelGroupRevokeAccess:
                    case PNOperationType.PNGetStateOperation:
                    case PNOperationType.PNSetStateOperation:
                    case PNOperationType.PushRegister:
                    case PNOperationType.PushRemove:
                    case PNOperationType.PushGet:
                    case PNOperationType.PushUnregister:
                    case PNOperationType.PNAddChannelsToGroupOperation:
                    case PNOperationType.PNRemoveChannelsFromGroupOperation:
                    case PNOperationType.PNRemoveGroupOperation:
                    case PNOperationType.ChannelGroupGet:
                    case PNOperationType.ChannelGroupAllGet:
                        if (result != null && result.Count > 0)
                        {
                            ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib, currentLog);
                            T userResult = responseBuilder.JsonToObject<T>(result, true);

                            StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
                            PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, (int)HttpStatusCode.OK, null);

                            if (userCallback != null)
                            {
                                userCallback.OnResponse(userResult, status);
                            }
                        }
                        break;
                    case PNOperationType.PNHeartbeatOperation:
                        if (result != null && result.Count > 0)
                        {
                            ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib, currentLog);
                            PNHeartbeatResult userResult = responseBuilder.JsonToObject<PNHeartbeatResult>(result, true);

                            if (userResult != null)
                            {
                                if (currentConfig.HeartbeatNotificationOption == PNHeartbeatNotificationOption.All)
                                {
                                    StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
                                    PNStatus status = null;

                                    PNException ex = null;
                                    if (userResult != null && userResult.Status == 200)
                                    {
                                        status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, (int)HttpStatusCode.OK, null);
                                    }
                                    else
                                    {
                                        ex = new PNException(userResult.Message);
                                        status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, userResult.Status, null);
                                    }

                                    Announce(status);
                                }
                                else if (currentConfig.HeartbeatNotificationOption == PNHeartbeatNotificationOption.Failures && userResult.Status != 200)
                                {
                                    StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
                                    PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, userResult.Status, new PNException(userResult.Message));
                                    Announce(status);
                                }
                            }

                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

#endregion

#region "Simulate network fail and machine sleep"
        public static void EnableMachineSleepModeForTestingOnly()
        {
            PubnetSystemActive = false;
        }

        public static void DisableMachineSleepModeForTestingOnly()
        {
            PubnetSystemActive = true;
        }

#endregion

#region "Helpers"

        protected string[] GetCurrentSubscriberChannels()
        {
            string[] channels = null;
            if (MultiChannelSubscribe != null && MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.Count > 0)
            {
                channels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
            }

            return channels;
        }

        protected string[] GetCurrentSubscriberChannelGroups()
        {
            string[] channelGroups = null;
            if (MultiChannelGroupSubscribe != null && MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.Count > 0)
            {
                channelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
            }

            return channelGroups;
        }
#endregion

#region "Build, process and send request"
        internal protected string UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest)
        {
            return UrlProcessRequest(requestUri, pubnubRequestState, terminateCurrentSubRequest, "");
        }

        internal protected string UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest, string jsonPostData)
        {
            string channel = "";
            string channelGroup = "";
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            try
            {
                if (terminateCurrentSubRequest)
                {
                    TerminateCurrentSubscriberRequest();
                }

                if (PubnubInstance == null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, PubnubInstance is null. Exiting UrlProcessRequest", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                    return "";
                }

                if (pubnubRequestState != null)
                {
                    channel = (pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0) ? string.Join(",", pubnubRequestState.Channels.OrderBy(x => x).ToArray()) : ",";
                    if (pubnubRequestState.ChannelGroups != null)
                    {
                        channelGroup = string.Join(",", pubnubRequestState.ChannelGroups.OrderBy(x => x).ToArray());
                    }
                }

                if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId) && !channel.Equals(",", StringComparison.CurrentCultureIgnoreCase) && !ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel) && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence))
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0}, UrlProcessRequest ChannelRequest PubnubInstance.InstanceId Channel NOT matching", DateTime.Now.ToString(CultureInfo.InvariantCulture)), currentConfig.LogVerbosity);
                    }
                    return "";
                }

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                //do nothing
#else
                // Create Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);

                request = pubnubHttp.SetNoCache<T>(request);
                request = pubnubHttp.SetProxy<T>(request);
                request = pubnubHttp.SetTimeout<T>(pubnubRequestState, request);

                pubnubRequestState.Request = request;

                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence)
                {
                    ChannelRequest[PubnubInstance.InstanceId].AddOrUpdate(channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
                }
#endif

                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                {
                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0}, Request={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri.ToString()), currentConfig.LogVerbosity);
                }

                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    SubscribeRequestTracker.AddOrUpdate(PubnubInstance.InstanceId, DateTime.Now, (key, oldState) => DateTime.Now);
                }

                string jsonString = "";
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                if (pubnubRequestState.UsePostMethod)
                {
                    Task<string> jsonResponse = pubnubHttp.SendRequestAndGetJsonResponseWithPOST(requestUri, pubnubRequestState, null, jsonPostData);
                    jsonString = jsonResponse.Result;
                }
                else
                {
                    Task<string> jsonResponse = pubnubHttp.SendRequestAndGetJsonResponse(requestUri, pubnubRequestState, null);
                    jsonString = jsonResponse.Result;
                }
#else
                if (pubnubRequestState.UsePostMethod)
                {
                    Task<string> jsonResponse = pubnubHttp.SendRequestAndGetJsonResponseWithPOST(requestUri, pubnubRequestState, request, jsonPostData);
                    jsonString = jsonResponse.Result;
                }
                else
                {
                    Task<string> jsonResponse = pubnubHttp.SendRequestAndGetJsonResponse(requestUri, pubnubRequestState, request);
                    jsonString = jsonResponse.Result;
                }
#endif

                if (SubscribeDisconnected.ContainsKey(PubnubInstance.InstanceId) && SubscribeDisconnected[PubnubInstance.InstanceId])
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0},Received JSON but SubscribeDisconnected = {1} for request={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonString, requestUri), currentConfig.LogVerbosity);
                    }
                    throw new OperationCanceledException("Disconnected");
                }

                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                {
                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0}, JSON= {1} for request={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonString, requestUri), currentConfig.LogVerbosity);
                }
                return jsonString;
            }
            catch (Exception ex)
            {
                string exceptionMessage = "";
                Exception innerEx = null;
                WebException webEx = null;

                if (ex.InnerException != null)
                {
                    if (ex is WebException)
                    {
                        webEx = ex as WebException;
                        exceptionMessage = webEx.ToString();
                    }
                    else
                    {
                        innerEx = ex.InnerException;
                        exceptionMessage = innerEx.ToString();
                    }
                }
                else
                {
                    innerEx = ex;
                    exceptionMessage = innerEx.ToString();
                }

                if (exceptionMessage.IndexOf("The request was aborted: The request was canceled", StringComparison.CurrentCultureIgnoreCase) == -1
                && exceptionMessage.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.CurrentCultureIgnoreCase) == -1
                && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation && exceptionMessage.IndexOf("The operation has timed out", StringComparison.CurrentCultureIgnoreCase) == -1)
                && exceptionMessage.IndexOf("A task was canceled", StringComparison.CurrentCultureIgnoreCase) == -1)
                {
                    PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(webEx == null ? innerEx : webEx);
                    if (PubnubInstance != null && pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                    {
                        PNStatus status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(pubnubRequestState.ResponseType, category, pubnubRequestState, (int)HttpStatusCode.NotFound, new PNException(ex));
                        if (pubnubRequestState != null && pubnubRequestState.PubnubCallback != null)
                        {
                            pubnubRequestState.PubnubCallback.OnResponse(default(T), status);
                        }
                        else
                        {
                            Announce(status);
                        }
                    }

                    if (PubnubInstance != null && pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} PubnubBaseCore UrlProcessRequest Exception={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), webEx != null ? webEx.ToString() : exceptionMessage), currentConfig.LogVerbosity);
                    }
                }

                return "";
            }
        }

        internal protected List<object> ProcessJsonResponse<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            List<object> result = new List<object>();
            PNConfiguration currentConfig;

            PNOperationType type = PNOperationType.None;
            if (asyncRequestState != null)
            {
                type = asyncRequestState.ResponseType;
            }

            bool errorCallbackRaised = false;
            if (jsonLib.IsDictionaryCompatible(jsonString, type))
            {
                PNStatus status = null;
                Dictionary<string, object> deserializeStatus = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                int statusCode = 0; //default. assuming all is ok 
                if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && string.Equals(deserializeStatus["error"].ToString(),"true", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                    {
                        status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new PNException(jsonString));
                    }
                }
                else if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                {
                    Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                    string statusMessage = deserializeStatus["message"].ToString();

                    if (statusCode != 200)
                    {
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                        if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                        {
                            status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                        }
                    }
                }

                if (status != null)
                {
                    errorCallbackRaised = true;
                    if (asyncRequestState != null && asyncRequestState.PubnubCallback != null)
                    {
                        asyncRequestState.PubnubCallback.OnResponse(default(T), status);
                    }
                    else
                    {
                        Announce(status);
                    }
                }
            }
            if (!errorCallbackRaised && asyncRequestState != null)
            {
                result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.PubnubCallback);
            }

            return result;
        }

        protected List<object> WrapResultBasedOnResponseType<T>(PNOperationType type, string jsonString, string[] channels, string[] channelGroups, bool reconnect, long lastTimetoken, PNCallback<T> callback)
        {
            List<object> result = new List<object>();
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            try
            {
                string multiChannel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
                string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

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
                            case PNOperationType.PNSubscribeOperation:
                            case PNOperationType.Presence:
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

                                long receivedTimetoken = GetTimetokenFromMultiplexResult(result);

                                long minimumTimetoken1 = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Min(token => token.Value) : 0;
                                long minimumTimetoken2 = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Min(token => token.Value) : 0;
                                long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

                                long maximumTimetoken1 = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Max(token => token.Value) : 0;
                                long maximumTimetoken2 = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Max(token => token.Value) : 0;
                                long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);

                                if (minimumTimetoken == 0 || lastTimetoken == 0)
                                {
                                    if (maximumTimetoken == 0)
                                    {
                                        LastSubscribeTimetoken[PubnubInstance.InstanceId] = receivedTimetoken;
                                    }
                                    else
                                    {
                                        if (!enableResumeOnReconnect)
                                        {
                                            LastSubscribeTimetoken[PubnubInstance.InstanceId] = receivedTimetoken;
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
                                        if (enableResumeOnReconnect)
                                        {
                                            //do nothing. keep last subscribe token
                                        }
                                        else
                                        {
                                            LastSubscribeTimetoken[PubnubInstance.InstanceId] = receivedTimetoken;
                                        }
                                    }
                                    else
                                    {
                                        LastSubscribeTimetoken[PubnubInstance.InstanceId] = receivedTimetoken;
                                    }
                                }
                                break;
                            case PNOperationType.Leave:
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNHeartbeatOperation:
                                Dictionary<string, object> heartbeatadictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(heartbeatadictionary);
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNTimeOperation:
                                break;
                            case PNOperationType.PNHistoryOperation:
                                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                                {
                                    result = SecureMessage.Instance(currentConfig, jsonLib, currentLog).DecodeDecryptLoop(result, channels, channelGroups, callback);
                                }
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNHereNowOperation:
                                Dictionary<string, object> dictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(dictionary);
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNWhereNowOperation:
                                Dictionary<string, object> whereNowDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(whereNowDictionary);
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNAccessManagerGrant:
                            case PNOperationType.PNAccessManagerAudit:
                            case PNOperationType.RevokeAccess:
                                Dictionary<string, object> grantDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(grantDictionary);
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.ChannelGroupGrantAccess:
                            case PNOperationType.ChannelGroupAuditAccess:
                            case PNOperationType.ChannelGroupRevokeAccess:
                                Dictionary<string, object> channelGroupPAMDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(channelGroupPAMDictionary);
                                result.Add(multiChannelGroup);
                                break;
                            case PNOperationType.PNGetStateOperation:
                            case PNOperationType.PNSetStateOperation:
                                Dictionary<string, object> userStateDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                result = new List<object>();
                                result.Add(userStateDictionary);
                                result.Add(multiChannelGroup);
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNPublishOperation:
                            case PNOperationType.PNFireOperation:
                            case PNOperationType.PushRegister:
                            case PNOperationType.PushRemove:
                            case PNOperationType.PushGet:
                            case PNOperationType.PushUnregister:
                                result.Add(multiChannel);
                                break;
                            case PNOperationType.PNAddChannelsToGroupOperation:
                            case PNOperationType.PNRemoveChannelsFromGroupOperation:
                            case PNOperationType.PNRemoveGroupOperation:
                            case PNOperationType.ChannelGroupGet:
                            case PNOperationType.ChannelGroupAllGet:
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
            catch { /* ignore */ }

            return result;
        }

        protected void ProcessResponseCallbacks<T>(List<object> result, RequestState<T> asyncRequestState)
        {
            bool callbackAvailable = false;
            if (result != null && result.Count >= 1)
            {
                if (asyncRequestState.PubnubCallback != null || SubscribeCallbackListenerList.Count >= 1)
                {
                    callbackAvailable = true;
                }
            }
            if (callbackAvailable)
            {
                bool zeroTimeTokenRequest = IsZeroTimeTokenRequest(asyncRequestState, result);
                if (zeroTimeTokenRequest)
                {
                    ResponseToConnectCallback<T>(asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState);
                }
                else
                {
                    ResponseToUserCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState);
                }
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
                if (!string.IsNullOrEmpty(channel) && ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    ChannelLocalUserState[PubnubInstance.InstanceId].TryGetValue(channel, out channelUserStateDictionary);
                }
                if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                {
                    ChannelGroupLocalUserState[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupUserStateDictionary);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(channel) && ChannelUserState[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    ChannelUserState[PubnubInstance.InstanceId].TryGetValue(channel, out channelUserStateDictionary);
                }
                if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                {
                    ChannelGroupUserState[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupUserStateDictionary);
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
                    string currentJsonState = BuildJsonUserState(channels[index], "", local);
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format("\"{0}\":{{{1}}}", channels[index], currentJsonState);
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
                    string currentJsonState = BuildJsonUserState("", channelGroups[index], local);
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format("\"{0}\":{{{1}}}", channelGroups[index], currentJsonState);
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
                retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder);
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
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            if (state != null && state.Request != null)
            {
                try
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime: {0}, TerminatePendingWebRequest - {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), state.Request.RequestUri.ToString()), currentConfig.LogVerbosity);
                    }
                    state.Request.Abort();
                }
                catch { /* ignore */ }
            }
            else
            {
                ICollection<string> keyCollection = ChannelRequest[PubnubInstance.InstanceId].Keys;
                foreach (string key in keyCollection)
                {
                    HttpWebRequest currentRequest;
                    if (ChannelRequest[PubnubInstance.InstanceId].TryGetValue(key, out currentRequest) && currentRequest != null)
                    {
                        TerminatePendingWebRequest(currentRequest);
                    }
                }
            }
        }

        protected static void TerminatePendingWebRequest(HttpWebRequest request)
        {
            if (request != null)
            {
                try
                {
                    request.Abort();
                }
                catch { /* ignore */ }
            }
        }

        private void RemoveChannelDictionary()
        {
            RemoveChannelDictionary<object>(null);
        }

        private void RemoveChannelDictionary<T>(RequestState<T> state)
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            if (state != null && state.Request != null)
            {
                string channel = (state.Channels != null) ? string.Join(",", state.Channels.OrderBy(x => x).ToArray()) : ",";

                if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    HttpWebRequest removedRequest;
                    bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(channel, out removedRequest);
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel), currentConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel), currentConfig.LogVerbosity);
                        }
                    }
                }
            }
            else
            {
                ICollection<string> keyCollection = ChannelRequest[PubnubInstance.InstanceId].Keys;
                if (keyCollection != null && keyCollection.Count > 0)
                {
                    List<string> keysList = keyCollection.ToList();
                    foreach (string key in keysList)
                    {
                        HttpWebRequest currentRequest;
                        if (ChannelRequest[PubnubInstance.InstanceId].TryGetValue(key, out currentRequest) && currentRequest != null)
                        {
                            bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(key, out currentRequest);
                            if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                            {
                                if (removeKey)
                                {
                                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                                }
                                else
                                {
                                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RemoveUserState()
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            if (ChannelLocalUserState.Count == 0 || !ChannelLocalUserState.ContainsKey(PubnubInstance.InstanceId)) { return; }

            ICollection<string> channelLocalUserStateCollection = ChannelLocalUserState[PubnubInstance.InstanceId].Keys;
            ICollection<string> channelUserStateCollection = ChannelUserState[PubnubInstance.InstanceId].Keys;

            ICollection<string> channelGroupLocalUserStateCollection = ChannelGroupLocalUserState[PubnubInstance.InstanceId].Keys;
            ICollection<string> channelGroupUserStateCollection = ChannelGroupUserState[PubnubInstance.InstanceId].Keys;

            if (channelLocalUserStateCollection != null && channelLocalUserStateCollection.Count > 0)
            {
                List<string> channelLocalStateList = channelLocalUserStateCollection.ToList();
                foreach (string key in channelLocalStateList)
                {
                    if (ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelLocalUserState[PubnubInstance.InstanceId].TryRemove(key, out tempUserState);
                        if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                        {
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }

            if (channelUserStateCollection != null && channelUserStateCollection.Count > 0)
            {
                List<string> channelStateList = channelUserStateCollection.ToList();
                foreach (string key in channelStateList)
                {
                    if (ChannelUserState[PubnubInstance.InstanceId].ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelUserState[PubnubInstance.InstanceId].TryRemove(key, out tempUserState);
                        if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                        {
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }

            if (channelGroupLocalUserStateCollection != null && channelGroupLocalUserStateCollection.Count > 0)
            {
                List<string> channelGroupLocalStateList = channelGroupLocalUserStateCollection.ToList();
                foreach (string key in channelGroupLocalStateList)
                {
                    if (ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelGroupLocalUserState[PubnubInstance.InstanceId].TryRemove(key, out tempUserState);
                        if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                        {
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }

            if (channelGroupUserStateCollection != null && channelGroupUserStateCollection.Count > 0)
            {
                List<string> channelGroupStateList = channelGroupUserStateCollection.ToList();

                foreach (string key in channelGroupStateList)
                {
                    if (ChannelGroupUserState[PubnubInstance.InstanceId].ContainsKey(key))
                    {
                        Dictionary<string, object> tempUserState;
                        bool removeKey = ChannelGroupUserState[PubnubInstance.InstanceId].TryRemove(key, out tempUserState);
                        if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                        {
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }
        }

        protected void TerminatePresenceHeartbeatTimer()
        {
            if (PresenceHeartbeatTimer != null)
            {
                PresenceHeartbeatTimer.Dispose();
                PresenceHeartbeatTimer = null;
            }
        }

        protected static void TerminateTelemetry()
        {
            if (pubnubTelemetryMgr != null)
            {
                pubnubTelemetryMgr.Destroy();
            }
        }

        protected static void TerminateDedupeManager()
        {
            if (pubnubSubscribeDuplicationManager != null)
            {
                pubnubSubscribeDuplicationManager.ClearHistory();
                pubnubSubscribeDuplicationManager = null;
            }
        }

        protected void UpdatePubnubNetworkTcpCheckIntervalInSeconds()
        {
            int timerInterval;
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            if (PubnubInstance != null && pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
            {
                if (currentConfig.ReconnectionPolicy == PNReconnectionPolicy.EXPONENTIAL)
                {
                    timerInterval = (int)(Math.Pow(2, ConnectionErrors) - 1);
                    if (timerInterval > MAXEXPONENTIALBACKOFF)
                    {
                        timerInterval = MINEXPONENTIALBACKOFF;
                        ConnectionErrors = 1;
                        if (pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                        {
                            LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0}, EXPONENTIAL timerInterval > MAXEXPONENTIALBACKOFF", DateTime.Now.ToString(CultureInfo.InvariantCulture)), currentConfig.LogVerbosity);
                        }
                    }
                    else if (timerInterval < 1)
                    {
                        timerInterval = MINEXPONENTIALBACKOFF;
                    }
                    if (pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0}, EXPONENTIAL timerInterval = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), timerInterval.ToString()), currentConfig.LogVerbosity);
                    }
                }
                else if (currentConfig.ReconnectionPolicy == PNReconnectionPolicy.LINEAR)
                {
                    timerInterval = INTERVAL;
                }
                else
                {
                    timerInterval = -1;
                }

                PubnubNetworkTcpCheckIntervalInSeconds = timerInterval;
            }
        }

        protected void TerminateReconnectTimer()
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            try
            {
                if (string.IsNullOrEmpty(PubnubInstance.InstanceId) || ChannelReconnectTimer == null || ChannelReconnectTimer.Count == 0 || !ChannelReconnectTimer.ContainsKey(PubnubInstance.InstanceId))
                {
                    return;
                }

                ConcurrentDictionary<string, Timer> channelReconnectCollection = ChannelReconnectTimer[PubnubInstance.InstanceId];
                ICollection<string> keyCollection = channelReconnectCollection.Keys;
                if (keyCollection != null && keyCollection.Count > 0)
                {
                    List<string> keyList = keyCollection.ToList();
                    foreach (string key in keyList)
                    {
                        if (ChannelReconnectTimer[PubnubInstance.InstanceId].ContainsKey(key))
                        {
                            try
                            {
                                Timer currentTimer;
                                if (ChannelReconnectTimer[PubnubInstance.InstanceId].TryGetValue(key, out currentTimer))
                                {
                                    currentTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                    currentTimer.Dispose();
                                }
                            }
                            catch { /* ignore */ }

                            Timer removedTimer = null;
                            bool removed = ChannelReconnectTimer[PubnubInstance.InstanceId].TryRemove(key, out removedTimer);
                            if (!removed)
                            {
                                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                                {
                                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channel reconnect timer reference from collection for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), currentConfig.LogVerbosity);
                                }
                            }
                        }
                    }
                }

                ICollection<string> groupKeyCollection = null;
                if (ChannelGroupReconnectTimer.Count > 0 && ChannelGroupReconnectTimer.ContainsKey(PubnubInstance.InstanceId))
                {
                    ConcurrentDictionary<string, Timer> channelGroupReconnectCollection = ChannelGroupReconnectTimer[PubnubInstance.InstanceId];
                    groupKeyCollection = channelGroupReconnectCollection.Keys;
                }
                if (groupKeyCollection != null && groupKeyCollection.Count > 0)
                {
                    List<string> groupKeyList = groupKeyCollection.ToList();
                    foreach (string groupKey in groupKeyList)
                    {
                        if (ChannelGroupReconnectTimer[PubnubInstance.InstanceId].ContainsKey(groupKey))
                        {
                            try
                            {
                                Timer currentTimer;
                                if (ChannelGroupReconnectTimer[PubnubInstance.InstanceId].TryGetValue(groupKey, out currentTimer))
                                {
                                    currentTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                    currentTimer.Dispose();
                                }
                            }
                            catch { /* ignore */ }

                            Timer removedTimer = null;
                            bool removed = ChannelGroupReconnectTimer[PubnubInstance.InstanceId].TryRemove(groupKey, out removedTimer);
                            if (!removed)
                            {
                                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                                {
                                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channelgroup reconnect timer reference from collection for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), groupKey), currentConfig.LogVerbosity);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                {
                    LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} TerminateReconnectTimer exception: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), currentConfig.LogVerbosity);
                }
            }
        }

        protected bool DeleteLocalChannelUserState(string channel)
        {
            bool userStateDeleted = false;

            if (ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(channel))
            {
                Dictionary<string, object> returnedUserState = null;
                userStateDeleted = ChannelLocalUserState[PubnubInstance.InstanceId].TryRemove(channel, out returnedUserState);
            }

            return userStateDeleted;
        }

        protected bool DeleteLocalChannelGroupUserState(string channelGroup)
        {
            bool userStateDeleted = false;

            if (ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
            {
                Dictionary<string, object> returnedUserState = null;
                userStateDeleted = ChannelGroupLocalUserState[PubnubInstance.InstanceId].TryRemove(channelGroup, out returnedUserState);
            }

            return userStateDeleted;
        }

        internal void EndPendingRequests()
        {
            SubscribeCallbackListenerList.Clear();

            RemoveChannelDictionary();
            TerminatePendingWebRequest();
            TerminateReconnectTimer();
            RemoveUserState();
            TerminatePresenceHeartbeatTimer();
            TerminateTelemetry();
            TerminateDedupeManager();

            if (MultiChannelSubscribe.Count > 0 && MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId))
            {
                MultiChannelSubscribe[PubnubInstance.InstanceId].Clear();
            }

            if (MultiChannelGroupSubscribe.Count > 0 && MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId))
            {
                MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Clear();
            }

            if (ChannelLocalUserState.Count > 0 && ChannelLocalUserState.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelLocalUserState[PubnubInstance.InstanceId].Clear();
            }

            if (ChannelUserState.Count > 0 && ChannelUserState.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelUserState[PubnubInstance.InstanceId].Clear();
            }

            if (ChannelGroupLocalUserState.Count > 0 && ChannelGroupLocalUserState.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelGroupLocalUserState[PubnubInstance.InstanceId].Clear();
            }

            if (ChannelGroupUserState.Count > 0 && ChannelGroupUserState.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelGroupUserState[PubnubInstance.InstanceId].Clear();
            }

            RemoveHttpClients();

            PubnubInstance = null;
        }

        internal static void RemoveHttpClients()
        {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientNetworkStatus != null)
            {
                try{
                    httpClientNetworkStatus.CancelPendingRequests();
                    httpClientNetworkStatus.Dispose();
                    httpClientNetworkStatus = null;
                }
                catch{}
            }
            if (httpClientSubscribe != null)
            {
                try{
                    httpClientSubscribe.CancelPendingRequests();
                    httpClientSubscribe.Dispose();
                    httpClientSubscribe = null;
                }
                catch{}
            }
            if (httpClientNonsubscribe != null)
            {
                try{
                    httpClientNonsubscribe.CancelPendingRequests();
                    httpClientNonsubscribe.Dispose();
                    httpClientNonsubscribe = null;
                }
                catch{}
            }
#endif
        }

        internal void TerminateCurrentSubscriberRequest()
        {
            string[] channels = GetCurrentSubscriberChannels();
            if (channels != null)
            {
                string multiChannel = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
                if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId))
                {
                    HttpWebRequest request;
                    if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannel) && ChannelRequest[PubnubInstance.InstanceId].TryGetValue(multiChannel, out request) && request != null)
                    {
                        if (request != null)
                        {
                            PNConfiguration currentConfig;
                            IPubnubLog currentLog;
                            if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format("DateTime {0} TerminateCurrentSubsciberRequest {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()), currentConfig.LogVerbosity);
                            }
                            try
                            {
                                request.Abort();
                            }
                            catch { /* ignore */ }
                        }
                    }
                }
            }
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientSubscribe != null)
            {
                try
                {
                    httpClientSubscribe.CancelPendingRequests();
                }
                catch {  /* ignore */ }
            }
#endif
        }

#endregion

        internal void Announce(PNStatus status)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].Status(PubnubInstance, status);
                }
            }
            
        }

        internal void Announce<T>(PNMessageResult<T> message)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].Message(PubnubInstance, message);
                }
            }
        }

        internal void Announce(PNPresenceEventResult presence)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].Presence(PubnubInstance, presence);
                }
            }
        }
    }
}
