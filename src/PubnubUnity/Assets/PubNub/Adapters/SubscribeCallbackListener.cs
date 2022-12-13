﻿using System;
using UnityEngine;
using PubnubApi.Unity.Internal;

namespace PubnubApi.Unity {
	/// <summary>
	/// Wrapper for the SubscribeCallback class. Allows dispatching events to main game loop, and direct subscription to every PubNub event.
	/// </summary>
	public class SubscribeCallbackListener<T> {
		public event Action<Pubnub, PNMessageResult<T>> onMessage;
		public event Action<Pubnub, PNPresenceEventResult> onPresence;
		public event Action<Pubnub, PNSignalResult<T>> onSignal;
		public event Action<Pubnub, PNObjectEventResult> onObject;
		public event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public event Action<Pubnub, PNFileEventResult> onFile;
		public event Action<Pubnub, PNStatus> onStatus;

		protected SubscribeCallbackExt listener;

		public SubscribeCallbackListener(Action<Pubnub, PNMessageResult<T>> messageCallback,
			Action<Pubnub, PNPresenceEventResult> presenceCallback,
			Action<Pubnub, PNSignalResult<T>> signalCallback,
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
					Dispatcher.Dispatch(() => onMessage?.Invoke(pnObj, pubMsg as PNMessageResult<T>));
				},
				// Presence
				(pnObj, presenceEvnt) => {
					#if PN_DEBUG
					Debug.Log(
						$"[LISTENER] onPresence {presenceEvnt.Channel}, {presenceEvnt.Uuid != pnObj.PNConfig.Uuid}");
					#endif
					Dispatcher.Dispatch(() => onPresence?.Invoke(pnObj, presenceEvnt));
				},
				// Signals
				(pnObj, signalMsg) => {
					#if PN_DEBUG
					Debug.Log(signalMsg.Channel);
					#endif
					Dispatcher.Dispatch(() => onSignal?.Invoke(pnObj, signalMsg as PNSignalResult<T>));
				},
				// Objects
				(pnObj, objectEventObj) => {
					#if PN_DEBUG
					Debug.Log(objectEventObj.Channel);
					#endif
					Dispatcher.Dispatch(() => onObject?.Invoke(pnObj, objectEventObj));
				},
				// Message actions
				(pnObj, msgActionEvent) => {
					#if PN_DEBUG
					Debug.Log(msgActionEvent.Channel);
					#endif
					Dispatcher.Dispatch(() => onMessageAction?.Invoke(pnObj, msgActionEvent));
				},
				// Files
				(pnObj, fileEvent) => { Dispatcher.Dispatch(() => onFile?.Invoke(pnObj, fileEvent)); },
				// Status
				(pnObj, pnStatus) => {
					#if PN_DEBUG
					Debug.Log($"[LISTENER] onPresence {pnStatus.Category}");
					#endif
					if (pnStatus.Error) {
						// TODO additional error handling?
						Dispatcher.Dispatch(() => Debug.LogError(pnStatus.ErrorData.Information));
					}

					Dispatcher.Dispatch(() => onStatus?.Invoke(pnObj, pnStatus));
				}
			);
		}

		public static implicit operator SubscribeCallback(SubscribeCallbackListener<T> listener) {
			return listener.listener;
		}
	}
}