using System;

namespace PubnubApi
{
    internal class InternetState
    {
        public Action<bool> Callback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }

        public InternetState()
        {
            Callback = null;
            ErrorCallback = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}