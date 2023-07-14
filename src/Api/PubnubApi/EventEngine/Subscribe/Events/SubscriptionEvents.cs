using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Events {
    public class UnsubscribeAllEvent : Core.IEvent {
    }
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
		public SubscriptionCursor Cursor;
		public PNStatus Status;
	}

	public class HandshakeFailureEvent : Core.IEvent {
		public PNStatus Status;
	}

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent {
		public SubscriptionCursor Cursor;
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent
	{
		public PNStatus Status;
	}

	public class HandshakeReconnectRetryEvent : Core.IEvent {
	}

	public class HandshakeReconnectGiveUpEvent : Core.IEvent {
		public PNStatus Status;
	}

	public class ReceiveSuccessEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public List<PNMessageResult<object>> Messages;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
	}

	public class ReceiveFailureEvent : Core.IEvent {
		public PNStatus Status;
	}

	public class ReceiveReconnectRetry : Core.IEvent {
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent {
	}

	public class ReceiveReconnectFailureEvent : ReceiveFailureEvent {
	}

	public class ReceiveReconnectGiveUpEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
	}

	public class DisconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
	}

	public class ReconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
	}
}