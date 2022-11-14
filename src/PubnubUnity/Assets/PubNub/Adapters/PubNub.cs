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