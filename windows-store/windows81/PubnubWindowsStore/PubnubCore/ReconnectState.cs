using System;

namespace PubNubMessaging.Core
{
	public class ReconnectState<T>
	{
		public string[] Channels;
        public string[] ChannelGroups;
		public ResponseType Type;
		public Action<T> SubscribeOrPresenceRegularCallback;
        public Action<T> WildcardPresenceCallback;
		public Action<PubnubClientError> ErrorCallback;
		public Action<T> ConnectCallback;
		public object Timetoken;
        public bool Reconnect;

		public ReconnectState()
		{
			Channels = null;
            ChannelGroups = null;
			SubscribeOrPresenceRegularCallback = null;
            WildcardPresenceCallback = null;
			ConnectCallback = null;
			Timetoken = null;
            Reconnect = false;
		}
	}
}

