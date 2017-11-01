using System;

namespace PubnubApi
{
	internal class ReconnectState<T>
	{
		public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public PNOperationType ResponseType { get; set; }
        public PNCallback<T> PubnubCallback { get; set; }
        public object Timetoken { get; set; }
        public bool Reconnect { get; set; }

        public ReconnectState()
		{
			Channels = null;
			ChannelGroups = null;
			Timetoken = null;
			Reconnect = false;
		}
	}
}
