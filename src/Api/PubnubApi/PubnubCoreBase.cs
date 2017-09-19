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
        private bool enableResumeOnReconnect = true;
        protected bool OverrideTcpKeepAlive { get; set; } = true;
        //protected static System.Threading.Timer LocalClientHeartBeatTimer { get; set; } = null;
        protected System.Threading.Timer PresenceHeartbeatTimer { get; set; } = null;
        protected static bool PubnetSystemActive { get; set; } = true;
        protected Collection<Uri> PushRemoteImageDomainUri { get; set; } = new Collection<Uri>();
        protected int ConnectionErrors { get; set; } = 0;
        #endregion

        private const int MINEXPONENTIALBACKOFF = 1;
        private const int MAXEXPONENTIALBACKOFF = 32;
        private const int INTERVAL = 3;

        private IPubnubHttp pubnubHttp = null;
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubUnitTest unitTest = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr = null;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClientSubscribe = null;
        private static HttpClient httpClientNonsubscribe = null;
        private static HttpClient httpClientNetworkStatus = null;
#endif

        private bool clientNetworkStatusInternetStatus = true;
        protected static Dictionary<string, bool> SubscribeDisconnected = new Dictionary<string, bool>();

        protected Pubnub PubnubInstance { get; set; } = null;

        protected bool UuidChanged { get; set; }

        protected string CurrentUuid { get; set; }

        protected static Dictionary<string, long> LastSubscribeTimetoken { get; set; } = new Dictionary<string, long>();

        protected int PubnubNetworkTcpCheckIntervalInSeconds { get; set; } = 3;
        private int PubnubLocalHeartbeatCheckIntervalInSeconds { get; set; } = 30;

        protected static Dictionary<string, List<SubscribeCallback>> SubscribeCallbackListenerList
        {
            get;
            set;
        } = new Dictionary<string, List<SubscribeCallback>>();

        protected static Dictionary<string, ConcurrentDictionary<string, long>> MultiChannelSubscribe
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, long>>();

        protected static Dictionary<string, ConcurrentDictionary<string, long>> MultiChannelGroupSubscribe
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, long>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Timer>> ChannelReconnectTimer
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Timer>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Timer>> ChannelGroupReconnectTimer
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Timer>>();

        protected static Dictionary<string, ConcurrentDictionary<string, HttpWebRequest>> ChannelRequest
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, HttpWebRequest>>();

        protected static Dictionary<string, ConcurrentDictionary<string, bool>> ChannelInternetStatus
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, bool>>();

        protected static Dictionary<string, ConcurrentDictionary<string, bool>> ChannelGroupInternetStatus
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, bool>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelLocalUserState
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelUserState
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelGroupLocalUserState
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        protected static Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>> ChannelGroupUserState
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<string, Dictionary<string, object>>>();

        //protected static Dictionary<string, ConcurrentDictionary<Uri, Timer>> ChannelLocalClientHeartbeatTimer { get; set; } = new Dictionary<string, ConcurrentDictionary<Uri, Timer>>();

        protected static ConcurrentDictionary<string, DateTime> SubscribeRequestTracker
        {
            get;
            set;
        } = new ConcurrentDictionary<string, DateTime>();

        public PubnubCoreBase(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager telemetryManager)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }
            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(pubnubConfiguation,log), pubnubUnitTest, log, telemetryManager);
            }
            else
            {
                InternalConstructor(pubnubConfiguation, jsonPluggableLibrary, pubnubUnitTest, log, telemetryManager);
            }
        }

        private void InternalConstructor(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager telemetryManager)
        {
            pubnubConfig = pubnubConfiguation;
            jsonLib = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            CurrentUuid = pubnubConfig.Uuid;

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientSubscribe == null)
            {
                httpClientSubscribe = new HttpClient();
                httpClientSubscribe.DefaultRequestHeaders.Accept.Clear();
                httpClientSubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientSubscribe.Timeout = TimeSpan.FromSeconds(pubnubConfig.SubscribeTimeout);
            }
            if (httpClientNonsubscribe == null)
            {
                httpClientNonsubscribe = new HttpClient();
                httpClientNonsubscribe.DefaultRequestHeaders.Accept.Clear();
                httpClientNonsubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientNonsubscribe.Timeout = TimeSpan.FromSeconds(pubnubConfig.NonSubscribeRequestTimeout);
            }
            pubnubHttp = new PubnubHttp(pubnubConfiguation, jsonLib, pubnubLog, pubnubTelemetryMgr, httpClientSubscribe, httpClientNonsubscribe);
