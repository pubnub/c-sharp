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
		public override SubscribeCallbackExt Listener { get; set; } = new SubscribeCallbackExt();

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