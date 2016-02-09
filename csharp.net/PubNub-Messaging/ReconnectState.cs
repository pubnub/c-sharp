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

    public class ReconnectState<T1, T2, T3,T4>
    {
        public string[] Channels;
        public string[] ChannelGroups;
        public ResponseType Type;
        public Action<T1> SubscribeRegularCallback;
        public Action<T2> PresenceRegularCallback;
        public Action<T3> ConnectCallback;
        public Action<T4> WildcardPresenceCallback;
        public Action<PubnubClientError> ErrorCallback;
        public object Timetoken;
        public bool Reconnect;

        public ReconnectState()
        {
            Channels = null;
            ChannelGroups = null;
            SubscribeRegularCallback = null;
            PresenceRegularCallback = null;
            WildcardPresenceCallback = null;
            ConnectCallback = null;
            Timetoken = null;
            Reconnect = false;
        }
    }
}

