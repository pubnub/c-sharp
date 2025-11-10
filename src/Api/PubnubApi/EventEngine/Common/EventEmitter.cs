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
        private IJsonPluggableLibrary jsonLibrary;
        private Pubnub instance;
        private TokenManager tokenManager;
        private Dictionary<string, List<SubscribeCallback>> channelListenersMap;
        private Dictionary<string, List<SubscribeCallback>> channelGroupListenersMap;

        public EventEmitter(PNConfiguration configuration, List<SubscribeCallback> listenerCallbacks, IJsonPluggableLibrary jsonPluggableLibrary, TokenManager tokenManager,Pubnub instance)
        {
            this.configuration = configuration;
            this.instance = instance;
            this.tokenManager = tokenManager;
            jsonLibrary = jsonPluggableLibrary;
            listeners = listenerCallbacks;
            channelGroupListenersMap = new Dictionary<string, List<SubscribeCallback>>();
            channelListenersMap = new Dictionary<string, List<SubscribeCallback>>();
        }

        /// <summary>
        /// Safely invokes a listener callback with exception handling to prevent listener exceptions from affecting SDK operations.
        /// </summary>
        /// <param name="listenerAction">The listener callback action to invoke.</param>
        /// <param name="eventType">The type of event being processed (for logging purposes).</param>
        /// <remarks>
        /// This method ensures that exceptions thrown by user-provided listener callbacks do not crash the SDK
        /// or prevent other listeners from receiving events. All exceptions are logged at Warning level.
        /// </remarks>
        private void SafeInvokeListener(Action listenerAction, string eventType)
        {
            if (listenerAction == null)
            {
                return;
            }

            try
            {
                listenerAction.Invoke();
            }
            catch (Exception ex)
            {
                configuration?.Logger?.Warn($"Exception in listener callback execution for {eventType}: {ex.Message}");
            }
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
            
            if (currentMessageChannel.Contains("-pnpres") || currentMessageChannel.Contains(".*-pnpres"))
            {
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
                        jsonFields.Add("payload", payload);
                    }
                }
            }
            else
            {
                if ((configuration.CryptoModule != null || configuration.CipherKey.Length > 0) && (eventData.MessageType == 0 || eventData.MessageType == 4)) //decrypt the subscriber message if cipherkey is available
                {
                    string decryptMessage = "";
                    configuration.CryptoModule ??= new CryptoModule(new AesCbcCryptor(configuration.CipherKey), new List<ICryptor>() { new LegacyCryptor(configuration.CipherKey, configuration.UseRandomInitializationVector) });
                    try
                    {
                        decryptMessage = configuration.CryptoModule.Decrypt(payload.ToString());
                    }
                    catch (Exception ex)
                    {
                        decryptMessage = "**DECRYPT ERROR**";
                        configuration?.Logger?.Warn("Failed to decrypt message on channel {currentMessageChannel} due to exception={ex}.\n Message might be not encrypted");
                    }

                    object decodeMessage = jsonLibrary.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLibrary.SerializeToJsonString(payload) : decryptMessage);
                    jsonFields.Add("payload", decodeMessage);
                }
                else
                {
                    string payloadJson = jsonLibrary.SerializeToJsonString(payload);
                    object payloadJObject = jsonLibrary.BuildJsonObject(payloadJson);
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

            var userMetaData = eventData.UserMetadata;
            jsonFields.Add("userMetadata", userMetaData);
            jsonFields.Add("publishTimetoken", GetTimetokenMetadata(eventData.PublishMetadata).Timetoken);
            jsonFields.Add("userId", eventData.IssuingClientId);

            jsonFields.Add("currentMessageChannelGroup", currentMessageChannelGroup);
            jsonFields.Add("currentMessageChannel", currentMessageChannel);

            switch (eventData.MessageType)
            {
                case 1:
                {
                    jsonFields.Add("customMessageType", eventData.CustomMessageType);
                    ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary);
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
                            SafeInvokeListener(() => listener?.Signal(instance, signalMessage), "Signal");
                        }

                        if (!string.IsNullOrEmpty(signalMessage.Channel) && channelListenersMap.ContainsKey(signalMessage.Channel))
                        {
                            foreach (var l in channelListenersMap[signalMessage.Channel])
                            {
                                SafeInvokeListener(() => l?.Signal(instance, signalMessage), "Signal");
                            }
                        }

                        if (!string.IsNullOrEmpty(signalMessage.Subscription) && channelGroupListenersMap.ContainsKey(signalMessage.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[signalMessage.Subscription])
                            {
                                SafeInvokeListener(() => l?.Signal(instance, signalMessage), "Signal");
                            }
                        }
                    }

                    break;
                }
                case 2:
                {
                    ResponseBuilder responseBuilder =new ResponseBuilder(configuration, jsonLibrary);
                    PNObjectEventResult objectApiEvent = responseBuilder.GetEventResultObject<PNObjectEventResult>(jsonFields);
                    if (objectApiEvent != null)
                    {
                        foreach (var listener in listeners)
                        {
                            SafeInvokeListener(() => listener?.ObjectEvent(instance, objectApiEvent), "ObjectEvent");
                        }

                        if (!string.IsNullOrEmpty(objectApiEvent.Channel) && channelListenersMap.ContainsKey(objectApiEvent.Channel))
                        {
                            foreach (var l in channelListenersMap[objectApiEvent.Channel])
                            {
                                SafeInvokeListener(() => l?.ObjectEvent(instance, objectApiEvent), "ObjectEvent");
                            }
                        }

                        if (!string.IsNullOrEmpty(objectApiEvent.Subscription) && channelGroupListenersMap.ContainsKey(objectApiEvent.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[objectApiEvent.Subscription])
                            {
                                SafeInvokeListener(() => l?.ObjectEvent(instance, objectApiEvent), "ObjectEvent");
                            }
                        }
                    }

                    break;
                }
                case 3:
                {
                    ResponseBuilder responseBuilder =new ResponseBuilder(configuration, jsonLibrary);
                    PNMessageActionEventResult messageActionEvent = responseBuilder.GetEventResultObject<PNMessageActionEventResult>(jsonFields);
                    if (messageActionEvent != null)
                    {
                        foreach (var listener in listeners)
                        {
                            SafeInvokeListener(() => listener?.MessageAction(instance, messageActionEvent), "MessageAction");
                        }

                        if (!string.IsNullOrEmpty(messageActionEvent.Channel) && channelListenersMap.ContainsKey(messageActionEvent.Channel))
                        {
                            foreach (var l in channelListenersMap[messageActionEvent.Channel])
                            {
                                SafeInvokeListener(() => l?.MessageAction(instance, messageActionEvent), "MessageAction");
                            }
                        }

                        if (!string.IsNullOrEmpty(messageActionEvent.Subscription) && channelGroupListenersMap.ContainsKey(messageActionEvent.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[messageActionEvent.Subscription])
                            {
                                SafeInvokeListener(() => l?.MessageAction(instance, messageActionEvent), "MessageAction");
                            }
                        }
                    }

                    break;
                }
                case 4:
                {
                    jsonFields.Add("customMessageType", eventData.CustomMessageType);
                    ResponseBuilder responseBuilder =new ResponseBuilder(configuration, jsonLibrary);
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
                            SafeInvokeListener(() => listener?.File(instance, fileMessage), "File");
                        }

                        if (!string.IsNullOrEmpty(fileMessage.Channel) && channelListenersMap.ContainsKey(fileMessage.Channel))
                        {
                            foreach (var l in channelListenersMap[fileMessage.Channel])
                            {
                                SafeInvokeListener(() => l?.File(instance, fileMessage), "File");
                            }
                        }

                        if (!string.IsNullOrEmpty(fileMessage.Subscription) && channelGroupListenersMap.ContainsKey(fileMessage.Subscription))
                        {
                            foreach (var l in channelGroupListenersMap[fileMessage.Subscription])
                            {
                                SafeInvokeListener(() => l?.File(instance, fileMessage), "File");
                            }
                        }
                    }

                    break;
                }
                default:
                {
                    if (currentMessageChannel.Contains("-pnpres"))
                    {
                        ResponseBuilder responseBuilder =new ResponseBuilder(configuration, jsonLibrary);
                        PNPresenceEventResult presenceEvent = responseBuilder.GetEventResultObject<PNPresenceEventResult>(jsonFields);
                        if (presenceEvent != null)
                        {
                            foreach (var listener in listeners)
                            {
                                SafeInvokeListener(() => listener?.Presence(instance, presenceEvent), "Presence");
                            }

                            if (!string.IsNullOrEmpty(presenceEvent.Channel) && channelListenersMap.ContainsKey(presenceEvent.Channel))
                            {
                                foreach (var l in channelListenersMap[presenceEvent.Channel])
                                {
                                    SafeInvokeListener(() => l?.Presence(instance, presenceEvent), "Presence");
                                }
                            }

                            if (!string.IsNullOrEmpty(presenceEvent.Subscription) && channelGroupListenersMap.ContainsKey(presenceEvent.Subscription))
                            {
                                foreach (var l in channelGroupListenersMap[presenceEvent.Subscription])
                                {
                                    SafeInvokeListener(() => l?.Presence(instance, presenceEvent), "Presence");
                                }
                            }
                        }
                    }
                    else
                    {
                        jsonFields.Add("customMessageType", eventData.CustomMessageType);
                        ResponseBuilder responseBuilder =new ResponseBuilder(configuration, jsonLibrary);
                        PNMessageResult<T> userMessage = responseBuilder.GetEventResultObject<PNMessageResult<T>>(jsonFields);
                        if (userMessage != null)
                        {
                            foreach (var listener in listeners)
                            {
                                SafeInvokeListener(() => listener?.Message(instance, userMessage), "Message");
                            }

                            if (!string.IsNullOrEmpty(userMessage.Channel) && channelListenersMap.ContainsKey(userMessage.Channel))
                            {
                                foreach (var l in channelListenersMap[userMessage.Channel])
                                {
                                    SafeInvokeListener(() => l?.Message(instance, userMessage), "Message");
                                }
                            }

                            if (!string.IsNullOrEmpty(userMessage.Subscription) && channelGroupListenersMap.ContainsKey(userMessage.Subscription))
                            {
                                foreach (var l in channelGroupListenersMap[userMessage.Subscription])
                                {
                                    SafeInvokeListener(() => l?.Message(instance, userMessage), "Message");
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}