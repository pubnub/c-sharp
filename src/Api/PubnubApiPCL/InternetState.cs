using System;

namespace PubnubApi
{
    internal class InternetState
    {
        public Action<bool> Callback;
        public Action<PubnubClientError> ErrorCallback;
        public string[] Channels;
        public string[] ChannelGroups;

        public InternetState()
        {
            Callback = null;
            ErrorCallback = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}