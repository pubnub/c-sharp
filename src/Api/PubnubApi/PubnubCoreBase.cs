﻿#region "Header"
#if (__MonoCS__)
#define TRACE
#endif
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Linq;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using System.Collections.Concurrent;
using System.Xml.Linq;

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
        protected static int ConnectionErrors { get; set; }
        #endregion

        private const int MINEXPONENTIALBACKOFF = 1;
        private const int MAXEXPONENTIALBACKOFF = 32;
        private const int INTERVAL = 3;
        
        private static ConcurrentDictionary<string, PNConfiguration> pubnubConfig { get; } = new ConcurrentDictionary<string, PNConfiguration>();
        private static IJsonPluggableLibrary jsonLib;
        private static IPubnubUnitTest unitTest;
        private static ConcurrentDictionary<string, IPubnubLog> pubnubLog { get; } = new ConcurrentDictionary<string, IPubnubLog>();
        protected static ConcurrentDictionary<string, EndPoint.TokenManager> PubnubTokenMgrCollection { get; } = new ConcurrentDictionary<string, EndPoint.TokenManager>();
        private static EndPoint.DuplicationManager pubnubSubscribeDuplicationManager { get; set; }

        private bool clientNetworkStatusInternetStatus = true;
        protected static ConcurrentDictionary<string, CancellationTokenSource> OngoingSubscriptionCancellationTokenSources { get; } = new();
        protected static ConcurrentDictionary<string, bool> IsCurrentSubscriptionCancellationRequested { get; } = new();
        protected static ConcurrentDictionary<string, bool> SubscribeDisconnected { get; set; } = new ConcurrentDictionary<string, bool>();

        protected Pubnub PubnubInstance { get; set; }

        protected PubnubLogModule logger;
        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> SubscriptionChannels
        {
            get;
        } = new();
        
        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> SubscriptionChannelGroups
        {
            get;
        } = new();
        protected static ConcurrentDictionary<string, bool> UserIdChanged { get; set; } = new ConcurrentDictionary<string, bool>();

        protected static ConcurrentDictionary<string, UserId> CurrentUserId { get; set; } = new ConcurrentDictionary<string, UserId>();

        protected static ConcurrentDictionary<string, long> LastSubscribeTimetoken { get; } = new();
        protected static ConcurrentDictionary<string, int> LastSubscribeRegion { get; } = new();

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

        protected static ConcurrentDictionary<string, ConcurrentDictionary<string, CancellationTokenSource>> ChannelRequest
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CancellationTokenSource>>();

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
        
        protected PubnubCoreBase(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, EndPoint.TokenManager tokenManager, Pubnub instance)
        {
            if (pubnubConfiguation == null)
            {
                throw new ArgumentException("PNConfiguration missing");
            }
            
            if (jsonPluggableLibrary == null)
            {
                InternalConstructor(pubnubConfiguation, new NewtonsoftJsonDotNet(pubnubConfiguation), pubnubUnitTest, tokenManager, instance);
            }
            else
            {
                InternalConstructor(pubnubConfiguation, jsonPluggableLibrary, pubnubUnitTest,tokenManager, instance);
            }
        }

        private void InternalConstructor(PNConfiguration pubnubConfiguation, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, EndPoint.TokenManager tokenManager, Pubnub instance)
        {
            PubnubInstance = instance;
            pubnubConfig.AddOrUpdate(instance.InstanceId, pubnubConfiguation, (k,o)=> pubnubConfiguation);
            jsonLib = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
            PubnubTokenMgrCollection.AddOrUpdate(instance.InstanceId, tokenManager, (k,o)=> tokenManager);
            pubnubSubscribeDuplicationManager = new EndPoint.DuplicationManager(pubnubConfiguation, jsonPluggableLibrary);
            CurrentUserId.AddOrUpdate(instance.InstanceId, pubnubConfiguation.UserId, (k,o) => pubnubConfiguation.UserId);
            UpdatePubnubNetworkTcpCheckIntervalInSeconds();
            if (pubnubConfiguation.PresenceInterval > 10)
            {
                PubnubLocalHeartbeatCheckIntervalInSeconds = pubnubConfiguation.PresenceInterval;
            }
            enableResumeOnReconnect = pubnubConfiguation.ReconnectionPolicy != PNReconnectionPolicy.NONE;
            logger = pubnubConfiguation.Logger;
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
            ClientNetworkStatus.PubnubInstance = PubnubInstance;
            if (!ClientNetworkStatus.IsInternetCheckRunning())
            {
                clientNetworkStatusInternetStatus = ClientNetworkStatus
                    .CheckInternetStatus<T>(PubnetSystemActive, type, callback, channels, channelGroups);
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
                    var _ = Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
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
                        var _ = Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
                    }
                }
                else
                {
                    var _ = long.TryParse(result[1].ToString(), out jsonTimetoken);
                }
            }

            return jsonTimetoken;
        }

        protected static int GetRegionFromMultiplexResult(List<object> result)
        {
            int jsonRegion = 0;
            Dictionary<string, object> timetokenObj = jsonLib.ConvertToDictionaryObject(result[0]);

            if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
            {
                Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("r"))
                {
                    var _ = Int32.TryParse(timeAndRegionDictionary["r"].ToString(), out jsonRegion);
                }
            }
            else
            {
                timetokenObj = jsonLib.ConvertToDictionaryObject(result[1]);
                if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
                {
                    Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                    if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("r"))
                    {
                        var _ = Int32.TryParse(timeAndRegionDictionary["r"].ToString(), out jsonRegion);
                    }
                }
            }

            return jsonRegion;
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
                                case "cmt":
                                    msg.CustomMessageType = dicItem[key]?.ToString();
                                    break;
                                case "d":
                                    msg.Payload = dicItem[key];
                                    break;
                                case "e":
                                    int subscriptionTypeIndicator;
                                    var _ = Int32.TryParse(dicItem[key].ToString(), out subscriptionTypeIndicator);
                                    msg.MessageType = subscriptionTypeIndicator;
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
                                    _ = Int32.TryParse(dicItem[key].ToString(), out seqNum);
                                    msg.SequenceNumber = seqNum;
                                    break;
                                case "o":
                                    Dictionary<string, object> ttOriginMetaData = jsonLib.ConvertToDictionaryObject(dicItem[key]);
                                    if (ttOriginMetaData != null && ttOriginMetaData.Count > 0)
                                    {
                                        TimetokenMetadata ttMeta = new TimetokenMetadata();

                                        foreach (string metaKey in ttOriginMetaData.Keys)
                                        {
                                            if (metaKey.ToLowerInvariant().Equals("t", StringComparison.OrdinalIgnoreCase))
                                            {
                                                long timetoken;
                                                _ = Int64.TryParse(ttOriginMetaData[metaKey].ToString(), out timetoken);
                                                ttMeta.Timetoken = timetoken;
                                            }
                                            else if (metaKey.ToLowerInvariant().Equals("r", StringComparison.OrdinalIgnoreCase))
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
                                                _ = Int64.TryParse(ttPublishMetaData[metaKey].ToString(), out timetoken);
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
                        LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Dedupe - Duplicate skipped - msg = {jsonLib.SerializeToJsonString(message)}", currentConfig.LogVerbosity);
                    }
                }
                else
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Dedupe - AddEntry - msg = {jsonLib.SerializeToJsonString(message)}", currentConfig.LogVerbosity);
                    }
                    pubnubSubscribeDuplicationManager.AddEntry(message);
                }
            }
            catch (Exception ex)
            {
                //Log and ignore any exception due to Dedupe manager
                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                {
                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] IsTargetFor Dedupe - dedupe error = {ex}", currentConfig.LogVerbosity);
                }
            }

            return isTargetOfDedup;
        }

        private bool IsZeroTimeTokenRequest<T>(RequestState<T> requestState, List<object> result)
        {
            bool ret = false;
            PNConfiguration currentConfig = null;
            IPubnubLog currentLog = null;
            try
            {
                pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig);
                pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog);

                if (requestState != null && requestState.ResponseType == PNOperationType.PNSubscribeOperation && requestState.Timetoken == 0 && result != null && result.Count > 0)
                {
                    List<SubscribeMessage> message = GetMessageFromMultiplexResult(result);
                    if (message != null && message.Count == 0)
                    {
                            ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (currentConfig != null && currentLog != null)
                {
                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: IsZeroTimeTokenRequest - Exception = {ex}", currentConfig.LogVerbosity);
                }
            }
            return ret;
        }

        private void ResponseToConnectCallback<T>(PNOperationType type, string[] channels, string[] channelGroups, RequestState<T> asyncRequestState)
        {
            PNConfiguration currentConfig;
            pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig);
            StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
            PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNConnectedCategory, asyncRequestState, Constants.HttpRequestSuccessStatusCode, null);

            //Check callback exists and make sure previous timetoken = 0
            if (channels != null && channels.Length > 0)
            {
                IEnumerable<string> newChannels = from channel in MultiChannelSubscribe[PubnubInstance.InstanceId]
                                                  where channel.Value == 0
                                                  select channel.Key;
                if (newChannels != null && newChannels.Any())
                {
                    status.AffectedChannels = newChannels.ToList();
                }
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                IEnumerable<string> newChannelGroups = from channelGroup in MultiChannelGroupSubscribe[PubnubInstance.InstanceId]
                                                       where channelGroup.Value == 0
                                                       select channelGroup.Key;

                if (newChannelGroups != null && newChannelGroups.Any())
                {
                    status.AffectedChannelGroups = newChannelGroups.ToList();
                }
            }

            Announce(status);
        }

        private void ResponseToUserCallback<T>(List<object> result, PNOperationType type, RequestState<T> asyncRequestState)
        {
            PNConfiguration currentConfig = null;
            IPubnubLog currentLog = null;
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
                                PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNRequestMessageCountExceededCategory, asyncRequestState, Constants.HttpRequestSuccessStatusCode, null);
                                Announce(status);
                            }

                            if (currentConfig != null && currentLog != null)
                            {
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback - messageList.Count = {messageList.Count}", currentConfig.LogVerbosity);
                            }
                            for (int messageIndex = 0; messageIndex < messageList.Count; messageIndex++)
                            {
                                SubscribeMessage currentMessage = messageList[messageIndex];
                                if (currentMessage != null)
                                {
                                    if (currentConfig != null && currentLog != null && currentConfig.DedupOnSubscribe && IsTargetForDedup(currentMessage))
                                    {
                                        LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback - messageList for loop - messageIndex = {messageIndex} => IsTargetForDedupe", currentConfig.LogVerbosity);
                                        continue;
                                    }

                                    string currentMessageChannel = currentMessage.Channel;
                                    string currentMessageChannelGroup = currentMessage.SubscriptionMatch;

                                    if (currentMessageChannel.Replace("-pnpres", "") == currentMessageChannelGroup.Replace("-pnpres", ""))
                                    {
                                        currentMessageChannelGroup = "";
                                    }

                                    object payload = currentMessage.Payload;
                                    var jsonFields = new Dictionary<string, object>();

                                    if (currentMessageChannel.Contains("-pnpres") || currentMessageChannel.Contains(".*-pnpres"))
                                    {
                                        jsonFields.Add("payload", payload);
                                    }
                                    else if (currentMessage.MessageType == 2) //Objects Simplification events
                                    {
                                        double objectsVersion = -1;
                                        Dictionary<string, object> objectsDic = payload as Dictionary<string, object>;
                                        if (objectsDic != null 
                                            && objectsDic.ContainsKey("source")
                                            && objectsDic.ContainsKey("version")
                                            && objectsDic["source"].ToString() == "objects" 
                                            && Double.TryParse(objectsDic["version"].ToString().Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out objectsVersion))
                                        {
                                            if (objectsVersion.CompareTo(2D) == 0) //Process only version=2 for Objects Simplification. Ignore 1. 
                                            {
                                                jsonFields.Add("payload", payload);
                                            }
                                            else
                                            {
                                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback - Legacy Objects V1. Ignoring this.", currentConfig.LogVerbosity);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback - MessageType =2 but NOT valid format to process", currentConfig.LogVerbosity);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if ((currentConfig.CryptoModule != null || currentConfig.CipherKey.Length > 0) && currentMessage.MessageType != 1) //decrypt the subscriber message if cipherkey is available
                                        {
                                            string decryptMessage = "";
                                            currentConfig.CryptoModule ??= new CryptoModule(new LegacyCryptor(currentConfig.CipherKey, currentConfig.UseRandomInitializationVector), null);
                                            try
                                            {
                                                decryptMessage = currentConfig.CryptoModule.Decrypt(payload.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                decryptMessage = "**DECRYPT ERROR**";

                                                PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                                                PNStatus status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, category, null, Constants.ResourceNotFoundStatusCode, new PNException(ex));
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
                                                
                                                LoggingMethod.WriteToLog(
                                                        currentLog,
                                                        $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Failed to decrypt message on channel {currentMessageChannel} in ResponseToUserCallback due to exception={ex}.\nMessage might be not encrypted, returning content as received",
                                                        currentConfig.LogVerbosity
                                                );
                                            }
                                            object decodeMessage = jsonLib.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLib.SerializeToJsonString(payload) : decryptMessage);
                                            jsonFields.Add("payload", decodeMessage);
                                        }
                                        else
                                        {
                                            string payloadJson = jsonLib.SerializeToJsonString(payload);
                                            object payloadJObject = jsonLib.BuildJsonObject(payloadJson);
                                            if (payloadJObject == null)
                                            {
                                                jsonFields.Add("payload", payload);
                                            }
                                            else
                                            {
                                                jsonFields.Add("payload", payloadJObject);    
                                            }
                                        }
                                    }

                                    object userMetaData = currentMessage.UserMetadata;
                                    jsonFields.Add("userMetadata", userMetaData);

                                    jsonFields.Add("publishTimetoken", currentMessage.PublishTimetokenMetadata.Timetoken);

                                    jsonFields.Add("userId", currentMessage.IssuingClientId);

                                    jsonFields.Add("channelGroup", currentMessageChannelGroup);

                                    jsonFields.Add("channel", currentMessageChannel);

                                    if (currentMessage.MessageType == 1)
                                    {
                                        jsonFields.Add("customMessageType", currentMessage.CustomMessageType);
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNMessageResult<T> pnMessageResult = responseBuilder.GetEventResultObject<PNMessageResult<T>>(jsonFields);
                                        if (pnMessageResult != null)
                                        {
                                            PNSignalResult<T> signalMessage = new PNSignalResult<T>
                                            {
                                                Channel = pnMessageResult.Channel,
                                                Message = pnMessageResult.Message,
                                                Subscription = pnMessageResult.Subscription,
                                                Timetoken = pnMessageResult.Timetoken,
                                                UserMetadata = pnMessageResult.UserMetadata,
                                                Publisher = pnMessageResult.Publisher,
                                                CustomMessageType = pnMessageResult.CustomMessageType
                                            };
                                            Announce(signalMessage);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 2)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNObjectEventResult objectApiEvent = responseBuilder.GetEventResultObject<PNObjectEventResult>(jsonFields);
                                        if (objectApiEvent != null)
                                        {
                                            Announce(objectApiEvent);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 3)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNMessageActionEventResult messageActionEvent = responseBuilder.GetEventResultObject<PNMessageActionEventResult>(jsonFields);
                                        if (messageActionEvent != null)
                                        {
                                            Announce(messageActionEvent);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 4)
                                    {
                                        jsonFields.Add("customMessageType", currentMessage.CustomMessageType);
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNMessageResult<object> filesEvent = responseBuilder.GetEventResultObject<PNMessageResult<object>>(jsonFields);
                                        if (filesEvent != null)
                                        {
                                            PNFileEventResult fileMessage = new PNFileEventResult
                                            {
                                                Channel = filesEvent.Channel,
                                                Subscription = filesEvent.Subscription,
                                                Timetoken = filesEvent.Timetoken,
                                                Publisher = filesEvent.Publisher,
                                            };
                                            Dictionary<string, object> fileEventMessageField = jsonLib.ConvertToDictionaryObject(filesEvent.Message);
                                            if (fileEventMessageField != null && fileEventMessageField.Count > 0)
                                            {
                                                if (fileEventMessageField.ContainsKey("message") && fileEventMessageField["message"]!= null)
                                                {
                                                    fileMessage.Message = fileEventMessageField["message"];
                                                }
                                                if (fileEventMessageField.ContainsKey("file"))
                                                {
                                                    Dictionary<string, object> fileDetailFields = jsonLib.ConvertToDictionaryObject(fileEventMessageField["file"]);
                                                    if (fileDetailFields != null && fileDetailFields.ContainsKey("id") && fileDetailFields.ContainsKey("name"))
                                                    {
                                                        fileMessage.File = new PNFile { Id = fileDetailFields["id"].ToString(), Name = fileDetailFields["name"].ToString() };
                                                        PubnubTokenMgrCollection.TryGetValue(
                                                            PubnubInstance.InstanceId ?? "", out var tokenManager);
                                                        fileMessage.File.Url = UriUtil.GetFileUrl(fileName: fileMessage.File.Name, fileId: fileMessage.File.Id, channel:fileMessage.Channel,
                                                            pnConfiguration:currentConfig, pubnub:PubnubInstance, tokenmanager: tokenManager);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (filesEvent.Message != null)
                                                {
                                                    fileMessage.Message = filesEvent.Message;
                                                }
                                            }
                                            Announce(fileMessage);
                                        }
                                    }
                                    else if (currentMessageChannel.Contains("-pnpres"))
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNPresenceEventResult presenceEvent = responseBuilder.GetEventResultObject<PNPresenceEventResult>(jsonFields);
                                        if (presenceEvent != null)
                                        {
                                            Announce(presenceEvent);
                                        }
                                    }
                                    else
                                    {
                                        if (currentConfig != null && currentLog != null)
                                        {
                                            LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback", currentConfig.LogVerbosity);
                                        }
                                        jsonFields.Add("customMessageType", currentMessage.CustomMessageType);
                                        ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                                        PNMessageResult<T> userMessage = responseBuilder.GetEventResultObject<PNMessageResult<T>>(jsonFields);
                                        if (userMessage != null)
                                        {
                                            Announce(userMessage);
                                        }
                                    }

                                }
                                else
                                {
                                    if (currentConfig != null && currentLog != null)
                                    {
                                        LoggingMethod.WriteToLog(currentLog,$"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ResponseToUserCallback - messageList for loop - messageIndex = {messageIndex} => null message", currentConfig.LogVerbosity);
                                    }
                                }
                            }

                        }
                        break;
                    case PNOperationType.PNTimeOperation:
                    case PNOperationType.PNPublishOperation:
                    case PNOperationType.PNFireOperation:
                    case PNOperationType.PNSignalOperation:
                    case PNOperationType.PNHistoryOperation:
                    case PNOperationType.PNFetchHistoryOperation:
                    case PNOperationType.PNDeleteMessageOperation:
                    case PNOperationType.PNMessageCountsOperation:
                    case PNOperationType.PNHereNowOperation:
                    case PNOperationType.PNWhereNowOperation:
                    case PNOperationType.PNAccessManagerGrantToken:
                    case PNOperationType.PNAccessManagerGrant:
                    case PNOperationType.PNAccessManagerRevokeToken:
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
                    case PNOperationType.PNSetUuidMetadataOperation:
                    case PNOperationType.PNDeleteUuidMetadataOperation:
                    case PNOperationType.PNGetAllUuidMetadataOperation:
                    case PNOperationType.PNGetUuidMetadataOperation:
                    case PNOperationType.PNSetChannelMetadataOperation:
                    case PNOperationType.PNDeleteChannelMetadataOperation:
                    case PNOperationType.PNGetAllChannelMetadataOperation:
                    case PNOperationType.PNGetChannelMetadataOperation:
                    case PNOperationType.PNGetMembershipsOperation:
                    case PNOperationType.PNManageMembershipsOperation:
                    case PNOperationType.PNSetMembershipsOperation:
                    case PNOperationType.PNRemoveMembershipsOperation:
                    case PNOperationType.PNGetChannelMembersOperation:
                    case PNOperationType.PNManageChannelMembersOperation:
                    case PNOperationType.PNSetChannelMembersOperation:
                    case PNOperationType.PNRemoveChannelMembersOperation:
                    case PNOperationType.PNAddMessageActionOperation:
                    case PNOperationType.PNRemoveMessageActionOperation:
                    case PNOperationType.PNGetMessageActionsOperation:
                    case PNOperationType.PNGenerateFileUploadUrlOperation:
                    case PNOperationType.PNPublishFileMessageOperation:
                    case PNOperationType.PNListFilesOperation:
                    case PNOperationType.PNDeleteFileOperation:
                        if (result != null && result.Count > 0)
                        {
                            ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
                            T userResult = responseBuilder.JsonToObject<T>(result, true);

                            StatusBuilder statusBuilder = new StatusBuilder(currentConfig, jsonLib);
                            PNStatus status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState,Constants.HttpRequestSuccessStatusCode, null);

                            if (userCallback != null)
                            {
                                userCallback.OnResponse(userResult, status);
                            }
                        }
                        break;
                    case PNOperationType.PNHeartbeatOperation:
                        if (result != null && result.Count > 0)
                        {
                            ResponseBuilder responseBuilder = new ResponseBuilder(currentConfig, jsonLib);
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
                                        status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, Constants.HttpRequestSuccessStatusCode, null);
                                    }
                                    else
                                    {
                                        ex = new PNException(userResult.Message);
                                        status = statusBuilder.CreateStatusResponse(type, PNStatusCategory.PNAcknowledgmentCategory, asyncRequestState, userResult.Status, ex);
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
                if (currentConfig != null && currentLog != null)
                {
                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: ResponseToUserCallback - Exception = {ex}", currentConfig.LogVerbosity);
                }
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

        protected string[] GetCurrentSubscriberChannels()
        {
            string[] channels = null;
            if (SubscriptionChannels.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannels[PubnubInstance.InstanceId] != null)
            {
                channels = SubscriptionChannels[PubnubInstance.InstanceId].Keys.ToArray();
            }

            return channels;
        }

        protected string[] GetCurrentSubscriberChannelGroups()
        {
            string[] channelGroups = null;
            if (SubscriptionChannelGroups.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannelGroups[PubnubInstance.InstanceId] != null)
            {
                channelGroups = SubscriptionChannelGroups[PubnubInstance.InstanceId].Keys.ToArray();
            }

            return channelGroups;
        }

        internal protected List<object> ProcessJsonResponse<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            List<object> result = new List<object>();

            bool errorCallbackRaised = false;
            PNStatus status = GetStatusIfError<T>(asyncRequestState, jsonString);
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
            if (!errorCallbackRaised && asyncRequestState != null)
            {
                result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.PubnubCallback);
            }

            return result;
        }

        protected PNStatus GetStatusIfError<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            PNStatus status = null;
            if (string.IsNullOrEmpty(jsonString)) { return status;  }

            PNConfiguration currentConfig;
            PNOperationType type = PNOperationType.None;
            if (asyncRequestState != null)
            {
                type = asyncRequestState.ResponseType;
            }
            if (jsonLib.IsDictionaryCompatible(jsonString, type))
            {
                Dictionary<string, object> deserializeStatus = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                int statusCode = 0; //default. assuming all is ok 
                if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && string.Equals(deserializeStatus["error"].ToString(), "true", StringComparison.OrdinalIgnoreCase))
                {
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                    {
                        status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
                    }
                }
                else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && deserializeStatus.ContainsKey("status") && Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode) && statusCode > 0)
                {
                    string errorMessageJson = deserializeStatus["error"].ToString();
                    Dictionary<string, object> errorDic = null;
                    if (jsonLib.IsDictionaryCompatible(errorMessageJson, type))
                    {
                        errorDic = jsonLib.DeserializeToDictionaryOfObject(errorMessageJson);
                    }
                    if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("message")
                        && statusCode != 200 && pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                    {
                        string statusMessage = errorDic["message"].ToString();
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                        status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                    }
                }
                else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("status") && string.Equals(deserializeStatus["status"].ToString(), "error", StringComparison.OrdinalIgnoreCase) && deserializeStatus.ContainsKey("error"))
                {
                    Dictionary<string, object> errorDic = null;
                    string errorMessageJson = deserializeStatus["error"].ToString();
                    if (jsonLib.IsDictionaryCompatible(errorMessageJson, type))
                    {
                        errorDic = jsonLib.DeserializeToDictionaryOfObject(errorMessageJson);
                    }
                    if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("code") && errorDic.ContainsKey("message"))
                    {
                        statusCode = PNStatusCodeHelper.GetHttpStatusCode(errorDic["code"].ToString());
                        string statusMessage = errorDic["message"].ToString();
                        if (statusCode != 200)
                        {
                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                            if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                            {
                                status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                            }
                        }
                    }
                }
                else if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                {
                    var _ = Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
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

            }
            else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<head", StringComparison.CurrentCultureIgnoreCase) == 0
                || jsonString.ToLowerInvariant().TrimStart().IndexOf("<html", StringComparison.CurrentCultureIgnoreCase) == 0
                || jsonString.ToLowerInvariant().TrimStart().IndexOf("<!doctype", StringComparison.CurrentCultureIgnoreCase) == 0)//Html is not expected. Only json format messages are expected.
            {
                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                {
                    status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, Constants.ResourceNotFoundStatusCode, new PNException(jsonString));
                }
            }
            else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<?xml", StringComparison.CurrentCultureIgnoreCase) == 0
                  || jsonString.ToLowerInvariant().TrimStart().IndexOf("<Error", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                {
                    var parsedXml = XDocument.Parse(jsonString);
                    var errorElement = parsedXml.Root;
                    var errorMessage = string.Empty;
                    if (errorElement?.Name == "Error")
                    {
                        var code = errorElement.Element("Code");
                        if (code != null)
                        {
                            errorMessage += $"Code: {code.Value} ";
                        }
                        var message = errorElement.Element("Message");
                        if (message != null)
                        {
                            errorMessage += $"Message: {message.Value} ";
                        }
                        var proposedSize = errorElement.Element("ProposedSize");
                        if (proposedSize != null)
                        {
                            errorMessage += $"Maximum allowed size is: {proposedSize.Value} ";
                        }
                    }
                    errorMessage = string.IsNullOrEmpty((errorMessage)) ? jsonString : errorMessage;
                    //TODO: might not always be applicable
                    var statusCode = asyncRequestState?.Response?.StatusCode ?? Constants.HttpRequestEntityTooLargeStatusCode;
                    status = new StatusBuilder(currentConfig, jsonLib).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, statusCode, new PNException(errorMessage));
                }
            }

            return status;
        }
        
        private static string ExtractValue(string input, string startTag, string endTag)
        {
            int startIndex = input.IndexOf(startTag) + startTag.Length;
            int endIndex = input.IndexOf(endTag, startIndex);
            if (startIndex < 0 || endIndex < 0)
            {
                return null;
            }
            return input.Substring(startIndex, endIndex - startIndex).Trim();
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
                            if (result.Count == 3 && result[0] is object[] && ((object[])result[0]).Length == 0 && result[2].ToString() == "")
                            {
                                result.RemoveAt(2);
                            }
                            if (result.Count == 4 && result[0] is object[] && ((object[])result[0]).Length == 0 && result[2].ToString() == "" && result[3].ToString() == "")
                            {
                                result.RemoveRange(2, 2);
                            }
                            result.Add(multiChannelGroup);
                            result.Add(multiChannel);

                            int receivedRegion = GetRegionFromMultiplexResult(result);
                            LastSubscribeRegion[PubnubInstance.InstanceId] = receivedRegion;

                            long receivedTimetoken = GetTimetokenFromMultiplexResult(result);
                            if (receivedTimetoken != 0)
                                LastSubscribeTimetoken[PubnubInstance.InstanceId] = receivedTimetoken;
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
                        case PNOperationType.PNFetchHistoryOperation:
                            if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig))
                            {
                                if (type == PNOperationType.PNFetchHistoryOperation)
                                {
                                    for (int index = 0; index < result.Count; index++)
                                    {
                                        Dictionary<string, object> messageContainer = jsonLib.ConvertToDictionaryObject(result[index]);
                                        if (messageContainer != null && messageContainer.Count > 0)
                                        {
                                            if (messageContainer.ContainsKey("channels"))
                                            {
                                                object channelMessageContainer = messageContainer["channels"];
                                                Dictionary<string, object> channelDic = jsonLib.ConvertToDictionaryObject(channelMessageContainer);
                                                if (channelDic != null && channelDic.Count > 0)
                                                {
                                                    result[index] = SecureMessage.Instance(currentConfig, jsonLib, currentConfig.Logger).FetchHistoryDecodeDecryptLoop(type, channelDic, channels, channelGroups, callback);
                                                }
                                            }
                                            else
                                            {
                                                result[index] = messageContainer;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    result = SecureMessage.Instance(currentConfig, jsonLib, currentConfig.Logger).HistoryDecodeDecryptLoop(type, result, channels, channelGroups, callback);
                                }
                            }
                            result.Add(multiChannel);
                            break;
                        case PNOperationType.PNMessageCountsOperation:
                            Dictionary<string, object> msgCountDictionary = jsonLib.DeserializeToDictionaryOfObject(jsonString);
                            result = new List<object>();
                            result.Add(msgCountDictionary);
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
                        case PNOperationType.PNAccessManagerGrantToken:
                        case PNOperationType.PNAccessManagerGrant:
                        case PNOperationType.PNAccessManagerRevokeToken:
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
                        case PNOperationType.PNSignalOperation:
                        case PNOperationType.PushRegister:
                        case PNOperationType.PushRemove:
                        case PNOperationType.PushGet:
                        case PNOperationType.PushUnregister:
                        case PNOperationType.Leave:
                        case PNOperationType.PNSetUuidMetadataOperation:
                        case PNOperationType.PNSetChannelMetadataOperation:
                        case PNOperationType.PNAddMessageActionOperation:
                        case PNOperationType.PNRemoveMessageActionOperation:
                        case PNOperationType.PNGetMessageActionsOperation:
                        case PNOperationType.PNGenerateFileUploadUrlOperation:
                        case PNOperationType.PNPublishFileMessageOperation:
                        case PNOperationType.PNListFilesOperation:
                        case PNOperationType.PNDeleteFileOperation:
                            result.Add(multiChannel);
                            break;
                        case PNOperationType.PNAddChannelsToGroupOperation:
                        case PNOperationType.PNRemoveChannelsFromGroupOperation:
                        case PNOperationType.PNRemoveGroupOperation:
                        case PNOperationType.ChannelGroupGet:
                        case PNOperationType.ChannelGroupAllGet:
                            Dictionary<string, object> channelGroupDictionary =
                                jsonLib.DeserializeToDictionaryOfObject(jsonString);
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
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return result;
        }

        protected void ProcessResponseCallbacks<T>(List<object> result, RequestState<T> asyncRequestState)
        {
            bool callbackAvailable = result != null && result.Count >= 1 && (asyncRequestState.PubnubCallback != null || SubscribeCallbackListenerList.Count >= 1);
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
                if (!string.IsNullOrEmpty(channel) && ChannelLocalUserState.ContainsKey(PubnubInstance.InstanceId) && ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    ChannelLocalUserState[PubnubInstance.InstanceId].TryGetValue(channel, out channelUserStateDictionary);
                }
                if (!string.IsNullOrEmpty(channelGroup) && ChannelLocalUserState.ContainsKey(PubnubInstance.InstanceId) && ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
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
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
                    }
                    else if (channelUserStateValue.GetType().ToString() == "System.Boolean")
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, channelUserStateValue.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, (channelUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelUserStateValue) : channelUserStateValue);
                    }
                    if (keyIndex < channelUserStateKeys.Length - 1)
                    {
                        jsonStateBuilder.Append(',');
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
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
                    }
                    else if (channelGroupUserStateValue.GetType().ToString() == "System.Boolean")
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, channelGroupUserStateValue.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, (channelGroupUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelGroupUserStateValue) : channelGroupUserStateValue);
                    }
                    if (keyIndex < channelGroupUserStateKeys.Length - 1)
                    {
                        jsonStateBuilder.Append(',');
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
                        currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channels[index], currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(',');
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
                        currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channelGroups[index], currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(',');
                        }
                        jsonStateBuilder.Append(currentJsonState);
                    }
                }
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", jsonStateBuilder);
            }

            return retJsonUserState;
        }

