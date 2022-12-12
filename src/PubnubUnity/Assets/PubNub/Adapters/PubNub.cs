using System;
using System.Collections;
using System.Collections.Generic;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubnubApi.Unity {
	public static class PubnubExtensions {
		public static SubscribeOperation<T> Subscribe<T>(this Pubnub pn) => pn.Subscribe<T>();
		
		[Obsolete("Use the generic version instead")]
		public static SubscribeOperation<string> Subscribe(this Pubnub pn) => pn.Subscribe<string>();

		[Obsolete("Use the generic version instead")]
		public static bool Reconnect(this Pubnub pn) => pn.Reconnect<string>();

		/// <summary>
		/// Add an event listener that dispatches to the main Unity thread. This allows manipulation of the built-in classes within callbacks.
		/// </summary>
		/// <param name="pn">PubNub instance</param>
		/// <param name="listener">Event listener instance</param>
		/// <returns>Operation status</returns>
		public static bool AddListener(this Pubnub pn, SubscribeCallbackListener listener) {
			return pn.AddListener(listener);
		}

		
		// TODO remove (?)
		public delegate void Listener(object sender, EventArgs args);

		// TODO remove (?)
		[Obsolete("Use the overload accepting SubscribeCallback instead")]
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