using System;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class ChannelGroup
	{
		public string Name { get; set; }
		private Pubnub Pubnub { get; set; }
		private EventEmitter EventEmitter { get; set; }
		public ChannelGroup(string name, Pubnub pubnub, EventEmitter eventEmitter)
		{
			Name = name;
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
		}

		Subscription Subscription(SubscriptionOptions options = SubscriptionOptions.None)
		{
			return new Subscription(this.Name, options, this.Pubnub, this.EventEmitter);
		}
	}
}