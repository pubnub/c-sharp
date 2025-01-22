using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using Newtonsoft.Json.Linq;

namespace PubnubApi.EventEngine.Common
{
    public class EventEmitter
    {
        private PNConfiguration configuration;
        private List<SubscribeCallback> listeners;
        private IPubnubLog log;
        private IJsonPluggableLibrary jsonLibrary;
        private Pubnub instance;
        private TokenManager tokenManager;
        private Dictionary<string, List<SubscribeCallback>> channelListenersMap;
        private Dictionary<string, List<SubscribeCallback>> channelGroupListenersMap;

        public EventEmitter(PNConfiguration configuration, List<SubscribeCallback> listenerCallbacks, IJsonPluggableLibrary jsonPluggableLibrary, TokenManager tokenManager, IPubnubLog log, Pubnub instance)
        {
            this.configuration = configuration;
            this.log = log;
            this.instance = instance;
            this.tokenManager = tokenManager;
            jsonLibrary = jsonPluggableLibrary;
            listeners = listenerCallbacks;
            channelGroupListenersMap = new Dictionary<string, List<SubscribeCallback>>();
            channelListenersMap = new Dictionary<string, List<SubscribeCallback>>();
        }

        private TimetokenMetadata GetTimetokenMetadata(object t)
        {
            Dictionary<string, object> ttOriginMetaData = jsonLibrary.ConvertToDictionaryObject(t);
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

                return ttMeta;
            }

            return null;
        }

        public void AddListener(SubscribeCallback listener, string[] channels, string[] groups)
        {
            foreach (var c in channels.Where(c => !c.EndsWith("-pnpres")))
            {
                if (channelListenersMap.ContainsKey(c))
                {
                    channelListenersMap[c].Add(listener);
                }
                else
                {
                    channelListenersMap[c] = new List<SubscribeCallback> { listener };
                }
            }

            foreach (var cg in groups.Where(cg => !cg.EndsWith("-pnpres")))
            {
                if (channelGroupListenersMap.ContainsKey(cg))
                {
                    channelGroupListenersMap[cg].Add(listener);
                }
                else
                {
                    channelGroupListenersMap[cg] = new List<SubscribeCallback> { listener };
                }
            }
        }

        public void RemoveListener(SubscribeCallback listener, string[] channels, string[] groups)
        {
            foreach (var c in channels.Where(c => !c.EndsWith("-pnpres")))
            {
                if (channelListenersMap.ContainsKey(c))
                {
                    channelListenersMap[c].Remove(listener);
                }
            }

            foreach (var cg in groups.Where(cg => !cg.EndsWith("-pnpres")))
            {
                if (channelGroupListenersMap.ContainsKey(cg))
                {
                    channelGroupListenersMap[cg].Remove(listener);
                }
            }
        }

