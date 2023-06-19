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

	public class HandshakeSuccessEvent : Core.IEvent {
		public SubscriptionCursor cursor;
	}

	public class HandshakeFailureEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class HandshakeReconnectSuccessEvent : Core.IEvent {
		public SubscriptionCursor cursor;
	}

	public class HandshakeReconnectFailureEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class HandshakeReconnectRetryEvent : Core.IEvent { }

	public class HandshakeReconnectGiveUpEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class ReceiveSuccessEvent : Core.IEvent {
		public List<PNMessageResult<object>> messages;
		public SubscriptionCursor cursor;
	}

	public class ReceiveFailureEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent { }
	public class ReceiveReconnectFailure : ReceiveFailureEvent { }

	public class ReceiveReconnectGiveUpEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}
}