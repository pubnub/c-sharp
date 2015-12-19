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
        public string PublishKey;
        public string SubscribeKey;
        public string SecretKey;
        public string Origin;
        public string CipherKey;
        public bool Ssl;

		public ReconnectState()
		{
			Channels = null;
            ChannelGroups = null;
			SubscribeOrPresenceRegularCallback = null;
            WildcardPresenceCallback = null;
			ConnectCallback = null;
			Timetoken = null;
            Reconnect = false;
            PublishKey = "";
            SubscribeKey = "";
            SecretKey = "";
            Origin = "";
            CipherKey = "";
            Ssl = false;
		}
	}
}

