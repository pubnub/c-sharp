using System.Collections.Generic;
using PubnubApi.Model.Consumer.PNStatus;
using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Events {
    public class JoinedEvent : Core.IEvent 
    {
        public PresenceInput Input { get; set; }
    }

    public class LeftEvent : Core.IEvent 
    {
        public PresenceInput Input { get; set; }
    }

    public class LeftAllEvent : Core.IEvent {}

    public class HeartbeatSuccessEvent : Core.IEvent {}

    public class HeartbeatFailureEvent : Core.IEvent 
    {
        public PNStatus Status { get; set; }
    }

    public class HeartbeatGiveUpEvent : Core.IEvent 
    {
        public PNStatus Status { get; set; }
    }

    public class ReconnectEvent : Core.IEvent {}

    public class DisconnectEvent : Core.IEvent {}

    public class TimesUpEvent : Core.IEvent {}
}
