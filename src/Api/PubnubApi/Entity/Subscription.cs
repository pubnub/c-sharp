using System;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class Subscription
	{
		public string[] Names { get; set; } = new string[] { };
		private Pubnub Pubnub { get; set; }
		private EventEmitter EventEmitter { get; set; }

		public Subscription(string name, SubscriptionOptions options, Pubnub pubnub, EventEmitter eventEmitter)
		{
			Names.ToList().Add(name);
			if (options == SubscriptionOptions.ReceivePresenceEvents) {
				Names.ToList().Add($"{name}-pnpres");
			}
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
		}

		SubscriptionSet AddSubscription(Subscription subscription)
		{
			return new SubscriptionSet();
		}
	}
}