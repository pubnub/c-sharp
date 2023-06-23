using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Events {
	public class SubscriptionChangedEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	public class SubscriptionRestoredEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
	}

	public class HandshakeSuccessEvent : Core.IEvent {
		public SubscriptionCursor cursor;
	}

	public class HandshakeFailureEvent : Core.IEvent {
		// TODO status or reason?
		public PNStatus status;
	}

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent {
	}

	public class HandshakeReconnectRetryEvent : Core.IEvent {
	}

	public class HandshakeReconnectGiveUpEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		// TODO status or reason?
		public PNStatus status;
	}

	public class ReceiveSuccessEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public List<PNMessageResult<object>> Messages;
		public SubscriptionCursor Cursor;
	}

	public class ReceiveFailureEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
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
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	public class ReconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}
}