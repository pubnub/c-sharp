using System;

namespace PubnubApi
{
	internal class ReconnectState<T>
	{
		public string[] Channels;
		public string[] ChannelGroups;
		public ResponseType ResponseType;
		public Action<T> NonSubscribeRegularCallback;
		public Action<Message<T>> SubscribeRegularCallback;
		public Action<PresenceAck> PresenceRegularCallback;
		public Action<ConnectOrDisconnectAck> ConnectCallback;
		public Action<PresenceAck> WildcardPresenceCallback;
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
