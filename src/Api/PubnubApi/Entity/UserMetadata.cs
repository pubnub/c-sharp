using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class UserMetadata
	{
		public string Id { get; set; }
		private Pubnub Pubnub { get; set; }
		private EventEmitter EventEmitter { get; set; }

		public UserMetadata(string id, Pubnub pubnub, EventEmitter eventEmitter)
		{
			Id = id;
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
		}

		public Subscription Subscription(SubscriptionOptions options = SubscriptionOptions.None)
		{
			return new Subscription(options == SubscriptionOptions.ReceivePresenceEvents ? new string[] { Id, $"{Id}-pnpres" } : new string[] { Id }, new string[] { }, options, this.Pubnub, this.EventEmitter);
		}
	}
}

