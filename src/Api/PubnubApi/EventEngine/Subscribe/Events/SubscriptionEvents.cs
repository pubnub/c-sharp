using System.Collections.Generic;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Events {
	public class UnsubscribeAllEvent : Core.IEvent {
		public string Name { get; set; } = "UNSUBSCRIBE_ALL";
	}
	
	public class SubscriptionChangedEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "SUBSCRIPTION_CHANGED";
	}

	public class SubscriptionRestoredEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "SUBSCRIPTION_RESTORED";
	}

	public class HandshakeSuccessEvent : Core.IEvent {
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public virtual string Name { get; set; } = "HANDSHAKE_SUCCESS";
	}

	public class HandshakeFailureEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public PNStatus Status;
		public int AttemptedRetries;
		public virtual string Name { get; set; } = "HANDSHAKE_FAILURE";
	}

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent {
		public PNStatus Status;
		public SubscriptionCursor Cursor;
		public override string Name { get; set; } = "HANDSHAKE_RECONNECT_SUCCESS";
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent
	{
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public override string Name { get; set; } = "HANDSHAKE_RECONNECT_FAILURE";
	}

	public class HandshakeReconnectGiveUpEvent : Core.IEvent {
		public PNStatus Status;
		public string Name { get; set; } = "HANDSHAKE_RECONNECT_GIVEUP";
	}

	public class ReceiveSuccessEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public ReceivingResponse<object> Messages;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public virtual string Name { get; set; } = "RECEIVE_SUCCESS";
	}

	public class ReceiveFailureEvent : Core.IEvent {
		public PNStatus Status;
		public int AttemptedRetries;
		public SubscriptionCursor Cursor;
		public virtual string Name { get; set; } = "RECEIVE_FAILURE";
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent {
		public override string Name { get; set; } = "RECEIVE_RECONNECT_SUCCESS";
	}

	public class ReceiveReconnectFailureEvent : ReceiveFailureEvent {
		public override string Name { get; set; } = "RECEIVE_RECONNECT_FAILURE";
	}

	public class ReceiveReconnectGiveUpEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public string Name { get; set; } = "RECEIVE_RECONNECT_GIVEUP";
	}

	public class DisconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "DISCONNECT";
	}

	public class ReconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "RECONNECT";
	}
}