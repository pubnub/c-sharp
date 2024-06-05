using System;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class Subscription : SubscribeCapable
	{
		public override List<string> ChannelNames { get; set; } = new List<string>();
		public override List<string> ChannelGroupNames { get; set; } = new List<string>();
		public override Pubnub Pubnub { get; set; }
		public override EventEmitter EventEmitter { get; set; }
		public override SubscriptionOptions? Options { get; set; }
		public override SubscribeCallbackExt Listener { get; set; } = new SubscribeCallbackExt();

		public Subscription(string[] channels, string[] channelGroups, SubscriptionOptions? options, Pubnub pubnub, EventEmitter eventEmitter)
		{
			this.ChannelNames = channels.ToList();
			this.ChannelGroupNames = channelGroups.ToList();
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
			this.Options = options;
			this.EventEmitter.AddListener(Listener, channels, channelGroups);
		}

		public SubscriptionSet Add(Subscription subscription)
		{
			this.ChannelNames.AddRange(subscription.ChannelNames);
			this.ChannelGroupNames.AddRange(subscription.ChannelGroupNames);
			return new SubscriptionSet(this.ChannelNames.ToArray(),this.ChannelGroupNames.ToArray() , this.Options, this.Pubnub, this.EventEmitter);
		}
	}
}