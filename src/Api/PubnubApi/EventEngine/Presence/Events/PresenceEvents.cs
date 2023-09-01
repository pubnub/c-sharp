using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.Events
{
	public class JoinedEvent : Core.IEvent {
		public IEnumerable<string> Channels{ get; set; }
		public IEnumerable<string> ChannelGroups{ get; set; }
		public string Name { get; set; } = "JOINED";
	}
	public class LeftEvent : Core.IEvent {
		public IEnumerable<string> Channels{ get; set; }
		public IEnumerable<string> ChannelGroups{ get; set; }
		public string Name { get; set; } = "LEFT";
	}
	public class StateSetEvent : Core.IEvent {
		public IEnumerable<string> Channels{ get; set; }
		public IEnumerable<string> ChannelGroups{ get; set; }
		public string Name { get; set; } = "STATE_SET";
	}
    public class LeftAllEvent : Core.IEvent {
		public string Name { get; set; } = "LEFT_ALL";
	}
	public class HeartbeatSuccessEvent : Core.IEvent {
		public PNStatus Status { get; set; }
		public virtual string Name { get; set; } = "HEARTBEAT_SUCCESS";
	}
	public class HeartbeatFailureEvent : Core.IEvent {
		public PNStatus Status { get; set; }
		public virtual string Name { get; set; } = "HEARTBEAT_FAILURE";
	}
	public class HeartbeatGiveUpEvent : Core.IEvent {  
		public PNStatus Status { get; set; }
		public virtual string Name { get; set; }
	}
	public class DisconnectEvent : Core.IEvent {
		public IEnumerable<string> Channels{ get; set; }
		public IEnumerable<string> ChannelGroups{ get; set; }
		public string Name { get; set; } = "DISCONNECT";
	}
	public class TimesUpEvent : Core.IEvent
	{
		public IEnumerable<string> Channels{ get; set; }
		public IEnumerable<string> ChannelGroups{ get; set; }
		public string Name { get; set; } = "TIMES_UP";
	}
}
