using System;
using System.Linq;
using System.Collections.Generic;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{

	public class SubscriptionSet : SubscribeCapable
	{
		public override List<string> ChannelNames { get; set; } = new List<string>();
		public override List<string> ChannelGroupNames { get; set; } = new List<string>();
		public override Pubnub Pubnub { get; set; }
		public override EventEmitter EventEmitter { get; set; }
		public override SubscriptionOptions? Options { get; set; }
		List<Subscription> SubscriptionList { get; set; } = new List<Subscription>();
		protected override SubscribeCallbackExt Listener { get; set; } = new SubscribeCallbackExt();

		public override event Action<Pubnub, PNMessageResult<object>> onMessage;
		public override event Action<Pubnub, PNPresenceEventResult> onPresence;
		public override event Action<Pubnub, PNSignalResult<object>> onSignal;
		public override event Action<Pubnub, PNObjectEventResult> onObject;
		public override event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public override event Action<Pubnub, PNFileEventResult> onFile;
		public override event Action<Pubnub, PNStatus> onStatus;

		public SubscriptionSet(string[] channels, string[] channelGroups, SubscriptionOptions? options, Pubnub pubnub, EventEmitter eventEmitter)
		{
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
			this.Options = options;

			foreach (var c in channels
				.Where(c => !c.EndsWith(Constants.Pnpres))) {
				var subscription = this.Pubnub.Channel(c).Subscription(this.Options);
				this.ChannelNames.AddRange(subscription.ChannelNames);
				this.SubscriptionList.Add(subscription);
			}

			foreach (var cg in channelGroups
				.Where(cg => !cg.EndsWith(Constants.Pnpres))) {
				var subscription = this.Pubnub.ChannelGroup(cg).Subscription(this.Options);
				this.ChannelGroupNames.AddRange(subscription.ChannelGroupNames);
				this.SubscriptionList.Add(subscription);
			}

			this.Listener = new SubscribeCallbackExt(
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
			this.EventEmitter.AddListener(Listener, channels: channels, groups: channelGroups );
		}

		public SubscriptionSet Add(Subscription subscription)
		{
			this.SubscriptionList.ToList().Add(subscription);
			if (subscription.ChannelNames.Count > 0) {
				this.ChannelNames.AddRange(subscription.ChannelNames);
			}
			if (subscription.ChannelGroupNames.Count > 0) {
				this.ChannelGroupNames.AddRange(subscription.ChannelGroupNames);
			}
			this.EventEmitter.AddListener(this.Listener, subscription.ChannelNames.ToArray(), subscription.ChannelGroupNames.ToArray());
			return this;
		}

		public SubscriptionSet Remove(Subscription subscription)
		{
			this.SubscriptionList.Remove(subscription);
			if (subscription.ChannelNames.Count > 0) {
				foreach (var c in subscription.ChannelNames) {
					this.ChannelNames.Remove(c);
				}
			}
			if (subscription.ChannelGroupNames.Count > 0) {
				foreach (var g in subscription.ChannelGroupNames) {
					this.ChannelGroupNames.Remove(g);
				}
			}
			this.EventEmitter.RemoveListener(this.Listener, subscription.ChannelNames.ToArray(), subscription.ChannelGroupNames.ToArray());
			return this;
		}

		public SubscriptionSet Add(SubscriptionSet subscriptionSet)
		{
			this.SubscriptionList.AddRange(subscriptionSet.SubscriptionList);
			if (subscriptionSet.ChannelNames.Count > 0) {
				this.ChannelNames.AddRange(subscriptionSet.ChannelNames);
			}
			if (subscriptionSet.ChannelGroupNames.Count > 0) {
				this.ChannelGroupNames.AddRange(subscriptionSet.ChannelGroupNames);
			}
			this.EventEmitter.AddListener(this.Listener, subscriptionSet.ChannelNames.ToArray(), subscriptionSet.ChannelGroupNames.ToArray());
			return this;
		}

		public SubscriptionSet Remove(SubscriptionSet subscriptionSet)
		{
			SubscriptionList =  this.SubscriptionList.Except(subscriptionSet.SubscriptionList).ToList();
			if (subscriptionSet.ChannelNames.Count > 0) {
				foreach (var c in subscriptionSet.ChannelNames) {
					this.ChannelNames.Remove(c);
				}
			}
			if (subscriptionSet.ChannelGroupNames.Count > 0) {
				foreach (var g in subscriptionSet.ChannelGroupNames) {
					this.ChannelGroupNames.Remove(g);
				}
			}
			this.EventEmitter.RemoveListener(this.Listener, subscriptionSet.ChannelNames.ToArray(), subscriptionSet.ChannelGroupNames.ToArray());
			return this;
		}
	}
}