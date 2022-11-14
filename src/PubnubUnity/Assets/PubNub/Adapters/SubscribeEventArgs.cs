using System;

namespace PubnubApi.Unity {
	
	// TODO check compatibility
	public class SubscribeEventEventArgs<T> : EventArgs {
		public PNStatus Status { get; set; }
		public PNPresenceEventResult PresenceEventResult { get; set; }
		public PNMessageResult<T> MessageResult { get; set; }
		public PNSignalResult<T> SignalEventResult { get; set; }
		public PNUuidMetadataResult UUIDEventResult { get; set; }
		public PNChannelMetadataResult ChannelEventResult { get; set; }
		public PNMembershipsResult MembershipEventResult { get; set; }
		public PNMessageActionEventResult MessageActionsEventResult { get; set; }
		public PNFileEventResult FileEventResult { get; set; }
		public PNObjectEventResult ObjectEventResult { get; set; }
	}
}