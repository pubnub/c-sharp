using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Events {
	public class SubscriptionChangedEvent : Core.IEvent {
		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;
		public SubscriptionCursor cursor;
	}
	
	public class SubscriptionRestoredEvent : Core.IEvent {
		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;
		public SubscriptionCursor cursor;
	}
}