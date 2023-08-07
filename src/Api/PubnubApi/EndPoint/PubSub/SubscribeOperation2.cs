using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using PubnubApi.PubnubEventEngine;
using PubnubApi.EventEngine.Subscribe;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi;

namespace PubnubApi.EndPoint
{
    public class SubscribeOperation2<T>: ISubscribeOperation<T>
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private List<string> subscribeChannelNames = new List<string>();
        private List<string> subscribeChannelGroupNames = new List<string>();
        private long subscribeTimetoken = -1;
        private bool presenceSubscribeEnabled;
        private SubscribeManager2 manager;
        private Dictionary<string, object> queryParam;
        private Pubnub PubnubInstance;
        private SubscribeEventEngine subscribeEventEngine;
        public SubscribeEventEngineFactory subscribeEventEngineFactory;
        public string instanceId;
		public List<SubscribeCallback> SubscribeListenerList
        {
            get;
            set;
        } = new List<SubscribeCallback>();

        public SubscribeOperation2(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager,SubscribeEventEngineFactory subscribeEventEngineFactory, string instanceId, Pubnub instance) 
        {
            PubnubInstance = instance;
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;
            this.subscribeEventEngineFactory = subscribeEventEngineFactory;
            this.instanceId = instanceId;
            
			var eventEmitter = new EventEmitter();
            eventEmitter.RegisterJsonListener(JsonCallback);

       //     pnEventEngine = new EventEngine(effectDispatcher, eventEmitter);
       //     pnEventEngine.PubnubUnitTest = unit;
       //     pnEventEngine.Setup<T>(config);

       //     if (pnEventEngine.PubnubUnitTest != null)
       //     {
       //         pnEventEngine.PubnubUnitTest.EventTypeList = new List<KeyValuePair<string, string>>();
       //     }
       //     else
       //     {
			    //pnEventEngine.InitialState(new State(StateType.Unsubscribed) { EventType = EventType.SubscriptionChanged });
       //     }
        }

        private void HandshakeEffect_CancelHandshakeRequested(object sender, CancelHandshakeRequestEventArgs e)
        {
            manager.HandshakeRequestCancellation();
        }
        private void HandshakeReconnectEffect_CancelHandshakeRequested(object sender, CancelHandshakeReconnectRequestEventArgs e)
        {
            manager.HandshakeRequestCancellation();
        }
        private void ReceivingEffect_CancelReceiveRequested(object sender, CancelReceiveRequestEventArgs e)
        {
            manager.ReceiveRequestCancellation();
        }
        private void ReceiveReconnectEffect_CancelReceiveRequested(object sender, CancelReceiveReconnectRequestEventArgs e)
        {
            manager.ReceiveReconnectRequestCancellation();
        }

        private void JsonCallback(string json, bool zeroTimeTokenRequest, int messageCount)
        {
        }