#else
            pubnubHttp = new PubnubHttp(pubnubConfiguation, jsonLib, pubnubLog, pubnubTelemetryMgr);
#endif


            UpdatePubnubNetworkTcpCheckIntervalInSeconds();
            if (pubnubConfig.PresenceInterval > 10)
            {
                PubnubLocalHeartbeatCheckIntervalInSeconds = pubnubConfig.PresenceInterval;
            }
            enableResumeOnReconnect = pubnubConfig.ReconnectionPolicy != PNReconnectionPolicy.NONE;

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
                return;

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
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientNetworkStatus == null)
            {
                httpClientNetworkStatus = new HttpClient();
                httpClientNetworkStatus.DefaultRequestHeaders.Accept.Clear();
                httpClientNetworkStatus.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClientNetworkStatus.Timeout = TimeSpan.FromSeconds(pubnubConfig.NonSubscribeRequestTimeout);
            }
            ClientNetworkStatus clientNetworkStatus = new ClientNetworkStatus(pubnubConfig, jsonLib, unitTest, pubnubLog, httpClientNetworkStatus);
#else
            ClientNetworkStatus clientNetworkStatus = new ClientNetworkStatus(pubnubConfig, jsonLib, unitTest, pubnubLog);
#endif
            if (!clientNetworkStatus.IsInternetCheckRunning())
            {
                clientNetworkStatusInternetStatus = clientNetworkStatus.CheckInternetStatus<T>(PubnetSystemActive, type, callback, channels, channelGroups);
            }
            return clientNetworkStatusInternetStatus;
        }

        //protected void OnPubnubLocalClientHeartBeatTimeoutCallback<T>(System.Object heartbeatState)
        //{
        //    RequestState<T> currentState = heartbeatState as RequestState<T>;
        //    if (currentState != null)
        //    {
        //        string channel = (currentState.Channels != null) ? string.Join(",", currentState.Channels.OrderBy(x => x).ToArray()) : "";
        //        string channelGroup = (currentState.ChannelGroups != null) ? string.Join(",", currentState.ChannelGroups.OrderBy(x => x).ToArray()) : "";

        //        if ((ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channel) || ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channelGroup))
        //                && (currentState.ResponseType == PNOperationType.PNSubscribeOperation || currentState.ResponseType == PNOperationType.Presence || currentState.ResponseType == PNOperationType.PNHeartbeatOperation)
        //                && OverrideTcpKeepAlive)
        //        {
        //            bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, currentState.ResponseType, currentState.PubnubCallback, currentState.Channels, currentState.ChannelGroups);

        //            ChannelInternetStatus[PubnubInstance.InstanceId][channel] = networkConnection;
        //            ChannelGroupInternetStatus[PubnubInstance.InstanceId][channelGroup] = networkConnection;

        //            if (!networkConnection)
        //            {
        //                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, OnPubnubLocalClientHeartBeatTimeoutCallback - Internet connection = {1}", DateTime.Now.ToString(), networkConnection), pubnubConfig.LogVerbosity);
        //                if (pubnubConfig.ReconnectionPolicy == PNReconnectionPolicy.NONE)
        //                {
        //                    if (LocalClientHeartBeatTimer != null)
        //                    {
        //                        try
        //                        {
        //                            LocalClientHeartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //                            LocalClientHeartBeatTimer.Dispose();
        //                        }
        //                        catch { }
        //                    }
        //                }
        //                TerminatePendingWebRequest(currentState);
        //            }
        //        }
        //    }
        //}

        protected long GetTimetokenFromMultiplexResult(List<object> result)
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

        private List<SubscribeMessage> GetMessageFromMultiplexResult(List<object> result)
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
                            switch (key.ToLower())
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
                                            switch (metaKey.ToLower())
                                            {
                                                case "t":
                                                    long timetoken;
                                                    Int64.TryParse(ttOriginMetaData[metaKey].ToString(), out timetoken);
                                                    ttMeta.Timetoken = timetoken;
                                                    break;
                                                case "r":
                                                    ttMeta.Region = ttOriginMetaData[metaKey].ToString();
                                                    break;
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
                                            switch (metaKey.ToLower())
                                            {
                                                case "t":
                                                    long timetoken;
                                                    Int64.TryParse(ttPublishMetaData[metaKey].ToString(), out timetoken);
                                                    ttMeta.Timetoken = timetoken;
                                                    break;
                                                case "r":
                                                    ttMeta.Region = ttPublishMetaData[metaKey].ToString();
                                                    break;
                                            }
                                        }
                                        msg.PublishTimetokenMetadata = ttMeta;
                                    }
                                    //TimetokenMetadata ttMeta = new TimetokenMetadata();
                                    //msg.PublishTimetokenMetadata = dicItem[key] as TimetokenMetadata;
                                    break;
                                case "u":
                                    //TimetokenMetadata ttMeta = new TimetokenMetadata();
                                    msg.UserMetadata = dicItem[key];
                                    break;
                            }
                        }

                        msgList.Add(msg);
                    }
                }
            }

            return msgList;
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
                        if (newChannels != null && newChannels.Count() > 0)
                        {
                            ret = true;
                        }
                        else if (newChannelGroups != null && newChannelGroups.Count() > 0)
                        {
                            ret = true;
                        }
                    }
                }
            }
            catch { }
            return ret;
        }

        private void ResponseToConnectCallback<T>(List<object> result, PNOperationType type, string[] channels, string[] channelGroups, RequestState<T> asyncRequestState)
        {
            StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
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

        private void ResponseToUserCallback<T>(List<object> result, PNOperationType type, string[] channels, string[] channelGroups, RequestState<T> asyncRequestState)
        {
            var userCallback = (asyncRequestState != null) ? asyncRequestState.PubnubCallback : null;
            switch (type)
            {
                case PNOperationType.PNSubscribeOperation:
                case PNOperationType.Presence:
                    List<SubscribeMessage> messageList = GetMessageFromMultiplexResult(result);
                    long messageTimetoken = GetTimetokenFromMultiplexResult(result);

                    if (messageList != null && messageList.Count > 0)
                    {
                        if (messageList.Count >= pubnubConfig.RequestMessageCountThreshold)
                        {
                            StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
                            PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNRequestMessageCountExceededCategory, asyncRequestState, (int)HttpStatusCode.OK, null);
                            Announce(status);
                        }

                        for (int messageIndex=0; messageIndex < messageList.Count; messageIndex++)
                        {
                            SubscribeMessage currentMessage = messageList[messageIndex];
                            //Dictionary<string, object> messageDic = jsonLib.ConvertToDictionaryObject(messageList[messageIndex]);
                            if (currentMessage != null)
                            {
                                string currentMessageChannel = currentMessage.Channel;
                                string currentMessageChannelGroup = currentMessage.SubscriptionMatch;

                                if (currentMessageChannel.Replace("-pnpres","") == currentMessageChannelGroup.Replace("-pnpres", ""))
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
                                    if (pubnubConfig.CipherKey.Length > 0) //decrypt the subscriber message if cipherkey is available
                                    {
                                        string decryptMessage = "";
                                        PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog);
                                        try
                                        {
                                            decryptMessage = aes.Decrypt(payload.ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            decryptMessage = "**DECRYPT ERROR**";

                                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                                            PNStatus status = new StatusBuilder(pubnubConfig, jsonLib).CreateStatusResponse<T>(type, category, null, (int)HttpStatusCode.NotFound, ex);
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
                                    Dictionary<string, object> presencePayload = payload as Dictionary<string, object>;
                                    ResponseBuilder responseBuilder = new ResponseBuilder(pubnubConfig, jsonLib, pubnubLog);
                                    PNPresenceEventResult presenceEvent = responseBuilder.JsonToObject<PNPresenceEventResult>(payloadContainer, true);
                                    if (presenceEvent != null)
                                    {
                                        Announce(presenceEvent);
                                    }
                                }
                                else
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, ResponseToUserCallback - payload = {1}", DateTime.Now.ToString(), jsonLib.SerializeToJsonString(payloadContainer)), pubnubConfig.LogVerbosity);
                                    ResponseBuilder responseBuilder = new ResponseBuilder(pubnubConfig, jsonLib, pubnubLog);
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
                        ResponseBuilder responseBuilder = new ResponseBuilder(pubnubConfig, jsonLib, pubnubLog);
                        T userResult = responseBuilder.JsonToObject<T>(result, true);

                        StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
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
                        ResponseBuilder responseBuilder = new ResponseBuilder(pubnubConfig, jsonLib, pubnubLog);
                        PNHeartbeatResult userResult = responseBuilder.JsonToObject<PNHeartbeatResult>(result, true);

                        if (userResult != null)
                        {
                            if (pubnubConfig.HeartbeatNotificationOption == PNHeartbeatNotificationOption.All)
                            {
                                StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
                                PNStatus status = null;

                                Exception ex = null;
                                if (userResult != null && userResult.Status == 200)
                                {
                                    status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, (int)HttpStatusCode.OK, null);
                                }
                                else
                                {
                                    ex = new Exception(userResult.Message);
                                    status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, userResult.Status, null);
                                }

                                Announce(status);
                            }
                            else if (pubnubConfig.HeartbeatNotificationOption == PNHeartbeatNotificationOption.Failures && userResult.Status != 200)
                            {
                                StatusBuilder statusBuilder = new StatusBuilder(pubnubConfig, jsonLib);
                                PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, userResult.Status, new Exception(userResult.Message));
                                Announce(status);
                            }
                        }

                    }
                    break;
                default:
                    break;
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

            try
            {
                if (terminateCurrentSubRequest)
                {
                    TerminateCurrentSubscriberRequest();
                }

                if (pubnubRequestState != null)
                {
                    if (pubnubRequestState.Channels != null)
                    {
                        channel = (pubnubRequestState.Channels.Length > 0) ? string.Join(",", pubnubRequestState.Channels.OrderBy(x => x).ToArray()) : ",";
                    }
                    if (pubnubRequestState.ChannelGroups != null)
                    {
                        channelGroup = string.Join(",", pubnubRequestState.ChannelGroups.OrderBy(x => x).ToArray());
                    }
                }

                if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId) && !ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel) && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence))
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, UrlProcessRequest ChannelRequest PubnubInstance.InstanceId Channel NOT matching", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
                    return "";
                }

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

                if (OverrideTcpKeepAlive) //overrideTcpKeepAlive must be true
                {
                    //Eventhough heart-beat is disabled, run one time to check internet connection by setting dueTime=0
                    //if (LocalClientHeartBeatTimer != null)
                    //{
                    //    try
                    //    {
                    //        LocalClientHeartBeatTimer.Dispose();
                    //    }
                    //    catch { }
                    //}

                    //LocalClientHeartBeatTimer = new System.Threading.Timer(
                    //    new TimerCallback(OnPubnubLocalClientHeartBeatTimeoutCallback<T>), pubnubRequestState, 0,
                    //    (-1 == PubnubLocalHeartbeatCheckIntervalInSeconds) ? Timeout.Infinite : PubnubLocalHeartbeatCheckIntervalInSeconds * 1000);
                    //ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].AddOrUpdate(requestUri, LocalClientHeartBeatTimer, (key, oldState) => LocalClientHeartBeatTimer);
                }
                else
                {
                    request = pubnubHttp.SetServicePointSetTcpKeepAlive(request); //To be removed.
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Request={1}", DateTime.Now.ToString(), requestUri.ToString()), pubnubConfig.LogVerbosity);

                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    SubscribeRequestTracker.AddOrUpdate(PubnubInstance.InstanceId, DateTime.Now, (key, oldState) => DateTime.Now);
                }

                string jsonString = "";
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

                if (SubscribeDisconnected.ContainsKey(PubnubInstance.InstanceId) && SubscribeDisconnected[PubnubInstance.InstanceId])
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0},Received JSON but SubscribeDisconnected = {1} for request={2}", DateTime.Now.ToString(), jsonString, requestUri), pubnubConfig.LogVerbosity);
                    throw new Exception("Disconnected");
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, JSON= {1} for request={2}", DateTime.Now.ToString(), jsonString, requestUri), pubnubConfig.LogVerbosity);
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
                        exceptionMessage = webEx.Message;
                    }
                    else
                    {
                        innerEx = ex.InnerException;
                        exceptionMessage = innerEx.Message;
                    }
                }
                else
                {
                    innerEx = ex;
                    exceptionMessage = innerEx.Message;
                }

                if (exceptionMessage.IndexOf("The request was aborted: The request was canceled") == -1
                && exceptionMessage.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1
                && exceptionMessage.IndexOf("A task was canceled") == -1)
                {
                    PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(webEx == null ? innerEx : webEx);
                    PNStatus status = new StatusBuilder(pubnubConfig, jsonLib).CreateStatusResponse<T>(pubnubRequestState.ResponseType, category, pubnubRequestState, (int)HttpStatusCode.NotFound, ex);
                    if (pubnubRequestState != null && pubnubRequestState.PubnubCallback != null)
                    {
                        pubnubRequestState.PubnubCallback.OnResponse(default(T), status);
                    }
                    else
                    {
                        Announce(status);
                    }

                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} PubnubBaseCore UrlProcessRequest Exception={1}", DateTime.Now.ToString(), webEx != null ? webEx.ToString() : exceptionMessage), pubnubConfig.LogVerbosity);
                }

                return "";
            }
        }

        internal protected List<object> ProcessJsonResponse<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            List<object> result = new List<object>();

            string channel = "";
            string channelGroup = "";
            PNOperationType type = PNOperationType.None;
            if (asyncRequestState != null)
            {
                if (asyncRequestState.Channels != null)
                {
                    channel = (asyncRequestState.Channels.Length > 0) ? string.Join(",", asyncRequestState.Channels.OrderBy(x => x).ToArray()) : ",";
                }
                if (asyncRequestState.ChannelGroups != null)
                {
                    channelGroup = string.Join(",", asyncRequestState.ChannelGroups.OrderBy(x => x).ToArray());
                }
                type = asyncRequestState.ResponseType;
            }

            bool errorCallbackRaised = false;
            if (jsonLib.IsDictionaryCompatible(jsonString))
            {
                PNStatus status = null;
                Dictionary<string, object> deserializeStatus = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                int statusCode = 0; //default. assuming all is ok 
                if (deserializeStatus.Count == 1 && deserializeStatus.ContainsKey("error"))
                {
                    status = new StatusBuilder(pubnubConfig, jsonLib).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new Exception(jsonString));
                }
                else if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                {
                    Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                    string statusMessage = deserializeStatus["message"].ToString();

                    if (statusCode != 200)
                    {
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                        status = new StatusBuilder(pubnubConfig, jsonLib).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new Exception(jsonString));
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
            if (!errorCallbackRaised)
            {
                result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.Request, asyncRequestState.PubnubCallback);
            }

            return result;
        }

        protected List<object> WrapResultBasedOnResponseType<T>(PNOperationType type, string jsonString, string[] channels, string[] channelGroups, bool reconnect, long lastTimetoken, HttpWebRequest request, PNCallback<T> callback)
        {
            List<object> result = new List<object>();

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
                            case PNOperationType.PNPublishOperation:
                            case PNOperationType.PNFireOperation:
                                #region "Publish"
                                result.Add(multiChannel);
                                #endregion
                                break;
                            case PNOperationType.PNHistoryOperation:
                                result = SecureMessage.Instance(pubnubConfig, jsonLib, pubnubLog).DecodeDecryptLoop(result, channels, channelGroups, callback);
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
            catch (Exception) { }

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
                    ResponseToConnectCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState);
                }
                else
                {
                    ResponseToUserCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState);
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
                    channelUserStateDictionary = ChannelLocalUserState[PubnubInstance.InstanceId][channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = ChannelGroupLocalUserState[PubnubInstance.InstanceId][channelGroup];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(channel) && ChannelUserState[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    channelUserStateDictionary = ChannelUserState[PubnubInstance.InstanceId][channel];
                }
                if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = ChannelGroupUserState[PubnubInstance.InstanceId][channelGroup];
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
                try
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, TerminatePendingWebRequest - {1}", DateTime.Now.ToString(), state.Request.RequestUri.ToString()), pubnubConfig.LogVerbosity);
                    state.Request.Abort();
                }
                catch { }
            }
            else
            {
                ICollection<string> keyCollection = ChannelRequest[PubnubInstance.InstanceId].Keys;
                foreach (string key in keyCollection)
                {
                    HttpWebRequest currentRequest = ChannelRequest[PubnubInstance.InstanceId][key];
                    if (currentRequest != null)
                    {
                        TerminatePendingWebRequest(currentRequest);
                    }
                }
            }
        }

        protected void TerminatePendingWebRequest(HttpWebRequest request)
        {
            if (request != null)
            {
                try
                {
                    request.Abort();
                }
                catch { }
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
                string channel = (state.Channels != null) ? string.Join(",", state.Channels.OrderBy(x => x).ToArray()) : ",";

                if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    HttpWebRequest removedRequest;
                    bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(channel, out removedRequest);
                    if (removeKey)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), channel), pubnubConfig.LogVerbosity);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), channel), pubnubConfig.LogVerbosity);
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
                        HttpWebRequest currentRequest = ChannelRequest[PubnubInstance.InstanceId][key];
                        if (currentRequest != null)
                        {
                            bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(key, out currentRequest);
                            if (removeKey)
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }
        }

        private void RemoveUserState()
        {
            if (ChannelLocalUserState.Count == 0 || !ChannelLocalUserState.ContainsKey(PubnubInstance.InstanceId)) return;

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
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
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
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channel= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
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
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to RemoveUserState from local user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
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
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to RemoveUserState from user state dictionary for channelgroup= {1}", DateTime.Now.ToString(), key), pubnubConfig.LogVerbosity);
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

        protected void TerminateTelemetry()
        {
            if (pubnubTelemetryMgr != null)
            {
                pubnubTelemetryMgr.Destroy();
            }
        }

        //protected virtual void TerminateLocalClientHeartbeatTimer()
        //{
        //    //TerminateLocalClientHeartbeatTimer(null);
        //}

        //protected virtual void TerminateLocalClientHeartbeatTimer(Uri requestUri)
        //{
        //    if (ChannelLocalClientHeartbeatTimer.Count == 0) return;

        //    if (requestUri != null)
        //    {
        //        if (ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].ContainsKey(requestUri))
        //        {
        //            Timer requestHeatbeatTimer = null;
        //            if (ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].TryGetValue(requestUri, out requestHeatbeatTimer) && requestHeatbeatTimer != null)
        //            {
        //                try
        //                {
        //                    requestHeatbeatTimer.Change(
        //                        (-1 == PubnubLocalHeartbeatCheckIntervalInSeconds) ? -1 : PubnubLocalHeartbeatCheckIntervalInSeconds * 1000,
        //                        (-1 == PubnubLocalHeartbeatCheckIntervalInSeconds) ? -1 : PubnubLocalHeartbeatCheckIntervalInSeconds * 1000);
        //                    requestHeatbeatTimer.Dispose();
        //                }
        //                catch (ObjectDisposedException){ /*Known exception to be ignored*/ }

        //                Timer removedTimer = null;
        //                bool removed = ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].TryRemove(requestUri, out removedTimer);
        //                if (removed)
        //                {
        //                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), requestUri.ToString()), pubnubConfig.LogVerbosity);
        //                }
        //                else
        //                {
        //                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), requestUri.ToString()), pubnubConfig.LogVerbosity);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ConcurrentDictionary<Uri, Timer> timerCollection = ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId];
        //        ICollection<Uri> keyCollection = timerCollection.Keys;
        //        if (keyCollection != null && keyCollection.Count > 0)
        //        {
        //            List<Uri> keyList = keyCollection.ToList();
        //            foreach (Uri key in keyList)
        //            {
        //                if (ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].ContainsKey(key))
        //                {
        //                    Timer currentTimer = null;
        //                    if (ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].TryGetValue(key, out currentTimer) && currentTimer != null)
        //                    {
        //                        currentTimer.Dispose();
        //                        Timer removedTimer = null;
        //                        bool removed = ChannelLocalClientHeartbeatTimer[PubnubInstance.InstanceId].TryRemove(key, out removedTimer);
        //                        if (!removed)
        //                        {
        //                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} TerminateLocalClientHeartbeatTimer(null) - Unable to remove local client heartbeat reference from collection for {1}", DateTime.Now.ToString(), key.ToString()), pubnubConfig.LogVerbosity);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        protected void UpdatePubnubNetworkTcpCheckIntervalInSeconds()
        {
            int timerInterval;

            if (pubnubConfig.ReconnectionPolicy == PNReconnectionPolicy.EXPONENTIAL)
            {
                timerInterval = (int)(Math.Pow(2, ConnectionErrors) - 1);
                if (timerInterval > MAXEXPONENTIALBACKOFF)
                {
                    timerInterval = MINEXPONENTIALBACKOFF;
                    ConnectionErrors = 1;
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, EXPONENTIAL timerInterval > MAXEXPONENTIALBACKOFF", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
                }
                else if (timerInterval < 1)
                {
                    timerInterval = MINEXPONENTIALBACKOFF;
                }
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, EXPONENTIAL timerInterval = {1}", DateTime.Now.ToString(), timerInterval.ToString()), pubnubConfig.LogVerbosity);
            }
            else if (pubnubConfig.ReconnectionPolicy == PNReconnectionPolicy.LINEAR)
            {
                timerInterval = INTERVAL;
            }
            else
            {
                timerInterval = -1;
            }

            PubnubNetworkTcpCheckIntervalInSeconds = timerInterval;
        }

        protected void TerminateReconnectTimer()
        {
            if (ChannelReconnectTimer.Count == 0 || !ChannelReconnectTimer.ContainsKey(PubnubInstance.InstanceId)) return;

            ConcurrentDictionary<string, Timer> channelReconnectCollection = ChannelReconnectTimer[PubnubInstance.InstanceId];
            ICollection<string> keyCollection = channelReconnectCollection.Keys;
            if (keyCollection != null && keyCollection.Count > 0)
            {
                List<string> keyList = keyCollection.ToList();
                foreach (string key in keyList)
                {
                    if (ChannelReconnectTimer[PubnubInstance.InstanceId].ContainsKey(key))
                    {
                        Timer currentTimer = ChannelReconnectTimer[PubnubInstance.InstanceId][key];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = ChannelReconnectTimer[PubnubInstance.InstanceId].TryRemove(key, out removedTimer);
                        if (!removed)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channel reconnect timer reference from collection for {1}", DateTime.Now.ToString(), key.ToString()), pubnubConfig.LogVerbosity);
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
                    if (ChannelGroupReconnectTimer.ContainsKey(groupKey))
                    {
                        Timer currentTimer = ChannelGroupReconnectTimer[PubnubInstance.InstanceId][groupKey];
                        currentTimer.Dispose();
                        Timer removedTimer = null;
                        bool removed = ChannelGroupReconnectTimer[PubnubInstance.InstanceId].TryRemove(groupKey, out removedTimer);
                        if (!removed)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} TerminateReconnectTimer(null) - Unable to remove channelgroup reconnect timer reference from collection for {1}", DateTime.Now.ToString(), groupKey.ToString()), pubnubConfig.LogVerbosity);
                        }
                    }
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
            PubnubInstance = null;
        }

        internal void TerminateCurrentSubscriberRequest()
        {
            string[] channels = GetCurrentSubscriberChannels();
            if (channels != null)
            {
                string multiChannel = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
                if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId))
                {
                    HttpWebRequest request = ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannel) ? ChannelRequest[PubnubInstance.InstanceId][multiChannel] : null;
                    if (request != null)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} TerminateCurrentSubsciberRequest {1}", DateTime.Now.ToString(), request.RequestUri.ToString()), pubnubConfig.LogVerbosity);
                        try
                        {
                            request.Abort();
                        }
                        catch { }
                    }
                }
            }
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
            if (httpClientSubscribe != null)
            {
                try
                {
                    httpClientSubscribe.CancelPendingRequests();
                    //httpClientSubscribe.Dispose();
                    //httpClientSubscribe = null;
                }
                catch { }
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
