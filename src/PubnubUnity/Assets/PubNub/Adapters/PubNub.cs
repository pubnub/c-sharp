using System;
using System.Collections;
using System.Collections.Generic;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubnubApi.Unity {
	public static class PubnubExtensions {
		public static SubscribeOperation<T> Subscribe<T>(this Pubnub pn) => pn.Subscribe<T>();
		public static SubscribeOperation<string> Subscribe(this Pubnub pn) => pn.Subscribe<string>();


		public delegate void Listener(object sender, EventArgs args);

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

		public static bool AddListener(this Pubnub pn, Listener listener) => pn.AddListener(new SubscribeCallbackExt(
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { MessageResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { PresenceEventResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { SignalEventResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { ObjectEventResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { MessageActionsEventResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { FileEventResult = r }),
			(s, r) => listener(s, new SubscribeEventEventArgs<object>() { Status = r })
		));
	}
}