        protected void ProcessListenerCallback<T>(List<object> result, bool zeroTimeTokenRequest, int messageCount, string[] channels, string[] channelGroups)
        {
            bool callbackAvailable = false;
            if (result != null && result.Count >= 1 && SubscribeListenerList.Count >= 1)
            {
                callbackAvailable = true;
            }
            if (callbackAvailable)
            {
                if (zeroTimeTokenRequest)
                {
                    ResponseToConnectCallback<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups);
                }
                else if (messageCount > 0)
                {
                    ResponseToUserCallback<T>(result, PNOperationType.PNSubscribeOperation);
                }
            }
        }

        private void ResponseToConnectCallback<T>(PNOperationType type, string[] channels, string[] channelGroups)
        {
            StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
            PNStatus status = statusBuilder.CreateStatusResponse<T>(type, PNStatusCategory.PNConnectedCategory, null, (int)HttpStatusCode.OK, null);

            Announce(status);
        }

        internal void Announce(PNStatus status)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].Status(PubnubInstance, status);
            }
        }

        internal void Announce<T>(PNMessageResult<T> message)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].Message(PubnubInstance, message);
            }
        }

        internal void Announce<T>(PNSignalResult<T> message)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].Signal(PubnubInstance, message);
            }
        }

        internal void Announce(PNFileEventResult message)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].File(PubnubInstance, message);
            }
        }

        internal void Announce(PNPresenceEventResult presence)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].Presence(PubnubInstance, presence);
            }
        }

        internal void Announce(PNObjectEventResult objectApiEvent)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].ObjectEvent(PubnubInstance, objectApiEvent);
            }
        }

        internal void Announce(PNMessageActionEventResult messageActionEvent)
        {
            List<SubscribeCallback> callbackList = SubscribeListenerList;
            for (int listenerIndex = 0; listenerIndex < callbackList.Count; listenerIndex++)
            {
                callbackList[listenerIndex].MessageAction(PubnubInstance, messageActionEvent);
            }
        }

        private void ResponseToUserCallback<T>(List<object> result, PNOperationType type)
        {
            IPubnubLog currentLog = null;
            try
            {
                switch (type)
                {
                    case PNOperationType.PNSubscribeOperation:
                    case PNOperationType.Presence:
                        List<SubscribeMessage> messageList = GetMessageFromMultiplexResult(result);
                        if (messageList != null && messageList.Count > 0)
                        {
                            if (messageList.Count >= config.RequestMessageCountThreshold)
                            {
                                StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                                PNStatus status = statusBuilder.CreateStatusResponse<T>(type, PNStatusCategory.PNRequestMessageCountExceededCategory, null, (int)HttpStatusCode.OK, null);
                                Announce(status);
                            }

                            if (config != null && currentLog != null)
                            {
                                LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - messageList.Count = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), messageList.Count), config.LogVerbosity);
                            }
                            for (int messageIndex = 0; messageIndex < messageList.Count; messageIndex++)
                            {
                                SubscribeMessage currentMessage = messageList[messageIndex];
                                if (currentMessage != null)
                                {
                                    if (config != null && currentLog != null && config.DedupOnSubscribe && IsTargetForDedup(currentMessage))
                                    {
                                        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - messageList for loop - messageIndex = {1} => IsTargetForDedup", DateTime.Now.ToString(CultureInfo.InvariantCulture), messageIndex), config.LogVerbosity);
                                        continue;
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
                                    else if (currentMessage.MessageType == 2) //Objects Simplification events
                                    {
                                        double objectsVersion = -1;
                                        Dictionary<string, object> objectsDic = payload as Dictionary<string, object>;
                                        if (objectsDic != null
                                            && objectsDic.ContainsKey("source") && objectsDic.ContainsKey("version")
                                            && objectsDic["source"].ToString() == "objects" && Double.TryParse(objectsDic["version"].ToString(), out objectsVersion))
                                        {
                                            if (objectsVersion.CompareTo(2D) == 0) //Process only version=2 for Objects Simplification. Ignore 1. 
                                            {
                                                payloadContainer.Add(payload);
                                            }
                                            else
                                            {
                                                LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - Legacy Objects V1. Ignoring this.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - MessageType =2 but NOT valid format to process", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (config.CipherKey.Length > 0 && currentMessage.MessageType != 1) //decrypt the subscriber message if cipherkey is available
                                        {
                                            string decryptMessage = "";
                                            PubnubCrypto aes = new PubnubCrypto(config.CipherKey, config, currentLog, null);
                                            try
                                            {
                                                decryptMessage = aes.Decrypt(payload.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                decryptMessage = "**DECRYPT ERROR**";

                                                PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                                                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, null, (int)HttpStatusCode.NotFound, new PNException(ex));
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
                                            object decodeMessage = (decryptMessage == "**DECRYPT ERROR**") ? decryptMessage : jsonLibrary.DeserializeToObject(decryptMessage);

                                            payloadContainer.Add(decodeMessage);
                                        }
                                        else
                                        {
                                            string payloadJson = jsonLibrary.SerializeToJsonString(payload);
                                            object payloadJObject = jsonLibrary.BuildJsonObject(payloadJson);
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

                                    if (currentMessage.MessageType == 1)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNMessageResult<T> pnMessageResult = responseBuilder.JsonToObject<PNMessageResult<T>>(payloadContainer, true);
                                        if (pnMessageResult != null)
                                        {
                                            PNSignalResult<T> signalMessage = new PNSignalResult<T>
                                            {
                                                Channel = pnMessageResult.Channel,
                                                Message = pnMessageResult.Message,
                                                Subscription = pnMessageResult.Subscription,
                                                Timetoken = pnMessageResult.Timetoken,
                                                UserMetadata = pnMessageResult.UserMetadata,
                                                Publisher = pnMessageResult.Publisher
                                            };
                                            Announce(signalMessage);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 2)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNObjectEventResult objectApiEvent = responseBuilder.JsonToObject<PNObjectEventResult>(payloadContainer, true);
                                        if (objectApiEvent != null)
                                        {
                                            Announce(objectApiEvent);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 3)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNMessageActionEventResult msgActionEventEvent = responseBuilder.JsonToObject<PNMessageActionEventResult>(payloadContainer, true);
                                        if (msgActionEventEvent != null)
                                        {
                                            Announce(msgActionEventEvent);
                                        }
                                    }
                                    else if (currentMessage.MessageType == 4)
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNMessageResult<object> pnFileResult = responseBuilder.JsonToObject<PNMessageResult<object>>(payloadContainer, true);
                                        if (pnFileResult != null)
                                        {
                                            PNFileEventResult fileMessage = new PNFileEventResult
                                            {
                                                Channel = pnFileResult.Channel,
                                                Subscription = pnFileResult.Subscription,
                                                Timetoken = pnFileResult.Timetoken,
                                                Publisher = pnFileResult.Publisher,
                                            };
                                            Dictionary<string, object> pnMsgObjDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(pnFileResult.Message);
                                            if (pnMsgObjDic != null && pnMsgObjDic.Count > 0)
                                            {
                                                if (pnMsgObjDic.ContainsKey("message") && pnMsgObjDic["message"] != null)
                                                {
                                                    fileMessage.Message = pnMsgObjDic["message"];
                                                }
                                                if (pnMsgObjDic.ContainsKey("file"))
                                                {
                                                    Dictionary<string, object> fileObjDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(pnMsgObjDic["file"]);
                                                    if (fileObjDic != null && fileObjDic.ContainsKey("id") && fileObjDic.ContainsKey("name"))
                                                    {
                                                        fileMessage.File = new PNFile { Id = fileObjDic["id"].ToString(), Name = fileObjDic["name"].ToString() };
                                                        IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, currentLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance.InstanceId);
                                                        Uri fileUrlRequest = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", fileMessage.Channel, fileMessage.File.Id, fileMessage.File.Name, null, type);
                                                        fileMessage.File.Url = fileUrlRequest.ToString();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (pnFileResult.Message != null)
                                                {
                                                    fileMessage.Message = pnFileResult.Message;
                                                }
                                            }
                                            Announce(fileMessage);
                                        }
                                    }
                                    else if (currentMessageChannel.Contains("-pnpres"))
                                    {
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNPresenceEventResult presenceEvent = responseBuilder.JsonToObject<PNPresenceEventResult>(payloadContainer, true);
                                        if (presenceEvent != null)
                                        {
                                            Announce(presenceEvent);
                                        }
                                    }
                                    else
                                    {
                                        if (config != null && currentLog != null)
                                        {
                                            LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - payload = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLibrary.SerializeToJsonString(payloadContainer)), config.LogVerbosity);
                                        }
                                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, currentLog);
                                        PNMessageResult<T> userMessage = responseBuilder.JsonToObject<PNMessageResult<T>>(payloadContainer, true);
                                        if (userMessage != null)
                                        {
                                            Announce(userMessage);
                                        }
                                    }

                                }
                                else
                                {
                                    if (config != null && currentLog != null)
                                    {
                                        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - messageList for loop - messageIndex = {1} => null message", DateTime.Now.ToString(CultureInfo.InvariantCulture), messageIndex), config.LogVerbosity);
                                    }
                                }
                            }

                        }
                        break;
                    case PNOperationType.PNHeartbeatOperation:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (config != null && currentLog != null)
                {
                    LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, ResponseToUserCallback - Exception = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
                }
            }
        }

        private bool IsTargetForDedup(SubscribeMessage message)
        {
            bool isTargetOfDedup = false;
            PNConfiguration currentConfig;
            IPubnubLog currentLog;
            try
            {
                //if (pubnubSubscribeDuplicationManager.IsDuplicate(message))
                //{
                //    isTargetOfDedup = true;
                //    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //    {
                //        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Dedupe - Duplicate skipped - msg = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLib.SerializeToJsonString(message)), currentConfig.LogVerbosity);
                //    }
                //}
                //else
                //{
                //    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //    {
                //        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Dedupe - AddEntry - msg = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonLib.SerializeToJsonString(message)), currentConfig.LogVerbosity);
                //    }
                //    pubnubSubscribeDuplicationManager.AddEntry(message);
                //}
            }
            catch (Exception ex)
            {
                //Log and ignore any exception due to Dedupe manager
                //if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //{
                //    LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, IsTargetForDedup - dedupe error = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), currentConfig.LogVerbosity);
                //}
            }

            return isTargetOfDedup;
        }

        private List<SubscribeMessage> GetMessageFromMultiplexResult(List<object> result)
        {
            List<object> jsonMessageList = null;
            List<SubscribeMessage> msgList = new List<SubscribeMessage>();

            Dictionary<string, object> messageDicObj = jsonLibrary.ConvertToDictionaryObject(result[1]);
            if (messageDicObj != null && messageDicObj.Count > 0 && messageDicObj.ContainsKey("m"))
            {
                jsonMessageList = messageDicObj["m"] as List<object>;
            }
            else
            {
                messageDicObj = jsonLibrary.ConvertToDictionaryObject(result[0]);
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
                                    Dictionary<string, object> ttOriginMetaData = jsonLibrary.ConvertToDictionaryObject(dicItem[key]);
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
                                    Dictionary<string, object> ttPublishMetaData = jsonLibrary.ConvertToDictionaryObject(dicItem[key]);
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

        private void LogCallback(string log)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), log), config.LogVerbosity);
        }

        public ISubscribeOperation<T> Channels(string[] channels)
        {
            if (channels != null && channels.Length > 0 && !string.IsNullOrEmpty(channels[0]))
            {
                this.subscribeChannelNames.AddRange(channels);
            }
            return this;
        }

        public ISubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            if (channelGroups != null && channelGroups.Length > 0 && !string.IsNullOrEmpty(channelGroups[0]))
            {
                this.subscribeChannelGroupNames.AddRange(channelGroups);
            }
            return this;
        }

        public ISubscribeOperation<T> WithTimetoken(long timetoken)
        {
            this.subscribeTimetoken = timetoken;
            return this;
        }

        public ISubscribeOperation<T> WithPresence()
        {
            this.presenceSubscribeEnabled = true;
            return this;
        }

        public ISubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute()
        {
            if (this.subscribeChannelNames == null)
            {
                this.subscribeChannelNames = new List<string>();
            }

            if (this.subscribeChannelGroupNames == null)
            {
                this.subscribeChannelGroupNames = new List<string>();
            }

            if (this.presenceSubscribeEnabled)
            {
                List<string> presenceChannelNames = (this.subscribeChannelNames != null && this.subscribeChannelNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelNames[0])) 
                                                ? this.subscribeChannelNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();
                List<string> presenceChannelGroupNames = (this.subscribeChannelGroupNames != null && this.subscribeChannelGroupNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelGroupNames[0])) 
                                                ? this.subscribeChannelGroupNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();

                if (this.subscribeChannelNames != null && presenceChannelNames.Count > 0)
                {
                    this.subscribeChannelNames.AddRange(presenceChannelNames);
                }

                if (this.subscribeChannelGroupNames != null && presenceChannelGroupNames.Count > 0)
                {
                    this.subscribeChannelGroupNames.AddRange(presenceChannelGroupNames);
                }
            }

            string[] channelNames = this.subscribeChannelNames != null ? this.subscribeChannelNames.ToArray() : null;
            string[] channelGroupNames = this.subscribeChannelGroupNames != null ? this.subscribeChannelGroupNames.ToArray() : null;

            Subscribe(channelNames, channelGroupNames, this.queryParam);
        }

        private void Subscribe(string[] channels, string[] channelGroups, Dictionary<string, object> externalQueryParam)
        {
			Action<Pubnub, PNStatus> statusListener = null;
			Action<Pubnub, PNMessageResult<T>> messageListener = null;
			if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}

			if (this.subscribeEventEngineFactory.hasEventEngine(instanceId))
            {
                subscribeEventEngine = subscribeEventEngineFactory.getEventEngine(instanceId);
			}
            else
            {
				if (SubscribeListenerList != null && SubscribeListenerList.Count > 0) {
					messageListener = MessageEmitter;
					statusListener = StatusEmitter;
				}
				var subscribeManager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
				subscribeEventEngine = subscribeEventEngineFactory.initializeEventEngine(instanceId, PubnubInstance, config, subscribeManager, statusListener, messageListener);

			}
			subscribeEventEngine.EventQueue.Enqueue(new SubscriptionChangedEvent() { Channels = channels, ChannelGroups = channelGroups });
		}

        internal bool Retry(bool reconnect)
        {
            return false;
            //if (manager == null)
            //{
            //    return false;
            //}

            //if (reconnect)
            //{
            //    return manager.Reconnect<T>(false);
            //}
            //else
            //{
            //    return manager.Disconnect();
            //}
        }

        internal bool Retry(bool reconnect, bool resetSubscribeTimetoken)
        {
            return false;
            //if (manager == null)
            //{
            //    return false;
            //}

            //if (reconnect)
            //{
            //    return manager.Reconnect<T>(resetSubscribeTimetoken);
            //}
            //else
            //{
            //    return manager.Disconnect();
            //}
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }
		private void MessageEmitter<T>(Pubnub pubnubInstance, PNMessageResult<T> messageResult)
		{
			foreach (var listener in SubscribeListenerList) {
				listener.Message(pubnubInstance, messageResult);
			}
		}

		private void StatusEmitter(Pubnub pubnubInstance, PNStatus status)
		{
            for(int i=0; i < SubscribeListenerList.Count; i++)
            {
                SubscribeCallback listener = SubscribeListenerList[i];
                if (listener != null)
                {
                    listener.Status(pubnubInstance, status);
                }
            }
		}

	}
}
