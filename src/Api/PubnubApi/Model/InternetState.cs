using System;

namespace PubnubApi
{
    internal class InternetState<T>
    {
        public Action<bool> InternalCallback { get; internal set; }
        public PNCallback<T> PubnubCallbacck { get; internal set; }
        public string[] Channels { get; internal set; }
        public string[] ChannelGroups { get; internal set; }
        public PNOperationType ResponseType { get; internal set; }

        public InternetState()
        {
            InternalCallback = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}