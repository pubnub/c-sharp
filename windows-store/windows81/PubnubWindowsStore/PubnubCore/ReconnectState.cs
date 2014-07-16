using System;

namespace PubNubMessaging.Core
{
	public class ReconnectState<T>
	{
		public string[] Channels;
		public ResponseType Type;
		public Action<T> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public Action<T> ConnectCallback;
		public object Timetoken;
        public bool Reconnect;

		public ReconnectState()
		{
			Channels = null;
			Callback = null;
			ConnectCallback = null;
			Timetoken = null;
            Reconnect = false;
		}
	}
}

