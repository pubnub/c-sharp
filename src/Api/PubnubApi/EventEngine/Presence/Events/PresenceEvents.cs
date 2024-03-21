using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Events
{
	public class JoinedEvent : Core.IEvent
	{
		public PresenceInput Input { get; set; }
		public string Name { get; set; } = "JOINED";
	}

	public class LeftEvent : Core.IEvent
	{
		public PresenceInput Input { get; set; }
		public string Name { get; set; } = "LEFT";
	}

	public class LeftAllEvent : Core.IEvent {
		public string Name { get; set; } = "LEFT_ALL";
	 }

	public class HeartbeatSuccessEvent : Core.IEvent {
		public virtual string Name { get; set; } = "HEARTBEAT_SUCCESS";
	 }

	public class HeartbeatFailureEvent : Core.IEvent
	{
		public PNStatus Status { get; set; }
		public int retryCount;
		public virtual string Name { get; set; } = "HEARTBEAT_FAILURE";
	}

	public class HeartbeatGiveUpEvent : Core.IEvent
	{
		public PNStatus Status { get; set; }
		public string Name { get; set; } = "HEARTBEAT_GIVEUP";
	}

	public class ReconnectEvent : Core.IEvent {
		public string Name { get; set; } = "RECONNECT";
	}

	public class DisconnectEvent : Core.IEvent { 
		public string Name { get; set; } = "DISCONNECT";
	}

	public class TimesUpEvent : Core.IEvent { 
		public string Name { get; set; } = "TIMES_UP";
	}
}
