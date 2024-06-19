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
		public abstract SubscriptionOptions? Options { get; set; }
		protected abstract SubscribeCallbackExt Listener { get; set; }

		public abstract event Action<Pubnub, PNMessageResult<object>> onMessage;
		public abstract event Action<Pubnub, PNPresenceEventResult> onPresence;
		public abstract event Action<Pubnub, PNSignalResult<object>> onSignal;
		public abstract event Action<Pubnub, PNObjectEventResult> onObject;
		public abstract event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public abstract event Action<Pubnub, PNFileEventResult> onFile;
		public abstract event Action<Pubnub, PNStatus> onStatus;

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

		public void AddListener(SubscribeCallback listener)
		{
			this.EventEmitter.AddListener(listener, this.ChannelNames.ToArray(), this.ChannelGroupNames.ToArray());
		}

		public void RemoveListener(SubscribeCallback listener)
		{
			this.EventEmitter.RemoveListener(listener, this.ChannelNames.ToArray(), this.ChannelGroupNames.ToArray());
		}

		public void Unsubscribe<T>()
		{
			this.Pubnub.Unsubscribe<T>().Channels(ChannelNames.ToArray()).ChannelGroups(ChannelGroupNames.ToArray()).Execute();
		}
	}
}

