using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Events {
	public class SubscriptionChangedEvent : Core.IEvent {
		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;
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

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent {
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent {
	}

	public class HandshakeReconnectRetryEvent : Core.IEvent {
	}

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

	public class ReceiveReconnectRetry : Core.IEvent {
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent {
	}

	public class ReceiveReconnectFailureEvent : ReceiveFailureEvent {
	}

	public class ReceiveReconnectGiveUpEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class DisconnectEvent : Core.IEvent {
	}

	public class ReconnectEvent : Core.IEvent {
	}
}