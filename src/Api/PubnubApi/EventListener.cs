using System;
namespace PubnubApi
{
	public class EventListener {
		public event Action<Pubnub, PNMessageResult<object>> onMessage;
		public event Action<Pubnub, PNPresenceEventResult> onPresence;
		public event Action<Pubnub, PNSignalResult<object>> onSignal;
		public event Action<Pubnub, PNObjectEventResult> onObject;
		public event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public event Action<Pubnub, PNFileEventResult> onFile;
		public event Action<Pubnub, PNStatus> onStatus;

		protected SubscribeCallbackExt listener;

		public EventListener(
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

		public EventListener() {
			this.listener = new SubscribeCallbackExt(
				(pnObj, pubMsg) => {
					onMessage?.Invoke(pnObj, pubMsg);
				},
				(pnObj, presenceEvnt) => {
					onPresence?.Invoke(pnObj, presenceEvnt);
				},
				(pnObj, signalMsg) => {
					onSignal?.Invoke(pnObj, signalMsg);
				},
				(pnObj, objectEventObj) => {
					onObject?.Invoke(pnObj, objectEventObj);
				},
				(pnObj, msgActionEvent) => {
					onMessageAction?.Invoke(pnObj, msgActionEvent);
				},
				(pnObj, fileEvent) => {
					onFile?.Invoke(pnObj, fileEvent);
				},
				(pnObj, pnStatus) => {
					onStatus?.Invoke(pnObj, pnStatus);
				}
			);
		}

		public static implicit operator SubscribeCallback(EventListener listener) {
			return listener.listener;
		}
	}
}

