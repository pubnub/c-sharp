using System.Collections.Generic;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Events
{
	public class UnsubscribeAllEvent : Core.IEvent
	{
	}

	public class SubscriptionChangedEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
	}

	public class SubscriptionRestoredEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
		public SubscriptionCursor Cursor { get; set; }
	}

	public class HandshakeSuccessEvent : Core.IEvent
	{
		public SubscriptionCursor Cursor { get; set; }
		public PNStatus Status { get; set; }
	}

	public class HandshakeFailureEvent : Core.IEvent
	{
		public PNStatus Status { get; set; }
		public int AttemptedRetries { get; set; }
	}

	public class HandshakeReconnectSuccessEvent : HandshakeSuccessEvent
	{
	}

	public class HandshakeReconnectFailureEvent : HandshakeFailureEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
	}

	// Do we have this in system description ?
	public class HandshakeReconnectRetryEvent : Core.IEvent
	{
	}

	public class HandshakeReconnectGiveUpEvent : Core.IEvent
	{
		public PNStatus Status { get; set; }
	}

	public class ReceiveSuccessEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
		public ReceivingResponse<string> Messages { get; set; }
		public SubscriptionCursor Cursor { get; set; }
		public PNStatus Status { get; set; }
	}

	public class ReceiveFailureEvent : Core.IEvent
	{
		public PNStatus Status { get; set; }
		public int AttemptedRetries { get; set; }
		public SubscriptionCursor Cursor { get; set; }
	}

	public class ReceiveReconnectRetry : Core.IEvent
	{
	}

	public class ReceiveReconnectSuccessEvent : ReceiveSuccessEvent
	{
	}

	public class ReceiveReconnectFailureEvent : ReceiveFailureEvent
	{
	}

	public class ReceiveReconnectGiveUpEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
		public SubscriptionCursor Cursor { get; set; }
		public PNStatus Status { get; set; }
	}

	public class DisconnectEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
		public SubscriptionCursor Cursor { get; set; }
	}

	public class ReconnectEvent : Core.IEvent
	{
		public IEnumerable<string> Channels { get; set; }
		public IEnumerable<string> ChannelGroups { get; set; }
		public SubscriptionCursor Cursor { get; set; }
	}
}