using System;

namespace PubnubApi
{
	internal class ReconnectState<T>
	{
		public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public ResponseType ResponseType { get; set; }
        public Action<T> NonSubscribeRegularCallback { get; set; }
        public Action<Message<T>> SubscribeRegularCallback { get; set; }
        public Action<PresenceAck> PresenceRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<PresenceAck> WildcardPresenceCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }
        public object Timetoken { get; set; }
        public bool Reconnect { get; set; }

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
