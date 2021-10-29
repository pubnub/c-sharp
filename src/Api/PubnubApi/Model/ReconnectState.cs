using System;

namespace PubnubApi
{
	internal class ReconnectState<T>
	{
		public string[] Channels { get; internal set; }
        public string[] ChannelGroups { get; internal set; }
        public PNOperationType ResponseType { get; internal set; }
        public PNCallback<T> PubnubCallback { get; internal set; }
        public object Timetoken { get; internal set; }
		public int Region { get; internal set; }

		public bool Reconnect { get; internal set; }

        public ReconnectState()
		{
			Channels = null;
			ChannelGroups = null;
			Timetoken = null;
			Reconnect = false;
		}
	}
}
