using System.Collections.Generic;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Events {
	public class UnsubscribeAllEvent : Core.IEvent {
		public string Name { get; set; } = "UNSUBSCRIBE_ALL";

		public override string ToString() => $"Event : {Name}";
		
	}
	
	public class SubscriptionChangedEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "SUBSCRIPTION_CHANGED";

		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)},Cursor= {Cursor}";
		}
	}

	public class SubscriptionRestoredEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "SUBSCRIPTION_RESTORED";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)},Cursor= {Cursor}";
		}
	}

	public class HandshakeSuccessEvent : Core.IEvent {
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public virtual string Name { get; set; } = "HANDSHAKE_SUCCESS";
		
		public override string ToString()
		{
			return $"Event : {Name},Cursor= {Cursor}";
		}
	}

	public class HandshakeFailureEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public int AttemptedRetries;
		public virtual string Name { get; set; } = "HANDSHAKE_FAILURE";
		
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)},Cursor= {Cursor},retries= {AttemptedRetries}";
		}
	}

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent {
		public PNStatus Status;
		public SubscriptionCursor Cursor;
		public override string Name { get; set; } = "HANDSHAKE_RECONNECT_SUCCESS";
		
		public override string ToString()
		{
			return $"Event : {Name},Cursor= {Cursor}";
		}
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent
	{
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public override string Name { get; set; } = "HANDSHAKE_RECONNECT_FAILURE";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)}";
		}
	}

	public class HandshakeReconnectGiveUpEvent : Core.IEvent {
		public PNStatus Status;
		public string Name { get; set; } = "HANDSHAKE_RECONNECT_GIVEUP";
		public override string ToString()
		{
			return $"Event : {Name}";
		}
	}

	public class ReceiveSuccessEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public ReceivingResponse<object> Messages;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public virtual string Name { get; set; } = "RECEIVE_SUCCESS";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)} Cursor= {Cursor}";
		}
	}

	public class ReceiveFailureEvent : Core.IEvent {
		public PNStatus Status;
		public int AttemptedRetries;
		public SubscriptionCursor Cursor;
		public virtual string Name { get; set; } = "RECEIVE_FAILURE";
		public override string ToString()
		{
			return $"Event : {Name}, Cursor= {Cursor}";
		}
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent {
		public override string Name { get; set; } = "RECEIVE_RECONNECT_SUCCESS";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)}";
		}
	}

	public class ReceiveReconnectFailureEvent : ReceiveFailureEvent {
		public override string Name { get; set; } = "RECEIVE_RECONNECT_FAILURE";
		
		public override string ToString()
		{
			return $"Event : {Name}";
		}
	}

	public class ReceiveReconnectGiveUpEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public PNStatus Status;
		public string Name { get; set; } = "RECEIVE_RECONNECT_GIVEUP";
		public override string ToString()
		{
			return $"Event : {Name},Cursor= {Cursor}, StatusCode= {Status.StatusCode}";
		}
	}

	public class DisconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "DISCONNECT";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)}, Cursor= {Cursor}";
		}
	}

	public class ReconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
		public string Name { get; set; } = "RECONNECT";
		public override string ToString()
		{
			return $"Event : {Name},Channels= {string.Join(", ", Channels)},Groups = {string.Join(", ", ChannelGroups)}, Cursor= {Cursor}";
		}
	}
}