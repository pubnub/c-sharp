using System.Collections.Generic;
using PubnubApi.Model.Consumer.PNStatus;

namespace PubnubApi.EventEngine.Presence.Events {
    public class JoinedEvent : Core.IEvent 
    {
        public IEnumerable<string> Channels { get; set; }
        public IEnumerable<string> ChannelGroups { get; set; }
    }

    public class LeftEvent : Core.IEvent 
    {
        public IEnumerable<string> Channels { get; set; }
        public IEnumerable<string> ChannelGroups { get; set; }
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
