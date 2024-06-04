using System;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class Channel
	{
		public string Name { get; set; }
		private Pubnub Pubnub { get; set; }
		private EventEmitter EventEmitter { get; set; }

		public Channel(string name, Pubnub pubnub, EventEmitter eventEmitter)
		{
			Name = name;
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
		}

		public Subscription Subscription(SubscriptionOptions options = SubscriptionOptions.None)
		{
			return new Subscription(options == SubscriptionOptions.ReceivePresenceEvents ? new string[] { Name, $"{Name}-pnpres" } : new string[] { Name }, new string[] { }, options, this.Pubnub, this.EventEmitter);
		}
	}
}