#region "Terminate requests and Timers"

        private void RemoveChannelDictionary()
        {
            RemoveChannelDictionary<object>(null);
        }

        private void RemoveChannelDictionary<T>(RequestState<T> state)
        {
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            if (state != null && state.RequestCancellationTokenSource != null)
            {
                string channel = (state.Channels != null) ? string.Join(",", state.Channels.OrderBy(x => x).ToArray()) : ",";

                if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    CancellationTokenSource removedRequest;
                    bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(channel, out removedRequest);
                    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        if (removeKey)
                        {
                            LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Remove web request from dictionary in RemoveChannelDictionary for channel= {channel}", currentConfig.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {channel}", currentConfig.LogVerbosity);
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
                        CancellationTokenSource currentRequest;
                        if (ChannelRequest[PubnubInstance.InstanceId].TryGetValue(key, out currentRequest) && currentRequest != null)
                        {
                            bool removeKey = ChannelRequest[PubnubInstance.InstanceId].TryRemove(key, out currentRequest);
                            if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                            {
                                if (removeKey)
                                {
                                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Remove web request from dictionary in RemoveChannelDictionary for channel= {key}", currentConfig.LogVerbosity);
                                }
                                else
                                {
                                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to remove web request from dictionary in RemoveChannelDictionary for channel= {key}", currentConfig.LogVerbosity);
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
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] RemoveUserState from local user state dictionary for channel= {key}", currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to RemoveUserState from local user state dictionary for channel= {key}", currentConfig.LogVerbosity);
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
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]RemoveUserState from user state dictionary for channel= {key}",currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to RemoveUserState from user state dictionary for channel= {key}", currentConfig.LogVerbosity);
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
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] RemoveUserState from local user state dictionary for channelGroup= {key}",  currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to RemoveUserState from local user state dictionary for channelGroup= {key}", currentConfig.LogVerbosity);
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
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] RemoveUserState from user state dictionary for channelgroup= {key}", currentConfig.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unable to RemoveUserState from user state dictionary for channelgroup= {key}", currentConfig.LogVerbosity);
                            }
                        }
                    }
                }
            }
        }

        protected static void TerminatePresenceHeartbeatTimer()
        {
            if (PresenceHeartbeatTimer != null)
            {
                PresenceHeartbeatTimer.Dispose();
                PresenceHeartbeatTimer = null;
            }
        }

        protected void TerminateTokenManagerCollection()
        {
            if (PubnubTokenMgrCollection != null && PubnubTokenMgrCollection.Count > 0)
            {
                List<string> tokenMgrList = PubnubTokenMgrCollection.Keys.ToList();
                foreach(string key in tokenMgrList)
                {
                    if (PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId))
                    {
                        PubnubTokenMgrCollection[PubnubInstance.InstanceId].Destroy();
                    }
                }
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
                            LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] EXPONENTIAL timerInterval > MAXEXPONENTIALBACKOFF", currentConfig.LogVerbosity);
                        }
                    }
                    else if (timerInterval < 1)
                    {
                        timerInterval = MINEXPONENTIALBACKOFF;
                    }
                    if (pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    {
                        LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] EXPONENTIAL timerInterval = {timerInterval.ToString(CultureInfo.InvariantCulture)}", currentConfig.LogVerbosity);
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
                                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] TerminateReconnectTimer(null) - Unable to remove channel reconnect timer reference from collection for {key}", currentConfig.LogVerbosity);
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
                                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] TerminateReconnectTimer(null) - Unable to remove channelGroup reconnect timer reference from collection for {groupKey}", currentConfig.LogVerbosity);
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
                    LoggingMethod.WriteToLog(currentLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: TerminateReconnectTimer exception: {ex}", currentConfig.LogVerbosity);
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
            if (SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                SubscribeCallbackListenerList[PubnubInstance.InstanceId].Clear();
            }

            RemoveChannelDictionary();
            TerminateReconnectTimer();
            RemoveUserState();
			TerminatePresenceHeartbeatTimer();
            TerminateDedupeManager();
            TerminateTokenManagerCollection();

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
            
            PubnubInstance = null;
        }
        
        internal void TerminateCurrentSubscriberRequest()
        {
            if (OngoingSubscriptionCancellationTokenSources.ContainsKey(PubnubInstance.InstanceId) 
                && OngoingSubscriptionCancellationTokenSources[PubnubInstance.InstanceId] is not null)
            {
                var cts = OngoingSubscriptionCancellationTokenSources[PubnubInstance.InstanceId];
                try {
                    cts.Cancel();
                    cts.Dispose();
                    OngoingSubscriptionCancellationTokenSources[PubnubInstance.InstanceId] = null;
                }
                catch {
                    // ignored
                }
            }
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

        internal void Announce<T>(PNSignalResult<T> message)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].Signal(PubnubInstance, message);
                }
            }
        }

        internal void Announce(PNFileEventResult message)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].File(PubnubInstance, message);
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

        internal void Announce(PNObjectEventResult objectApiEvent)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].ObjectEvent(PubnubInstance, objectApiEvent);
                }
            }
        }

        internal void Announce(PNMessageActionEventResult messageActionEvent)
        {
            if (PubnubInstance != null && SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
            {
                List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
                {
                    callbackList[listenerIndex].MessageAction(PubnubInstance, messageActionEvent);
                }
            }
        }

        internal void InitializeDefaultVariableObjectStates()
        {
            if (!ChannelRequest.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelRequest.GetOrAdd(PubnubInstance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
            }
            if (!ChannelInternetStatus.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(PubnubInstance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(PubnubInstance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(PubnubInstance.InstanceId, new ConcurrentDictionary<string, bool>());
            }

        }
    }
}
