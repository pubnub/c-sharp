using System;
using System.Collections.Generic;
using System.Globalization;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

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

		public EventEmitter(PNConfiguration configuration, List<SubscribeCallback> listenerCallbacks, IJsonPluggableLibrary jsonPluggableLibrary, TokenManager tokenManager, IPubnubLog log, Pubnub instance)
		{
			this.configuration = configuration;
			this.log = log;
			this.instance = instance;
			this.tokenManager = tokenManager;
			jsonLibrary = jsonPluggableLibrary;
			listeners = listenerCallbacks;
		}

		private TimetokenMetadata GetTimetokenMetadata(object t)
		{
			Dictionary<string, object> ttOriginMetaData = jsonLibrary.ConvertToDictionaryObject(t);
			if (ttOriginMetaData != null && ttOriginMetaData.Count > 0) {
				TimetokenMetadata ttMeta = new TimetokenMetadata();

				foreach (string metaKey in ttOriginMetaData.Keys) {
					if (metaKey.ToLowerInvariant().Equals("t", StringComparison.OrdinalIgnoreCase)) {
						long timetoken;
						_ = Int64.TryParse(ttOriginMetaData[metaKey].ToString(), out timetoken);
						ttMeta.Timetoken = timetoken;
					} else if (metaKey.ToLowerInvariant().Equals("r", StringComparison.OrdinalIgnoreCase)) {
						ttMeta.Region = ttOriginMetaData[metaKey].ToString();
					}
				}
				return ttMeta;
			}
			return null;
		}

		public void EmitEvent<T>(object e)
		{
			Message<T> eventData = e as Message<T>;

			string currentMessageChannel = eventData.Channel;
			string currentMessageChannelGroup = eventData.SubscriptionMatch;

			if (currentMessageChannel.Replace("-pnpres", "") == currentMessageChannelGroup?.Replace("-pnpres", "")) {
				currentMessageChannelGroup = "";
			}
			object payload = eventData.Payload;
			List<object> payloadContainer = new List<object>(); //First item always message
			if (currentMessageChannel.Contains("-pnpres") || currentMessageChannel.Contains(".*-pnpres")) {
				payloadContainer.Add(payload);
			} else if (eventData.MessageType == 2) //Objects Simplification events
			  {
				double objectsVersion = -1;
				Dictionary<string, object> objectsDic = payload as Dictionary<string, object>;
				if (objectsDic != null
					&& objectsDic.ContainsKey("source")
					&& objectsDic.ContainsKey("version")
					&& objectsDic["source"].ToString() == "objects"
					&& Double.TryParse(objectsDic["version"].ToString().Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out objectsVersion)) {
					if (objectsVersion.CompareTo(2D) == 0) //Process only version=2 for Objects Simplification. Ignore 1. 
					{
						payloadContainer.Add(payload);
					}
				}
			} else {
				if ((configuration.CryptoModule != null || configuration.CipherKey.Length > 0) && eventData.MessageType != 1) //decrypt the subscriber message if cipherkey is available
				{
					string decryptMessage = "";
					configuration.CryptoModule ??= new CryptoModule(new LegacyCryptor(configuration.CipherKey, configuration.UseRandomInitializationVector, log), null);
					try {
						decryptMessage = configuration.CryptoModule.Decrypt(payload.ToString());
					} catch (Exception ex) {
						decryptMessage = "**DECRYPT ERROR**";
						LoggingMethod.WriteToLog(
								log,
								string.Format(
									CultureInfo.InvariantCulture,
									"Failed to decrypt message on channel {0} in ResponseToUserCallback due to exception={1}.\nMessage might be not encrypted, returning as is...",
									currentMessageChannel,
									ex
								),
								configuration.LogVerbosity
						);
					}
					object decodeMessage = jsonLibrary.DeserializeToObject((decryptMessage == "**DECRYPT ERROR**") ? jsonLibrary.SerializeToJsonString(payload) : decryptMessage);
					payloadContainer.Add(decodeMessage);
				} else {
					string payloadJson = jsonLibrary.SerializeToJsonString(payload);
					object payloadJObject = jsonLibrary.BuildJsonObject(payloadJson);
					if (payloadJObject == null) {
						payloadContainer.Add(payload);
					} else {
						payloadContainer.Add(payloadJObject);
					}
				}
			}

			object userMetaData = eventData.UserMetadata;

			payloadContainer.Add(userMetaData); //Second one always user meta data

			payloadContainer.Add(GetTimetokenMetadata(eventData.PublishMetadata)); //Third one always Timetoken

			payloadContainer.Add(eventData.IssuingClientId); //Fourth one always Publisher

			if (!string.IsNullOrEmpty(currentMessageChannelGroup)) //Add cg first before channel
			{
				payloadContainer.Add(currentMessageChannelGroup);
			}

			if (!string.IsNullOrEmpty(currentMessageChannel)) {
				payloadContainer.Add(currentMessageChannel);
			}

			if (eventData.MessageType == 1) {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNMessageResult<T> pnMessageResult = responseBuilder.JsonToObject<PNMessageResult<T>>(payloadContainer, true);
				if (pnMessageResult != null) {
					PNSignalResult<T> signalMessage = new PNSignalResult<T> {
						Channel = pnMessageResult.Channel,
						Message = pnMessageResult.Message,
						Subscription = pnMessageResult.Subscription,
						Timetoken = pnMessageResult.Timetoken,
						UserMetadata = pnMessageResult.UserMetadata,
						Publisher = pnMessageResult.Publisher
					};
					foreach (var listener in listeners) {
						listener?.Signal(instance, signalMessage);
					}
				}
			} else if (eventData.MessageType == 2) {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNObjectEventResult objectApiEvent = responseBuilder.JsonToObject<PNObjectEventResult>(payloadContainer, true);
				if (objectApiEvent != null) {
					foreach (var listener in listeners) {
						listener?.ObjectEvent(instance, objectApiEvent);
					}
				}
			} else if (eventData.MessageType == 3) {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNMessageActionEventResult messageActionEvent = responseBuilder.JsonToObject<PNMessageActionEventResult>(payloadContainer, true);
				if (messageActionEvent != null) {
					foreach (var listener in listeners) {
						listener?.MessageAction(instance, messageActionEvent);
					}
				}
			} else if (eventData.MessageType == 4) {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNMessageResult<object> pnFileResult = responseBuilder.JsonToObject<PNMessageResult<object>>(payloadContainer, true);
				if (pnFileResult != null) {
					PNFileEventResult fileMessage = new PNFileEventResult {
						Channel = pnFileResult.Channel,
						Subscription = pnFileResult.Subscription,
						Timetoken = pnFileResult.Timetoken,
						Publisher = pnFileResult.Publisher,
					};
					Dictionary<string, object> pnMsgObjDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(pnFileResult.Message);
					if (pnMsgObjDic != null && pnMsgObjDic.Count > 0) {
						if (pnMsgObjDic.ContainsKey("message") && pnMsgObjDic["message"] != null) {
							fileMessage.Message = pnMsgObjDic["message"];
						}
						if (pnMsgObjDic.ContainsKey("file")) {
							Dictionary<string, object> fileObjDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(pnMsgObjDic["file"]);
							if (fileObjDic != null && fileObjDic.ContainsKey("id") && fileObjDic.ContainsKey("name")) {
								fileMessage.File = new PNFile { Id = fileObjDic["id"].ToString(), Name = fileObjDic["name"].ToString() };

								IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(configuration, jsonLibrary, null, log, null, tokenManager, (instance != null) ? instance.InstanceId : "");
								Uri fileUrlRequest = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", fileMessage.Channel, fileMessage.File.Id, fileMessage.File.Name, null, PNOperationType.PNGenerateFileUploadUrlOperation);
								fileMessage.File.Url = fileUrlRequest.ToString();
							}
						}
					} else {
						if (pnFileResult.Message != null) {
							fileMessage.Message = pnFileResult.Message;
						}
					}
					foreach (var listener in listeners) {
						listener?.File(instance, fileMessage);
					}
				}
			}
			else if (currentMessageChannel.Contains("-pnpres")) {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNPresenceEventResult presenceEvent = responseBuilder.JsonToObject<PNPresenceEventResult>(payloadContainer, true);
				if (presenceEvent != null) {
					foreach (var listener in listeners) {
						listener?.Presence(instance, presenceEvent);
					}
				}
			}
			else {
				ResponseBuilder responseBuilder = new ResponseBuilder(configuration, jsonLibrary, log);
				PNMessageResult<T> message = responseBuilder.JsonToObject<PNMessageResult<T>>(payloadContainer, true);
				if (message != null) {
					foreach (var listener in listeners) {
						listener?.Message(instance, message);
					}
				}
			}
		}
	}
}