        public void EmitEvent<T>(object e)
        {
            var jsonFields = new Dictionary<string, object>();
            var eventData = e as Message<object>;

            string currentMessageChannel = eventData?.Channel;
            jsonFields.Add("channel", currentMessageChannel);

            string currentMessageChannelGroup = eventData?.SubscriptionMatch;
            jsonFields.Add("channelGroup", currentMessageChannelGroup);

            if (currentMessageChannel?.Replace("-pnpres", "") == currentMessageChannelGroup?.Replace("-pnpres", ""))
            {
                currentMessageChannelGroup = "";
                jsonFields["channelGroup"] = currentMessageChannelGroup;
            }

            object payload;
            string payloadAsString = eventData?.Payload as string;
            if (payloadAsString != null)
            {
                var jsonObject = jsonLibrary.BuildJsonObject(payloadAsString.ToString());
                payload = jsonObject ?? payloadAsString;
            }
            else
            {
                payload = eventData?.Payload;
            }

            List<object> payloadContainer = new List<object>(); //First item always message
            if (currentMessageChannel.Contains("-pnpres") || currentMessageChannel.Contains(".*-pnpres"))
            {
                payloadContainer.Add(payload);
                jsonFields.Add("payload", payload);
            }
            else if (eventData.MessageType == 2) //Objects Simplification events
            {
                Dictionary<string, object> appContextEventFields = payload as Dictionary<string, object> ?? (payload as JObject)?.ToObject<Dictionary<string, object>>();
                if (appContextEventFields != null
                    && appContextEventFields.ContainsKey("source")
                    && appContextEventFields.ContainsKey("version")
                    && appContextEventFields["source"].ToString() == "objects"
                    && Double.TryParse(appContextEventFields["version"]?.ToString()?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var objectsVersion))
                {
                    if (objectsVersion.CompareTo(2D) == 0) //Process only version=2 for Objects Simplification. Ignore 1. 
                    {
                        payloadContainer.Add(payload);
                        jsonFields.Add("payload", payload);
                    }
                }
            }
            else
            {
                if ((configuration.CryptoModule != null || configuration.CipherKey.Length > 0) && (eventData.MessageType == 0 || eventData.MessageType == 4)) //decrypt the subscriber message if cipherkey is available
                {
                    string decryptMessage = "";
                    configuration.CryptoModule ??= new CryptoModule(new AesCbcCryptor(configuration.CipherKey, log), new List<ICryptor>() { new LegacyCryptor(configuration.CipherKey, configuration.UseRandomInitializationVector) });
                    try
                    {
                        decryptMessage = configuration.CryptoModule.Decrypt(payload.ToString());
                    }
                    catch (Exception ex)
                    {
                        decryptMessage = "**DECRYPT ERROR**";
                        LoggingMethod.WriteToLog(log, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Failed to decrypt message on channel {currentMessageChannel} due to exception={ex}.\n Message might be not encrypted", configuration.LogVerbosity);
                    }

                    object decodeMessage = jsonLibrary.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLibrary.SerializeToJsonString(payload) : decryptMessage);
                    payloadContainer.Add(decodeMessage);
                    jsonFields.Add("payload", decodeMessage);
                }
                else
                {
                    string payloadJson = jsonLibrary.SerializeToJsonString(payload);
                    object payloadJObject = jsonLibrary.BuildJsonObject(payloadJson);
                    if (payloadJObject == null)
                    {
                        payloadContainer.Add(payload);
                        jsonFields.Add("payload", payload);
                    }
                    else
                    {
                        payloadContainer.Add(payloadJObject);
                        jsonFields.Add("payload", payloadJObject);
                    }
                }
            }

            var userMetaData = eventData.UserMetadata;
            payloadContainer.Add(userMetaData); //Second one always user meta data
            jsonFields.Add("userMetadata", userMetaData);
            payloadContainer.Add(GetTimetokenMetadata(eventData.PublishMetadata).Timetoken); //Third one always Timetoken - 2
            jsonFields.Add("publishTimetoken", GetTimetokenMetadata(eventData.PublishMetadata).Timetoken);
            payloadContainer.Add(eventData.IssuingClientId); //Fourth one always Publisher
            jsonFields.Add("userId", eventData.IssuingClientId); // - 3

            payloadContainer.Add(currentMessageChannelGroup);
            payloadContainer.Add(currentMessageChannel);

            switch (eventData.MessageType)
            {
                case 1:
                {
                    payloadContainer.Add(eventData.CustomMessageType);
                    jsonFields.Add("customMessageType", eventData.CustomMessageType);
                    ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
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
                            Publisher = pnMessageResult.Publisher
                        };
                        foreach (var listener in listeners)
                        {
                            listener?.Signal(instance, signalMessage);
                        }

                        if (!string.IsNullOrEmpty(signalMessage.Channel) && channelListenersMap.ContainsKey(signalMessage.Channel))
                        {
                            foreach (var l in channelListenersMap[signalMessage.Channel])
                            {
                                l?.Signal(instance, signalMessage);
                            }
                        }

                        if (!string.IsNullOrEmpty(signalMessage.Subscription) && channelGroupListenersMap.ContainsKey(signalMessage.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[signalMessage.Subscription])
                            {
                                l?.Signal(instance, signalMessage);
                            }
                        }
                    }

                    break;
                }
                case 2:
                {
                    ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
                    PNObjectEventResult objectApiEvent = responseBuilder.GetEventResultObject<PNObjectEventResult>(jsonFields);
                    if (objectApiEvent != null)
                    {
                        foreach (var listener in listeners)
                        {
                            listener?.ObjectEvent(instance, objectApiEvent);
                        }

                        if (!string.IsNullOrEmpty(objectApiEvent.Channel) && channelListenersMap.ContainsKey(objectApiEvent.Channel))
                        {
                            foreach (var l in channelListenersMap[objectApiEvent.Channel])
                            {
                                l?.ObjectEvent(instance, objectApiEvent);
                            }
                        }

                        if (!string.IsNullOrEmpty(objectApiEvent.Subscription) && channelGroupListenersMap.ContainsKey(objectApiEvent.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[objectApiEvent.Subscription])
                            {
                                l?.ObjectEvent(instance, objectApiEvent);
                            }
                        }
                    }

                    break;
                }
                case 3:
                {
                    ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
                    PNMessageActionEventResult messageActionEvent = responseBuilder.GetEventResultObject<PNMessageActionEventResult>(jsonFields);
                    if (messageActionEvent != null)
                    {
                        foreach (var listener in listeners)
                        {
                            listener?.MessageAction(instance, messageActionEvent);
                        }

                        if (!string.IsNullOrEmpty(messageActionEvent.Channel) && channelListenersMap.ContainsKey(messageActionEvent.Channel))
                        {
                            foreach (var l in channelListenersMap[messageActionEvent.Channel])
                            {
                                l?.MessageAction(instance, messageActionEvent);
                            }
                        }

                        if (!string.IsNullOrEmpty(messageActionEvent.Subscription) && channelGroupListenersMap.ContainsKey(messageActionEvent.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[messageActionEvent.Subscription])
                            {
                                l?.MessageAction(instance, messageActionEvent);
                            }
                        }
                    }

                    break;
                }
                case 4:
                {
                    payloadContainer.Add(eventData.CustomMessageType);
                    ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
                    PNMessageResult<object> filesEvent = responseBuilder.GetEventResultObject<PNMessageResult<object>>(jsonFields);
                    if (filesEvent != null)
                    {
                        PNFileEventResult fileMessage = new PNFileEventResult
                        {
                            Channel = filesEvent.Channel,
                            Subscription = filesEvent.Subscription,
                            Timetoken = filesEvent.Timetoken,
                            Publisher = filesEvent.Publisher,
                            CustomMessageType = filesEvent.CustomMessageType,
                        };
                        Dictionary<string, object> fileEventMessageField = jsonLibrary.ConvertToDictionaryObject(filesEvent.Message);
                        if (fileEventMessageField is { Count: > 0 })
                        {
                            if (fileEventMessageField.ContainsKey("message") && fileEventMessageField["message"] != null)
                            {
                                fileMessage.Message = fileEventMessageField["message"];
                            }

                            if (fileEventMessageField.TryGetValue("file", out var fileField))
                            {
                                Dictionary<string, object> fileDetailFields = jsonLibrary.ConvertToDictionaryObject(fileField);
                                if (fileDetailFields != null && fileDetailFields.ContainsKey("id") && fileDetailFields.ContainsKey("name"))
                                {
                                    fileMessage.File = new PNFile { Id = fileDetailFields["id"].ToString(), Name = fileDetailFields["name"].ToString() };
                                    fileMessage.File.Url = UriUtil.GetFileUrl(fileName: fileMessage.File.Name, fileId: fileMessage.File.Id, channel: fileMessage.Channel,
                                        pnConfiguration: configuration, pubnub: instance, tokenmanager: tokenManager);
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

                        foreach (var listener in listeners)
                        {
                            listener?.File(instance, fileMessage);
                        }

                        if (!string.IsNullOrEmpty(fileMessage.Channel) && channelListenersMap.ContainsKey(fileMessage.Channel))
                        {
                            foreach (var l in channelListenersMap[fileMessage.Channel])
                            {
                                l?.File(instance, fileMessage);
                            }
                        }

                        if (!string.IsNullOrEmpty(fileMessage.Subscription) && channelGroupListenersMap.ContainsKey(fileMessage.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[fileMessage.Subscription])
                            {
                                l?.File(instance, fileMessage);
                            }
                        }
                    }

                    break;
                }
                default:
                {
                    if (currentMessageChannel.Contains("-pnpres"))
                    {
                        ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
                        PNPresenceEventResult presenceEvent = responseBuilder.GetEventResultObject<PNPresenceEventResult>(jsonFields);
                        if (presenceEvent != null)
                        {
                            foreach (var listener in listeners)
                            {
                                listener?.Presence(instance, presenceEvent);
                            }

                            if (!string.IsNullOrEmpty(presenceEvent.Channel) && channelListenersMap.ContainsKey(presenceEvent.Channel))
                            {
                                foreach (var l in channelListenersMap[presenceEvent.Channel])
                                {
                                    l?.Presence(instance, presenceEvent);
                                }
                            }

                            if (!string.IsNullOrEmpty(presenceEvent.Subscription) && channelGroupListenersMap.ContainsKey(presenceEvent.Subscription))
                            {
                                foreach (var l in channelGroupListenersMap[presenceEvent.Subscription])
                                {
                                    l?.Presence(instance, presenceEvent);
                                }
                            }
                        }
                    }
                    else
                    {
                        payloadContainer.Add(eventData.CustomMessageType);
                        ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
                        PNMessageResult<T> userMessage = responseBuilder.GetEventResultObject<PNMessageResult<T>>(jsonFields);
                        try
                        {
                            if (userMessage != null)
                            {
                                foreach (var listener in listeners)
                                {
                                    listener?.Message(instance, userMessage);
                                }

                                if (!string.IsNullOrEmpty(userMessage.Channel) && channelListenersMap.ContainsKey(userMessage.Channel))
                                {
                                    foreach (var l in channelListenersMap[userMessage.Channel])
                                    {
                                        l?.Message(instance, userMessage);
                                    }
                                }

                                if (!string.IsNullOrEmpty(userMessage.Subscription) && channelGroupListenersMap.ContainsKey(userMessage.Subscription))
                                {
                                    foreach (var l in channelGroupListenersMap[userMessage.Subscription])
                                    {
                                        l?.Message(instance, userMessage);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingMethod.WriteToLog(this.log, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: Listener call back execution encounters error: {ex.Message}\n{ex?.StackTrace}", configuration.LogVerbosity);
                        }
                    }

                    break;
                }
            }
        }
    }
}