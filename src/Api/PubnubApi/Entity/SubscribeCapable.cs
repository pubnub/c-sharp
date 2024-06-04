using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi
{
	public abstract class SubscribeCapable
	{
		public abstract List<string> ChannelNames { get; set; }
		public abstract List<string> ChannelGroupNames { get; set; }
		public abstract Pubnub Pubnub { get; set; }
		public abstract EventEmitter EventEmitter { get; set; }
		public abstract SubscriptionOptions Options { get; set; }
		public abstract SubscribeCallbackExt Listener { get; set; }

		public Action<Pubnub, PNPresenceEventResult> OnPresence {
			set {
				Listener.presenceAction = value;
			}
		}
		public Action<Pubnub, PNObjectEventResult> OnObjects {
			set {
				Listener.objectEventAction = value;
			}
		}
		public Action<Pubnub, PNFileEventResult> OnFile {
			set {
				Listener.fileAction = value;
			}
		}
		public Action<Pubnub, PNMessageActionEventResult> OnMessageAction {
			set {
				Listener.messageAction = value;
			}
		}
		public Action<Pubnub, PNMessageResult<object>> OnMessage {
			set {
				Listener.subscribeAction = value;
			}
		}
		public Action<Pubnub, PNSignalResult<object>> OnSignal {
			set {
				Listener.signalAction = value;
			}
		}

		public void Subscribe<T>(SubscriptionCursor cursor = null)
		{
			var subscription = this.Pubnub.Subscribe<T>().Channels(this.ChannelNames.ToArray()).ChannelGroups(this.ChannelGroupNames.ToArray());
			if (cursor is not null && cursor.Timetoken != 0) {
				var timetoken = cursor.Timetoken ?? 0;
				subscription.WithTimetoken(timetoken).Execute();

			} else {
				subscription.Execute();
			}
		}

		public void AddListener(SubscribeCallbackExt listener)
		{
			this.EventEmitter.AddListener(listener, this.ChannelNames.ToArray(), this.ChannelGroupNames.ToArray());
		}

		public void RemoveListener(SubscribeCallbackExt listener)
		{
			this.EventEmitter.RemoveListener(listener, this.ChannelNames.ToArray(), this.ChannelGroupNames.ToArray());
		}

		public void Unsubscribe<T>()
		{
			this.Pubnub.Unsubscribe<T>().Channels(ChannelNames.ToArray()).ChannelGroups(ChannelGroupNames.ToArray()).Execute();
		}
	}
}

