using System;
using UnityEngine;
using PubnubApi.Unity.Internal;

namespace PubnubApi.Unity {
	/// <summary>
	/// Wrapper for the SubscribeCallback class. Allows dispatching events to main game loop, and direct subscription to every PubNub event.
	/// </summary>
	public class SubscribeCallbackListener {
		public event Action<Pubnub, PNMessageResult<object>> onMessage;
		public event Action<Pubnub, PNPresenceEventResult> onPresence;
		public event Action<Pubnub, PNSignalResult<object>> onSignal;
		public event Action<Pubnub, PNObjectEventResult> onObject;
		public event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public event Action<Pubnub, PNFileEventResult> onFile;
		public event Action<Pubnub, PNStatus> onStatus;

		protected SubscribeCallbackExt listener;

		public SubscribeCallbackListener(
			Action<Pubnub, PNMessageResult<object>> messageCallback,
			Action<Pubnub, PNPresenceEventResult> presenceCallback,
			Action<Pubnub, PNSignalResult<object>> signalCallback,
			Action<Pubnub, PNObjectEventResult> objectEventCallback,
			Action<Pubnub, PNMessageActionEventResult> messageActionCallback,
			Action<Pubnub, PNFileEventResult> fileCallback,
			Action<Pubnub, PNStatus> statusCallback) : this() {
			this.onMessage += messageCallback;
			this.onPresence += presenceCallback;
			this.onSignal += signalCallback;
			this.onObject += objectEventCallback;
			this.onMessageAction += messageActionCallback;
			this.onFile += fileCallback;
			this.onStatus += statusCallback;
		}

		public SubscribeCallbackListener() {
			this.listener = new SubscribeCallbackExt(
				// Messages
				(pnObj, pubMsg) => {
					#if PN_DEBUG
					Debug.Log($"[LISTENER] onMessage {pubMsg.Message}");
					#endif
					onMessage.Dispatch(pnObj, pubMsg);
				},
				// Presence
				(pnObj, presenceEvnt) => {
					#if PN_DEBUG
					Debug.Log(
						$"[LISTENER] onPresence {presenceEvnt.Channel}, Different UserId: {presenceEvnt.Uuid != pnObj.PNConfig.UserId}");
					#endif
					onPresence.Dispatch(pnObj, presenceEvnt);
				},
				// Signals
				(pnObj, signalMsg) => {
					#if PN_DEBUG
					Debug.Log(signalMsg.Channel);
					#endif
					onSignal.Dispatch(pnObj, signalMsg);
				},
				// Objects
				(pnObj, objectEventObj) => {
					#if PN_DEBUG
					Debug.Log(objectEventObj.Channel);
					#endif
					onObject.Dispatch(pnObj, objectEventObj);
				},
				// Message actions
				(pnObj, msgActionEvent) => {
					#if PN_DEBUG
					Debug.Log(msgActionEvent.Channel);
					#endif
					onMessageAction.Dispatch(pnObj, msgActionEvent);
				},
				// Files
				(pnObj, fileEvent) => {
					#if PN_DEBUG
					Debug.Log($"[LISTENER] onFile {fileEvent.Timetoken}");
					#endif
					onFile.Dispatch(pnObj, fileEvent);
				},
				// Status
				(pnObj, pnStatus) => {
					#if PN_DEBUG
					Debug.Log($"[LISTENER] onPresence {pnStatus.Category}");
					#endif
					if (pnStatus.Error) {
						// TODO additional error handling?
						Dispatcher.Dispatch(() => Debug.LogError(pnStatus.ErrorData.Information));
					}

					onStatus.Dispatch(pnObj, pnStatus);
				}
			);
		}

		public static implicit operator SubscribeCallback(SubscribeCallbackListener listener) {
			return listener.listener;
		}
	}
}